using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vamp.Core.Config;
using Vamp.Core.Math;
using Vamp.Core.Meta;
using Vamp.Core.Simulation;
using Vamp.Platform;

namespace Vamp.Game
{
    /// <summary>
    /// Composition root and app state machine. Owns the simulation, presentation systems, save and platform
    /// services. Everything is created from code so the game runs from an empty Boot scene.
    /// </summary>
    public sealed partial class GameRoot : MonoBehaviour
    {
        /// <summary>The single instance.</summary>
        public static GameRoot Instance { get; private set; }

        [SerializeField, Tooltip("URP Simple Lit material template (assigned by Brasslight > Setup).")]
        private Material _litTemplate;
        [SerializeField, Tooltip("URP Unlit material template (assigned by Brasslight > Setup).")]
        private Material _unlitTemplate;

        internal BalanceConfig Config;
        internal SaveData Save;
        internal MetaProgress Meta;
        internal SaveStore Store;
        internal IPlatformServices Platform;
        internal readonly Loc Loc = new Loc();
        internal GameInput Input;
        internal CameraRig Rig;
        internal PostFx Post;
        internal WorldView World;
        internal SpriteBatcher Batcher;
        internal MaterialFactory Materials;
        internal Vfx Fx;
        internal SoundBank Sound;
        internal Hud Hud;
        internal GameSimulation Sim;
        internal GameScreen Screen = GameScreen.Loading;
        internal QualityTier Tier;
        internal int SelectedWorld, SelectedLeader;
        internal int LastRunPoints;
        internal readonly List<string> NewAchievements = new List<string>();
        internal readonly List<int> RecruitedThisRun = new List<int>();
        internal readonly HashSet<int> SeenThisRun = new HashSet<int>();

        private readonly AutoPilot _pilot = new AutoPilot();
        private bool _attract;
        private float _achievementTimer;
        private string _enJson, _activeJson;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            foreach (var cam in Camera.allCameras) cam.gameObject.SetActive(false);

            Platform = PlatformFactory.Create();
            Store = new SaveStore(Platform);
            Input = new GameInput();
            Rig = new CameraRig(transform);
            Post = new PostFx(transform);
            Materials = new MaterialFactory(_litTemplate, _unlitTemplate);
            Batcher = new SpriteBatcher();
            World = new WorldView(transform, Materials);
            Fx = new Vfx(transform, Batcher, Materials);
            Sound = new SoundBank(transform);
            Hud = new Hud(this);
        }

        private IEnumerator Start()
        {
            string balanceJson = null;
            yield return StreamingFile.Read("balance.json", t => balanceJson = t);
            Config = LoadConfig(balanceJson);

            Save = Store.Load();
            Meta = new MetaProgress(Config, Save);
            yield return LoadLanguage(Loc.Resolve(Save.settings.language));

            BuildSprites();
            SelectedWorld = Mathf.Max(0, Config.WorldIndex(Save.selectedWorld));
            SelectedLeader = Mathf.Max(0, Config.LeaderIndex(Save.selectedLeader));
            ApplySettings();
            StartAttract();
            Screen = GameScreen.Title;
        }

        private static BalanceConfig LoadConfig(string json)
        {
            var config = BalanceConfig.CreateDefault();
            if (!string.IsNullOrEmpty(json))
            {
                var loaded = JsonUtility.FromJson<BalanceConfig>(json);
                var errors = loaded?.Resolve();
                if (loaded != null && errors.Count == 0) return loaded;
                Debug.LogError("[Config] balance.json invalid, using defaults: " + (errors == null ? "parse error" : string.Join("; ", errors)));
            }
            config.Resolve();
            return config;
        }

        internal IEnumerator LoadLanguage(string lang)
        {
            if (_enJson == null) yield return StreamingFile.Read("loc/en.json", t => _enJson = t);
            if (lang == "en") _activeJson = _enJson;
            else yield return StreamingFile.Read($"loc/{lang}.json", t => _activeJson = t);
            Loc.Load(lang, _activeJson, _enJson);
        }

