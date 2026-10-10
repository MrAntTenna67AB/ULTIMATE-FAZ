using System.Drawing.Drawing2D;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace FazbearPizzeria;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        try
        {
            var root = AppContext.BaseDirectory;
            var sheets = SheetLoader.Load(root);
            var primary = ArgValue(args, "--fnaf1") ?? SteamInstallFinder.FindSteamPath();
            var sources = SteamInstallFinder.FindAll(sheets.Games, primary);
            Application.Run(new GameForm(sheets, sources, args.Contains("--capture-demo", StringComparer.OrdinalIgnoreCase)));
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ultimate Fazbear", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static string? ArgValue(string[] args, string name)
    {
        for (var i = 0; i < args.Length - 1; i++)
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase)) return args[i + 1];
        return null;
    }
}

internal sealed record GameSource(string Id, string? Slug, string Name, [property: JsonPropertyName("steamAppId")] string AppId, string Exe, string Role, string Use, int AnimatronicUnlockCost);
internal sealed record Animatronic(string Id, string Name, string GameId, int Cost, string Rule, string Action, int FirstAttackSeconds, int WarningSeconds, int Pressure, string? DoorSide, int Atmosphere, int HealthSafety, int Entertainment, int BonusRevenue);
internal sealed record Attraction(string Id, string Name, string Category, int Cost, int Atmosphere, int HealthSafety, int Entertainment, int BonusRevenue, int LiabilityRisk, string Description, string Style);
internal sealed record SystemRow(string Id, string[] GameRefs, string[] Fields, string Behavior);
internal sealed record DesignSheets(List<GameSource> Games, List<Animatronic> Animatronics, List<Attraction> Attractions, List<SystemRow> Systems, int StartingFunds, int DefaultAiLevel, int MaximumAiLevel);

internal static class SheetLoader
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };
    public static DesignSheets Load(string root)
    {
        var games = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "sheets", "games.json")))
            .RootElement.GetProperty("games").Deserialize<List<GameSource>>(Options) ?? throw new InvalidDataException("No game rows in sheets/games.json");
        using var animDoc = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "sheets", "animatronics.json")));
        var funds = animDoc.RootElement.GetProperty("startingFunds").GetInt32();
        var defaultAi = animDoc.RootElement.GetProperty("defaultAiLevel").GetInt32();
        var maximumAi = animDoc.RootElement.GetProperty("maximumAiLevel").GetInt32();
        var anims = animDoc.RootElement.GetProperty("animatronics").Deserialize<List<Animatronic>>(Options) ?? throw new InvalidDataException("No animatronic rows in sheets/animatronics.json");
        var attractions = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "sheets", "attractions.json")))
            .RootElement.GetProperty("attractions").Deserialize<List<Attraction>>(Options) ?? throw new InvalidDataException("No attractions in sheets/attractions.json");
        var systems = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "sheets", "systems.json")))
            .RootElement.GetProperty("systems").Deserialize<List<SystemRow>>(Options) ?? throw new InvalidDataException("No system rows in sheets/systems.json");
        return new DesignSheets(games, anims, attractions, systems, funds, defaultAi, maximumAi);
    }
}

