using System;
using System.Collections.Generic;
using UnityEngine;
using Vamp.Core.Config;
using Vamp.Core.Simulation;

namespace Vamp.Game
{
    /// <summary>
    /// All screens and the in-run HUD, drawn with IMGUI on a virtual 1280x720 canvas that respects the safe area.
    /// Every menu is a list of <see cref="Item"/>s navigable by mouse/touch, keyboard and gamepad
    /// (no mouse-only UI, required for Steam Deck). Placeholder until the UI Toolkit implementation
    /// (design/ux, unity-ui-specialist).
    /// </summary>
    public sealed partial class Hud
    {
        private sealed class Item
        {
            public string Label;
            public string Detail;
            public bool Enabled = true;
            public Action Submit;
            public Action<int> Adjust;
        }

        private struct ToastMsg { public string Text; public float Until; }

        private readonly GameRoot _g;
        private readonly List<Item> _items = new List<Item>();
        private readonly List<ToastMsg> _toasts = new List<ToastMsg>();
        private int _sel;
        private const int PauseHit = -2;
        private readonly List<Rect> _hitRects = new List<Rect>(16);
        private readonly List<int> _hitIndex = new List<int>(16);
        private GameScreen _lastScreen = GameScreen.Loading;
        private bool _horizontal;
        private Action _back;
        private float _scale = 1f;
        private float _w = 1280f, _h = 720f;
        private Rect _safe;

        /// <summary>Creates the HUD for a game root.</summary>
        public Hud(GameRoot root) { _g = root; }

        /// <summary>Shows a short message.</summary>
        public void Toast(string text)
        {
            if (_toasts.Count > 4) _toasts.RemoveAt(0);
            _toasts.Add(new ToastMsg { Text = text, Until = Time.unscaledTime + 3f });
        }

        /// <summary>Rebuilds the current menu and processes navigation input. Call from Update.</summary>
        public void Update()
        {
            if (_g.Screen != _lastScreen)
            {
                _sel = 0;
                _lastScreen = _g.Screen;
            }
            _items.Clear();
            _back = null;
            _horizontal = false;
            if (_g.Config == null) return;
            BuildItems();
            if (_items.Count == 0) return;
            _sel = Mathf.Clamp(_sel, 0, _items.Count - 1);

            var input = _g.Input;
            // Touch taps via the Input System (IMGUI may not receive touches with the legacy input backend off).
            foreach (var tap in input.Taps)
            {
                var p = tap / _scale;
                for (int i = 0; i < _hitRects.Count; i++)
                {
                    if (!_hitRects[i].Contains(p)) continue;
                    if (_hitIndex[i] == PauseHit) _g.OpenPause();
                    else if (_hitIndex[i] < _items.Count) { _sel = _hitIndex[i]; Activate(_sel); }
                    return;
                }
            }
            int dir = _horizontal ? input.NavStep.x : -input.NavStep.y;
            if (dir != 0) { _sel = (_sel + dir + _items.Count) % _items.Count; _g.Sound.Click(); }
            int adjust = _horizontal ? 0 : input.NavStep.x;
            if (adjust != 0 && _items[_sel].Adjust != null && _items[_sel].Enabled) { _items[_sel].Adjust(adjust); _g.Sound.Click(); }
            if (input.Submit) Activate(_sel);
            else if (input.Cancel && _back != null) { _back(); _g.Sound.Click(); }
        }

        private void Activate(int index)
        {
            if (index < 0 || index >= _items.Count) return;
            var it = _items[index];
            if (!it.Enabled) return;
            _g.Sound.Click();
            if (it.Submit != null) it.Submit();
            else it.Adjust?.Invoke(1);
        }

        private Item Add(string label, Action submit, string detail = null, bool enabled = true, Action<int> adjust = null)
        {
            var it = new Item { Label = label, Submit = submit, Detail = detail, Enabled = enabled, Adjust = adjust };
            _items.Add(it);
            return it;
        }

        private string L(string key) => _g.Loc[key];

        private string Name(string prefix, string id, string fallback) => _g.Loc.Get(prefix + "." + id + ".name", fallback);

        private string Desc(string prefix, string id, string fallback) => _g.Loc.Get(prefix + "." + id + ".desc", fallback);