        /// <summary>Applies options (quality, audio, accessibility) and persists nothing.</summary>
        internal void ApplySettings()
        {
            var s = Save.settings;
            Tier = QualityTierDetector.Resolve(s.qualityTier);
            Rig.ShakeEnabled = s.screenShake;
            Rig.HalfHeight = Application.isMobilePlatform ? 8.5f : 9.5f;
            Fx.ShowNumbers = s.damageNumbers;
            Sound.SetVolumes(s.musicVolume, s.sfxVolume);
            PostFx.EnableOn(Rig.Cam, Tier);
            PostFx.ApplyPipeline(Tier);
            var w = Config.worlds[Mathf.Clamp(Sim != null ? Sim.Setup.World : SelectedWorld, 0, Config.worlds.Length - 1)];
            Post.Apply(Tier, s.depthOfField, Rig.Distance, VecExt.Hex(w.accentColor));
            World.ApplyQuality(Tier, Rig.Distance, VecExt.Hex(w.fogColor));
            Rig.Cam.backgroundColor = VecExt.Hex(w.fogColor);
        }

        internal void WriteSave()
        {
            Save.selectedWorld = Config.worlds[SelectedWorld].id;
            Save.selectedLeader = Config.leaders[SelectedLeader].id;
            Store.Write(Save);
        }

        private void StartAttract()
        {
            _attract = true;
            CreateRun(SelectedWorld, 0, 12345u, new RunModifiers { MaxHpBonus = 100000f });
            Sound.Music(true, 0.94f);
        }

        /// <summary>Starts a real expedition.</summary>
        internal void StartRun()
        {
            _attract = false;
            WriteSave();
            RecruitedThisRun.Clear();
            SeenThisRun.Clear();
            NewAchievements.Clear();
            CreateRun(SelectedWorld, SelectedLeader, (uint)System.Environment.TickCount | 1u, Meta.CreateModifiers(SelectedLeader));
            Sound.Music(true, 1f);
            Screen = GameScreen.Run;
        }

        private void CreateRun(int world, int leader, uint seed, RunModifiers mods)
        {
            Sim = new GameSimulation(Config, new RunSetup { World = world, Leader = leader, Seed = seed, Mobile = Application.isMobilePlatform, Modifiers = mods });
            World.SetWorld(Config.worlds[world], world, seed, Config.build);
            Fx.Clear();
            ApplySettings();
            Rig.Snap(Sim.Player.Pos.ToWorld());
        }

        /// <summary>Returns to the title screen with the attract run.</summary>
        internal void ReturnToTitle()
        {
            StartAttract();
            Screen = GameScreen.Title;
        }

        /// <summary>Abandons the current run (counts as a defeat).</summary>
        internal void AbandonRun()
        {
            if (Sim == null || _attract) { ReturnToTitle(); return; }
            EndRun();
        }

        private void Update()
        {
            Platform.Tick();
            Input.Update();
            var screenBefore = Screen;
            Hud.Update();
            if (Sim == null) return;
            // A key that just closed a menu (Escape = back and pause) must not reopen it in the same frame.
            bool screenChanged = screenBefore != Screen;

            if (_attract)
            {
                if (Sim.IsOver) StartAttract();
                for (int g = 0; g < 10 && (Sim.Phase == RunPhase.LevelUpChoice || Sim.Phase == RunPhase.BuildChoice); g++) _pilot.ResolveChoices(Sim);
                Sim.Step(Time.deltaTime, _pilot.Steer(Sim));
                ConsumeEvents(false);
                return;
            }

            if (Screen == GameScreen.Run)
            {
                if ((Input.Pause && !screenChanged) || Platform.IsOverlayActive) { OpenPause(); return; }
                Sim.Step(Time.deltaTime, Input.Move.ToSim());
                ConsumeEvents(true);
                if (Sim.Phase == RunPhase.LevelUpChoice) { Screen = GameScreen.LevelUp; Input.ResetTouch(); }
                else if (Sim.Phase == RunPhase.BuildChoice) { Screen = GameScreen.Build; Input.ResetTouch(); }

                _achievementTimer -= Time.deltaTime;
                if (_achievementTimer <= 0f)
                {
                    _achievementTimer = 1f;
                    var list = new List<string>();
                    AchievementRules.EvaluateDuringRun(Sim, Save, list);
                    ReportAchievements(list);
                }
                if (Sim.IsOver) EndRun();
            }
        }

