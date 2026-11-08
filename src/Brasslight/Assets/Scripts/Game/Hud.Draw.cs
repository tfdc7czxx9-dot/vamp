using UnityEngine;
using Vamp.Core.Simulation;

namespace Vamp.Game
{
    public sealed partial class Hud
    {
        private static readonly Color Brass = new Color(0.93f, 0.72f, 0.33f);
        private static readonly Color Ink = new Color(0.07f, 0.06f, 0.08f, 0.86f);
        private static readonly Color Parchment = new Color(0.96f, 0.91f, 0.8f);
        private GUIStyle _title, _h1, _body, _small, _button, _buttonSel, _buttonOff, _center, _number, _h1Center, _timer, _cardTitle, _pauseGlyph;
        private Texture2D _white;
        private Rect[] _exclusion = System.Array.Empty<Rect>();

        private void EnsureStyles()
        {
            if (_title != null) return;
            _white = Texture2D.whiteTexture;
            _title = new GUIStyle(GUI.skin.label) { fontSize = 76, fontStyle = FontStyle.Bold, normal = { textColor = Brass } };
            _h1 = new GUIStyle(GUI.skin.label) { fontSize = 34, fontStyle = FontStyle.Bold, normal = { textColor = Parchment } };
            _body = new GUIStyle(GUI.skin.label) { fontSize = 20, wordWrap = true, normal = { textColor = Parchment } };
            _small = new GUIStyle(_body) { fontSize = 16 };
            _center = new GUIStyle(_body) { alignment = TextAnchor.MiddleCenter };
            _number = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } };
            _button = new GUIStyle(GUI.skin.label) { fontSize = 24, alignment = TextAnchor.MiddleLeft, padding = new RectOffset(18, 18, 6, 6), normal = { textColor = Parchment } };
            _buttonSel = new GUIStyle(_button) { normal = { textColor = new Color(0.1f, 0.07f, 0.05f) } };
            _buttonOff = new GUIStyle(_button) { normal = { textColor = new Color(0.6f, 0.58f, 0.55f) } };
            _h1Center = new GUIStyle(_h1) { alignment = TextAnchor.MiddleCenter };
            _timer = new GUIStyle(_h1) { alignment = TextAnchor.MiddleCenter, fontSize = 30 };
            _cardTitle = new GUIStyle(_h1) { fontSize = 26, wordWrap = true, normal = { textColor = Color.white } };
            _pauseGlyph = new GUIStyle(_h1) { alignment = TextAnchor.MiddleCenter };
        }

        /// <summary>Draws the current screen. Call from OnGUI.</summary>
        public void Draw()
        {
            EnsureStyles();
            _scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            _w = Screen.width / _scale;
            _h = Screen.height / _scale;
            var sa = Screen.safeArea;
            _safe = new Rect(sa.x / _scale, (Screen.height - sa.yMax) / _scale, sa.width / _scale, sa.height / _scale);
            GUI.matrix = Matrix4x4.Scale(new Vector3(_scale, _scale, 1f));

            var g = _g;
            switch (g.Screen)
            {
                case GameScreen.Loading: Text(new Rect(0, 0, _w, _h), "BRASSLIGHT", _center); break;
                case GameScreen.Title: DrawTitle(); break;
                case GameScreen.Expedition: DrawPanelMenu(L("expedition.title"), null); break;
                case GameScreen.Research: DrawPanelMenu(L("research.title"), g.Loc.F("menu.points", g.Save.researchPoints)); break;
                case GameScreen.Options: DrawPanelMenu(L("options.title"), null); break;
                case GameScreen.Credits: DrawPanelMenu(L("credits.title"), L("credits.body")); break;
                case GameScreen.Run: DrawRunHud(); break;
                case GameScreen.Pause: DrawRunHud(); Dim(); DrawPanelMenu(L("pause.title"), null); break;
                case GameScreen.LevelUp: DrawRunHud(); Dim(); DrawCards(g.Loc.F("levelup.title", g.Sim.Player.Level), null); break;
                case GameScreen.Build: DrawRunHud(); Dim(); DrawCards(L("build.title"), g.Loc.F("build.brass", g.Sim.Player.Brass)); break;
                case GameScreen.Results: Dim(); DrawResults(); break;
            }
            DrawToasts();
        }

        /// <summary>Shadowed label; colour tints via contentColor (no per-call style allocation).</summary>
        private void Text(Rect r, string s, GUIStyle style, Color? color = null)
        {
            var old = GUI.contentColor;
            Color c = color ?? Color.white;
            GUI.contentColor = new Color(0f, 0f, 0f, 0.75f * c.a);
            GUI.Label(new Rect(r.x + 2, r.y + 2, r.width, r.height), s, style);
            GUI.contentColor = c;
            GUI.Label(r, s, style);
            GUI.contentColor = old;
        }

        private void Fill(Rect r, Color c)
        {
            var old = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, _white);
            GUI.color = old;
        }

        private void Frame(Rect r, Color fill, Color border)
        {
            Fill(r, border);
            Fill(new Rect(r.x + 2, r.y + 2, r.width - 4, r.height - 4), fill);
        }

        private void Dim() => Fill(new Rect(0, 0, _w, _h), new Color(0, 0, 0, 0.55f));

        private bool MenuButton(Rect r, int index)
        {
            var it = _items[index];
            bool sel = index == _sel;
            if (sel) Frame(r, Brass, new Color(1f, 0.92f, 0.6f));
            else Frame(r, new Color(0.12f, 0.1f, 0.1f, 0.85f), new Color(0.45f, 0.34f, 0.18f));
            var style = !it.Enabled ? _buttonOff : sel ? _buttonSel : _button;
            GUI.Label(r, it.Label, style);
            if (GUI.Button(r, GUIContent.none, GUIStyle.none))
            {
                _sel = index;
                Activate(index);
                return true;
            }
            return false;
        }

        private void DrawTitle()
        {
            Fill(new Rect(0, 0, _safe.x + 520, _h), new Color(0.03f, 0.03f, 0.05f, 0.65f));
            float x = _safe.x + 60, y = _safe.y + 60;
            Text(new Rect(x, y, 900, 100), "BRASSLIGHT", _title);
            Text(new Rect(x + 4, y + 92, 800, 40), L("subtitle"), _body, Brass);
            y += 190;
            for (int i = 0; i < _items.Count; i++)
            {
                MenuButton(new Rect(x, y, 400, 54), i);
                if (_items[i].Detail != null) Text(new Rect(x + 410, y + 12, 300, 30), _items[i].Detail, _small);
                y += 64;
            }
            Text(new Rect(x, _safe.yMax - 40, 900, 30), L("title.footer"), _small, new Color(1, 1, 1, 0.6f));
        }

        private void DrawPanelMenu(string title, string subtitle)
        {
            float pw = Mathf.Min(820, _safe.width - 40);
            float itemH = 54, gap = 10;
            float detailH = 0;
            foreach (var it in _items) if (it.Detail != null) detailH += 28;
            float bodyH = _g.Screen == GameScreen.Credits ? 300 : 0;
            float ph = 120 + (subtitle != null ? 40 : 0) + bodyH + _items.Count * (itemH + gap) + detailH;
            ph = Mathf.Min(ph, _safe.height - 20);
            var panel = new Rect(_safe.center.x - pw / 2, _safe.center.y - ph / 2, pw, ph);
            Frame(panel, Ink, Brass);
            float y = panel.y + 24;
            Text(new Rect(panel.x + 30, y, pw - 60, 44), title, _h1);
            y += 56;
            if (subtitle != null && _g.Screen != GameScreen.Credits)
            {
                Text(new Rect(panel.x + 30, y, pw - 60, 30), subtitle, _body, Brass);
                y += 40;
            }
            if (_g.Screen == GameScreen.Credits)
            {
                Text(new Rect(panel.x + 30, y, pw - 60, bodyH), subtitle, _small);
                y += bodyH;
            }
            for (int i = 0; i < _items.Count; i++)
            {
                MenuButton(new Rect(panel.x + 30, y, pw - 60, itemH), i);
                y += itemH + 4;
                if (_items[i].Detail != null)
                {
                    Text(new Rect(panel.x + 44, y, pw - 80, 26), _items[i].Detail, _small, new Color(1, 1, 1, 0.75f));
                    y += 28;
                }
                y += gap - 4;
            }
        }

        private void DrawCards(string title, string subtitle)
        {
            Text(new Rect(0, _safe.y + 70, _w, 50), title, _h1Center);
            if (subtitle != null) Text(new Rect(0, _safe.y + 120, _w, 30), subtitle, _center, Brass);
            int cards = 0;
            foreach (var it in _items) if (it.Detail != null) cards++;
            float cw = Mathf.Min(320, (_safe.width - 80) / Mathf.Max(1, cards) - 20), ch = 300;
            float total = cards * cw + (cards - 1) * 24;
            float x = _safe.center.x - total / 2, y = _safe.center.y - ch / 2 + 20;
            for (int i = 0; i < _items.Count; i++)
            {
                var it = _items[i];
                if (it.Detail == null)
                {
                    MenuButton(new Rect(_safe.center.x - 160, y + ch + 30, 320, 54), i);
                    continue;
                }
                var r = new Rect(x, y, cw, ch);
                bool sel = i == _sel;
                Frame(r, it.Enabled ? new Color(0.13f, 0.1f, 0.09f, 0.95f) : new Color(0.1f, 0.1f, 0.1f, 0.9f), sel ? new Color(1f, 0.9f, 0.55f) : new Color(0.45f, 0.34f, 0.18f));
                if (sel) Fill(new Rect(r.x + 2, r.y + 2, r.width - 4, 8), Brass);
                Text(new Rect(r.x + 18, r.y + 22, cw - 36, 60), it.Label, _cardTitle, it.Enabled ? Brass : Color.gray);
                Text(new Rect(r.x + 18, r.y + 90, cw - 36, ch - 100), it.Detail, _body, it.Enabled ? Parchment : Color.gray);
                if (GUI.Button(r, GUIContent.none, GUIStyle.none))
                {
                    _sel = i;
                    Activate(i);
                }
                x += cw + 24;
            }
        }

        private void Bar(Rect r, float t, Color c)
        {
            Fill(r, new Color(0, 0, 0, 0.6f));
            Fill(new Rect(r.x + 2, r.y + 2, (r.width - 4) * Mathf.Clamp01(t), r.height - 4), c);
        }

        private void DrawRunHud()
        {
            var sim = _g.Sim;
            if (sim == null) return;
            var p = sim.Player;
            var cfg = _g.Config;

            // XP bar across the top.
            Bar(new Rect(_safe.x + 10, _safe.y + 8, _safe.width - 20, 18), p.Xp / (float)Mathf.Max(1, p.XpToNext), new Color(0.35f, 0.85f, 1f));
            Text(new Rect(_safe.x + 16, _safe.y + 4, 200, 26), _g.Loc.F("hud.level", p.Level), _small);

            // Timer centre.
            int secs = (int)sim.Time;
            Text(new Rect(0, _safe.y + 30, _w, 40), $"{secs / 60:00}:{secs % 60:00}", _timer);

            // HP bottom-left of the top area.
            Bar(new Rect(_safe.x + 10, _safe.y + 34, 260, 20), p.Hp / Mathf.Max(1f, p.MaxHp), new Color(0.9f, 0.25f, 0.2f));
            Text(new Rect(_safe.x + 16, _safe.y + 32, 260, 24), $"{Mathf.CeilToInt(p.Hp)} / {Mathf.CeilToInt(p.MaxHp)}", _small);

            // Resources right.
            float rx = _safe.xMax - 300;
            Text(new Rect(rx, _safe.y + 32, 220, 26), _g.Loc.F("hud.brass", p.Brass), _body, Brass);
            Text(new Rect(rx, _safe.y + 58, 220, 26), _g.Loc.F("hud.crew", sim.CrewCount, sim.CrewCapacity), _body);
            Text(new Rect(rx, _safe.y + 84, 220, 26), _g.Loc.F("hud.kills", sim.Stats.Kills), _small);

            // Pause button (touch friendly), excluded from the joystick.
            var pause = new Rect(_safe.xMax - 70, _safe.y + 34, 56, 56);
            Frame(pause, new Color(0.12f, 0.1f, 0.1f, 0.8f), Brass);
            GUI.Label(pause, "II", _pauseGlyph);
            if (_g.Screen == GameScreen.Run && GUI.Button(pause, GUIContent.none, GUIStyle.none)) _g.OpenPause();
            if (_exclusion.Length == 0) _exclusion = new Rect[1];
            _exclusion[0] = new Rect(pause.x * _scale, pause.y * _scale, pause.width * _scale, pause.height * _scale);
            _g.Input.JoystickExclusions = _exclusion;

            // Gadgets bottom-left.
            float gy = _safe.yMax - 28;
            for (int i = sim.Gadgets.Count - 1; i >= 0; i--)
            {
                var gd = cfg.gadgets[sim.Gadgets[i].Def];
                Text(new Rect(_safe.x + 14, gy, 320, 26), $"{Name("gadget", gd.id, gd.displayName)}  {sim.Gadgets[i].Level}", _small);
                gy -= 24;
            }

            // Recruit / build prompts.
            float py = _safe.yMax - 120;
            if (sim.ActiveRemnant >= 0)
            {
                var r = sim.Remnants[sim.ActiveRemnant];
                string what = sim.ActiveRemnantWillScrap ? _g.Loc.F("hud.scrapping", _g.Config.recruit.scrapBrass) : _g.Loc.F("hud.recruiting", Name("enemy", cfg.enemies[r.Def].id, cfg.enemies[r.Def].displayName));
                Prompt(py, what, r.Progress, new Color(0.4f, 0.95f, 1f));
                py -= 60;
            }
            if (sim.OnPlot)
            {
                int si = sim.StructureAt(Vamp.Core.Simulation.BuildLattice.Key(
                    Mathf.FloorToInt(sim.CurrentPlotPos.X / cfg.build.latticeCellSize), Mathf.FloorToInt(sim.CurrentPlotPos.Y / cfg.build.latticeCellSize)));
                string what;
                if (si < 0) what = L("hud.surveying");
                else
                {
                    var s = sim.Structures[si];
                    var d = cfg.structures[s.Def];
                    what = s.Level >= cfg.build.maxStructureLevel ? _g.Loc.F("hud.maxed", Name("structure", d.id, d.displayName))
                        : _g.Loc.F("hud.upgrading", Name("structure", d.id, d.displayName), s.Level + 1, sim.UpgradeCost(s));
                }
                Prompt(py, what, sim.PlotProgress, Brass);
            }

            // Damage numbers.
            if (_g.Fx.ShowNumbers)
            {
                var cam = _g.Rig.Cam;
                foreach (var n in _g.Fx.Numbers)
                {
                    var sp = cam.WorldToScreenPoint(n.Pos);
                    if (sp.z <= 0f) continue;
                    var rect = new Rect(sp.x / _scale - 40, (Screen.height - sp.y) / _scale - 12, 80, 24);
                    var col = n.Big ? new Color(1f, 0.85f, 0.3f, Mathf.Clamp01(n.Life * 2f)) : new Color(1f, 1f, 1f, Mathf.Clamp01(n.Life * 2f));
                    Text(rect, Mathf.RoundToInt(n.Value).ToString(), _number, col);
                }
            }

            // Touch joystick.
            var input = _g.Input;
            if (input.JoystickActive)
            {
                var o = new Vector2(input.JoystickOrigin.x / _scale, (Screen.height - input.JoystickOrigin.y) / _scale);
                var k = new Vector2(input.JoystickKnob.x / _scale, (Screen.height - input.JoystickKnob.y) / _scale);
                float rad = input.JoystickRadius / _scale;
                Fill(new Rect(o.x - rad, o.y - rad, rad * 2, rad * 2), new Color(1f, 1f, 1f, 0.08f));
                Fill(new Rect(k.x - 26, k.y - 26, 52, 52), new Color(0.93f, 0.72f, 0.33f, 0.55f));
            }
        }

        private void Prompt(float y, string text, float progress, Color c)
        {
            float w = 460;
            var r = new Rect(_safe.center.x - w / 2, y, w, 50);
            Frame(r, Ink, c);
            Fill(new Rect(r.x + 2, r.yMax - 8, (w - 4) * Mathf.Clamp01(progress), 6), c);
            Text(new Rect(r.x, r.y + 2, w, 40), text, _center);
        }

        private void DrawResults()
        {
            var sim = _g.Sim;
            var st = sim.Stats;
            float pw = Mathf.Min(760, _safe.width - 40), ph = Mathf.Min(560, _safe.height - 20);
            var panel = new Rect(_safe.center.x - pw / 2, _safe.center.y - ph / 2, pw, ph);
            Frame(panel, Ink, Brass);
            bool won = sim.Phase == RunPhase.Victory;
            Text(new Rect(panel.x + 30, panel.y + 24, pw - 60, 50), won ? L("results.victory") : L("results.defeat"), _h1, won ? Brass : Parchment);
            int secs = (int)sim.Time;
            string body = _g.Loc.F("results.body", secs / 60, secs % 60, sim.Player.Level, st.Kills, st.Recruited, st.StructuresBuilt, st.BrassCollected);
            Text(new Rect(panel.x + 30, panel.y + 90, pw - 60, 220), body, _body);
            Text(new Rect(panel.x + 30, panel.y + 320, pw - 60, 40), _g.Loc.F("results.points", _g.LastRunPoints, _g.Save.researchPoints), _h1, Brass);
            if (_g.NewAchievements.Count > 0)
            {
                string list = "";
                foreach (var a in _g.NewAchievements) list += "★ " + _g.Loc.Get("ach." + a, a) + "\n";
                Text(new Rect(panel.x + 30, panel.y + 370, pw - 60, 100), list, _small, Brass);
            }
            MenuButton(new Rect(panel.x + 30, panel.yMax - 74, pw - 60, 54), 0);
        }

        private void DrawToasts()
        {
            float now = Time.unscaledTime;
            for (int i = _toasts.Count - 1; i >= 0; i--) if (_toasts[i].Until < now) _toasts.RemoveAt(i);
            float y = _safe.y + 130;
            foreach (var t in _toasts)
            {
                var r = new Rect(_safe.center.x - 300, y, 600, 40);
                Fill(r, new Color(0, 0, 0, 0.55f));
                Text(r, t.Text, _center, Brass);
                y += 46;
            }
        }
    }
}
