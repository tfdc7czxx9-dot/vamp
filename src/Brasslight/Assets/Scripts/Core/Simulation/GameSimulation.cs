using System;
using System.Collections.Generic;
using Vamp.Core.Config;
using Vamp.Core.Math;

namespace Vamp.Core.Simulation
{
    /// <summary>
    /// The complete, engine-independent simulation of one run. The Unity layer feeds movement
    /// input via <see cref="Step"/>, renders the public state arrays and reacts to <see cref="Events"/>.
    /// Deterministic for a given seed and input sequence.
    /// </summary>
    public sealed partial class GameSimulation
    {
        private const int MaxEvents = 2048;
        private const int MaxProjectiles = 1024;
        private const int MaxPickups = 1500;
        private const int MaxRemnants = 16;
        private const int MaxStructures = 64;
        private const float FixedStep = 1f / 60f;

        private readonly int[] _query = new int[1024];
        private readonly int[] _chainHit = new int[16];
        private readonly List<UpgradeOption> _candidates = new List<UpgradeOption>(32);
        private readonly int[] _activeLures = new int[MaxStructures];
        private int _activeLureCount;
        private readonly Stack<int> _freeEnemies;
        private float _spawnAccumulator;
        private float _nextEliteAt;
        private bool _bossSpawned;
        private float _accumulator;

        /// <summary>Balance data in use.</summary>
        public BalanceConfig Config { get; }
        /// <summary>Start parameters.</summary>
        public RunSetup Setup { get; }
        /// <summary>World being played.</summary>
        public WorldDef World { get; }
        /// <summary>Random source.</summary>
        public Rng Rng { get; }
        /// <summary>Current phase.</summary>
        public RunPhase Phase { get; private set; }
        /// <summary>Elapsed simulated time (s).</summary>
        public float Time { get; private set; }
        /// <summary>Counters.</summary>
        public RunStats Stats { get; } = new RunStats();
        /// <summary>Player state.</summary>
        public PlayerState Player { get; }
        /// <summary>Presentation events of the last <see cref="Step"/>.</summary>
        public List<SimEvent> Events { get; } = new List<SimEvent>(MaxEvents);

        /// <summary>Enemy pool (check <c>Active</c>).</summary>
        public Enemy[] Enemies { get; }
        /// <summary>Number of active enemies.</summary>
        public int EnemyCount { get; private set; }
        /// <summary>Normal-enemy cap for this platform.</summary>
        public int EnemyCap { get; }
        /// <summary>Projectile pool.</summary>
        public Projectile[] Projectiles { get; } = new Projectile[MaxProjectiles];
        /// <summary>Pickup pool.</summary>
        public Pickup[] Pickups { get; } = new Pickup[MaxPickups];
        /// <summary>Remnant pool.</summary>
        public Remnant[] Remnants { get; } = new Remnant[MaxRemnants];
        /// <summary>Companion slots (length = max crew capacity).</summary>
        public Companion[] Companions { get; }
        /// <summary>Structure pool.</summary>
        public Structure[] Structures { get; } = new Structure[MaxStructures];
        /// <summary>Gadgets carried by the player.</summary>
        public List<OwnedGadget> Gadgets { get; } = new List<OwnedGadget>(8);
        /// <summary>Passive levels aligned with <c>Config.passives</c>.</summary>
        public int[] PassiveLevels { get; }
        /// <summary>Current level-up offers (valid in <see cref="RunPhase.LevelUpChoice"/>).</summary>
        public List<UpgradeOption> LevelUpOffers { get; } = new List<UpgradeOption>(4);
        /// <summary>Current structure offers as structure def indices (valid in <see cref="RunPhase.BuildChoice"/>).</summary>
        public List<int> BuildOffers { get; } = new List<int>(4);

        /// <summary>Creates a run. Throws if the config is invalid.</summary>
        public GameSimulation(BalanceConfig config, RunSetup setup)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            Setup = setup ?? throw new ArgumentNullException(nameof(setup));
            var errors = config.Resolve();
            if (errors.Count > 0) throw new ArgumentException("Invalid balance config: " + string.Join("; ", errors));
            if (setup.World < 0 || setup.World >= config.worlds.Length) throw new ArgumentOutOfRangeException(nameof(setup), "world");
            if (setup.Leader < 0 || setup.Leader >= config.leaders.Length) throw new ArgumentOutOfRangeException(nameof(setup), "leader");

            World = config.worlds[setup.World];
            Rng = new Rng(setup.Seed);
            EnemyCap = setup.Mobile ? config.run.enemyCapMobile : config.run.enemyCapPc;
            int capacity = EnemyCap + 64;
            Enemies = new Enemy[capacity];
            _freeEnemies = new Stack<int>(capacity);
            for (int i = capacity - 1; i >= 0; i--) _freeEnemies.Push(i);
            _grid = new SpatialGrid(2f, capacity);
            Companions = new Companion[System.Math.Max(1, config.recruit.maxCrewCapacity)];
            PassiveLevels = new int[config.passives.Length];
            _structureByCell = new Dictionary<long, int>(64);