        private void ConsumeEvents(bool audible)
        {
            foreach (var e in Sim.Events)
            {
                Rig.Shake(Fx.Handle(e));
                if (!audible) continue;
                Sound.Play(e.Type);
                switch (e.Type)
                {
                    case SimEventType.EnemyKilled: SeenThisRun.Add(e.Index); break;
                    case SimEventType.Evolved: Hud.Toast(Loc.F("toast.evolved", Loc.Get("evolution." + Config.gadgets[e.Index].id + ".name", Config.gadgets[e.Index].evolvedName))); break;
                    case SimEventType.Synergy: Hud.Toast(Loc[e.Value > 1.5f ? "toast.overclocked" : "toast.charged"]); break;
                    case SimEventType.BossCharge: Rig.Shake(0.3f); break;
                    case SimEventType.EliteSpawned: Hud.Toast(Loc.F("toast.elite", Config.enemies[e.Index].displayName)); break;
                    case SimEventType.BossSpawned: Hud.Toast(Loc.F("toast.boss", Config.enemies[e.Index].displayName)); break;
                    case SimEventType.Recruited:
                        RecruitedThisRun.Add(e.Index);
                        Hud.Toast(Loc.F("toast.recruited", Config.enemies[e.Index].displayName));
                        break;
                    case SimEventType.Scrapped: Hud.Toast(Loc.F("toast.scrapped", (int)e.Value)); break;
                    case SimEventType.CompanionLost: Hud.Toast(Loc.F("toast.lost", Config.enemies[e.Index].displayName)); break;
                    case SimEventType.NotEnoughBrass: Hud.Toast(Loc.F("toast.nobrass", (int)e.Value)); break;
                    case SimEventType.StructureDestroyed: Hud.Toast(Loc.F("toast.destroyed", Config.structures[e.Index].displayName)); break;
                    case SimEventType.PlayerRevived: Hud.Toast(Loc["toast.revived"]); break;
                }
            }
        }

        private void EndRun()
        {
            LastRunPoints = Meta.RecordRun(Sim, RecruitedThisRun, SeenThisRun);
            var list = new List<string>();
            AchievementRules.EvaluateDuringRun(Sim, Save, list);
            AchievementRules.EvaluateLifetime(Config, Save, list);
            ReportAchievements(list);
            WriteSave();
            Screen = GameScreen.Results;
            Sound.Music(true, 0.9f);
        }

        internal void ReportAchievements(List<string> list)
        {
            foreach (var id in list)
            {
                Platform.UnlockAchievement(id);
                NewAchievements.Add(id);
                Hud.Toast(Loc.F("toast.achievement", Loc["ach." + id]));
            }
        }

        internal void OpenPause()
        {
            if (Screen != GameScreen.Run) return;
            Screen = GameScreen.Pause;
            Input.ResetTouch();
            WriteSave();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) OpenPause();
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused) OpenPause();
        }

        private void OnApplicationQuit()
        {
            if (Save != null) WriteSave();
            Platform?.Shutdown();
        }

        private void LateUpdate()
        {
            if (Sim == null) return;
            var focus = Sim.Player.Pos.ToWorld();
            Rig.Follow(focus, Time.unscaledDeltaTime);
            World.Update(focus, Time.unscaledDeltaTime);
            bool paused = Screen != GameScreen.Run && !_attract;
            if (!paused) Fx.Update(Time.deltaTime);
            DrawWorld();
        }

        private void OnGUI()
        {
            Hud.Draw();
        }
    }
}