internal static class SteamInstallFinder
{
    public static string? FindSteamPath()
    {
        try { return Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null) as string; }
        catch { return null; }
    }

    public static Dictionary<string, SourceInstall> FindAll(IEnumerable<GameSource> games, string? primary)
    {
        var roots = SteamRoots();
        var result = new Dictionary<string, SourceInstall>(StringComparer.OrdinalIgnoreCase);
        foreach (var game in games)
        {
            string? dir = null;
            if (game.Id == "fnaf1" && !string.IsNullOrWhiteSpace(primary))
            {
                var candidate = Directory.Exists(primary) ? primary : Path.GetDirectoryName(primary);
                if (candidate is not null && File.Exists(Path.Combine(candidate, game.Exe))) dir = candidate;
            }
            if (string.IsNullOrWhiteSpace(dir)) dir = FindAppInstall(roots, game.AppId);
            if (string.IsNullOrWhiteSpace(dir))
            {
                var fallback = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam", "steamapps", "common", game.Name);
                if (File.Exists(Path.Combine(fallback, game.Exe))) dir = fallback;
            }
            var exePath = dir is null ? null : Path.Combine(dir, game.Exe);
            Icon? icon = null;
            if (exePath is not null && File.Exists(exePath))
            {
                try { icon = Icon.ExtractAssociatedIcon(exePath); } catch { }
            }
            result[game.Id] = new SourceInstall(game, dir, exePath, icon);
        }
        return result;
    }

    private static List<string> SteamRoots()
    {
        var roots = new List<string>();
        var registered = FindSteamPath();
        if (!string.IsNullOrWhiteSpace(registered)) roots.Add(registered.Replace('/', '\\'));
        roots.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam"));
        roots.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Steam"));
        var libraries = new List<string>(roots);
        foreach (var root in roots.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var vdf = Path.Combine(root, "steamapps", "libraryfolders.vdf");
            if (!File.Exists(vdf)) continue;
            foreach (Match m in Regex.Matches(File.ReadAllText(vdf), "\\\"path\\\"\\s+\\\"([^\\\"]+)\\\"", RegexOptions.IgnoreCase))
                libraries.Add(m.Groups[1].Value.Replace("\\\\", "\\"));
        }
        return libraries.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static string? FindAppInstall(IEnumerable<string> roots, string appId)
    {
        foreach (var root in roots)
        {
            var manifest = Path.Combine(root, "steamapps", $"appmanifest_{appId}.acf");
            if (!File.Exists(manifest)) continue;
            var text = File.ReadAllText(manifest);
            var match = Regex.Match(text, "\\\"installdir\\\"\\s+\\\"([^\\\"]+)\\\"", RegexOptions.IgnoreCase);
            if (!match.Success) continue;
            var dir = Path.Combine(root, "steamapps", "common", match.Groups[1].Value);
            if (Directory.Exists(dir)) return dir;
        }
        return null;
    }
}

internal sealed class SourceInstall(GameSource game, string? directory, string? exePath, Icon? icon) : IDisposable
{
    public GameSource Game { get; } = game;
    public string? Directory { get; } = directory;
    public string? ExePath { get; } = exePath;
    public Icon? Icon { get; } = icon;
    public bool IsAvailable => Game.Id == "fnaf-mades" || ExePath is not null && File.Exists(ExePath);
    public void Dispose() => Icon?.Dispose();
}