        private void BuildItems()
        {
            var g = _g;
            var cfg = g.Config;
            switch (g.Screen)
            {
                case GameScreen.Title:
                    Add(L("menu.start"), () => g.Screen = GameScreen.Expedition);
                    Add(L("menu.research"), () => g.Screen = GameScreen.Research, g.Loc.F("menu.points", g.Save.researchPoints));
                    Add(L("menu.bestiary"), () => g.Screen = GameScreen.Bestiary, g.Loc.F("bestiary.count", g.Save.seenSpecies.Count, cfg.enemies.Length));
                    Add(L("menu.options"), () => g.Screen = GameScreen.Options);
                    Add(L("menu.credits"), () => g.Screen = GameScreen.Credits);
                    if (!Application.isMobilePlatform) Add(L("menu.quit"), Application.Quit);
                    break;

                case GameScreen.Expedition:
                {
                    var w = cfg.worlds[g.SelectedWorld];
                    bool worldOpen = g.Meta.IsWorldUnlocked(w);
                    Add("◀ " + Name("world", w.id, w.displayName) + " ▶", () => CycleWorld(1),
                        worldOpen ? w.source : g.Loc.F("expedition.locked", Name("world", w.unlockedByWorld, w.unlockedByWorld)), true, CycleWorld);
                    var l = cfg.leaders[g.SelectedLeader];
                    bool leaderOpen = g.Meta.IsLeaderUnlocked(l);
                    Add("◀ " + Name("leader", l.id, l.displayName) + " ▶", () => CycleLeader(1),
                        leaderOpen ? Desc("leader", l.id, l.description) : g.Loc.F("expedition.leaderlocked", l.unlockCost), true, CycleLeader);
                    if (!leaderOpen)
                        Add(g.Loc.F("expedition.unlock", l.unlockCost), () => { if (g.Meta.UnlockLeader(l)) { AfterMetaChange(); } }, null, g.Save.researchPoints >= l.unlockCost);
                    Add(L("expedition.begin"), g.StartRun, null, worldOpen && leaderOpen);
                    Add(L("menu.back"), () => g.Screen = GameScreen.Title);
                    _back = () => g.Screen = GameScreen.Title;
                    break;
                }

                case GameScreen.Research:
                    foreach (var u in cfg.metaUpgrades)
                    {
                        int lvl = g.Meta.Level(u.id), cost = g.Meta.NextCost(u);
                        string detail = Desc("research", u.id, u.description) + "   " + (cost < 0 ? L("research.max") : g.Loc.F("research.cost", cost));
                        var up = u;
                        Add($"{Name("research", u.id, u.displayName)}  {lvl}/{u.maxLevel}", () => { if (g.Meta.Buy(up)) AfterMetaChange(); }, detail, cost >= 0 && g.Save.researchPoints >= cost);
                    }
                    Add(L("menu.back"), () => { g.WriteSave(); g.Screen = GameScreen.Title; });
                    _back = () => { g.WriteSave(); g.Screen = GameScreen.Title; };
                    break;

                case GameScreen.Options:
                    BuildOptions();
                    break;

                case GameScreen.Bestiary:
                case GameScreen.Credits:
                    Add(L("menu.back"), () => g.Screen = GameScreen.Title);
                    _back = () => g.Screen = GameScreen.Title;
                    break;

                case GameScreen.Pause:
                    Add(L("pause.resume"), () => g.Screen = GameScreen.Run);
                    Add(L("menu.options"), () => { _optionsReturn = GameScreen.Pause; g.Screen = GameScreen.Options; });
                    Add(L("pause.abandon"), g.AbandonRun);
                    _back = () => g.Screen = GameScreen.Run;
                    break;

                case GameScreen.LevelUp:
                    _horizontal = true;
                    for (int i = 0; i < g.Sim.LevelUpOffers.Count; i++)
                    {
                        var o = g.Sim.LevelUpOffers[i];
                        int idx = i;
                        Add(OfferTitle(o), () => { g.Sim.ChooseUpgrade(idx); if (g.Sim.Phase == RunPhase.Playing) g.Screen = GameScreen.Run; }, OfferDetail(o));
                    }
                    break;

                case GameScreen.Build:
                    _horizontal = true;
                    for (int i = 0; i < g.Sim.BuildOffers.Count; i++)
                    {
                        var d = cfg.structures[g.Sim.BuildOffers[i]];
                        int idx = i;
                        Add(Name("structure", d.id, d.displayName), () => { if (g.Sim.ChooseStructure(idx)) g.Screen = GameScreen.Run; },
                            Desc("structure", d.id, d.description) + "\n" + g.Loc.F("build.cost", d.cost), g.Sim.CanAfford(g.Sim.BuildOffers[i]));
                    }
                    Add(L("build.leave"), () => { g.Sim.ChooseStructure(-1); g.Screen = GameScreen.Run; });
                    _back = () => { g.Sim.ChooseStructure(-1); g.Screen = GameScreen.Run; };
                    break;

                case GameScreen.Results:
                    Add(L("results.continue"), g.ReturnToTitle);
                    _back = g.ReturnToTitle;
                    break;
            }
        }

        private GameScreen _optionsReturn = GameScreen.Title;