            var mods = setup.Modifiers ?? new RunModifiers();
            Player = new PlayerState
            {
                Pos = Vec2.Zero,
                Facing = new Vec2(1f, 0f),
                MaxHp = config.player.maxHp + mods.MaxHpBonus,
                Level = 1,
                Brass = mods.StartingBrass,
                RevivesLeft = mods.Revives,
            };
            Player.Hp = Player.MaxHp;
            Player.XpToNext = XpForLevel(1);

            var leader = config.leaders[setup.Leader];
            Gadgets.Add(new OwnedGadget { Def = config.GadgetIndex(leader.startingGadget), Level = 1 });
            _nextEliteAt = config.run.eliteFirstAt;
            Phase = RunPhase.Playing;
        }

        private readonly SpatialGrid _grid;

        /// <summary>Crew capacity including research bonuses.</summary>
        public int CrewCapacity => System.Math.Min(Config.recruit.maxCrewCapacity, Config.recruit.baseCrewCapacity + (Setup.Modifiers?.CrewBonus ?? 0));

        /// <summary>Number of active companions.</summary>
        public int CrewCount
        {
            get
            {
                int n = 0;
                for (int i = 0; i < Companions.Length; i++) if (Companions[i].Active) n++;
                return n;
            }
        }

        /// <summary>True once the run has ended (victory or defeat).</summary>
        public bool IsOver => Phase == RunPhase.Victory || Phase == RunPhase.Defeat;

        /// <summary>
        /// Advances the simulation by <paramref name="deltaTime"/> seconds using fixed 1/60 s sub-steps.
        /// <paramref name="move"/> is the movement input (length is clamped to 1).
        /// Events from all sub-steps are collected in <see cref="Events"/>.
        /// </summary>
        public void Step(float deltaTime, Vec2 move)
        {
            Events.Clear();
            if (Phase != RunPhase.Playing) return;
            _accumulator += System.Math.Min(deltaTime, 0.25f);
            while (_accumulator >= FixedStep && Phase == RunPhase.Playing)
            {
                _accumulator -= FixedStep;
                Tick(FixedStep, move.ClampMagnitude(1f));
            }
            if (Phase != RunPhase.Playing) _accumulator = 0f;
        }

        private void Tick(float dt, Vec2 move)
        {
            Time += dt;
            UpdatePlayer(dt, move);
            UpdateSpawning(dt);
            RebuildGrid();
            CollectActiveLures();
            UpdateEnemies(dt);
            UpdateGadgets(dt);
            UpdateProjectiles(dt);
            UpdateCompanions(dt);
            UpdateStructures(dt);
            UpdateRemnants(dt);
            UpdatePickups(dt);
            UpdateBuildPlots(dt);
            if (Phase == RunPhase.Playing && Player.PendingLevelUps > 0) OpenLevelUp();
        }

        private void UpdatePlayer(float dt, Vec2 move)
        {
            float speed = Config.player.speed * (1f + PassiveValue(PassiveKind.Speed) + (Setup.Modifiers?.Speed ?? 0f));
            if (move.SqrMagnitude > 1e-4f) Player.Facing = move.Normalized;
            Player.Pos += move * speed * dt;
            if (Player.Invuln > 0f) Player.Invuln -= dt;
            float maxHp = Config.player.maxHp + (Setup.Modifiers?.MaxHpBonus ?? 0f) + PassiveValue(PassiveKind.Plating);
            if (maxHp > Player.MaxHp) Player.Hp += maxHp - Player.MaxHp;
            Player.MaxHp = maxHp;
        }

        private void RebuildGrid()
        {
            _grid.Clear();
            for (int i = 0; i < Enemies.Length; i++)
                if (Enemies[i].Active) _grid.Insert(i, Enemies[i].Pos);
        }

        /// <summary>Sum of a passive's value at its current level.</summary>
        public float PassiveValue(PassiveKind kind)
        {
            float v = 0f;
            for (int i = 0; i < Config.passives.Length; i++)
                if (Config.passives[i].Kind == kind) v += Config.passives[i].valuePerLevel * PassiveLevels[i];
            return v;
        }

        /// <summary>XP required to advance from <paramref name="level"/> to the next level.</summary>
        public int XpForLevel(int level)
        {
            float l = level - 1;
            var p = Config.player;
            return (int)System.MathF.Round(p.xpBase + p.xpLinear * l + p.xpQuadratic * l * l);
        }

        /// <summary>Research points earned so far (final once <see cref="IsOver"/>).</summary>
        public int ResearchPoints
        {
            get
            {
                var r = Config.research;
                int pts = Stats.Kills / System.Math.Max(1, r.killsPerPoint) + r.pointsPerMinute * (int)(Time / 60f);
                if (Stats.BossDefeated) pts += r.victoryBonus;
                return pts;
            }
        }

        private void Emit(SimEventType type, Vec2 a, Vec2 b = default, float value = 0f, int index = 0)
        {
            if (Events.Count >= MaxEvents) return;
            Events.Add(new SimEvent { Type = type, A = a, B = b, Value = value, Index = index });
        }
    }
}