internal sealed class MainForm : Form
{
    private readonly DesignSheets _sheets;
    private readonly Dictionary<string, SourceInstall> _sources;
    private readonly bool _demo;
    private readonly HashSet<string> _owned = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _ai = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _nextAttack = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _pendingUntil = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, bool> _resolved = new(StringComparer.OrdinalIgnoreCase);
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 1000 };
    private readonly Label _headline = new();
    private readonly Label _subtitle = new();
    private readonly Label _status = new();
    private readonly Panel _body = new();
    private readonly Panel _footer = new();
    private readonly Random _rng = new(41);
    private int _money;
    private int _day = 1;
    private int _nightTick;
    private int _strikes;
    private int _nightScore;
    private int _mode;
    private int _ventilation = 100;
    private int _ventCooldown;
    private int _nightSecond;
    private bool _doorClosed;
    private bool _maskOn;
    private bool _listening;
    private bool _lureReady = true;
    private bool _ventScanned;
    private bool _nightEnded;
    private string _phaseMessage = "Your purchases become tonight's threats.";
    private Label? _clockLabel;
    private Label? _warningLabel;

    private static readonly Color Ink = Color.FromArgb(236, 238, 240);
    private static readonly Color Muted = Color.FromArgb(161, 171, 180);
    private static readonly Color Teal = Color.FromArgb(119, 220, 206);
    private static readonly Color Gold = Color.FromArgb(242, 194, 99);
    private static readonly Color Panel = Color.FromArgb(19, 25, 31);
    private static readonly Color Line = Color.FromArgb(53, 67, 74);

    public MainForm(DesignSheets sheets, Dictionary<string, SourceInstall> sources, bool demo)
    {
        _sheets = sheets;
        _sources = sources;
        _demo = demo;
        _money = sheets.StartingFunds;
        foreach (var row in sheets.Animatronics) _ai[row.Id] = sheets.DefaultAiLevel;
        Text = "Ultimate Fazbear";
        ClientSize = new Size(1180, 820);
        MinimumSize = new Size(960, 720);
        BackColor = Color.FromArgb(8, 12, 16);
        ForeColor = Ink;
        Font = new Font("Segoe UI", 10F, FontStyle.Regular);
        KeyPreview = true;
        BuildShell();
        _timer.Tick += (_, _) => AdvanceNight();
        _timer.Start();
        ShowDay();
        if (_demo)
        {
            Shown += async (_, _) =>
            {
                foreach (var a in _sheets.Animatronics) _owned.Add(a.Id);
                _money -= _sheets.Animatronics.Sum(a => a.Cost);
                ShowDay();
                await Task.Delay(300);
                StartNight();
            };
        }
    }

    private void BuildShell()
    {
        var header = new Panel { Dock = DockStyle.Top, Height = 105, BackColor = Color.FromArgb(13, 19, 23), Padding = new Padding(28, 16, 24, 10) };
        _headline.SetBounds(0, 4, 760, 38); _headline.Font = new Font("Georgia", 22, FontStyle.Bold); _headline.ForeColor = Ink; _headline.BackColor = Color.Transparent;
        _subtitle.SetBounds(1, 48, 900, 25); _subtitle.Font = new Font("Segoe UI", 9, FontStyle.Bold); _subtitle.ForeColor = Teal; _subtitle.BackColor = Color.Transparent;
        header.Controls.Add(_headline); header.Controls.Add(_subtitle);
        var ucn = _sources["ucn"];
        if (ucn.Icon is not null)
        {
            var badge = new PictureBox { Left = 1024, Top = 9, Width = 52, Height = 52, BackColor = Color.FromArgb(7, 11, 14), SizeMode = PictureBoxSizeMode.Zoom, Image = ucn.Icon.ToBitmap() };
            var label = new Label { Left = 1082, Top = 13, Width = 150, Height = 42, Text = "UCN\nCUSTOM AI", ForeColor = Gold, BackColor = Color.Transparent, Font = new Font("Consolas", 9, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
            header.Controls.Add(badge); header.Controls.Add(label);
        }
        Controls.Add(header);
        _body.Dock = DockStyle.Fill; _body.BackColor = Color.FromArgb(8, 12, 16); _body.Padding = new Padding(24, 20, 24, 16); Controls.Add(_body);
        _footer.Dock = DockStyle.Bottom; _footer.Height = 52; _footer.BackColor = Color.FromArgb(13, 19, 23); _footer.Padding = new Padding(24, 10, 24, 8);
        _status.Dock = DockStyle.Fill; _status.ForeColor = Muted; _status.Font = new Font("Consolas", 9F); _status.TextAlign = ContentAlignment.MiddleLeft;
        _footer.Controls.Add(_status); Controls.Add(_footer);
        header.BringToFront(); _footer.BringToFront();
    }

    private int LinkedSourceCount() => _sources.Values.Count(s => s.Game.Id != "fnaf-mades" && s.IsAvailable);

    private void ShowDay()
    {
        _mode = 0; _timer.Stop();
        ClearBody();
        _headline.Text = $"DAY {_day}  /  THE PIZZERIA";
        _subtitle.Text = "BUILD THE STAGE   •   CHOOSE TONIGHT'S ROSTER   •   WATCH THE COSTS";
        _status.Text = $"FUNDS  ${_money:N0}     OWNED  {_owned.Count}/{_sheets.Animatronics.Count}     SOURCES LINKED  {LinkedSourceCount()}/6";
        var intro = new Label { Dock = DockStyle.Top, Height = 60, Text = "Lay out the roster for opening night. Every animatronic you buy can show up in the office. Set its UCN-style AI level before the shift.", ForeColor = Muted, Font = new Font("Segoe UI", 11), Padding = new Padding(3, 0, 3, 12) };
        _body.Controls.Add(intro);
        var outer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Color.Transparent };
        outer.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); outer.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        var tiles = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = true, AutoScroll = true, Padding = new Padding(0, 8, 0, 8), BackColor = Color.Transparent };
        foreach (var anim in _sheets.Animatronics) tiles.Controls.Add(BuildTile(anim));
        outer.Controls.Add(tiles, 0, 0);
        var bottom = new Panel { Dock = DockStyle.Fill };
        var note = new Label { Dock = DockStyle.Left, Width = 590, Text = "UCN SETTINGS  •  Difficulty runs from 0 (asleep) to 20 (relentless).\nA balanced first night usually means setting only the animatronics you want active.", ForeColor = Muted, Font = new Font("Segoe UI", 9), TextAlign = ContentAlignment.MiddleLeft };
        var go = MakeButton("START THE NIGHT  →", Gold, Color.FromArgb(35, 29, 20), 240, 50);
        go.Dock = DockStyle.Right; go.Click += (_, _) => StartNight();
        bottom.Controls.Add(go); bottom.Controls.Add(note); outer.Controls.Add(bottom, 0, 1);
        _body.Controls.Add(outer);
        _timer.Start();
    }

    private Control BuildTile(Animatronic anim)
    {
        var src = _sources[anim.GameId];
        var card = new Panel { Width = 210, Height = 262, BackColor = Panel, Margin = new Padding(0, 0, 14, 14), Padding = new Padding(14), BorderStyle = BorderStyle.FixedSingle };
        var portrait = new PictureBox { Left = 14, Top = 14, Width = 72, Height = 72, BackColor = Color.FromArgb(9, 13, 16), SizeMode = PictureBoxSizeMode.Zoom };
        if (src.Icon is not null) { try { portrait.Image = src.Icon.ToBitmap(); } catch { } }
        card.Controls.Add(portrait);
        var game = new Label { Left = 98, Top = 19, Width = 96, Height = 34, Text = ShortName(src.Game.Name).ToUpperInvariant(), ForeColor = Teal, Font = new Font("Segoe UI", 8, FontStyle.Bold) };
        var availability = new Label { Left = 98, Top = 52, Width = 96, Height = 26, Text = src.IsAvailable ? "SOURCE READY" : "SOURCE MISSING", ForeColor = src.IsAvailable ? Color.LightGreen : Color.Salmon, Font = new Font("Consolas", 7, FontStyle.Bold) };
        card.Controls.Add(game); card.Controls.Add(availability);
        var name = new Label { Left = 14, Top = 96, Width = 180, Height = 27, Text = anim.Name, ForeColor = Ink, Font = new Font("Georgia", 12, FontStyle.Bold) };
        var rule = new Label { Left = 14, Top = 126, Width = 180, Height = 34, Text = anim.Rule, ForeColor = Muted, Font = new Font("Segoe UI", 9) };
        var cost = new Label { Left = 14, Top = 165, Width = 180, Height = 25, Text = $"${anim.Cost:N0}   •   AI LEVEL", ForeColor = Gold, Font = new Font("Consolas", 9, FontStyle.Bold) };
        card.Controls.Add(name); card.Controls.Add(rule); card.Controls.Add(cost);
        if (_owned.Contains(anim.Id))
        {
            var ai = new NumericUpDown { Left = 14, Top = 192, Width = 84, Height = 28, Minimum = 0, Maximum = _sheets.MaximumAiLevel, Value = _ai[anim.Id], BackColor = Color.FromArgb(11, 16, 20), ForeColor = Ink, BorderStyle = BorderStyle.FixedSingle };
            ai.ValueChanged += (_, _) => _ai[anim.Id] = (int)ai.Value;
            card.Controls.Add(ai);
            var bought = new Label { Left = 108, Top = 197, Width = 88, Height = 24, Text = "IN ROSTER", ForeColor = Teal, Font = new Font("Consolas", 8, FontStyle.Bold), TextAlign = ContentAlignment.MiddleRight };
            card.Controls.Add(bought);
        }
        else
        {
            var buy = MakeButton("BUY ANIMATRONIC", Teal, Color.FromArgb(16, 32, 34), 180, 38);
            buy.Left = 14; buy.Top = 198; buy.Enabled = src.IsAvailable && _money >= anim.Cost;
            buy.Click += (_, _) => { if (_money >= anim.Cost && src.IsAvailable) { _money -= anim.Cost; _owned.Add(anim.Id); ShowDay(); } };
            card.Controls.Add(buy);
        }
        return card;
    }

    private void StartNight()
    {
        if (_owned.Count == 0)
        {
            MessageBox.Show("Buy at least one animatronic before the office shift.", "Roster is empty", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        _mode = 1; _nightTick = 0; _nightSecond = 0; _strikes = 0; _nightScore = 0; _nightEnded = false; _ventilation = 100; _ventCooldown = 0; _doorClosed = false; _maskOn = false; _listening = false; _lureReady = true; _ventScanned = false;
        _pendingUntil.Clear(); _resolved.Clear(); _nextAttack.Clear();
        foreach (var a in _sheets.Animatronics.Where(a => _owned.Contains(a.Id) && _ai[a.Id] > 0))
            _nextAttack[a.Id] = Math.Max(5, a.FirstAttackSeconds - _ai[a.Id] / 2);
        ShowNight();
        _timer.Start();
    }

    private void ShowNight()
    {
        _mode = 1; ClearBody();
        _headline.Text = "OFFICE SHIFT  /  12:00 AM";
        _subtitle.Text = "LISTEN CLOSELY   •   USE THE RIGHT COUNTER   •   REACH 6:00 AM";
        var wrap = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Color.Transparent };
        wrap.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 67)); wrap.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
        var scene = BuildOfficeScene(); wrap.Controls.Add(scene, 0, 0);
        var right = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, BackColor = Color.Transparent, Padding = new Padding(14, 0, 0, 0) };
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 88)); right.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); right.RowStyles.Add(new RowStyle(SizeType.Absolute, 180)); right.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
        _clockLabel = new Label { Dock = DockStyle.Fill, Text = "12:00 AM\nNIGHT 1", ForeColor = Gold, Font = new Font("Consolas", 20, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter, BackColor = Panel };
        right.Controls.Add(_clockLabel, 0, 0);
        _warningLabel = new Label { Dock = DockStyle.Fill, Text = _phaseMessage, ForeColor = Ink, Font = new Font("Segoe UI", 12, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.FromArgb(13, 19, 23), Padding = new Padding(18) };
        right.Controls.Add(_warningLabel, 0, 1);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, BackColor = Panel, Padding = new Padding(12) };
        AddAction(actions, "TOGGLE DOOR  [D]", "door", () => { _doorClosed = !_doorClosed; _phaseMessage = _doorClosed ? "Right door closed. Watch the power." : "Right door open."; });
        AddAction(actions, "PUT ON MASK  [M]", "mask", () => { _maskOn = !_maskOn; _phaseMessage = _maskOn ? "Mask on. Keep still." : "Mask off."; });
        AddAction(actions, "AUDIO LURE  [L]", "lure", () => { if (_lureReady && _ventilation > 0) { _lureReady = false; _phaseMessage = "Audio lure sent down the hall."; } else _phaseMessage = "Lure unavailable. Reset ventilation first."; });
        AddAction(actions, "LISTEN  [Q]", "listen", () => { _listening = true; _phaseMessage = "You hear movement near the doorway."; });
        AddAction(actions, "VENT SCAN  [V]", "scan", () => { _phaseMessage = "Vent scan complete. Keep the ducts clear."; _ventScanned = true; });
        AddAction(actions, "RESET VENT  [R]", "vent", () => { if (_ventCooldown <= 0) { _ventilation = 100; _ventCooldown = 10; _lureReady = true; _phaseMessage = "Ventilation restored. Audio lure is ready."; } else _phaseMessage = $"Vent reset cooling down: {_ventCooldown}s."; });
        right.Controls.Add(actions, 0, 2);
        var utility = MakeButton("GIVE UP  •  RETURN TO PIZZERIA", Color.FromArgb(227, 139, 116), Color.FromArgb(38, 22, 20), 290, 44);
        utility.Dock = DockStyle.Fill; utility.Click += (_, _) => ShowDay(); right.Controls.Add(utility, 0, 3);
        wrap.Controls.Add(right, 1, 0);
        _body.Controls.Add(wrap);
        _status.Text = $"NIGHT  {_nightTick:00}/75s      VENT  {_ventilation}%      STRIKES  {_strikes}/3      SCORE  {_nightScore}";
        _timer.Start();
    }

    private Control BuildOfficeScene()
    {
        var scene = new OfficePanel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(4, 8, 11), Owner = this };
        scene.Paint += (_, e) => DrawOffice(e.Graphics, scene.ClientRectangle);
        return scene;
    }

    private void DrawOffice(Graphics g, Rectangle bounds)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var bg = new LinearGradientBrush(bounds, Color.FromArgb(22, 31, 35), Color.FromArgb(3, 6, 9), 90f); g.FillRectangle(bg, bounds);
        var w = bounds.Width; var h = bounds.Height;
        using var frame = new Pen(Color.FromArgb(43, 60, 63), Math.Max(2, w / 300));
        g.DrawLine(frame, 0, (int)(h * .23), w, (int)(h * .23)); g.DrawLine(frame, (int)(w * .18), 0, (int)(w * .18), (int)(h * .76)); g.DrawLine(frame, (int)(w * .82), 0, (int)(w * .82), (int)(h * .76));
        using var glow = new SolidBrush(Color.FromArgb(16, 119, 220, 206)); g.FillEllipse(glow, (int)(w * .30), (int)(h * .12), (int)(w * .40), (int)(h * .42));
        using var desk = new SolidBrush(Color.FromArgb(27, 34, 34)); g.FillRectangle(desk, (int)(w * .08), (int)(h * .72), (int)(w * .84), (int)(h * .25));
        using var font = new Font("Consolas", Math.Max(13, w / 42), FontStyle.Bold); using var brush = new SolidBrush(Color.FromArgb(152, 171, 162));
        var time = Math.Min(6, _nightTick / 12); g.DrawString($"{time:00}:00 AM", font, brush, (int)(w * .38), (int)(h * .83));
        using var small = new Font("Consolas", Math.Max(8, w / 92), FontStyle.Regular); g.DrawString("VENT " + _ventilation + "%", small, brush, 12, 12);
        if (_doorClosed) { using var door = new SolidBrush(Color.FromArgb(70, 19, 23, 22)); g.FillRectangle(door, (int)(w * .82), (int)(h * .23), (int)(w * .18), (int)(h * .50)); }
        var threat = CurrentThreat(); if (threat is not null && _pendingUntil.TryGetValue(threat.Id, out var end) && end > _nightTick)
        {
            var src = _sources[threat.GameId]; if (src.Icon is not null)
            {
                try { using var bitmap = src.Icon.ToBitmap(); g.DrawImage(bitmap, new Rectangle((int)(w * .43), (int)(h * .31), (int)(w * .14), (int)(h * .19))); } catch { }
            }
        }
    }

    private Animatronic? CurrentThreat() => _sheets.Animatronics.FirstOrDefault(a => _pendingUntil.TryGetValue(a.Id, out var end) && end > _nightTick && !_resolved.GetValueOrDefault(a.Id));

    private void AddAction(Control parent, string text, string action, Action onClick)
    {
        var b = MakeButton(text, Teal, Color.FromArgb(14, 27, 30), 270, 31);
        b.Enabled = ActionAvailable(action);
        b.Click += (_, _) => { onClick(); UpdateNightUI(); };
        parent.Controls.Add(b);
    }

    private bool ActionAvailable(string action) => action switch
    {
        "door" => _owned.Contains("freddy"), "mask" => _owned.Contains("toy-bonnie"), "lure" => _owned.Contains("springtrap"),
        "listen" => _owned.Contains("nightmare-chica"), "scan" => _owned.Contains("scrap-baby"), "vent" => _owned.Contains("springtrap") || _owned.Contains("scrap-baby"), _ => false
    };

    private void AdvanceNight()
    {
        if (_mode != 1 || _nightEnded) return;
        _nightSecond++;
        if (_nightSecond % 4 != 0) { _nightTick = _nightSecond; UpdateNightUI(); return; }
        _nightTick = _nightSecond;
        if (_doorClosed) _ventilation = Math.Max(0, _ventilation - 2);
        if (_ventCooldown > 0) _ventCooldown--;
        if (_ventilation < 25) _lureReady = false;
        foreach (var anim in _sheets.Animatronics.Where(a => _owned.Contains(a.Id) && _ai[a.Id] > 0))
        {
            if (_nextAttack.TryGetValue(anim.Id, out var due) && due <= _nightTick && !_pendingUntil.ContainsKey(anim.Id))
            {
                _pendingUntil[anim.Id] = _nightTick + anim.WarningSeconds;
                _resolved[anim.Id] = false;
                _phaseMessage = WarningFor(anim);
                _nextAttack[anim.Id] = _nightTick + Math.Max(12, 52 - _ai[anim.Id] * 2) + _rng.Next(0, 9);
            }
            if (_pendingUntil.TryGetValue(anim.Id, out var until) && until <= _nightTick && !_resolved.GetValueOrDefault(anim.Id))
            {
                if (WasHandled(anim)) { _nightScore += 100 + _ai[anim.Id] * 5; _phaseMessage = $"{anim.Name} moved away. Stay alert."; }
                else { _strikes++; _phaseMessage = $"{anim.Name} got into the office. Strike {_strikes} of 3."; }
                _resolved[anim.Id] = true;
                _pendingUntil.Remove(anim.Id);
                _listening = false;
                _ventScanned = false;
            }
        }
        if (_nightTick >= 75) { EndNight(_strikes < 3); return; }
        if (_strikes >= 3) { EndNight(false); return; }
        UpdateNightUI();
    }

    private bool WasHandled(Animatronic a) => a.Action switch
    {
        "door" => _doorClosed, "mask" => _maskOn, "lure" => !_lureReady || _ventilation < 25,
        "listen" => _listening && _doorClosed, "scan" => _ventScanned, _ => false
    };

    private static string WarningFor(Animatronic a) => a.Action switch
    {
        "door" => "Movement at the right doorway. Close the door before it reaches you.",
        "mask" => "A Toy Bonnie silhouette in the hall. Put the mask on and stay still.",
        "lure" => "Springtrap is near the office. Use the audio lure while the vents are clear.",
        "listen" => "The hall is quiet. Listen for breathing, then block the doorway.",
        "scan" => "Scrap Baby is in the vents. Run a scan before she reaches the office.", _ => "Something is moving nearby."
    };

    private void UpdateNightUI()
    {
        if (_mode != 1) return;
        _status.Text = $"NIGHT  {_nightTick:00}/75s      VENT  {_ventilation}%      STRIKES  {_strikes}/3      SCORE  {_nightScore}";
        _subtitle.Text = $"NIGHT {_day}   •   {ActiveThreats()} ACTIVE THREATS";
        if (_clockLabel is not null) _clockLabel.Text = $"{Math.Min(6, _nightTick / 12):00}:00 AM\nNIGHT {_day}";
        if (_warningLabel is not null) _warningLabel.Text = _phaseMessage;
        foreach (Control c in _body.Controls) if (c is OfficePanel p) p.Invalidate(); else InvalidateNested(c);
    }

    private static void InvalidateNested(Control c)
    {
        c.Invalidate(); foreach (Control child in c.Controls) InvalidateNested(child);
    }

    private string ActiveThreats() => _sheets.Animatronics.Count(a => _owned.Contains(a.Id) && _ai[a.Id] > 0).ToString();

    private void EndNight(bool survived)
    {
        _nightEnded = true; _timer.Stop(); ClearBody(); _mode = 2;
        _headline.Text = survived ? "6:00 AM  /  SHIFT COMPLETE" : "SHIFT OVER  /  OFFICE BREACHED";
        _subtitle.Text = survived ? "THE PIZZERIA MADE IT THROUGH THE NIGHT" : "THE ANIMATRONICS GOT INSIDE";
        _status.Text = $"NIGHT SCORE  {_nightScore:N0}      STRIKES  {_strikes}/3      ACTIVE ROSTER  {ActiveThreats()}";
        var center = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(11, 17, 21), Padding = new Padding(30) };
        var message = new Label { Dock = DockStyle.Fill, Text = survived ? "The doors unlock. The stage is still yours.\n\nYour purchases carried real risk into the office, and UCN-style AI levels set how often each threat moved. Ready to open for another day?" : "A three-strike night ends the shift.\n\nLower an AI level, remove a threat from the roster, or try a different mix before the next run.", ForeColor = Ink, Font = new Font("Georgia", 18), TextAlign = ContentAlignment.MiddleCenter };
        center.Controls.Add(message);
        var again = MakeButton(survived ? "OPEN THE PIZZERIA AGAIN  →" : "REWORK THE PIZZERIA  →", Gold, Color.FromArgb(35, 29, 20), 290, 52);
        again.Anchor = AnchorStyles.Bottom; again.Left = (ClientSize.Width - again.Width) / 2; again.Top = 400; again.Click += (_, _) => { if (survived) _day++; ShowDay(); };
        center.Controls.Add(again);
        var end = MakeButton("EXIT", Muted, Color.FromArgb(19, 25, 31), 110, 40); end.Left = (ClientSize.Width - end.Width) / 2; end.Top = 470; end.Click += (_, _) => Close(); center.Controls.Add(end);
        _body.Controls.Add(center);
    }

    private void KeyDownHandler(object? sender, KeyEventArgs e)
    {
        if (_mode != 1) return;
        switch (e.KeyCode)
        {
            case Keys.D: _doorClosed = !_doorClosed; _phaseMessage = _doorClosed ? "Right door closed." : "Right door open."; break;
            case Keys.M: _maskOn = !_maskOn; _phaseMessage = _maskOn ? "Mask on. Keep still." : "Mask off."; break;
            case Keys.L: if (_lureReady && _ventilation > 0) { _lureReady = false; _phaseMessage = "Audio lure sent down the hall."; } break;
            case Keys.Q: _listening = true; _phaseMessage = "You listen for breathing at the doorway."; break;
            case Keys.V: _ventScanned = true; _phaseMessage = "Vent scan complete."; break;
            case Keys.R: if (_ventCooldown <= 0) { _ventilation = 100; _ventCooldown = 10; _lureReady = true; _phaseMessage = "Ventilation restored."; } break;
        }
        UpdateNightUI();
    }

    private static string ShortName(string name) => name switch
    {
        "Five Nights at Freddy's" => "FNAF 1",
        "Five Nights at Freddy's 2" => "FNAF 2",
        "Five Nights at Freddy's 3" => "FNAF 3",
        "Five Nights at Freddy's 4" => "FNAF 4",
        "Freddy Fazbear's Pizzeria Simulator" => "PIZZERIA SIM",
        _ => "UCN"
    };

    private static Button MakeButton(string text, Color fore, Color back, int width, int height)
    {
        return new Button { Text = text, Width = width, Height = height, FlatStyle = FlatStyle.Flat, BackColor = back, ForeColor = fore, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), Cursor = Cursors.Hand, UseVisualStyleBackColor = false, FlatAppearance = { BorderColor = Color.FromArgb(66, 92, 92), BorderSize = 1, MouseOverBackColor = Color.FromArgb(34, 48, 50) } };
    }

    private void ClearBody()
    {
        foreach (Control c in _body.Controls) c.Dispose();
        _body.Controls.Clear();
    }

    protected override void OnKeyDown(KeyEventArgs e) { base.OnKeyDown(e); KeyDownHandler(this, e); }
    protected override void OnFormClosed(FormClosedEventArgs e) { _timer.Dispose(); foreach (var s in _sources.Values) s.Dispose(); base.OnFormClosed(e); }
}

internal sealed class OfficePanel : Panel { public MainForm? Owner { get; set; } }