        private void BuildOptions()
        {
            var g = _g;
            var s = g.Save.settings;
            Action back = () =>
            {
                g.WriteSave();
                g.Screen = _optionsReturn;
                _optionsReturn = GameScreen.Title;
            };
            Add(g.Loc.F("options.music", Mathf.RoundToInt(s.musicVolume * 100)), null, null, true, d => { s.musicVolume = Mathf.Clamp01(s.musicVolume + d * 0.1f); g.ApplySettings(); });
            Add(g.Loc.F("options.sfx", Mathf.RoundToInt(s.sfxVolume * 100)), null, null, true, d => { s.sfxVolume = Mathf.Clamp01(s.sfxVolume + d * 0.1f); g.ApplySettings(); });
            string tier = s.qualityTier < 0 ? L("options.auto") + " (" + L("quality." + g.Tier) + ")" : L("quality." + (QualityTier)s.qualityTier);
            Add(g.Loc.F("options.quality", tier), null, null, true, d => { s.qualityTier = (s.qualityTier + 1 + d + 5) % 5 - 1; g.ApplySettings(); });
            Add(g.Loc.F("options.dof", OnOff(s.depthOfField)), null, null, true, d => { s.depthOfField = !s.depthOfField; g.ApplySettings(); });
            Add(g.Loc.F("options.shake", OnOff(s.screenShake)), null, null, true, d => { s.screenShake = !s.screenShake; g.ApplySettings(); });
            Add(g.Loc.F("options.numbers", OnOff(s.damageNumbers)), null, null, true, d => { s.damageNumbers = !s.damageNumbers; g.ApplySettings(); });
            Add(g.Loc.F("options.language", L("language." + g.Loc.Language)), null, null, true, d =>
            {
                int i = Array.IndexOf(Loc.Languages, g.Loc.Language);
                s.language = Loc.Languages[(i + d + Loc.Languages.Length) % Loc.Languages.Length];
                g.StartCoroutine(g.LoadLanguage(s.language));
            });
            Add(L("menu.back"), back);
            _back = back;
        }

        private string OnOff(bool b) => b ? L("options.on") : L("options.off");

        /// <summary>Bestiary text: every creature per world, unknown until defeated, ★ once recruited.</summary>
        private string BestiaryText()
        {
            var cfg = _g.Config;
            var sb = new System.Text.StringBuilder();
            foreach (var w in cfg.worlds)
            {
                sb.Append(Name("world", w.id, w.displayName)).Append(":  ");
                bool first = true;
                foreach (var id in w.enemyIds)
                {
                    if (!first) sb.Append(" · ");
                    first = false;
                    var e = cfg.enemies[cfg.EnemyIndex(id)];
                    bool seen = _g.Save.seenSpecies.Contains(id);
                    sb.Append(seen ? Name("enemy", id, e.displayName) : L("bestiary.unknown"));
                    if (_g.Save.recruitedSpecies.Contains(id)) sb.Append(" ★");
                }
                sb.Append('\n');
            }
            sb.Append('\n').Append(L("bestiary.legend"));
            return sb.ToString();
        }

        private void CycleWorld(int d)
        {
            int n = _g.Config.worlds.Length;
            _g.SelectedWorld = (_g.SelectedWorld + d + n) % n;
        }

        private void CycleLeader(int d)
        {
            int n = _g.Config.leaders.Length;
            _g.SelectedLeader = (_g.SelectedLeader + d + n) % n;
        }

        private void AfterMetaChange()
        {
            var list = new List<string>();
            Vamp.Core.Meta.AchievementRules.EvaluateLifetime(_g.Config, _g.Save, list);
            _g.ReportAchievements(list);
            _g.WriteSave();
        }

        private string OfferTitle(UpgradeOption o)
        {
            var cfg = _g.Config;
            switch (o.Kind)
            {
                case UpgradeOptionKind.NewGadget: return Name("gadget", cfg.gadgets[o.Index].id, cfg.gadgets[o.Index].displayName);
                case UpgradeOptionKind.GadgetLevel: return Name("gadget", cfg.gadgets[o.Index].id, cfg.gadgets[o.Index].displayName);
                case UpgradeOptionKind.Passive: return Name("passive", cfg.passives[o.Index].id, cfg.passives[o.Index].displayName);
                case UpgradeOptionKind.Evolve: return _g.Loc.Get("evolution." + cfg.gadgets[o.Index].id + ".name", cfg.gadgets[o.Index].evolvedName);
                default: return L("levelup.heal");
            }
        }

        private string OfferDetail(UpgradeOption o)
        {
            var cfg = _g.Config;
            switch (o.Kind)
            {
                case UpgradeOptionKind.NewGadget: return L("levelup.new") + "\n" + Desc("gadget", cfg.gadgets[o.Index].id, cfg.gadgets[o.Index].description);
                case UpgradeOptionKind.GadgetLevel: return _g.Loc.F("levelup.level", o.NextLevel) + "\n" + Desc("gadget", cfg.gadgets[o.Index].id, cfg.gadgets[o.Index].description);
                case UpgradeOptionKind.Passive: return _g.Loc.F("levelup.level", o.NextLevel) + "\n" + Desc("passive", cfg.passives[o.Index].id, cfg.passives[o.Index].description);
                case UpgradeOptionKind.Evolve: return L("levelup.evolve") + "\n" + _g.Loc.Get("evolution." + cfg.gadgets[o.Index].id + ".desc", cfg.gadgets[o.Index].evolvedDescription);
                default: return L("levelup.healdesc");
            }
        }
    }
}
