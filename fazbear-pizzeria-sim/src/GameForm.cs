using System.Drawing.Drawing2D;
using System.Text.Json;

namespace FazbearPizzeria;

internal sealed class GameForm : Form
{
    private const float DesignWidth = 1280;
    private const float DesignHeight = 720;
    private readonly DesignSheets _sheets;
    private readonly Dictionary<string, SourceInstall> _sources;
    private readonly Dictionary<string, Bitmap?> _sourceArt = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Bitmap> _portraitArt = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Bitmap> _catalogAttractionArt = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _owned = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _unlockedAnimatronicGames = new(StringComparer.OrdinalIgnoreCase) { "fnaf1", "ucn" };
    private readonly HashSet<string> _stageAnimatronics = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _builtAttractions = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Point> _placedAttractions = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, RectangleF> _placedAttractionRemoveRects = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, RectangleF> _blueprintInventoryRects = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, RectangleF> _stageAnimatronicRects = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, RectangleF> _rosterGroupHeaderRects = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, RectangleF> _animatronicCategoryUnlockRects = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, RectangleF> _rosterCardRects = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _collapsedRosterGroups = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _ai = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Keys> _controlBindings = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, RectangleF> _settingsBindingRects = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _nextAttack = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _animatronicCameras = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _animatronicRouteIndices = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _pendingUntil = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _resolved = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<Keys> _cheatKeysHeld = [];
    // One simulation step is one second; 535 seconds is the classic 8:55 shift.
    private const int NightSteps = 535;
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 1000 };
    private readonly System.Windows.Forms.Timer _blinkTimer = new() { Interval = 300 };
    private readonly System.Windows.Forms.Timer _jumpscareTimer = new() { Interval = 1100 };
    private readonly Random _random = new(41);
    private readonly string[] _sourceOrder = ["fnaf1", "fnaf2", "fnaf3", "fnaf4", "ffps", "ucn"];
    private int _funds;
    private int _day = 1;
    private int _seconds;
    private int _strikes;
    private int _score;
    private int _vent = 100;
    private int _ventCooldown;
    private int _doorPower = 100;
    private int _doorPowerDrainCounter;
    private int _doorPowerRechargeCounter;
    private int _payoutPaidDay;
    private int _lawsuitCheckedDay;
    private int _payoutForNight;
    private int _payoutCollectedThisNight;
    private int _payoutAiBonusCollected;
    private int _konamiIndex;
    private int _nightSkipCheatIndex;
    private DateTime _cheatToastUntilUtc;
    private string _cheatToastMessage = "CHEAT ACCEPTED · +$1,000";
    private string? _selectedAttraction;
    private bool _leftDoorClosed;
    private bool _rightDoorClosed;
    private bool _maskOn;
    private bool _listening;
    private bool _lureSpent;
    private bool _ventScanned;
    private bool _monitorOpen;
    private bool _cameraMonitorMode;
    private string _selectedCamera = "1A";
    private bool _leftLightOn;
    private bool _rightLightOn;
    private string? _activeOfficeTaskId;
    private int _officeTaskSecondsRemaining;
    private string? _ventThreatSide;
    private bool _demo;
    private bool _nightOver;
    private bool _nightWon;
    private bool _nightStarted;
    private bool _dayResetByDeath;
    private bool _jumpscareActive;
    private string? _officeIntruderId;
    private string? _jumpscareAnimatronicId;
    private DateTime _officeIntruderUntilUtc;
    private bool _showMenu = true;
    private bool _settingsOpen;
    private bool _showRatingGuide;
    private bool _isFullscreen = true;
    private Rectangle _windowedBounds;
    private PrepScreen _prepScreen = PrepScreen.Builder;
    private string _message = "Choose a stage lineup for tonight.";
    private string _lastResolved = "";
    private float _scale = 1;
    private float _offsetX;
    private float _offsetY;
    private PointF _mouse;
    private readonly Font _brandFont = new("Consolas", 16, FontStyle.Bold, GraphicsUnit.Pixel);
    private readonly Font _titleFont = new("Consolas", 21, FontStyle.Bold, GraphicsUnit.Pixel);
    private readonly Font _bodyFont = new("Consolas", 12, FontStyle.Regular, GraphicsUnit.Pixel);
    private readonly Font _smallFont = new("Consolas", 11, FontStyle.Bold, GraphicsUnit.Pixel);
    private readonly Font _cardTitleFont = new("Consolas", 16, FontStyle.Bold, GraphicsUnit.Pixel);
    private readonly Font _panelTitleFont = new("Consolas", 14, FontStyle.Bold, GraphicsUnit.Pixel);
    private readonly Font _messageFont = new("Consolas", 15, FontStyle.Bold, GraphicsUnit.Pixel);
    private readonly Font _aiFont = new("Consolas", 14, FontStyle.Bold, GraphicsUnit.Pixel);
    private readonly Font _noMovementFont = new("Consolas", 13, FontStyle.Bold, GraphicsUnit.Pixel);
    private readonly Font _keyFont = new("Consolas", 14, FontStyle.Bold, GraphicsUnit.Pixel);
    private readonly Font _menuTitleFont = new("Consolas", 50, FontStyle.Bold, GraphicsUnit.Pixel);
    private readonly Font _menuSubtitleFont = new("Consolas", 27, FontStyle.Bold, GraphicsUnit.Pixel);
    private readonly Color _ink = Color.FromArgb(245, 239, 222);
    private readonly Color _muted = Color.FromArgb(185, 170, 164);
    private readonly Color _gold = Color.FromArgb(255, 198, 59);
    private readonly Color _teal = Color.FromArgb(190, 74, 199);
    private readonly Color _red = Color.FromArgb(194, 49, 53);
    private readonly Color _panel = Color.FromArgb(28, 19, 29);
    private readonly Color _edge = Color.FromArgb(91, 56, 81);
    private RectangleF _startNightRect;
    private RectangleF _menuStartRect;
    private RectangleF _menuSettingsRect;
    private RectangleF _settingsBackRect;
    private RectangleF _ratingGuideButtonRect;
    private RectangleF _ratingGuideCloseRect;
    private RectangleF _catalogViewportRect;
    private RectangleF _catalogScrollTrackRect;
    private RectangleF _catalogScrollThumbRect;
    private RectangleF _blueprintInventoryViewportRect;
    private RectangleF _blueprintInventoryScrollTrackRect;
    private RectangleF _buildTabRect;
    private RectangleF _blueprintTabRect;
    private RectangleF _rosterTabRect;
    private RectangleF _blueprintGridRect;
    private RectangleF _rosterViewportRect;
    private RectangleF _rosterScrollTrackRect;
    private RectangleF _rosterScrollThumbRect;
    private RectangleF _rosterAiMinusRect;
    private RectangleF _rosterAiPlusRect;
    private RectangleF _purchaseConfirmRect;
    private RectangleF _purchaseCancelRect;
    private string? _pendingPurchaseId;
    private PurchaseKind _pendingPurchaseKind;
    private int _rosterScrollOffset;
    private int _rosterContentHeight;
    private int _catalogScrollOffset;
    private int _catalogContentHeight;
    private int _blueprintInventoryScrollOffset;
    private int _lawsuitsThisShift;
    private int _lawsuitFeesThisShift;
    private string _catalogMessage = "Unlock better catalogs by raising your pizzeria rating.";
    private string _selectedCatalogCategory = "dumpster";
    private bool _draggingRosterScroll;
    private bool _suppressNextMouseClick;
    private float _rosterDragGrabOffset;
    private string? _rosterHoveredAnimatronic;
    private string? _waitingForBindingAction;
    private string _settingsMessage = "Select a control, then press the key you want to bind.";
    private RectangleF _continueRect;
    private RectangleF _monitorToggleRect;
    private readonly Dictionary<string, RectangleF> _officeTaskRects = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, RectangleF> _officeMonitorTabRects = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, RectangleF> _cameraSelectRects = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, RectangleF> _officeLightRects = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _completedOfficeTasks = new(StringComparer.OrdinalIgnoreCase);
    private static readonly (string Id, string Name, int Seconds, bool Noisy)[] OfficeTasks =
    [
        ("cups", "ORDER CUPS", 12, false), ("plates", "ORDER PLATES", 12, false),
        ("napkins", "ORDER NAPKINS", 12, false), ("utensils", "ORDER UTENSILS", 12, false),
        ("pizza-kits", "ORDER PIZZA KITS", 12, false), ("menus", "PRINT MENUS", 20, true),
        ("posters", "PRINT POSTERS", 20, true), ("flyers", "PRINT FLYERS", 20, true),
        ("bulbs", "REPLACE BULBS", 17, false), ("ovens", "CLEAN OVENS", 17, false),
        ("toilets", "UNCLOG TOILETS", 17, false)
    ];
    private static readonly (string Id, string Name)[] FnafCameras =
    [
        ("1A", "SHOW STAGE"), ("1B", "DINING AREA"), ("1C", "PIRATE COVE"),
        ("2A", "WEST HALL"), ("2B", "WEST CORNER"), ("3", "SUPPLY CLOSET"),
        ("4A", "EAST HALL"), ("4B", "EAST CORNER"), ("5", "BACKSTAGE"),
        ("6", "KITCHEN"), ("7", "RESTROOMS")
    ];
    private static readonly Dictionary<string, string[]> AnimatronicRoutes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["freddy"] = ["1A", "1B", "7", "6", "4A", "4B"],
        ["toy-bonnie"] = ["1A", "5", "1B", "2A", "2B"],
        ["springtrap"] = ["1A", "5", "3", "2A", "2B"],
        ["nightmare-chica"] = ["1A", "1B", "6", "2A", "2B"],
        ["scrap-baby"] = ["1A", "5", "7", "V1"]
    };
    private readonly Dictionary<string, RectangleF> _attractionBuyRects = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, RectangleF> _catalogCategoryRects = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, RectangleF> _nightActionRects = new(StringComparer.OrdinalIgnoreCase);
    private readonly SaveGameData?[] _saveSlots = new SaveGameData?[3];
    private readonly bool[] _saveSlotFilesPresent = new bool[3];
    private readonly RectangleF[] _saveLoadRects = new RectangleF[3];
    private readonly RectangleF[] _saveDeleteRects = new RectangleF[3];
    private RectangleF _saveMenuBackRect;
    private RectangleF _deleteSaveConfirmRect;
    private RectangleF _deleteSaveCancelRect;
    private string _saveMenuMessage = "Progress saves automatically in the selected slot.";
    private int _activeSaveSlot = -1;
    private int _pendingDeleteSlot = -1;
    private bool _saveMenuOpen;

    private enum PrepScreen { Builder, Blueprint, Roster }
    private enum PurchaseKind { None, Attraction, Animatronic, AnimatronicCategory }
    private sealed record DayStartSnapshot(int Day, int Funds, string[] OwnedAnimatronics, string[] StagedAnimatronics,
        string[] BuiltAttractions, Dictionary<string, int> AttractionTiles, Dictionary<string, int> AiLevels,
        string[] UnlockedAnimatronicGames, int PayoutPaidDay, int LawsuitCheckedDay);
    private sealed record SaveGameData(int Version, DateTime SavedAt, int Day, int Funds, string[] OwnedAnimatronics,
        string[] StagedAnimatronics, string[] BuiltAttractions, Dictionary<string, int> AttractionTiles,
        Dictionary<string, int> AiLevels, string[] UnlockedAnimatronicGames, int PayoutPaidDay, int LawsuitCheckedDay,
        int PrepScreen, bool NightOver, bool NightWon, int PayoutCollectedThisNight, int LawsuitsThisShift, int LawsuitFeesThisShift,
        int AiPayoutBonus = 0, DayStartSnapshot? DayStartState = null, int NightStrikes = 0, bool DayResetByDeath = false);
    private DayStartSnapshot? _dayStartSnapshot;

    public GameForm(DesignSheets sheets, Dictionary<string, SourceInstall> sources, bool demo)
    {
        _sheets = sheets;
        _sources = sources;
        _demo = demo;
        _funds = sheets.StartingFunds;
        // Freddy is the starter character; $200 remains available for the first attraction.
        _owned.Add("freddy");
        foreach (var a in sheets.Animatronics) _ai[a.Id] = sheets.DefaultAiLevel;
        _controlBindings["left-door"] = Keys.A;
        _controlBindings["right-door"] = Keys.D;
        _controlBindings["mask"] = Keys.H;
        _controlBindings["lure"] = Keys.L;
        _controlBindings["listen"] = Keys.Q;
        _controlBindings["scan"] = Keys.V;
        _controlBindings["vent"] = Keys.R;
        _controlBindings["monitor"] = Keys.M;
        _controlBindings["camera-prev"] = Keys.Left;
        _controlBindings["camera-next"] = Keys.Right;
        _controlBindings["left-light"] = Keys.Z;
        _controlBindings["right-light"] = Keys.C;
        LoadControlBindings();
        RefreshSaveSlots();
        foreach (var id in _sourceOrder)
        {
            var icon = sources[id].Icon;
            try { _sourceArt[id] = icon?.ToBitmap(); } catch { _sourceArt[id] = null; }
        }
        var portraitDirectory = Path.Combine(AppContext.BaseDirectory, "assets", "portraits");
        foreach (var a in sheets.Animatronics)
        {
            var path = Path.Combine(portraitDirectory, a.Id + ".png");
            if (!File.Exists(path)) continue;
            try { using var image = Image.FromFile(path); _portraitArt[a.Id] = new Bitmap(image); } catch { }
        }
        var attractionArtDirectory = Path.Combine(AppContext.BaseDirectory, "assets", "attractions");
        foreach (var attraction in sheets.Attractions)
        {
            var path = Path.Combine(attractionArtDirectory, attraction.Id + ".png");
            if (!File.Exists(path)) continue;
            try { using var image = Image.FromFile(path); _catalogAttractionArt[attraction.Id] = new Bitmap(image); } catch { }
        }
        Text = "Fazbear Pizzeria After Hours";
        FormBorderStyle = FormBorderStyle.None;
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(800, 600);
        BackColor = Color.FromArgb(5, 8, 9);
        DoubleBuffered = true;
        KeyPreview = true;
        StartPosition = FormStartPosition.CenterScreen;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        _timer.Tick += (_, _) => AdvanceNight();
        _jumpscareTimer.Tick += (_, _) =>
        {
            _jumpscareTimer.Stop();
            _jumpscareActive = false;
            FinishNight(false, died: true);
        };
        _blinkTimer.Tick += (_, _) =>
        {
            if (_nightStarted && !_nightOver) Invalidate();
            if (_cheatToastUntilUtc != default)
            {
                if (DateTime.UtcNow >= _cheatToastUntilUtc) _cheatToastUntilUtc = default;
                Invalidate();
            }
        };
        _blinkTimer.Start();
        if (_demo)
        {
            Shown += async (_, _) =>
            {
                foreach (var a in _sheets.Animatronics) if (_sources[a.GameId].IsAvailable) { _owned.Add(a.Id); _stageAnimatronics.Add(a.Id); }
                _funds = Math.Max(0, _funds - _sheets.Animatronics.Where(a => _owned.Contains(a.Id) && a.Id != "freddy").Sum(a => a.Cost));
                await Task.Delay(250);
                StartNight();
            };
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        g.Clear(Color.FromArgb(5, 8, 9));
        _scale = Math.Min(ClientSize.Width / DesignWidth, ClientSize.Height / DesignHeight);
        _offsetX = (ClientSize.Width - DesignWidth * _scale) / 2;
        _offsetY = (ClientSize.Height - DesignHeight * _scale) / 2;
        g.TranslateTransform(_offsetX, _offsetY);
        g.ScaleTransform(_scale, _scale);
        DrawBackdrop(g);
        DrawHeader(g);
        if (_settingsOpen) DrawSettings(g);
        else if (_showMenu) DrawMenu(g);
        else if (_jumpscareActive) DrawJumpscare(g);
        else if (_nightStarted && _nightOver) DrawResult(g);
        else if (_nightStarted) DrawNight(g);
        else DrawPizzeria(g);
        DrawCheatToast(g);
        if (_pendingPurchaseKind != PurchaseKind.None) DrawPurchaseConfirmation(g);
    }

    private void DrawBackdrop(Graphics g)
    {
        using var gradient = new LinearGradientBrush(new RectangleF(0, 0, DesignWidth, DesignHeight), Color.FromArgb(30, 18, 27), Color.FromArgb(4, 5, 7), 90f);
        g.FillRectangle(gradient, 0, 0, DesignWidth, DesignHeight);
        using var pen = new Pen(Color.FromArgb(14, 190, 74, 199), 1);
        for (var x = 24; x < DesignWidth; x += 32) g.DrawLine(pen, x, 84, x, DesignHeight - 34);
        using var top = new SolidBrush(Color.FromArgb(220, 12, 7, 13));
        g.FillRectangle(top, 0, 0, DesignWidth, 88);
        using var bottom = new SolidBrush(Color.FromArgb(220, 8, 11, 12));
        g.FillRectangle(bottom, 0, 680, DesignWidth, 40);
        DrawLine(g, _edge, 1, 0, 88, DesignWidth, 88);
        DrawLine(g, _edge, 1, 0, 680, DesignWidth, 680);
    }

    private void DrawHeader(Graphics g)
    {
        DrawText(g, "FAZBEAR ENTERTAINMENT", _smallFont, _gold, 48, 17);
        DrawText(g, "FREDDY FAZBEAR'S PIZZERIA", _brandFont, _ink, 48, 34);
        if (!_showMenu)
        {
            var phase = _nightStarted ? $"NIGHT {_day:00}  /  OFFICE SHIFT" : $"DAY {_day:00}  /  {(_prepScreen == PrepScreen.Builder ? "CATALOG" : _prepScreen == PrepScreen.Blueprint ? "BLUEPRINT" : "ANIMATRONIC ROSTER")}";
            DrawText(g, phase, _smallFont, _teal, 500, 51, 260);
            DrawText(g, _nightStarted && !_nightOver ? $"${_funds:N0}  /  {ActiveThreatCount()} ACTIVE" : $"${_funds:N0} AVAILABLE", _smallFont, _gold, 760, 51, 150);
        }
        DrawImageFrame(g, "ucn", new RectangleF(1094, 15, 48, 48));
        DrawText(g, "UCN", _smallFont, _gold, 1152, 24);
        DrawText(g, "AI RULES", _smallFont, _muted, 1152, 42);
        if (_nightStarted && !_nightOver)
        {
            var hour = Math.Min(6, _seconds * 6 / NightSteps);
            DrawText(g, $"{hour:00}:00 AM", _smallFont, _gold, 915, 39, 150, 24, StringAlignment.Far);
        }
    }

    private void DrawMenu(Graphics g)
    {
        if (_saveMenuOpen)
        {
            DrawSaveMenu(g);
            return;
        }
        DrawText(g, "FREDDY FAZBEAR'S", _smallFont, _gold, 88, 179);
        DrawText(g, "PIZZERIA", _menuTitleFont, _ink, 82, 198, 720, 80);
        DrawText(g, "AFTER HOURS", _menuSubtitleFont, _teal, 88, 273, 600, 48);
        DrawText(g, "BUILD A FAMILY PIZZERIA BY DAY.", _panelTitleFont, _ink, 90, 365, 640, 30);
        DrawText(g, "BUY ATTRACTIONS. ADD ANIMATRONICS. SURVIVE THE NIGHT.", _bodyFont, _muted, 90, 401, 800, 25);
        _menuStartRect = new RectangleF(88, 485, 330, 58);
        DrawButton(g, _menuStartRect, "SAVE SLOTS", "→", _gold, Color.FromArgb(47, 35, 19), true);
        _menuSettingsRect = new RectangleF(434, 485, 220, 58);
        DrawButton(g, _menuSettingsRect, "SETTINGS", "⚙", _teal, Color.FromArgb(22, 28, 29), true);
        DrawText(g, "A SOLO FNAF MASHUP TEST BUILD", _smallFont, _muted, 90, 568);
        DrawText(g, "ESC  QUIT", _smallFont, Color.FromArgb(106, 115, 110), 48, 694);
        DrawText(g, "F11  WINDOWED / FULLSCREEN", _smallFont, Color.FromArgb(106, 115, 110), 1010, 694, 220, 18, StringAlignment.Far);
        var marquee = new RectangleF(790, 145, 418, 326);
        var tile = 38f;
        for (var row = 0; row < (int)Math.Ceiling(marquee.Height / tile); row++)
        for (var col = 0; col < (int)Math.Ceiling(marquee.Width / tile); col++)
        {
            using var checker = new SolidBrush((row + col) % 2 == 0 ? Color.FromArgb(230, 72, 42, 119) : Color.FromArgb(230, 231, 220, 199));
            g.FillRectangle(checker, marquee.X + col * tile, marquee.Y + row * tile, tile, tile);
        }
        var curtain = new RectangleF(821, 196, 356, 151);
        using (var curtainFill = new SolidBrush(Color.FromArgb(210, 88, 17, 28))) g.FillRectangle(curtainFill, curtain);
        for (var x = curtain.X + 12; x < curtain.Right; x += 32)
            DrawLine(g, Color.FromArgb(115, 234, 68, 53), 5, x, curtain.Y + 3, x, curtain.Bottom - 4);
        DrawText(g, "FAZBEAR'S ALL-STAR STAGE", _smallFont, _gold, curtain.X, curtain.Y + 10, curtain.Width, 18, StringAlignment.Center);
        string[] stageSources = ["fnaf1", "fnaf2", "fnaf3", "fnaf4", "ffps"];
        for (var i = 0; i < stageSources.Length; i++)
            DrawImageFrame(g, stageSources[i], new RectangleF(curtain.X + 34 + i * 65, curtain.Y + 40, 54, 54));
        var stageDeck = new RectangleF(807, 344, 384, 39);
        using (var deck = new SolidBrush(Color.FromArgb(224, 112, 65, 38))) g.FillRectangle(deck, stageDeck);
        using (var deckEdge = new Pen(Color.FromArgb(240, 230, 177, 77), 2)) g.DrawRectangle(deckEdge, stageDeck.X, stageDeck.Y, stageDeck.Width, stageDeck.Height);
        DrawText(g, "OPENING NIGHT  ·  DAY 01", _smallFont, _gold, 838, 411, 320, 25, StringAlignment.Center);
    }

    private void DrawSaveMenu(Graphics g)
    {
        DrawText(g, "SAVE FILES", _titleFont, _ink, 88, 139, 700, 34);
        DrawText(g, "Choose one of three slots. Progress autosaves; delete a slot to start fresh in it.", _bodyFont, _muted, 90, 177, 1000, 22);
        for (var i = 0; i < _saveSlots.Length; i++)
        {
            var slot = new RectangleF(80, 218 + i * 120, 1120, 92);
            var exists = _saveSlotFilesPresent[i];
            var data = _saveSlots[i];
            DrawPanel(g, slot, data is null && exists ? Color.FromArgb(40, 22, 23) : _panel);
            DrawText(g, $"SAVE SLOT {i + 1:00}", _smallFont, _gold, slot.X + 20, slot.Y + 17, 160, 18);
            var summary = !exists ? "EMPTY" : data is null ? "SAVE DATA UNREADABLE · DELETE TO CLEAR" : $"DAY {data.Day:00}   ·   ${data.Funds:N0}   ·   {data.OwnedAnimatronics.Length} ANIMATRONIC{(data.OwnedAnimatronics.Length == 1 ? "" : "S")} OWNED";
            DrawText(g, summary, _panelTitleFont, data is null && exists ? _red : data is null ? _muted : _ink, slot.X + 190, slot.Y + 16, 510, 26);
            var saved = data is null ? "" : $"LAST SAVED  {data.SavedAt.ToLocalTime():dd MMM yyyy  HH:mm}";
            DrawText(g, saved, _smallFont, _muted, slot.X + 190, slot.Y + 52, 510, 18);
            _saveLoadRects[i] = new RectangleF(slot.Right - 364, slot.Y + 22, 194, 48);
            _saveDeleteRects[i] = new RectangleF(slot.Right - 154, slot.Y + 22, 138, 48);
            DrawButton(g, _saveLoadRects[i], !exists ? "NEW GAME" : data is null ? "UNAVAILABLE" : "LOAD SAVE", !exists ? "＋" : "→", _gold, Color.FromArgb(47, 35, 19), !exists || data is not null);
            DrawButton(g, _saveDeleteRects[i], "DELETE", "X", _red, Color.FromArgb(44, 20, 22), exists);
        }
        DrawText(g, _saveMenuMessage, _smallFont, _muted, 90, 590, 820, 20);
        _saveMenuBackRect = new RectangleF(88, 624, 260, 42);
        DrawButton(g, _saveMenuBackRect, "BACK TO MENU", "←", _gold, Color.FromArgb(47, 35, 19), true);
        DrawText(g, "F11  WINDOWED / FULLSCREEN", _smallFont, Color.FromArgb(106, 115, 110), 1010, 694, 220, 18, StringAlignment.Far);
        if (_pendingDeleteSlot >= 0) DrawDeleteSaveConfirmation(g);
    }

    private void DrawDeleteSaveConfirmation(Graphics g)
    {
        using var shade = new SolidBrush(Color.FromArgb(210, 0, 0, 0));
        g.FillRectangle(shade, 0, 0, DesignWidth, DesignHeight);
        var panel = new RectangleF(405, 252, 470, 204);
        DrawPanel(g, panel, Color.FromArgb(28, 17, 22));
        using var border = new Pen(_red, 2);
        g.DrawRectangle(border, panel.X, panel.Y, panel.Width, panel.Height);
        DrawText(g, "DELETE THIS SAVE?", _panelTitleFont, _red, panel.X + 24, panel.Y + 24, panel.Width - 48, 24, StringAlignment.Center);
        DrawText(g, $"Save slot {_pendingDeleteSlot + 1} and its pizzeria progress will be erased.", _bodyFont, _muted, panel.X + 24, panel.Y + 68, panel.Width - 48, 42, StringAlignment.Center);
        _deleteSaveCancelRect = new RectangleF(panel.X + 34, panel.Y + 139, 180, 40);
        _deleteSaveConfirmRect = new RectangleF(panel.Right - 214, panel.Y + 139, 180, 40);
        DrawButton(g, _deleteSaveCancelRect, "CANCEL", "ESC", _muted, Color.FromArgb(18, 23, 23), true);
        DrawButton(g, _deleteSaveConfirmRect, "DELETE SAVE", "X", _red, Color.FromArgb(50, 19, 22), true);
    }

    private void DrawSettings(Graphics g)
    {
        DrawText(g, "SETTINGS", _titleFont, _ink, 88, 145, 700, 36);
        DrawText(g, "CONTROL BINDINGS", _smallFont, _gold, 90, 190, 400, 20);
        DrawText(g, _waitingForBindingAction is null ? _settingsMessage : $"PRESS A KEY FOR {ControlName(_waitingForBindingAction)}  ·  ESC TO CANCEL", _bodyFont, _teal, 90, 220, 1000, 26);
        _settingsBindingRects.Clear();
        var actions = new[] { "left-door", "right-door", "left-light", "right-light", "monitor", "camera-prev", "camera-next", "mask", "lure", "listen", "scan", "vent" };
        for (var i = 0; i < actions.Length; i++)
        {
            var action = actions[i];
            var row = new RectangleF(88, 245 + i * 30, 600, 26);
            DrawPanel(g, row, _waitingForBindingAction == action ? Color.FromArgb(41, 37, 21) : _panel);
            DrawText(g, ControlName(action), _smallFont, _ink, row.X + 16, row.Y + 5, 360, 18);
            var bind = new RectangleF(row.Right - 152, row.Y + 2, 136, 22);
            _settingsBindingRects[action] = bind;
            DrawButton(g, bind, _waitingForBindingAction == action ? "PRESS KEY" : KeyLabel(_controlBindings[action]), "↵", _waitingForBindingAction == action ? _gold : _teal, Color.FromArgb(13, 20, 21), true);
        }
        _settingsBackRect = new RectangleF(88, 625, 260, 42);
        DrawButton(g, _settingsBackRect, "BACK TO MENU", "←", _gold, Color.FromArgb(47, 35, 19), true);
        DrawText(g, "F11  WINDOWED / FULLSCREEN", _smallFont, Color.FromArgb(106, 115, 110), 1010, 694, 220, 18, StringAlignment.Far);
    }

    private static string ControlName(string action) => action switch
    {
        "left-door" => "Left door", "right-door" => "Right door", "left-light" => "Left doorway light", "right-light" => "Right doorway light",
        "monitor" => "Raise / lower monitor", "camera-prev" => "Previous camera", "camera-next" => "Next camera",
        "mask" => "Put on mask", "lure" => "Audio lure", "listen" => "Listen", "scan" => "Vent scan", "vent" => "Reset vents", _ => action
    };

    private static string KeyLabel(Keys key) => key switch
    {
        Keys.Space => "SPACE", Keys.Enter => "ENTER", Keys.Back => "BACKSPACE", Keys.PageUp => "PAGE UP", Keys.PageDown => "PAGE DOWN",
        _ => key.ToString().ToUpperInvariant()
    };

    private static string ControlSettingsPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FazbearPizzeriaAfterHours", "controls.json");

    private void LoadControlBindings()
    {
        try
        {
            if (!File.Exists(ControlSettingsPath)) return;
            var saved = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(ControlSettingsPath));
            if (saved is null) return;
            foreach (var (action, text) in saved)
                if (_controlBindings.ContainsKey(action) && Enum.TryParse<Keys>(text, true, out var key) && key is not Keys.None and not Keys.Escape and not Keys.F11 && !_controlBindings.Values.Contains(key))
                    _controlBindings[action] = key;
        }
        catch { }
    }

    private void SaveControlBindings()
    {
        try
        {
            var directory = Path.GetDirectoryName(ControlSettingsPath)!;
            Directory.CreateDirectory(directory);
            File.WriteAllText(ControlSettingsPath, JsonSerializer.Serialize(_controlBindings.ToDictionary(pair => pair.Key, pair => pair.Value.ToString())));
        }
        catch { _settingsMessage = "Could not save controls; they will reset next launch."; }
    }

    private static string SaveSlotPath(int slot) => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "FazbearPizzeriaAfterHours", $"save-{slot + 1}.json");

    private void RefreshSaveSlots()
    {
        for (var i = 0; i < _saveSlots.Length; i++)
        {
            var path = SaveSlotPath(i);
            _saveSlotFilesPresent[i] = File.Exists(path);
            _saveSlots[i] = null;
            if (!_saveSlotFilesPresent[i]) continue;
            try
            {
                var slot = JsonSerializer.Deserialize<SaveGameData>(File.ReadAllText(path));
                if (slot is { Version: 1, OwnedAnimatronics: not null, StagedAnimatronics: not null,
                    BuiltAttractions: not null, AttractionTiles: not null, AiLevels: not null,
                    UnlockedAnimatronicGames: not null }) _saveSlots[i] = slot;
            }
            catch { }
        }
    }

    private void StartNewGameInSlot(int slot)
    {
        if (_saveSlotFilesPresent[slot]) return;
        _timer.Stop();
        _activeSaveSlot = slot;
        _funds = _sheets.StartingFunds;
        _day = 1;
        _payoutPaidDay = 0;
        _lawsuitCheckedDay = 0;
        _payoutCollectedThisNight = 0;
        _payoutAiBonusCollected = 0;
        _lawsuitsThisShift = 0;
        _lawsuitFeesThisShift = 0;
        _owned.Clear();
        _owned.Add("freddy");
        _unlockedAnimatronicGames.Clear();
        _unlockedAnimatronicGames.UnionWith(["fnaf1", "ucn"]);
        _stageAnimatronics.Clear();
        _builtAttractions.Clear();
        _placedAttractions.Clear();
        _ai.Clear();
        foreach (var animatronic in _sheets.Animatronics) _ai[animatronic.Id] = _sheets.DefaultAiLevel;
        _nightStarted = false;
        _nightOver = false;
        _nightWon = false;
        _seconds = 0;
        _strikes = 0;
        _payoutForNight = 0;
        _settingsOpen = false;
        _saveMenuOpen = false;
        _showMenu = false;
        _prepScreen = PrepScreen.Builder;
        _selectedAttraction = null;
        _dayResetByDeath = false;
        CaptureDayStartSnapshot();
        _saveMenuMessage = "Progress saves automatically in the selected slot.";
        SaveActiveSlot();
        Invalidate();
    }

    private void LoadGameFromSlot(int slot)
    {
        var data = _saveSlots[slot];
        if (data is null) { _saveMenuMessage = "This save cannot be read. Delete it to start a new game here."; Invalidate(); return; }
        _activeSaveSlot = slot;
        _funds = Math.Max(0, data.Funds);
        _day = Math.Max(1, data.Day);
        _payoutPaidDay = Math.Max(0, data.PayoutPaidDay);
        _lawsuitCheckedDay = Math.Max(0, data.LawsuitCheckedDay);
        _payoutCollectedThisNight = Math.Max(0, data.PayoutCollectedThisNight);
        _payoutAiBonusCollected = Math.Max(0, data.AiPayoutBonus);
        _strikes = Math.Clamp(data.NightStrikes, 0, 3);
        _dayResetByDeath = data.DayResetByDeath;
        _lawsuitsThisShift = Math.Max(0, data.LawsuitsThisShift);
        _lawsuitFeesThisShift = Math.Max(0, data.LawsuitFeesThisShift);
        _owned.Clear();
        _owned.UnionWith((data.OwnedAnimatronics ?? []).Where(id => _sheets.Animatronics.Any(a => a.Id == id)));
        _owned.Add("freddy");
        _stageAnimatronics.Clear();
        _stageAnimatronics.UnionWith((data.StagedAnimatronics ?? []).Where(_owned.Contains));
        _builtAttractions.Clear();
        _builtAttractions.UnionWith((data.BuiltAttractions ?? []).Where(id => _sheets.Attractions.Any(a => a.Id == id)));
        _placedAttractions.Clear();
        foreach (var (id, tile) in data.AttractionTiles ?? [])
        {
            if (!_builtAttractions.Contains(id) || tile is < 0 or >= 32) continue;
            var cell = new Point(tile % 8, tile / 8);
            if (IsDiningTableTile(cell) || _placedAttractions.Values.Contains(cell)) continue;
            _placedAttractions[id] = cell;
        }
        _ai.Clear();
        foreach (var animatronic in _sheets.Animatronics)
            _ai[animatronic.Id] = Math.Clamp(data.AiLevels is not null && data.AiLevels.TryGetValue(animatronic.Id, out var level) ? level : _sheets.DefaultAiLevel, 0, _sheets.MaximumAiLevel);
        _unlockedAnimatronicGames.Clear();
        _unlockedAnimatronicGames.UnionWith(["fnaf1", "ucn"]);
        _unlockedAnimatronicGames.UnionWith((data.UnlockedAnimatronicGames ?? []).Where(id => _sheets.Games.Any(game => game.Id == id)));
        _dayStartSnapshot = data.DayStartState;
        _prepScreen = Enum.IsDefined(typeof(PrepScreen), data.PrepScreen) ? (PrepScreen)data.PrepScreen : PrepScreen.Builder;
        _nightOver = data.NightOver;
        _nightWon = data.NightWon;
        _nightStarted = data.NightOver;
        _showMenu = false;
        _saveMenuOpen = false;
        _settingsOpen = false;
        _selectedAttraction = null;
        _timer.Stop();
        if (_nightOver) { _nightStarted = true; }
        if (_dayStartSnapshot is null) CaptureDayStartSnapshot();
        _saveMenuMessage = "Save loaded.";
        Invalidate();
    }

    private void SaveActiveSlot()
    {
        if (_activeSaveSlot is < 0 or >= 3) return;
        try
        {
            var directory = Path.GetDirectoryName(SaveSlotPath(_activeSaveSlot))!;
            Directory.CreateDirectory(directory);
            var path = SaveSlotPath(_activeSaveSlot);
            var temporary = path + ".tmp";
            var data = new SaveGameData(1, DateTime.UtcNow, _day, _funds,
                _owned.Order(StringComparer.OrdinalIgnoreCase).ToArray(), _stageAnimatronics.Order(StringComparer.OrdinalIgnoreCase).ToArray(),
                _builtAttractions.Order(StringComparer.OrdinalIgnoreCase).ToArray(),
                _placedAttractions.ToDictionary(pair => pair.Key, pair => pair.Value.Y * 8 + pair.Value.X, StringComparer.OrdinalIgnoreCase),
                new Dictionary<string, int>(_ai, StringComparer.OrdinalIgnoreCase),
                _unlockedAnimatronicGames.Order(StringComparer.OrdinalIgnoreCase).ToArray(), _payoutPaidDay, _lawsuitCheckedDay,
                (int)_prepScreen, _nightOver, _nightWon, _payoutCollectedThisNight, _lawsuitsThisShift, _lawsuitFeesThisShift,
                _payoutAiBonusCollected, _dayStartSnapshot, _strikes, _dayResetByDeath);
            File.WriteAllText(temporary, JsonSerializer.Serialize(data));
            File.Move(temporary, path, true);
            RefreshSaveSlots();
            _saveMenuMessage = "Progress saved.";
        }
        catch { _saveMenuMessage = "Could not save progress. Check local storage permissions."; }
    }

    private void CaptureDayStartSnapshot()
    {
        _dayStartSnapshot = new DayStartSnapshot(_day, _funds,
            _owned.Order(StringComparer.OrdinalIgnoreCase).ToArray(),
            _stageAnimatronics.Order(StringComparer.OrdinalIgnoreCase).ToArray(),
            _builtAttractions.Order(StringComparer.OrdinalIgnoreCase).ToArray(),
            _placedAttractions.ToDictionary(pair => pair.Key, pair => pair.Value.Y * 8 + pair.Value.X, StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, int>(_ai, StringComparer.OrdinalIgnoreCase),
            _unlockedAnimatronicGames.Order(StringComparer.OrdinalIgnoreCase).ToArray(), _payoutPaidDay, _lawsuitCheckedDay);
    }

    private void RestoreDayStartSnapshot()
    {
        if (_dayStartSnapshot is null) { CaptureDayStartSnapshot(); return; }
        var snapshot = _dayStartSnapshot;
        _day = Math.Max(1, snapshot.Day);
        _funds = Math.Max(0, snapshot.Funds);
        _payoutPaidDay = Math.Max(0, snapshot.PayoutPaidDay);
        _lawsuitCheckedDay = Math.Max(0, snapshot.LawsuitCheckedDay);
        _owned.Clear(); _owned.UnionWith(snapshot.OwnedAnimatronics.Where(id => _sheets.Animatronics.Any(a => a.Id == id))); _owned.Add("freddy");
        _stageAnimatronics.Clear(); _stageAnimatronics.UnionWith(snapshot.StagedAnimatronics.Where(_owned.Contains));
        _builtAttractions.Clear(); _builtAttractions.UnionWith(snapshot.BuiltAttractions.Where(id => _sheets.Attractions.Any(a => a.Id == id)));
        _placedAttractions.Clear();
        foreach (var (id, tile) in snapshot.AttractionTiles)
        {
            if (!_builtAttractions.Contains(id) || tile is < 0 or >= 32) continue;
            var cell = new Point(tile % 8, tile / 8);
            if (IsDiningTableTile(cell) || _placedAttractions.Values.Contains(cell)) continue;
            _placedAttractions[id] = cell;
        }
        _ai.Clear();
        foreach (var animatronic in _sheets.Animatronics)
            _ai[animatronic.Id] = Math.Clamp(snapshot.AiLevels.TryGetValue(animatronic.Id, out var level) ? level : _sheets.DefaultAiLevel, 0, _sheets.MaximumAiLevel);
        _unlockedAnimatronicGames.Clear(); _unlockedAnimatronicGames.UnionWith(["fnaf1", "ucn"]);
        _unlockedAnimatronicGames.UnionWith(snapshot.UnlockedAnimatronicGames.Where(id => _sheets.Games.Any(game => game.Id == id)));
        _payoutCollectedThisNight = 0;
        _payoutAiBonusCollected = 0;
        _lawsuitsThisShift = 0;
        _lawsuitFeesThisShift = 0;
    }

    private void DeleteSaveSlot(int slot)
    {
        try
        {
            var path = SaveSlotPath(slot);
            if (File.Exists(path)) File.Delete(path);
            var temporary = path + ".tmp";
            if (File.Exists(temporary)) File.Delete(temporary);
            if (_activeSaveSlot == slot)
            {
                _activeSaveSlot = -1;
                _nightStarted = false;
                _nightOver = false;
                _timer.Stop();
                _showMenu = true;
                _saveMenuOpen = true;
            }
            RefreshSaveSlots();
            _saveMenuMessage = $"Save slot {slot + 1} deleted.";
        }
        catch { _saveMenuMessage = "Could not delete that save file."; }
        _pendingDeleteSlot = -1;
        Invalidate();
    }

    private void BindCapturedKey(Keys key)
    {
        if (_waitingForBindingAction is null) return;
        if (key == Keys.Escape)
        {
            _waitingForBindingAction = null;
            _settingsMessage = "Binding cancelled.";
            Invalidate();
            return;
        }
        if (key is Keys.F11 or Keys.ShiftKey or Keys.ControlKey or Keys.Menu or Keys.LWin or Keys.RWin or Keys.None)
        {
            _settingsMessage = "That key is reserved. Press a different key, or Esc to cancel.";
            Invalidate();
            return;
        }
        if (_controlBindings.Any(pair => pair.Key != _waitingForBindingAction && pair.Value == key))
        {
            _settingsMessage = $"{KeyLabel(key)} is already assigned. Press a different key, or Esc to cancel.";
            Invalidate();
            return;
        }
        _controlBindings[_waitingForBindingAction] = key;
        _waitingForBindingAction = null;
        _settingsMessage = "Control saved. Select another row to change it.";
        SaveControlBindings();
        Invalidate();
    }

    private void DrawPizzeria(Graphics g)
    {
        DrawPrepTabs(g);
        if (_prepScreen == PrepScreen.Builder) DrawBuilder(g);
        else if (_prepScreen == PrepScreen.Blueprint) DrawBlueprint(g);
        else DrawRoster(g);
        DrawPrepFooter(g);
    }

    private void DrawPrepTabs(Graphics g)
    {
        _buildTabRect = new RectangleF(48, 100, 150, 32);
        _blueprintTabRect = new RectangleF(208, 100, 140, 32);
        _rosterTabRect = new RectangleF(358, 100, 190, 32);
        DrawButton(g, _buildTabRect, "CATALOG", "", _prepScreen == PrepScreen.Builder ? _gold : _muted, _prepScreen == PrepScreen.Builder ? Color.FromArgb(47, 35, 19) : _panel, true);
        DrawButton(g, _blueprintTabRect, "BLUEPRINT", "", _prepScreen == PrepScreen.Blueprint ? _gold : _muted, _prepScreen == PrepScreen.Blueprint ? Color.FromArgb(47, 35, 19) : _panel, true);
        DrawButton(g, _rosterTabRect, "ANIMATRONICS", "", _prepScreen == PrepScreen.Roster ? _gold : _muted, _prepScreen == PrepScreen.Roster ? Color.FromArgb(47, 35, 19) : _panel, true);
    }

    private void DrawBuilder(Graphics g)
    {
        DrawText(g, "THE CATALOG", _titleFont, _ink, 48, 141, 700, 34);
        var rating = PizzeriaRating();
        var points = rating.Atmosphere + rating.HealthSafety + rating.Entertainment + rating.BonusRevenue;
        DrawText(g, $"Pizzeria rating {points} · Place purchases to unlock better catalogs. Select a category below.", _bodyFont, _muted, 50, 178, 1120, 24);
        var categories = new[]
        {
            (Id: "dumpster", Name: "DUMPSTER DIVER WEEKLY", Required: 0),
            (Id: "budget-tech", Name: "STAN'S BUDGET TECH", Required: 6),
            (Id: "smiles", Name: "SMILES AND SERVOS, INC.", Required: 15),
            (Id: "rare", Name: "RARE FINDS AUCTION", Required: 30)
        };
        _catalogCategoryRects.Clear();
        for (var i = 0; i < categories.Length; i++)
        {
            var category = categories[i];
            var rect = new RectangleF(48 + i * 296, 207, 284, 32);
            _catalogCategoryRects[category.Id] = rect;
            var unlocked = IsCatalogCategoryUnlocked(category.Id, points);
            var label = unlocked ? category.Name : $"LOCKED · {category.Required} PTS";
            DrawButton(g, rect, label, unlocked ? category.Id == _selectedCatalogCategory ? "✓" : "→" : "×", !unlocked ? _muted : category.Id == _selectedCatalogCategory ? _gold : _teal, category.Id == _selectedCatalogCategory ? Color.FromArgb(47, 35, 19) : _panel, unlocked);
        }
        var selectedCategory = categories.FirstOrDefault(category => category.Id == _selectedCatalogCategory);
        if (selectedCategory.Id is null || points < selectedCategory.Required) _selectedCatalogCategory = "dumpster";
        var items = _sheets.Attractions.Where(attraction => attraction.Category == _selectedCatalogCategory).ToArray();
        _catalogViewportRect = new RectangleF(48, 247, 1174, 351);
        _catalogContentHeight = items.Length * 82;
        _catalogScrollOffset = Math.Clamp(_catalogScrollOffset, 0, Math.Max(0, _catalogContentHeight - (int)_catalogViewportRect.Height));
        _attractionBuyRects.Clear();
        var state = g.Save();
        g.SetClip(_catalogViewportRect);
        for (var i = 0; i < items.Length; i++)
        {
            var attraction = items[i];
            var card = new RectangleF(48, _catalogViewportRect.Y + i * 82 - _catalogScrollOffset, 1174, 74);
            DrawPanel(g, card, _builtAttractions.Contains(attraction.Id) ? Color.FromArgb(20, 34, 29) : _panel);
            DrawAttractionIcon(g, attraction.Style, new RectangleF(card.X + 10, card.Y + 8, 58, 58), catalogImageId: attraction.Id);
            DrawText(g, attraction.Name, _panelTitleFont, _ink, card.X + 82, card.Y + 7, 365, 20);
            DrawText(g, attraction.Description, _smallFont, _muted, card.X + 82, card.Y + 29, 580, 17);
            DrawText(g, $"ATM {attraction.Atmosphere} · H&S {attraction.HealthSafety} · ENT {attraction.Entertainment} · REV {attraction.BonusRevenue} · RISK {attraction.LiabilityRisk}", _smallFont, _teal, card.X + 82, card.Y + 50, 770, 17);
            DrawText(g, $"+${AttractionPayout(attraction):N0} / SHIFT", _smallFont, _gold, card.Right - 330, card.Y + 15, 172, 30, StringAlignment.Center);
            var buy = new RectangleF(card.Right - 146, card.Y + 15, 132, 42);
            _attractionBuyRects[attraction.Id] = buy;
            var canBuy = !_builtAttractions.Contains(attraction.Id) && _funds >= attraction.Cost;
            DrawButton(g, buy, _builtAttractions.Contains(attraction.Id) ? "BUILT" : canBuy ? "BUY" : "NEED CASH", _builtAttractions.Contains(attraction.Id) ? "✓" : canBuy ? $"${attraction.Cost:N0}" : "—", canBuy ? _teal : _muted, Color.FromArgb(13, 28, 28), canBuy);
        }
        g.Restore(state);
        _catalogScrollTrackRect = new RectangleF(1227, _catalogViewportRect.Y, 7, _catalogViewportRect.Height);
        using (var track = new SolidBrush(Color.FromArgb(30, 34, 33))) g.FillRectangle(track, _catalogScrollTrackRect);
        var thumbHeight = _catalogContentHeight <= _catalogViewportRect.Height ? _catalogViewportRect.Height : Math.Max(36, _catalogScrollTrackRect.Height * _catalogViewportRect.Height / _catalogContentHeight);
        var maxScroll = Math.Max(1, _catalogContentHeight - _catalogViewportRect.Height);
        var thumbY = _catalogScrollTrackRect.Y + (_catalogScrollTrackRect.Height - thumbHeight) * _catalogScrollOffset / maxScroll;
        _catalogScrollThumbRect = new RectangleF(_catalogScrollTrackRect.X, thumbY, _catalogScrollTrackRect.Width, thumbHeight);
        using (var thumb = new SolidBrush(Color.FromArgb(142, 121, 113))) g.FillRectangle(thumb, _catalogScrollThumbRect);
        DrawText(g, _catalogMessage, _smallFont, _muted, 50, 604, 850, 18);
    }

    private void DrawBlueprint(Graphics g)
    {
        DrawText(g, "BLUEPRINT MODE", _titleFont, _ink, 48, 141, 700, 34);
        DrawText(g, "Select a purchase, then click an empty tile to place it. Use the red X on a placed item to return it to storage.", _bodyFont, _muted, 50, 178, 1120, 24);
        var floor = new RectangleF(48, 214, 700, 382);
        DrawPanel(g, floor, Color.FromArgb(50, 35, 28));
        DrawText(g, "RESTAURANT FLOORPLAN", _smallFont, _muted, floor.X + 16, floor.Y + 12);
        var stage = new RectangleF(floor.X + 52, floor.Y + 42, 596, 42);
        using (var stageFill = new SolidBrush(Color.FromArgb(128, 25, 31))) g.FillRectangle(stageFill, stage);
        using (var stageEdge = new Pen(Color.FromArgb(238, 189, 59), 2)) g.DrawRectangle(stageEdge, stage.X, stage.Y, stage.Width, stage.Height);
        var stageCrew = _sheets.Animatronics.Where(a => _stageAnimatronics.Contains(a.Id)).ToArray();
        _stageAnimatronicRects.Clear();
        if (stageCrew.Length == 0) DrawText(g, "EMPTY SHOW STAGE", _smallFont, _gold, stage.X, stage.Y + 12, stage.Width, 20, StringAlignment.Center);
        else
        {
            var iconSize = 34f;
            var totalWidth = stageCrew.Length * 52f;
            var firstX = stage.X + (stage.Width - totalWidth) / 2;
            for (var i = 0; i < stageCrew.Length; i++)
            {
                var portraitRect = new RectangleF(firstX + i * 52, stage.Y + 4, iconSize, iconSize);
                DrawPortraitFrame(g, stageCrew[i], portraitRect);
                _stageAnimatronicRects[stageCrew[i].Id] = portraitRect;
            }
        }

        _blueprintGridRect = new RectangleF(floor.X + 24, floor.Y + 100, 640, 224);
        _placedAttractionRemoveRects.Clear();
        const int columns = 8, rows = 4;
        var cellW = _blueprintGridRect.Width / columns;
        var cellH = _blueprintGridRect.Height / rows;
        for (var row = 0; row < rows; row++)
        for (var col = 0; col < columns; col++)
        {
            var cell = new Point(col, row);
            var rect = new RectangleF(_blueprintGridRect.X + col * cellW, _blueprintGridRect.Y + row * cellH, cellW, cellH);
            var placedId = _placedAttractions.FirstOrDefault(item => item.Value == cell).Key;
            var selected = placedId is not null && placedId == _selectedAttraction;
            var tileColor = (row + col) % 2 == 0 ? Color.FromArgb(122, 81, 52) : Color.FromArgb(106, 70, 45);
            using var fill = new SolidBrush(selected ? Color.FromArgb(74, 40, 92) : placedId is null ? tileColor : Color.FromArgb(86, 68, 39));
            using var edge = new Pen(selected ? _gold : Color.FromArgb(174, 116, 69), selected ? 2 : 1);
            g.FillRectangle(fill, rect); g.DrawRectangle(edge, rect.X, rect.Y, rect.Width, rect.Height);
            if (placedId is null) continue;
            var attraction = _sheets.Attractions.First(a => a.Id == placedId);
            DrawAttractionIcon(g, attraction.Style, new RectangleF(rect.X + 2, rect.Y + 2, rect.Width - 4, rect.Height - 4), false, placedId);
            var remove = new RectangleF(rect.Right - 19, rect.Y + 2, 16, 16);
            _placedAttractionRemoveRects[placedId] = remove;
            using (var removeFill = new SolidBrush(Color.FromArgb(235, 107, 25, 34))) g.FillRectangle(removeFill, remove);
            DrawText(g, "X", _smallFont, _ink, remove.X, remove.Y + 1, remove.Width, 14, StringAlignment.Center);
        }
        DrawDiningTable(g, 1, 1, cellW, cellH);
        DrawDiningTable(g, 4, 1, cellW, cellH);
        DrawText(g, "FIXED TABLES · PAPER CUPS AUTO-FIT BOTH · STATION SITS BY THE STAGE", _smallFont, _muted, floor.X + 16, floor.Bottom - 30, floor.Width - 32, 20);
        if (_builtAttractions.Contains("sanitation-station"))
        {
            var station = new RectangleF(stage.Right + 7, stage.Y + 2, 38, 34);
            DrawAttractionIcon(g, "sanitation", station, false, "sanitation-station");
        }

        var side = new RectangleF(770, 214, 462, 382);
        DrawPanel(g, side, Color.FromArgb(13, 22, 22));
        DrawText(g, "PURCHASED ITEMS", _smallFont, _gold, side.X + 16, side.Y + 12);
        _ratingGuideButtonRect = new RectangleF(side.Right - 160, side.Y + 7, 144, 30);
        DrawButton(g, _ratingGuideButtonRect, "RATING GUIDE", "?", _teal, Color.FromArgb(13, 28, 28), true);
        _blueprintInventoryRects.Clear();
        DrawText(g, "UNPLACED PURCHASES · CLICK A FLOOR TILE", _smallFont, _muted, side.X + 16, side.Y + 37, side.Width - 38, 18);
        var purchased = _sheets.Attractions.Where(a => _builtAttractions.Contains(a.Id) && !IsAttractionEffectivelyPlaced(a)).ToArray();
        _blueprintInventoryViewportRect = new RectangleF(side.X + 12, side.Y + 58, side.Width - 30, 210);
        var inventoryContentHeight = purchased.Length * 48;
        _blueprintInventoryScrollOffset = Math.Clamp(_blueprintInventoryScrollOffset, 0, Math.Max(0, inventoryContentHeight - (int)_blueprintInventoryViewportRect.Height));
        var inventoryState = g.Save();
        g.SetClip(_blueprintInventoryViewportRect);
        for (var i = 0; i < purchased.Length; i++)
        {
            var attraction = purchased[i];
            var card = new RectangleF(_blueprintInventoryViewportRect.X, _blueprintInventoryViewportRect.Y + i * 48 - _blueprintInventoryScrollOffset, _blueprintInventoryViewportRect.Width - 8, 42);
            var active = _selectedAttraction == attraction.Id;
            DrawPanel(g, card, active ? Color.FromArgb(38, 52, 43) : _panel);
            DrawAttractionIcon(g, attraction.Style, new RectangleF(card.X + 5, card.Y + 4, 34, 34), catalogImageId: attraction.Id);
            DrawText(g, attraction.Name, _smallFont, _ink, card.X + 48, card.Y + 4, 250, 16);
            DrawText(g, $"+${AttractionPayout(attraction)} / SURVIVED SHIFT", _smallFont, _muted, card.X + 48, card.Y + 21, 260, 15);
            _blueprintInventoryRects[attraction.Id] = card;
        }
        g.Restore(inventoryState);
        _blueprintInventoryScrollTrackRect = new RectangleF(side.Right - 13, _blueprintInventoryViewportRect.Y, 5, _blueprintInventoryViewportRect.Height);
        using (var inventoryTrack = new SolidBrush(Color.FromArgb(30, 34, 33))) g.FillRectangle(inventoryTrack, _blueprintInventoryScrollTrackRect);
        var inventoryThumbHeight = inventoryContentHeight <= _blueprintInventoryViewportRect.Height ? _blueprintInventoryViewportRect.Height : Math.Max(30, _blueprintInventoryScrollTrackRect.Height * _blueprintInventoryViewportRect.Height / inventoryContentHeight);
        var inventoryMax = Math.Max(1, inventoryContentHeight - _blueprintInventoryViewportRect.Height);
        var inventoryThumbY = _blueprintInventoryScrollTrackRect.Y + (_blueprintInventoryScrollTrackRect.Height - inventoryThumbHeight) * _blueprintInventoryScrollOffset / inventoryMax;
        using (var inventoryThumb = new SolidBrush(Color.FromArgb(142, 121, 113))) g.FillRectangle(inventoryThumb, new RectangleF(_blueprintInventoryScrollTrackRect.X, inventoryThumbY, _blueprintInventoryScrollTrackRect.Width, inventoryThumbHeight));

        var rating = PizzeriaRating();
        DrawText(g, "PIZZERIA RATINGS", _smallFont, _gold, side.X + 16, side.Y + 278);
        DrawText(g, $"Atmosphere: {rating.Atmosphere}", _smallFont, _muted, side.X + 16, side.Y + 300, 200, 18);
        DrawText(g, $"Health & Safety: {rating.HealthSafety}", _smallFont, _muted, side.X + 16, side.Y + 322, 215, 18);
        DrawText(g, $"Entertainment: {rating.Entertainment}", _smallFont, _muted, side.X + 234, side.Y + 300, 210, 18);
        DrawText(g, $"Bonus Revenue: {rating.BonusRevenue}", _smallFont, _muted, side.X + 234, side.Y + 322, 210, 18);
        DrawText(g, $"Net Liability Risk: {rating.LiabilityRisk}", _smallFont, rating.LiabilityRisk > 0 ? _red : _teal, side.X + 16, side.Y + 344, 220, 18);
        DrawText(g, $"GUEST SALES PREVIEW: ATM ${rating.Atmosphere * 2}  ·  ENT ${rating.Entertainment * 3} / NIGHT", _smallFont, _muted, side.X + 16, side.Y + 364, side.Width - 32, 16);
        if (_showRatingGuide) DrawRatingGuide(g);
    }

    private void DrawDiningTable(Graphics g, int column, int row, float cellW, float cellH)
    {
        var table = new RectangleF(_blueprintGridRect.X + column * cellW + 4, _blueprintGridRect.Y + row * cellH + 5, cellW * 2 - 8, cellH - 10);
        using var fill = new SolidBrush(Color.FromArgb(123, 77, 45));
        using var edge = new Pen(Color.FromArgb(233, 180, 104), 2);
        g.FillRectangle(fill, table);
        g.DrawRectangle(edge, table.X, table.Y, table.Width, table.Height);
        DrawText(g, "DINING TABLE", _smallFont, _ink, table.X + 1, table.Y + 3, table.Width - 2, 12, StringAlignment.Center);
        if (_builtAttractions.Contains("paper-cups"))
            DrawAttractionIcon(g, "cups", new RectangleF(table.X + 19, table.Y + 14, table.Width - 38, table.Height - 16), false, "paper-cups");
    }

    private static bool IsDiningTableTile(Point cell) => cell.Y == 1 && cell.X is 1 or 2 or 4 or 5;

    private bool IsAttractionEffectivelyPlaced(Attraction attraction) =>
        _placedAttractions.ContainsKey(attraction.Id) ||
        (_builtAttractions.Contains(attraction.Id) && attraction.Id is "paper-cups" or "sanitation-station");

    private (int Atmosphere, int HealthSafety, int Entertainment, int BonusRevenue, int LiabilityRisk) PizzeriaRating()
    {
        var placed = _sheets.Attractions.Where(IsAttractionEffectivelyPlaced).ToArray();
        var performers = _sheets.Animatronics.Where(a => _stageAnimatronics.Contains(a.Id)).ToArray();
        return (
            placed.Sum(a => a.Atmosphere) + performers.Sum(a => a.Atmosphere),
            placed.Sum(a => a.HealthSafety) + performers.Sum(a => a.HealthSafety),
            placed.Sum(a => a.Entertainment) + performers.Sum(a => a.Entertainment),
            placed.Sum(a => a.BonusRevenue) + performers.Sum(a => a.BonusRevenue),
            Math.Max(0, placed.Sum(a => a.LiabilityRisk) - placed.Sum(a => a.HealthSafety) - performers.Sum(a => a.HealthSafety)));
    }

    private static int CatalogRequirement(string category) => category switch
    {
        "dumpster" => 0, "budget-tech" => 6, "smiles" => 15, "rare" => 30, _ => int.MaxValue
    };

    private int AnimatronicCategoryCost(string gameId) =>
        _sheets.Games.FirstOrDefault(game => game.Id.Equals(gameId, StringComparison.OrdinalIgnoreCase))?.AnimatronicUnlockCost ?? int.MaxValue;

    private bool IsCatalogCategoryUnlocked(string category, int? points = null)
    {
        var rating = PizzeriaRating();
        return (points ?? (rating.Atmosphere + rating.HealthSafety + rating.Entertainment + rating.BonusRevenue)) >= CatalogRequirement(category);
    }

    private void DrawRatingGuide(Graphics g)
    {
        using (var shade = new SolidBrush(Color.FromArgb(205, 0, 0, 0))) g.FillRectangle(shade, 0, 0, DesignWidth, DesignHeight);
        var panel = new RectangleF(250, 145, 780, 456);
        DrawPanel(g, panel, Color.FromArgb(22, 17, 23));
        using (var edge = new Pen(_gold, 2)) g.DrawRectangle(edge, panel.X, panel.Y, panel.Width, panel.Height);
        DrawText(g, "HOW TONIGHT'S PAYOUT WORKS", _titleFont, _gold, panel.X + 28, panel.Y + 22, panel.Width - 56, 32);
        var definitions = new[]
        {
            ("BASE RETURN", "Placed item cost ÷ 4, rounded down; paid after surviving."),
            ("ATMOSPHERE", $"New-guest sales add ${PizzeriaRating().Atmosphere * 2:N0} tonight (${2:N0} per point)."),
            ("HEALTH & SAFETY", "Subtracts from liability risk before the lawsuit check."),
            ("ENTERTAINMENT", $"Returning-guest sales add ${PizzeriaRating().Entertainment * 3:N0} tonight (${3:N0} per point)."),
            ("BONUS REVENUE", $"Adds ${PizzeriaRating().BonusRevenue * 10:N0} tonight (${10:N0} per point)."),
            ("LIABILITY RISK", $"After H&S: 3 rolls from 1–50; each roll ≤ risk adds a $100 lawsuit. Current risk: {PizzeriaRating().LiabilityRisk}.")
        };
        for (var i = 0; i < definitions.Length; i++)
        {
            var y = panel.Y + 72 + i * 43;
            DrawText(g, definitions[i].Item1, _smallFont, i == 5 ? _red : _teal, panel.X + 30, y, 180, 20);
            DrawText(g, definitions[i].Item2, _bodyFont, _ink, panel.X + 216, y, 530, 34);
        }
        DrawText(g, $"ESTIMATED TONIGHT: ${NightlyPayout():N0} · LAWSUITS STILL CHARGED IF THE SHIFT FAILS", _smallFont, _gold, panel.X + 28, panel.Bottom - 57, panel.Width - 56, 20, StringAlignment.Center);
        _ratingGuideCloseRect = new RectangleF(panel.Right - 158, panel.Bottom - 34, 130, 26);
        DrawButton(g, _ratingGuideCloseRect, "CLOSE GUIDE", "×", _muted, Color.FromArgb(18, 23, 23), true);
    }

    private void DrawAttractionIcon(Graphics g, string style, RectangleF r, bool framed = true, string? catalogImageId = null)
    {
        var state = g.Save();
        g.TranslateTransform(r.X, r.Y);
        g.ScaleTransform(r.Width / 80f, r.Height / 80f);
        r = new RectangleF(0, 0, 80, 80);
        if (framed) DrawPanel(g, r, Color.FromArgb(8, 13, 13));
        if (catalogImageId is not null && _catalogAttractionArt.TryGetValue(catalogImageId, out var catalogArt))
        {
            g.DrawImage(catalogArt, new RectangleF(3, 3, 74, 74));
            if (framed) using (var border = new Pen(_edge, 1)) g.DrawRectangle(border, 2, 2, 76, 76);
            g.Restore(state);
            return;
        }
        switch (style)
        {
            case "arcade":
                for (var i = 0; i < 3; i++)
                {
                    var machine = new RectangleF(r.X + 7 + i * 19, r.Y + 9, 15, 48);
                    using var fill = new SolidBrush(new[] { _red, _teal, _gold }[i]); g.FillRectangle(fill, machine);
                    using var screen = new SolidBrush(Color.FromArgb(12, 18, 18)); g.FillRectangle(screen, machine.X + 2, machine.Y + 5, machine.Width - 4, 22);
                }
                break;
            case "pit":
                using (var pit = new SolidBrush(Color.FromArgb(62, 86, 117))) g.FillEllipse(pit, r.X + 8, r.Y + 15, r.Width - 16, r.Height - 27);
                for (var i = 0; i < 8; i++)
                {
                    using var ball = new SolidBrush(new[] { _red, _gold, _teal }[i % 3]);
                    g.FillEllipse(ball, r.X + 13 + (i * 17) % (int)(r.Width - 28), r.Y + 25 + (i * 23) % (int)(r.Height - 42), 8, 8);
                }
                break;
            case "paper-pals": case "cups": case "party-set":
                for (var i = 0; i < 3; i++)
                {
                    var color = new[] { _red, _teal, _gold }[i];
                    if (style == "paper-pals")
                    {
                        using var figure = new SolidBrush(color);
                        g.FillEllipse(figure, r.X + 10 + i * 21, r.Y + 12, 16, 16);
                        g.FillRectangle(figure, r.X + 14 + i * 21, r.Y + 28, 8, 31);
                    }
                    else
                    {
                        using var cup = new SolidBrush(color);
                        PointF[] shape = [new(r.X + 8 + i * 22, r.Y + 16), new(r.X + 27 + i * 22, r.Y + 16), new(r.X + 23 + i * 22, r.Y + 59), new(r.X + 12 + i * 22, r.Y + 59)];
                        g.FillPolygon(cup, shape);
                    }
                }
                break;
            case "duck-pond":
                using (var water = new SolidBrush(Color.FromArgb(50, 85, 124))) g.FillEllipse(water, r.X + 7, r.Y + 34, 66, 32);
                for (var i = 0; i < 3; i++)
                {
                    using var duck = new SolidBrush(_gold);
                    g.FillEllipse(duck, r.X + 11 + i * 21, r.Y + 23 + (i % 2) * 9, 19, 15);
                    g.FillEllipse(duck, r.X + 23 + i * 21, r.Y + 18 + (i % 2) * 9, 9, 9);
                }
                break;
            case "lights": case "disco":
                using (var pole = new Pen(_gold, 3)) g.DrawLine(pole, r.X + 40, r.Y + 19, r.X + 40, r.Y + 66);
                for (var i = 0; i < 5; i++)
                {
                    using var lamp = new SolidBrush(new[] { _red, _teal, _gold, _ink, _red }[i]);
                    g.FillEllipse(lamp, r.X + 8 + i * 16, r.Y + 11 + Math.Abs(2 - i) * 5, 12, 12);
                }
                break;
            case "sanitation": case "medical": case "cooling":
                var cabinet = new RectangleF(r.X + 17, r.Y + 9, 46, 62);
                using (var fill = new SolidBrush(Color.FromArgb(69, 94, 81))) g.FillRectangle(fill, cabinet);
                using (var edge = new Pen(_teal, 2)) g.DrawRectangle(edge, cabinet.X, cabinet.Y, cabinet.Width, cabinet.Height);
                DrawLine(g, _ink, 4, cabinet.X + 23, cabinet.Y + 14, cabinet.X + 23, cabinet.Y + 35);
                DrawLine(g, _ink, 4, cabinet.X + 13, cabinet.Y + 24, cabinet.X + 33, cabinet.Y + 24);
                break;
            case "balloon": case "clown": case "gumball": case "cadet": case "puppet":
                using (var body = new SolidBrush(style == "balloon" ? _red : Color.FromArgb(83, 52, 69))) g.FillEllipse(body, r.X + 15, r.Y + 7, 50, 50);
                using (var trim = new Pen(_gold, 3)) g.DrawEllipse(trim, r.X + 15, r.Y + 7, 50, 50);
                DrawText(g, style == "gumball" ? "G" : style == "balloon" ? "★" : "!", _panelTitleFont, _ink, r.X + 15, r.Y + 22, 50, 24, StringAlignment.Center);
                DrawLine(g, _muted, 2, r.X + 40, r.Y + 57, r.X + 40, r.Y + 73);
                break;
            case "rockets": case "tower": case "vortex":
                using (var rail = new Pen(_teal, 4))
                {
                    g.DrawArc(rail, r.X + 8, r.Y + 14, 65, 53, 190, 160);
                    g.DrawLine(rail, r.X + 17, r.Y + 47, r.X + 29, r.Y + 17);
                    g.DrawLine(rail, r.X + 63, r.Y + 47, r.X + 54, r.Y + 17);
                }
                using (var cart = new SolidBrush(_gold)) g.FillEllipse(cart, r.X + 34, r.Y + 21, 15, 12);
                break;
            default:
                using (var axle = new Pen(_gold, 3))
                {
                    g.DrawEllipse(axle, r.X + 12, r.Y + 10, r.Width - 24, r.Height - 20);
                    g.DrawEllipse(axle, r.X + 23, r.Y + 20, r.Width - 46, r.Height - 40);
                    g.DrawLine(axle, r.X + r.Width / 2, r.Y + 7, r.X + r.Width / 2, r.Bottom - 7);
                    g.DrawLine(axle, r.X + 7, r.Y + r.Height / 2, r.Right - 7, r.Y + r.Height / 2);
                }
                break;
        }
        g.Restore(state);
    }

    private void DrawRoster(Graphics g)
    {
        DrawText(g, "ANIMATRONIC FILES", _titleFont, _ink, 48, 141, 530, 34);
        DrawText(g, "Unlock a game's character file once, then buy its animatronics. Hover portraits for details; wheel over owned portraits to change AI.", _bodyFont, _muted, 50, 178, 1172, 24);
        _rosterViewportRect = new RectangleF(48, 282, 1174, 317);
        _rosterGroupHeaderRects.Clear();
        _animatronicCategoryUnlockRects.Clear();
        _rosterCardRects.Clear();
        _rosterAiMinusRect = RectangleF.Empty;
        _rosterAiPlusRect = RectangleF.Empty;

        const int slotsPerGame = 12;
        const float cardW = 88, cardH = 98, cardGap = 5, groupGap = 10, headerH = 27, cardTopGap = 5;
        var fullHeight = 0;
        foreach (var game in _sheets.Games)
        {
            var rowHeight = _unlockedAnimatronicGames.Contains(game.Id) ? cardH + cardTopGap : 76;
            fullHeight += (int)(headerH + groupGap + (_collapsedRosterGroups.Contains(game.Id) ? 0 : rowHeight) + groupGap);
        }
        _rosterContentHeight = fullHeight;
        var maxScroll = Math.Max(0, fullHeight - (int)_rosterViewportRect.Height);
        _rosterScrollOffset = Math.Clamp(_rosterScrollOffset, 0, maxScroll);

        var state = g.Save();
        g.SetClip(_rosterViewportRect);
        var y = _rosterViewportRect.Y - _rosterScrollOffset;
        foreach (var game in _sheets.Games)
        {
            var header = new RectangleF(_rosterViewportRect.X, y, _rosterViewportRect.Width - 18, headerH);
            _rosterGroupHeaderRects[game.Id] = header;
            var collapsed = _collapsedRosterGroups.Contains(game.Id);
            var source = _sources[game.Id];
            var categoryUnlocked = _unlockedAnimatronicGames.Contains(game.Id);
            var categoryCost = AnimatronicCategoryCost(game.Id);
            var performers = _sheets.Animatronics.Where(a => a.GameId.Equals(game.Id, StringComparison.OrdinalIgnoreCase)).ToArray();
            var boughtCount = performers.Count(a => _owned.Contains(a.Id));
            DrawPanel(g, header, collapsed ? Color.FromArgb(23, 18, 25) : Color.FromArgb(42, 27, 43));
            DrawText(g, collapsed ? "+" : "−", _panelTitleFont, _gold, header.X + 10, header.Y + 3, 18, 21, StringAlignment.Center);
            DrawText(g, ShortName(game.Name), _smallFont, _ink, header.X + 38, header.Y + 6, 185, 18);
            var categoryStatus = !source.IsAvailable ? "GAME COPY NOT FOUND" : categoryUnlocked ? $"CATEGORY OPEN · {boughtCount} OWNED" : $"LOCKED · UNLOCK ${categoryCost:N0}";
            DrawText(g, categoryStatus, _smallFont, !source.IsAvailable ? _muted : categoryUnlocked ? _teal : _gold, header.X + 250, header.Y + 6, 380, 18);
            DrawText(g, "12 PORTRAIT SLOTS", _smallFont, _muted, header.Right - 195, header.Y + 6, 180, 18, StringAlignment.Far);
            y += headerH + groupGap;
            if (collapsed) { y += groupGap; continue; }

            if (!categoryUnlocked)
            {
                var unlockPanel = new RectangleF(_rosterViewportRect.X + 19, y, 1095, 66);
                DrawPanel(g, unlockPanel, Color.FromArgb(20, 17, 23));
                DrawText(g, $"{ShortName(game.Name).ToUpperInvariant()} CHARACTER FILE", _smallFont, _ink, unlockPanel.X + 14, unlockPanel.Y + 10, 330, 18);
                DrawText(g, source.IsAvailable ? "One-time category unlock; characters still cost extra." : "Install this game to unlock its character file.", _smallFont, _muted, unlockPanel.X + 14, unlockPanel.Y + 34, 610, 18);
                var unlockButton = new RectangleF(unlockPanel.Right - 250, unlockPanel.Y + 12, 234, 42);
                _animatronicCategoryUnlockRects[game.Id] = unlockButton;
                DrawButton(g, unlockButton, source.IsAvailable ? "UNLOCK CATEGORY" : "SOURCE REQUIRED", $"${categoryCost:N0}", _gold, Color.FromArgb(47, 35, 19), source.IsAvailable && _funds >= categoryCost);
                y += 76;
                continue;
            }

            var cardX = _rosterViewportRect.X + 19;
            for (var slot = 0; slot < slotsPerGame; slot++)
            {
                var card = new RectangleF(cardX + slot * (cardW + cardGap), y, cardW, cardH);
                if (slot >= performers.Length)
                {
                    DrawPanel(g, card, Color.FromArgb(12, 14, 15));
                    DrawText(g, "EMPTY", _smallFont, Color.FromArgb(82, 91, 91), card.X + 5, card.Y + 40, card.Width - 10, 18, StringAlignment.Center);
                    continue;
                }
                var animatronic = performers[slot];
                _rosterCardRects[animatronic.Id] = card;
                if (card.Contains(_mouse)) _rosterHoveredAnimatronic = animatronic.Id;
                DrawRosterPortraitCard(g, animatronic, card, _stageAnimatronics.Contains(animatronic.Id));
            }
            y += cardH + cardTopGap + groupGap;
        }
        g.Restore(state);

        DrawRosterTooltip(g);
        _rosterScrollTrackRect = new RectangleF(1228, _rosterViewportRect.Y, 8, _rosterViewportRect.Height);
        using (var track = new SolidBrush(Color.FromArgb(30, 34, 33))) g.FillRectangle(track, _rosterScrollTrackRect);
        var thumbHeight = _rosterContentHeight <= _rosterViewportRect.Height
            ? _rosterViewportRect.Height
            : Math.Max(40, _rosterScrollTrackRect.Height * _rosterViewportRect.Height / _rosterContentHeight);
        var scrollRange = Math.Max(1, _rosterContentHeight - _rosterViewportRect.Height);
        var thumbY = _rosterScrollTrackRect.Y + (_rosterScrollTrackRect.Height - thumbHeight) * _rosterScrollOffset / scrollRange;
        _rosterScrollThumbRect = new RectangleF(_rosterScrollTrackRect.X, thumbY, _rosterScrollTrackRect.Width, thumbHeight);
        using var thumb = new SolidBrush(Color.FromArgb(142, 121, 113));
        g.FillRectangle(thumb, _rosterScrollThumbRect);
    }

    private void DrawRosterPortraitCard(Graphics g, Animatronic a, RectangleF r, bool staged)
    {
        var bought = _owned.Contains(a.Id);
        var isHovered = r.Contains(_mouse);
        DrawPanel(g, r, bought && staged ? Color.FromArgb(50, 37, 20) : bought ? Color.FromArgb(22, 35, 34) : _panel);
        using (var outline = new Pen(bought && staged ? _gold : isHovered ? _teal : _edge, bought && staged ? 2 : 1))
            g.DrawRectangle(outline, r.X, r.Y, r.Width, r.Height);
        DrawPortraitFrame(g, a, new RectangleF(r.X + 13, r.Y + 5, 62, 62));
        var badge = new RectangleF(r.X + 5, r.Bottom - 25, r.Width - 10, 19);
        using (var fill = new SolidBrush(bought ? staged ? Color.FromArgb(61, 46, 22) : Color.FromArgb(21, 37, 36) : Color.FromArgb(13, 18, 18))) g.FillRectangle(fill, badge);
        DrawText(g, bought ? $"AI {_ai[a.Id]:00}" : $"$ {a.Cost:N0}", _smallFont, bought ? staged ? _gold : _teal : _gold, badge.X + 2, badge.Y + 2, badge.Width - 4, 16, StringAlignment.Center);
    }

    private void DrawRosterTooltip(Graphics g)
    {
        var panel = new RectangleF(48, 209, 1174, 62);
        DrawPanel(g, panel, Color.FromArgb(15, 20, 20));
        if (_rosterHoveredAnimatronic is null)
        {
            _rosterAiMinusRect = RectangleF.Empty; _rosterAiPlusRect = RectangleF.Empty;
            DrawText(g, "HOVER A PORTRAIT FOR ITS RULES AND PIZZERIA RATINGS", _smallFont, _muted, panel.X + 14, panel.Y + 22, panel.Width - 28, 20);
            return;
        }
        var a = _sheets.Animatronics.First(entry => entry.Id == _rosterHoveredAnimatronic);
        DrawText(g, a.Name, _panelTitleFont, _ink, panel.X + 14, panel.Y + 5, 330, 22);
        DrawText(g, $"ATM {a.Atmosphere} · H&S {a.HealthSafety} · ENT {a.Entertainment} · REV {a.BonusRevenue}", _smallFont, _teal, panel.X + 352, panel.Y + 8, panel.Width - 366, 18, StringAlignment.Far);
        DrawText(g, a.Rule, _smallFont, _muted, panel.X + 14, panel.Y + 34, 690, 20);
        if (!_owned.Contains(a.Id))
        {
            _rosterAiMinusRect = RectangleF.Empty; _rosterAiPlusRect = RectangleF.Empty;
            DrawText(g, $"PRICE ${a.Cost:N0} · CLICK TO BUY", _smallFont, _gold, panel.X + 746, panel.Y + 34, 410, 18, StringAlignment.Far);
            return;
        }
        DrawText(g, _stageAnimatronics.Contains(a.Id) ? "ON STAGE" : "OFF STAGE", _smallFont, _stageAnimatronics.Contains(a.Id) ? _gold : _muted, panel.X + 746, panel.Y + 34, 90, 18, StringAlignment.Far);
        DrawText(g, "AI", _smallFont, _muted, panel.X + 850, panel.Y + 34, 22, 18);
        _rosterAiMinusRect = new RectangleF(panel.X + 878, panel.Y + 31, 24, 24);
        _rosterAiPlusRect = new RectangleF(panel.X + 974, panel.Y + 31, 24, 24);
        DrawAiStepButton(g, _rosterAiMinusRect, "−", _ai[a.Id] > 0);
        DrawText(g, _ai[a.Id].ToString("00"), _smallFont, _gold, panel.X + 910, panel.Y + 35, 56, 17, StringAlignment.Center);
        DrawAiStepButton(g, _rosterAiPlusRect, "+", _ai[a.Id] < _sheets.MaximumAiLevel);
    }

    private void DrawPrepFooter(Graphics g)
    {
        _startNightRect = new RectangleF(936, 624, 296, 43);
        DrawButton(g, _startNightRect, "OPEN FOR THE NIGHT", "→", _gold, Color.FromArgb(47, 35, 19), true);
        if (_prepScreen is PrepScreen.Builder or PrepScreen.Blueprint)
        {
            var payout = _payoutPaidDay == _day ? 0 : NightlyPayout();
            var multiplier = StagedAiPayoutMultiplier();
            var aiBonus = (int)Math.Round(payout * multiplier, MidpointRounding.AwayFromZero);
            DrawText(g, $"SHIFT PAYOUT: +${payout:N0}  ·  STAGE AI +{multiplier:P2}  (EST. +${aiBonus:N0})", _smallFont, _teal, 48, 640, 1120);
        }
        else
        {
            var linked = _sources.Values.Count(s => s.IsAvailable);
            if (_prepScreen == PrepScreen.Roster)
            {
                var rating = PizzeriaRating();
                DrawText(g, $"{linked}/6 COPIES  ·  STAGE {_stageAnimatronics.Count}  ·  ATM {rating.Atmosphere}  H&S {rating.HealthSafety}  ENT {rating.Entertainment}  REV {rating.BonusRevenue}  ·  AI PAYOUT +{StagedAiPayoutMultiplier():P2}", _smallFont, _muted, 48, 640, 1160);
            }
            else DrawText(g, $"{linked}/6 GAME COPIES FOUND  ·  SURVIVE TO 6 AM  ·  3 MISSES = NIGHT OVER", _smallFont, _muted, 48, 640, 820);
        }
        DrawText(g, "ESC  BACK TO MENU", _smallFont, Color.FromArgb(106, 115, 110), 48, 694);
        DrawText(g, "F11  WINDOWED / FULLSCREEN", _smallFont, Color.FromArgb(106, 115, 110), 1010, 694, 220, 18, StringAlignment.Far);
    }

    private void DrawPurchaseConfirmation(Graphics g)
    {
        using (var shade = new SolidBrush(Color.FromArgb(202, 0, 0, 0))) g.FillRectangle(shade, 0, 0, DesignWidth, DesignHeight);
        var panel = new RectangleF(405, 257, 470, 206);
        DrawPanel(g, panel, Color.FromArgb(22, 17, 23));
        using (var border = new Pen(_gold, 2)) g.DrawRectangle(border, panel.X, panel.Y, panel.Width, panel.Height);
        var name = "";
        var cost = 0;
        if (_pendingPurchaseKind == PurchaseKind.Animatronic)
        {
            var item = _sheets.Animatronics.FirstOrDefault(a => a.Id == _pendingPurchaseId);
            if (item is not null) { name = item.Name; cost = item.Cost; }
        }
        else if (_pendingPurchaseKind == PurchaseKind.Attraction)
        {
            var item = _sheets.Attractions.FirstOrDefault(a => a.Id == _pendingPurchaseId);
            if (item is not null) { name = item.Name; cost = item.Cost; }
        }
        else if (_pendingPurchaseKind == PurchaseKind.AnimatronicCategory && _pendingPurchaseId is not null)
        {
            var game = _sheets.Games.FirstOrDefault(entry => entry.Id == _pendingPurchaseId);
            if (game is not null) { name = $"{ShortName(game.Name)} CHARACTER FILE"; cost = AnimatronicCategoryCost(game.Id); }
        }
        DrawText(g, "CONFIRM PURCHASE", _panelTitleFont, _gold, panel.X + 24, panel.Y + 22, panel.Width - 48, 24, StringAlignment.Center);
        DrawText(g, name, _cardTitleFont, _ink, panel.X + 25, panel.Y + 66, panel.Width - 50, 25, StringAlignment.Center);
        DrawText(g, $"SPEND ${cost:N0}?  AVAILABLE: ${_funds:N0}", _smallFont, _muted, panel.X + 25, panel.Y + 101, panel.Width - 50, 20, StringAlignment.Center);
        _purchaseCancelRect = new RectangleF(panel.X + 34, panel.Y + 143, 180, 40);
        _purchaseConfirmRect = new RectangleF(panel.Right - 214, panel.Y + 143, 180, 40);
        DrawButton(g, _purchaseCancelRect, "CANCEL", "ESC", _muted, Color.FromArgb(18, 23, 23), true);
        DrawButton(g, _purchaseConfirmRect, "CONFIRM", $"${cost:N0}", _gold, Color.FromArgb(47, 35, 19), _funds >= cost);
    }

    private void ConfirmPendingPurchase()
    {
        if (_pendingPurchaseKind == PurchaseKind.Animatronic && _pendingPurchaseId is not null)
        {
            var item = _sheets.Animatronics.FirstOrDefault(a => a.Id == _pendingPurchaseId);
            if (item is not null && !_owned.Contains(item.Id) && _sources[item.GameId].IsAvailable && _funds >= item.Cost)
            {
                _funds -= item.Cost;
                _owned.Add(item.Id);
                _stageAnimatronics.Add(item.Id);
            }
        }
        else if (_pendingPurchaseKind == PurchaseKind.Attraction && _pendingPurchaseId is not null)
        {
            var item = _sheets.Attractions.FirstOrDefault(a => a.Id == _pendingPurchaseId);
            if (item is not null && !_builtAttractions.Contains(item.Id) && _funds >= item.Cost)
            {
                _funds -= item.Cost;
                _builtAttractions.Add(item.Id);
            }
        }
        else if (_pendingPurchaseKind == PurchaseKind.AnimatronicCategory && _pendingPurchaseId is not null)
        {
            var gameId = _pendingPurchaseId;
            var cost = AnimatronicCategoryCost(gameId);
            if (_sources.TryGetValue(gameId, out var source) && source.IsAvailable && !_unlockedAnimatronicGames.Contains(gameId) && _funds >= cost)
            {
                _funds -= cost;
                _unlockedAnimatronicGames.Add(gameId);
            }
        }
        _pendingPurchaseKind = PurchaseKind.None;
        _pendingPurchaseId = null;
        SaveActiveSlot();
        Invalidate();
    }

    private void CancelPendingPurchase()
    {
        _pendingPurchaseKind = PurchaseKind.None;
        _pendingPurchaseId = null;
        Invalidate();
    }

    private void DrawAnimatronicCard(Graphics g, Animatronic a, RectangleF r)
    {
        var source = _sources[a.GameId];
        var bought = _owned.Contains(a.Id);
        var active = bought && _ai[a.Id] > 0;
        DrawPanel(g, r, active ? Color.FromArgb(20, 29, 29) : _panel);
        DrawPortraitFrame(g, a, new RectangleF(r.X + 18, r.Y + 20, 72, 72));
        DrawText(g, ShortName(source.Game.Name).ToUpperInvariant(), _smallFont, _teal, r.X + 108, r.Y + 22);
        DrawText(g, source.IsAvailable ? "SOURCE CONNECTED" : "GAME COPY NOT FOUND", _smallFont, source.IsAvailable ? Color.FromArgb(123, 186, 143) : Color.FromArgb(210, 125, 105), r.X + 108, r.Y + 40);
        DrawText(g, a.Name, _cardTitleFont, _ink, r.X + 108, r.Y + 58, r.Width - 126, 28);
        DrawText(g, a.Rule, _bodyFont, _muted, r.X + 20, r.Y + 107, r.Width - 40, 25);
        var action = new RectangleF(r.X + 18, r.Bottom - 48, r.Width - 36, 31);
        if (!bought)
        {
            var canBuy = source.IsAvailable && _funds >= a.Cost;
            DrawButton(g, action, canBuy ? "BUY FOR" : source.IsAvailable ? "NEED MORE FUNDS" : "INSTALL SOURCE GAME", canBuy ? $"${a.Cost:N0}" : "—", canBuy ? _teal : _muted, Color.FromArgb(13, 28, 28), canBuy);
        }
        else
        {
            DrawText(g, active ? "AI LEVEL" : "SLEEPING", _smallFont, active ? _gold : _muted, r.X + 20, r.Bottom - 41);
            var stageToggle = new RectangleF(r.X + 98, r.Bottom - 48, 148, 31);
            DrawStageToggleButton(g, stageToggle, _stageAnimatronics.Contains(a.Id));
            var minus = new RectangleF(r.Right - 114, r.Bottom - 48, 30, 30);
            var plus = new RectangleF(r.Right - 42, r.Bottom - 48, 30, 30);
            DrawAiStepButton(g, minus, "-", _ai[a.Id] > 0);
            DrawText(g, _ai[a.Id].ToString("00"), _aiFont, _gold, r.Right - 80, r.Bottom - 44, 36, 25, StringAlignment.Center);
            DrawAiStepButton(g, plus, "+", _ai[a.Id] < _sheets.MaximumAiLevel);
        }
    }

    private void DrawNight(Graphics g)
    {
        var left = new RectangleF(48, 112, 776, 546);
        DrawOffice(g, left);
        var right = new RectangleF(842, 112, 390, 546);
        DrawPanel(g, right, Color.FromArgb(14, 19, 20));
        _nightActionRects.Clear();
        DrawText(g, "OFFICE MONITOR", _smallFont, _teal, right.X + 22, right.Y + 19);
        DrawText(g, _message, _messageFont, _ink, right.X + 22, right.Y + 48, right.Width - 44, 75);
        DrawLine(g, _edge, 1, right.X + 20, right.Y + 132, right.Right - 20, right.Y + 132);
        DrawText(g, "SYSTEMS", _smallFont, _muted, right.X + 22, right.Y + 147);
        DrawMeter(g, right.X + 22, right.Y + 169, right.Width - 44, "VENTILATION", _vent, 100, _vent > 25 ? _teal : _red);
        DrawMeter(g, right.X + 22, right.Y + 218, right.Width - 44, "DOOR POWER", _doorPower, 100, _doorPower > 25 ? _gold : _red);
        DrawText(g, "MONITOR 1%/s · CAMS +1%/s · EACH LIGHT 1%/s", _smallFont, _muted, right.X + 22, right.Y + 251, right.Width - 44, 15);
        _monitorToggleRect = new RectangleF(right.X + 22, right.Y + 272, right.Width - 44, 37);
        DrawButton(g, _monitorToggleRect, _monitorOpen ? "LOWER MONITOR · CANCEL TASK" : "RAISE MONITOR · TASKS / CAMERAS", KeyLabel(_controlBindings["monitor"]), _gold, Color.FromArgb(29, 26, 18), true);
        DrawNightAction(g, right, "left-door", KeyLabel(_controlBindings["left-door"]), _leftDoorClosed ? "OPEN LEFT" : _doorPower > 0 ? "CLOSE LEFT" : "NO POWER", 0);
        DrawNightAction(g, right, "right-door", KeyLabel(_controlBindings["right-door"]), _rightDoorClosed ? "OPEN RIGHT" : _doorPower > 0 ? "CLOSE RIGHT" : "NO POWER", 1);
        DrawNightAction(g, right, "mask", KeyLabel(_controlBindings["mask"]), "PUT ON MASK", 2);
        DrawNightAction(g, right, "lure", KeyLabel(_controlBindings["lure"]), "AUDIO LURE", 3);
        DrawNightAction(g, right, "listen", KeyLabel(_controlBindings["listen"]), _listening ? "HEARD · CLOSE LEFT" : "LISTEN", 4);
        DrawNightAction(g, right, "scan", KeyLabel(_controlBindings["scan"]), "VENT SCAN", 5);
        DrawNightAction(g, right, "vent", KeyLabel(_controlBindings["vent"]), "RESET VENTS", 6);
        DrawText(g, "FOLLOW THE WARNING. 3 MISSES = GAME OVER.", _smallFont, _muted, right.X + 22, right.Bottom - 34, right.Width - 42);
        DrawText(g, "ESC  RETURN TO PREP", _smallFont, Color.FromArgb(106, 115, 110), 48, 694);
    }

    private void DrawOffice(Graphics g, RectangleF r)
    {
        DrawPanel(g, r, Color.FromArgb(7, 12, 13));
        using var room = new LinearGradientBrush(r, Color.FromArgb(40, 33, 35), Color.FromArgb(7, 6, 8), 90f);
        g.FillRectangle(room, r.X + 1, r.Y + 1, r.Width - 2, r.Height - 2);
        using var shadow = new SolidBrush(Color.FromArgb(135, 3, 5, 6));
        g.FillRectangle(shadow, r.X + 101, r.Y + 65, 98, 330);
        g.FillRectangle(shadow, r.Right - 199, r.Y + 65, 98, 330);
        var poster = new RectangleF(r.X + 224, r.Y + 78, 62, 62);
        using (var posterFrame = new SolidBrush(Color.FromArgb(125, 27, 32))) g.FillRectangle(posterFrame, poster.X - 4, poster.Y - 4, poster.Width + 8, poster.Height + 22);
        DrawImageFrame(g, "fnaf1", poster);
        DrawText(g, "CELEBRATE!", _smallFont, _gold, poster.X - 2, poster.Bottom + 1, poster.Width + 4, 16, StringAlignment.Center);
        using var desk = new SolidBrush(Color.FromArgb(23, 30, 29));
        g.FillPolygon(desk, [new PointF(r.X + 45, r.Y + 398), new PointF(r.Right - 45, r.Y + 398), new PointF(r.Right - 10, r.Bottom - 8), new PointF(r.X + 10, r.Bottom - 8)]);
        DrawDeskFan(g, r.X + r.Width / 2 - 22, r.Y + 426);
        DrawDeskCup(g, r.X + r.Width / 2 + 76, r.Y + 433);
        using (var paper = new SolidBrush(Color.FromArgb(174, 161, 132))) g.FillRectangle(paper, r.X + r.Width / 2 - 100, r.Y + 446, 51, 28);
        DrawText(g, "PARTY", _smallFont, Color.FromArgb(46, 32, 30), r.X + r.Width / 2 - 97, r.Y + 451, 45, 16, StringAlignment.Center);
        DrawText(g, "CAM FEED  /  EAST HALL", _smallFont, _muted, r.X + 22, r.Y + 20);
        DrawText(g, "FREDDY FAZBEAR'S PIZZERIA", _smallFont, Color.FromArgb(105, 122, 112), r.X + 22, r.Bottom - 35);
        DrawText(g, $"LEFT {(_leftDoorClosed ? "CLOSED" : "OPEN")}  ·  RIGHT {(_rightDoorClosed ? "CLOSED" : "OPEN")}", _smallFont, (_leftDoorClosed || _rightDoorClosed) ? _gold : _muted, r.Right - 390, r.Y + 20, 364, 20, StringAlignment.Far);
        DrawOfficeDoor(g, new RectangleF(r.X + 101, r.Y + 65, 98, 330), _leftDoorClosed);
        DrawOfficeDoor(g, new RectangleF(r.Right - 199, r.Y + 65, 98, 330), _rightDoorClosed);
        DrawDoorWarning(g, r, "left");
        DrawDoorWarning(g, r, "right");
        _officeLightRects.Clear();
        DrawDoorLight(g, r, "left");
        DrawDoorLight(g, r, "right");
        DrawOfficeVent(g, r, "left");
        DrawOfficeVent(g, r, "right");
        DrawVentWarning(g, r);
        var threat = CurrentThreat();
        var intruderVisible = _officeIntruderId is not null && DateTime.UtcNow < _officeIntruderUntilUtc;
        if (intruderVisible) DrawOfficeIntruder(g, r, _officeIntruderId!);
        if (threat is null && !intruderVisible)
            DrawText(g, "NO MOVEMENT", _noMovementFont, Color.FromArgb(103, 136, 121), r.X + 260, r.Y + 222, 250, 28, StringAlignment.Center);
        var bar = new RectangleF(r.X + 35, r.Bottom - 83, r.Width - 70, 7);
        using var track = new SolidBrush(Color.FromArgb(45, 56, 53)); g.FillRectangle(track, bar);
        using var progress = new SolidBrush(_teal); g.FillRectangle(progress, bar.X, bar.Y, Math.Max(0, bar.Width * _seconds / NightSteps), bar.Height);
        DrawText(g, "12:00 AM", _smallFont, _gold, r.X + 35, r.Bottom - 68);
        DrawText(g, "6:00 AM", _smallFont, _gold, r.Right - 98, r.Bottom - 68);
        if (_monitorOpen) DrawOfficeMonitor(g, r);
    }

    private void DrawOfficeMonitor(Graphics g, RectangleF office)
    {
        var panel = office;
        using var casing = new SolidBrush(Color.FromArgb(255, 5, 7, 8));
        using var edge = new Pen(Color.FromArgb(255, 165, 168, 145), 3);
        g.FillRectangle(casing, panel); g.DrawRectangle(edge, panel.X + 1, panel.Y + 1, panel.Width - 2, panel.Height - 2);
        DrawText(g, "FAZBEAR ENTERTAINMENT  /  TERMINAL", _smallFont, _gold, panel.X + 18, panel.Y + 12, 300, 18);
        _officeMonitorTabRects.Clear();
        var tabs = new[] { (Id: "tasks", Label: "DAILY TASKS"), (Id: "cameras", Label: "CAMERAS") };
        for (var i = 0; i < tabs.Length; i++)
        {
            var tab = new RectangleF(panel.Right - 258 + i * 122, panel.Y + 9, 116, 30);
            _officeMonitorTabRects[tabs[i].Id] = tab;
            var selected = _cameraMonitorMode == (tabs[i].Id == "cameras");
            using var fill = new SolidBrush(selected ? Color.FromArgb(47, 35, 19) : Color.FromArgb(18, 23, 23));
            using var border = new Pen(selected ? _gold : _edge);
            g.FillRectangle(fill, tab); g.DrawRectangle(border, tab.X, tab.Y, tab.Width, tab.Height);
            DrawText(g, tabs[i].Label, _smallFont, selected ? _gold : _muted, tab.X, tab.Y + 8, tab.Width, 18, StringAlignment.Center);
        }
        DrawLine(g, _edge, 1, panel.X + 14, panel.Y + 49, panel.Right - 14, panel.Y + 49);
        _officeTaskRects.Clear();
        _cameraSelectRects.Clear();
        if (_cameraMonitorMode)
        {
            DrawCameraMonitor(g, panel);
        }
        else
        {
            DrawText(g, $"DAILY TASKS  {_completedOfficeTasks.Count}/11", _bodyFont, _teal, panel.X + 18, panel.Y + 60, panel.Width - 36, 20);
            const float cardWidth = 352, cardHeight = 48, gapX = 18, gapY = 12;
            for (var i = 0; i < OfficeTasks.Length; i++)
            {
                var task = OfficeTasks[i];
                var column = i % 2;
                var row = i / 2;
                var rect = new RectangleF(panel.X + 18 + column * (cardWidth + gapX), panel.Y + 94 + row * (cardHeight + gapY), cardWidth, cardHeight);
                _officeTaskRects[task.Id] = rect;
                var done = _completedOfficeTasks.Contains(task.Id);
                var active = _activeOfficeTaskId == task.Id;
                var fillColor = done ? Color.FromArgb(26, 49, 38) : active ? Color.FromArgb(52, 42, 20) : Color.FromArgb(19, 25, 25);
                using var fill = new SolidBrush(fillColor);
                using var border = new Pen(done ? _teal : active ? _gold : _edge, active ? 2 : 1);
                g.FillRectangle(fill, rect); g.DrawRectangle(border, rect.X, rect.Y, rect.Width, rect.Height);
                var label = done ? "✓  " + task.Name : active ? $"WORKING  {_officeTaskSecondsRemaining}s" : task.Name;
                DrawText(g, label, _smallFont, done ? _teal : active ? _gold : _ink, rect.X + 13, rect.Y + 15, rect.Width - 26, 18);
            }
            var footer = _activeOfficeTaskId is not null ? "TASK IN PROGRESS · PRINTER NOISE MAY DRAW THEM CLOSER" : "SELECT A TASK · ADVERTISING PRINTS ARE LOUD";
            DrawText(g, footer, _smallFont, _activeOfficeTaskId is not null ? _gold : _muted, panel.X + 18, panel.Bottom - 29, panel.Width - 36, 16);
        }
    }

    private void DrawCameraMonitor(Graphics g, RectangleF panel)
    {
        var feed = new RectangleF(panel.X + 18, panel.Y + 62, 474, panel.Height - 80);
        using (var fill = new SolidBrush(Color.FromArgb(18, 25, 24))) g.FillRectangle(fill, feed);
        using (var border = new Pen(_edge)) g.DrawRectangle(border, feed.X, feed.Y, feed.Width, feed.Height);
        var camera = FnafCameras.FirstOrDefault(c => c.Id == _selectedCamera);
        DrawText(g, $"CAM {camera.Id}  /  {camera.Name}", _smallFont, _gold, feed.X + 14, feed.Y + 12, feed.Width - 28, 18);
        using (var room = new LinearGradientBrush(new RectangleF(feed.X + 12, feed.Y + 42, feed.Width - 24, 234), Color.FromArgb(31, 41, 39), Color.FromArgb(8, 12, 12), 90f))
            g.FillRectangle(room, feed.X + 12, feed.Y + 42, feed.Width - 24, 234);
        DrawLine(g, Color.FromArgb(85, 110, 103), 1, feed.X + 24, feed.Y + 204, feed.Right - 24, feed.Y + 204);
        DrawLine(g, Color.FromArgb(85, 110, 103), 1, feed.X + 34, feed.Y + 258, feed.Right - 34, feed.Y + 258);
        var occupants = _animatronicCameras.Where(x => x.Value == camera.Id)
            .Select(x => _sheets.Animatronics.FirstOrDefault(a => a.Id == x.Key)).Where(a => a is not null).ToArray();
        if (occupants.Length == 0) DrawText(g, "NO MOVEMENT", _bodyFont, _muted, feed.X + 28, feed.Y + 140, feed.Width - 56, 25, StringAlignment.Center);
        else
        {
            var portraitWidth = Math.Min(112, (feed.Width - 40f) / occupants.Length);
            for (var i = 0; i < occupants.Length; i++)
            {
                var portrait = new RectangleF(feed.X + 20 + i * (portraitWidth + 12), feed.Y + 66, portraitWidth, 132);
                DrawPortraitFrame(g, occupants[i]!, portrait);
                DrawText(g, occupants[i]!.Name.ToUpperInvariant(), _smallFont, _ink, portrait.X - 2, portrait.Bottom + 8, portrait.Width + 4, 28, StringAlignment.Center);
            }
        }
        DrawText(g, "CAMERA MAP", _smallFont, _teal, feed.X + 12, feed.Bottom - 28, feed.Width - 24, 18);
        DrawText(g, "AI-weighted movement checks: 3s Freddy / 5s others.", _smallFont, _muted, feed.X + 12, feed.Bottom - 14, feed.Width - 24, 16);

        var map = new RectangleF(feed.Right + 12, feed.Y, panel.Right - feed.Right - 30, feed.Height);
        using (var fill = new SolidBrush(Color.FromArgb(12, 17, 17))) g.FillRectangle(fill, map);
        using (var border = new Pen(_edge)) g.DrawRectangle(border, map.X, map.Y, map.Width, map.Height);
        var placements = new Dictionary<string, PointF>
        {
            ["1A"] = new(.5f, .08f), ["1B"] = new(.5f, .37f), ["1C"] = new(.14f, .1f),
            ["2A"] = new(.12f, .48f), ["2B"] = new(.12f, .68f), ["3"] = new(.12f, .31f),
            ["4A"] = new(.88f, .48f), ["4B"] = new(.88f, .68f), ["5"] = new(.5f, .22f),
            ["6"] = new(.88f, .1f), ["7"] = new(.88f, .31f)
        };
        PointF CenterOf(string id) => new(map.X + map.Width * placements[id].X, map.Y + map.Height * placements[id].Y + 17);
        var links = new[] { ("1A", "5"), ("5", "1B"), ("1A", "1C"), ("1B", "2A"), ("2A", "2B"), ("1B", "7"), ("7", "6"), ("1B", "4A"), ("4A", "4B") };
        using (var corridor = new Pen(Color.FromArgb(85, 95, 83, 65), 3))
            foreach (var (from, to) in links) g.DrawLine(corridor, CenterOf(from), CenterOf(to));
        var nodeW = Math.Min(82, map.Width * .43f); const float nodeH = 35;
        foreach (var (id, point) in placements)
        {
            var node = new RectangleF(map.X + map.Width * point.X - nodeW / 2, map.Y + map.Height * point.Y, nodeW, nodeH);
            _cameraSelectRects[id] = node;
            var selected = id == _selectedCamera;
            var occupied = _animatronicCameras.Values.Contains(id, StringComparer.OrdinalIgnoreCase);
            using var nodeFill = new SolidBrush(selected ? Color.FromArgb(53, 39, 21) : Color.FromArgb(24, 31, 30));
            using var nodeEdge = new Pen(occupied ? _red : selected ? _gold : _edge, selected || occupied ? 2 : 1);
            g.FillRectangle(nodeFill, node); g.DrawRectangle(nodeEdge, node.X, node.Y, node.Width, node.Height);
            DrawText(g, $"CAM {id}", _smallFont, occupied ? _red : selected ? _gold : _ink, node.X, node.Y + 9, node.Width, 17, StringAlignment.Center);
        }
        var office = new RectangleF(map.X + map.Width * .28f, map.Bottom - 48, map.Width * .44f, 32);
        using var officeFill = new SolidBrush(Color.FromArgb(61, 28, 26)); g.FillRectangle(officeFill, office);
        DrawText(g, "OFFICE", _smallFont, _red, office.X, office.Y + 8, office.Width, 17, StringAlignment.Center);
    }

    private void DrawOfficeIntruder(Graphics g, RectangleF office, string id)
    {
        if (!_portraitArt.TryGetValue(id, out var portrait)) return;
        var t = Environment.TickCount64 / 90 % 2 == 0;
        var frame = new RectangleF(office.X + office.Width * .39f, office.Y + 112, office.Width * .22f, 250);
        using var glow = new SolidBrush(Color.FromArgb(t ? 145 : 85, 166, 18, 24));
        g.FillRectangle(glow, frame.X - 16, frame.Y - 16, frame.Width + 32, frame.Height + 32);
        g.DrawImage(portrait, frame);
        using var edge = new Pen(Color.FromArgb(235, 187, 33, 39), 3);
        g.DrawRectangle(edge, frame.X, frame.Y, frame.Width, frame.Height);
    }

    private void DrawJumpscare(Graphics g)
    {
        var flash = Environment.TickCount64 / 85 % 2 == 0;
        g.Clear(flash ? Color.FromArgb(155, 9, 12) : Color.FromArgb(3, 3, 4));
        using var shade = new SolidBrush(Color.FromArgb(105, 0, 0, 0));
        g.FillRectangle(shade, 0, 0, DesignWidth, DesignHeight);
        if (_jumpscareAnimatronicId is not null && _portraitArt.TryGetValue(_jumpscareAnimatronicId, out var portrait))
        {
            var shake = (int)(Environment.TickCount64 / 35 % 19) - 9;
            var target = new RectangleF(185 + shake, 35, 910 - Math.Abs(shake) * 2, 630);
            g.DrawImage(portrait, target);
            using var edge = new Pen(Color.FromArgb(flash ? 230 : 110, 190, 20, 28), flash ? 12 : 5);
            g.DrawRectangle(edge, target.X, target.Y, target.Width, target.Height);
        }
    }

    private void DrawOfficeDoor(Graphics g, RectangleF door, bool closed)
    {
        if (!closed) return;
        using var doorFill = new SolidBrush(Color.FromArgb(190, 62, 66, 57));
        using var doorEdge = new Pen(Color.FromArgb(205, _gold), 2);
        g.FillRectangle(doorFill, door); g.DrawRectangle(doorEdge, door.X, door.Y, door.Width, door.Height);
        for (var y = door.Y + 28; y < door.Bottom - 8; y += 34) DrawLine(g, Color.FromArgb(105, 209, 182, 121), 1, door.X + 8, y, door.Right - 8, y);
    }

    private void DrawDoorLight(Graphics g, RectangleF office, string side)
    {
        var left = side == "left";
        var door = new RectangleF(left ? office.X + 101 : office.Right - 199, office.Y + 65, 98, 330);
        var on = left ? _leftLightOn : _rightLightOn;
        if (on)
        {
            using var glow = new SolidBrush(Color.FromArgb(72, 250, 211, 123));
            g.FillRectangle(glow, door);
            var terminal = left ? "2B" : "4B";
            var threat = _sheets.Animatronics.FirstOrDefault(a => _animatronicCameras.TryGetValue(a.Id, out var camera) && camera == terminal);
            if (threat is not null && _portraitArt.TryGetValue(threat.Id, out var portrait))
            {
                var figure = new RectangleF(door.X + 18, door.Y + 70, door.Width - 36, 185);
                g.DrawImage(portrait, figure);
                using var frame = new Pen(Color.FromArgb(190, _gold), 2);
                g.DrawRectangle(frame, figure.X, figure.Y, figure.Width, figure.Height);
                DrawText(g, threat.Name.ToUpperInvariant(), _smallFont, _gold, door.X - 24, figure.Bottom + 6, door.Width + 48, 24, StringAlignment.Center);
            }
        }
        var button = new RectangleF(left ? door.Right + 5 : door.X - 68, door.Bottom - 47, 63, 34);
        _officeLightRects[side + "-light"] = button;
        using var fill = new SolidBrush(on ? Color.FromArgb(77, 59, 22) : Color.FromArgb(19, 25, 25));
        using var edge = new Pen(on ? _gold : _edge, on ? 2 : 1);
        g.FillRectangle(fill, button); g.DrawRectangle(edge, button.X, button.Y, button.Width, button.Height);
        DrawText(g, on ? "LIT" : "LIGHT", _smallFont, on ? _gold : _muted, button.X, button.Y + 9, button.Width, 18, StringAlignment.Center);
    }

    private void DrawDeskFan(Graphics g, float x, float y)
    {
        using var frame = new Pen(Color.FromArgb(188, 191, 178), 2);
        using var blade = new SolidBrush(Color.FromArgb(110, 150, 161, 151));
        g.DrawEllipse(frame, x, y, 44, 44);
        for (var i = 0; i < 4; i++)
        {
            var angle = i * Math.PI / 2;
            var cx = x + 22; var cy = y + 22;
            var bx = cx + (float)Math.Cos(angle) * 12; var by = cy + (float)Math.Sin(angle) * 12;
            g.FillEllipse(blade, bx - 5, by - 5, 10, 10);
        }
        g.FillEllipse(Brushes.Silver, x + 18, y + 18, 8, 8);
        DrawLine(g, Color.FromArgb(151, 157, 146), 2, x + 22, y + 44, x + 22, y + 56);
        DrawLine(g, Color.FromArgb(151, 157, 146), 3, x + 10, y + 57, x + 34, y + 57);
    }

    private void DrawDeskCup(Graphics g, float x, float y)
    {
        using var cup = new SolidBrush(Color.FromArgb(225, 218, 196));
        using var stripe = new SolidBrush(Color.FromArgb(184, 33, 37));
        g.FillPolygon(cup, [new PointF(x + 3, y), new PointF(x + 25, y), new PointF(x + 21, y + 29), new PointF(x + 7, y + 29)]);
        g.FillRectangle(stripe, x + 5, y + 9, 18, 5);
        g.FillRectangle(stripe, x + 7, y + 21, 15, 4);
        DrawLine(g, Color.FromArgb(226, 206, 110), 2, x + 15, y + 1, x + 23, y - 9);
    }

    private void DrawDoorWarning(Graphics g, RectangleF office, string side)
    {
        var threatPending = _sheets.Animatronics.Any(a => DoorSideFor(a) == side && _pendingUntil.TryGetValue(a.Id, out var until) && until > _seconds && !_resolved.Contains(a.Id));
        if (!threatPending) return;
        var centerX = side == "left" ? office.X + 150 : office.Right - 150;
        var y = office.Y + 46;
        var bright = (Environment.TickCount64 / 300 % 2) == 0;
        using var fill = new SolidBrush(Color.FromArgb(bright ? 255 : 110, 224, 45, 38));
        using var edge = new Pen(Color.FromArgb(245, 255, 193, 163), 1);
        PointF[] triangle = [new(centerX, y), new(centerX - 15, y + 25), new(centerX + 15, y + 25)];
        g.FillPolygon(fill, triangle); g.DrawPolygon(edge, triangle);
        DrawText(g, "!", _keyFont, Color.White, centerX - 6, y + 6, 12, 16, StringAlignment.Center);
    }

    private void DrawOfficeVent(Graphics g, RectangleF office, string side)
    {
        var ventX = side == "left" ? office.X + office.Width * .34f - 42 : office.X + office.Width * .66f - 42;
        var vent = new RectangleF(ventX, office.Y + 34, 84, 26);
        using var fill = new SolidBrush(Color.FromArgb(25, 34, 33));
        using var edge = new Pen(Color.FromArgb(101, 118, 108), 1);
        g.FillRectangle(fill, vent);
        g.DrawRectangle(edge, vent.X, vent.Y, vent.Width, vent.Height);
        for (var i = 0; i < 5; i++)
            DrawLine(g, Color.FromArgb(111, 129, 117), 1, vent.X + 8, vent.Y + 5 + i * 4, vent.Right - 8, vent.Y + 5 + i * 4);
        DrawText(g, side == "left" ? "LEFT VENT" : "RIGHT VENT", _smallFont, _muted, vent.X - 10, vent.Y - 18, vent.Width + 20, 16, StringAlignment.Center);
    }

    private void DrawVentWarning(Graphics g, RectangleF office)
    {
        var threatPending = _sheets.Animatronics.Any(a => a.Action == "scan" && _pendingUntil.TryGetValue(a.Id, out var until) && until > _seconds && !_resolved.Contains(a.Id));
        if (!threatPending) return;
        var centerX = _ventThreatSide == "right" ? office.X + office.Width * .66f : office.X + office.Width * .34f;
        var y = office.Y + 64;
        var bright = (Environment.TickCount64 / 300 % 2) == 0;
        using var fill = new SolidBrush(Color.FromArgb(bright ? 255 : 110, 224, 45, 38));
        using var edge = new Pen(Color.FromArgb(245, 255, 193, 163), 1);
        PointF[] triangle = [new(centerX, y), new(centerX - 15, y + 25), new(centerX + 15, y + 25)];
        g.FillPolygon(fill, triangle); g.DrawPolygon(edge, triangle);
        DrawText(g, "!", _keyFont, Color.White, centerX - 6, y + 6, 12, 16, StringAlignment.Center);
    }

    private void DrawNightAction(Graphics g, RectangleF parent, string action, string key, string label, int index)
    {
        var col = index % 2;
        var row = index / 2;
        var r = new RectangleF(parent.X + 20 + col * 174, parent.Y + 326 + row * 44, 162, 36);
        _nightActionRects[action] = r;
        var enabled = ActionAvailable(action);
        var hover = r.Contains(_mouse);
        using var fill = new SolidBrush(!enabled ? Color.FromArgb(19, 24, 24) : hover ? Color.FromArgb(34, 49, 46) : Color.FromArgb(22, 31, 30));
        using var edge = new Pen(enabled ? Color.FromArgb(69, 105, 95) : Color.FromArgb(41, 49, 48));
        g.FillRectangle(fill, r); g.DrawRectangle(edge, r.X, r.Y, r.Width, r.Height);
        DrawText(g, key, _keyFont, enabled ? _gold : Color.FromArgb(86, 94, 91), r.X + 12, r.Y + 9, 24, 24, StringAlignment.Center);
        DrawText(g, label, _smallFont, enabled ? _ink : Color.FromArgb(93, 101, 98), r.X + 40, r.Y + 14, r.Width - 46, 20);
    }

    private void DrawResult(Graphics g)
    {
        using var shade = new SolidBrush(Color.FromArgb(225, 4, 7, 8)); g.FillRectangle(shade, 0, 88, DesignWidth, 592);
        DrawText(g, _dayResetByDeath ? "DAY RESTARTED" : _nightWon ? "THE SHIFT IS OVER" : "SHIFT ENDED", _smallFont, _teal, 0, 175, DesignWidth, 24, StringAlignment.Center);
        DrawText(g, _dayResetByDeath ? "BACK TO THE START" : _nightWon ? "TASKS COMPLETE" : "OFFICE BREACHED", _titleFont, _ink, 0, 216, DesignWidth, 46, StringAlignment.Center);
        var payoutMessage = _dayResetByDeath
            ? "Caught on the third strike. Today's purchases were undone; day-start funds and inventory restored."
            : _nightWon
                ? $"Pizzeria payout +${_payoutCollectedThisNight:N0} (AI +${_payoutAiBonusCollected:N0}; {_strikes} strikes, −{Math.Min(100, _strikes * 25)}%)."
                : "Shift failed. No pizzeria payout.";
        DrawText(g, payoutMessage, _bodyFont, _muted, 0, 279, DesignWidth, 34, StringAlignment.Center);
        var lawsuitMessage = _lawsuitsThisShift == 0 ? "No lawsuits filed this day." : $"{_lawsuitsThisShift} LAWSUIT{(_lawsuitsThisShift == 1 ? "" : "S")}  ·  SETTLED FOR ${_lawsuitFeesThisShift:N0}";
        DrawText(g, lawsuitMessage, _smallFont, _lawsuitsThisShift > 0 ? _red : _teal, 0, 310, DesignWidth, 20, StringAlignment.Center);
        DrawText(g, $"NIGHT SCORE   {_score:N0}     ·     STRIKES   {_strikes}/3     ·     THREATS   {ActiveThreatCount()}", _smallFont, _gold, 0, 339, DesignWidth, 24, StringAlignment.Center);
        _continueRect = new RectangleF(430, 410, 420, 49);
        DrawButton(g, _continueRect, _dayResetByDeath ? "REOPEN THE DAY" : _nightWon ? "OPEN FOR THE NEXT DAY" : "TRY THE SHIFT AGAIN", "→", _gold, Color.FromArgb(47, 35, 19), true);
        DrawText(g, "ESC  QUIT", _smallFont, Color.FromArgb(106, 115, 110), 48, 694);
    }

    private void ContinueAfterNight()
    {
        _timer.Stop();
        if (_nightWon) { _day++; CaptureDayStartSnapshot(); }
        _nightStarted = false;
        _nightOver = false;
        _showRatingGuide = false;
        _prepScreen = _nightWon || _dayResetByDeath ? PrepScreen.Builder : PrepScreen.Roster;
        _dayResetByDeath = false;
        _rosterHoveredAnimatronic = null;
        SaveActiveSlot();
        Invalidate();
    }

    private bool HandleResultButton(PointF point)
    {
        if (!_nightStarted || !_nightOver) return false;
        if (new RectangleF(430, 410, 420, 49).Contains(point)) ContinueAfterNight();
        else return false;
        return true;
    }

    private void DrawMeter(Graphics g, float x, float y, float w, string label, int value, int max, Color color)
    {
        DrawText(g, label, _smallFont, _muted, x, y, 160, 18);
        DrawText(g, $"{value}%", _smallFont, color, x + w - 54, y, 54, 18, StringAlignment.Far);
        var track = new RectangleF(x, y + 22, w, 5);
        using var baseBrush = new SolidBrush(Color.FromArgb(43, 51, 49)); g.FillRectangle(baseBrush, track);
        using var valueBrush = new SolidBrush(color); g.FillRectangle(valueBrush, track.X, track.Y, track.Width * Math.Clamp(value / (float)max, 0, 1), track.Height);
    }

    private void DrawPanel(Graphics g, RectangleF r, Color color)
    {
        using var fill = new SolidBrush(color); using var edge = new Pen(_edge, 1);
        g.FillRectangle(fill, r); g.DrawRectangle(edge, r.X, r.Y, r.Width, r.Height);
        using var hairline = new Pen(Color.FromArgb(20, 228, 211, 177), 1); g.DrawLine(hairline, r.X + 1, r.Y + 1, r.Right - 1, r.Y + 1);
    }

    private void DrawImageFrame(Graphics g, string id, RectangleF r)
    {
        using var fill = new SolidBrush(Color.FromArgb(5, 8, 8)); using var edge = new Pen(Color.FromArgb(62, 77, 70));
        g.FillRectangle(fill, r); g.DrawRectangle(edge, r.X, r.Y, r.Width, r.Height);
        if (_sourceArt.TryGetValue(id, out var img) && img is not null)
        {
            var inset = RectangleF.Inflate(r, -5, -5);
            g.DrawImage(img, inset);
        }
        else
        {
            DrawText(g, "NO COPY", _smallFont, Color.FromArgb(111, 119, 113), r.X + 4, r.Y + r.Height / 2 - 7, r.Width - 8, 16, StringAlignment.Center);
        }
    }

    private void DrawPortraitFrame(Graphics g, Animatronic a, RectangleF r)
    {
        if (!_portraitArt.TryGetValue(a.Id, out var portrait)) { DrawImageFrame(g, a.GameId, r); return; }
        using var fill = new SolidBrush(Color.FromArgb(5, 8, 8));
        using var edge = new Pen(Color.FromArgb(62, 77, 70));
        g.FillRectangle(fill, r);
        g.DrawRectangle(edge, r.X, r.Y, r.Width, r.Height);
        var inner = RectangleF.Inflate(r, -3, -3);
        var scale = Math.Min(inner.Width / portrait.Width, inner.Height / portrait.Height);
        var width = portrait.Width * scale;
        var height = portrait.Height * scale;
        var fitted = new RectangleF(inner.X + (inner.Width - width) / 2, inner.Y + (inner.Height - height) / 2, width, height);
        g.DrawImage(portrait, fitted);
    }

    private void DrawStageToggleButton(Graphics g, RectangleF r, bool staged)
    {
        var back = staged ? Color.FromArgb(48, 35, 34) : Color.FromArgb(24, 46, 40);
        if (r.Contains(_mouse)) back = ControlPaint.Light(back, .12f);
        using var fill = new SolidBrush(back);
        using var edge = new Pen(staged ? Color.FromArgb(130, _red) : Color.FromArgb(120, _teal));
        g.FillRectangle(fill, r); g.DrawRectangle(edge, r.X, r.Y, r.Width, r.Height);
        DrawText(g, staged ? "REMOVE FROM STAGE" : "PUT ON STAGE", _smallFont, staged ? _red : _teal, r.X + 3, r.Y + 8, r.Width - 6, 16, StringAlignment.Center);
    }

    private void DrawButton(Graphics g, RectangleF r, string text, string value, Color fore, Color back, bool enabled)
    {
        var hover = r.Contains(_mouse);
        using var fill = new SolidBrush(enabled && hover ? ControlPaint.Light(back, .12f) : enabled ? back : Color.FromArgb(17, 21, 21));
        using var edge = new Pen(enabled ? Color.FromArgb(76, fore.R, fore.G, fore.B) : Color.FromArgb(42, 47, 46), 1);
        g.FillRectangle(fill, r); g.DrawRectangle(edge, r.X, r.Y, r.Width, r.Height);
        DrawText(g, text, _smallFont, enabled ? fore : Color.FromArgb(92, 99, 96), r.X + 13, r.Y + r.Height / 2 - 8, r.Width - 62, 18);
        DrawText(g, value, _smallFont, enabled ? fore : Color.FromArgb(92, 99, 96), r.Right - 52, r.Y + r.Height / 2 - 8, 39, 18, StringAlignment.Far);
    }

    private void DrawAiStepButton(Graphics g, RectangleF r, string glyph, bool enabled)
    {
        var hover = r.Contains(_mouse);
        var fore = enabled ? _ink : Color.FromArgb(92, 99, 96);
        var back = !enabled ? Color.FromArgb(17, 21, 21) : hover ? Color.FromArgb(43, 53, 49) : Color.FromArgb(29, 35, 35);
        using var fill = new SolidBrush(back);
        using var edge = new Pen(enabled ? Color.FromArgb(76, fore.R, fore.G, fore.B) : Color.FromArgb(42, 47, 46), 1);
        g.FillRectangle(fill, r); g.DrawRectangle(edge, r.X, r.Y, r.Width, r.Height);
        DrawText(g, glyph, _aiFont, fore, r.X, r.Y + 3, r.Width, r.Height - 6, StringAlignment.Center);
    }

    private static void DrawText(Graphics g, string text, Font font, Color color, float x, float y, float width = 800, float height = 30, StringAlignment align = StringAlignment.Near)
    {
        using var brush = new SolidBrush(color);
        using var format = new StringFormat { Alignment = align, LineAlignment = StringAlignment.Near, Trimming = StringTrimming.EllipsisCharacter };
        g.DrawString(text, font, brush, new RectangleF(x, y, width, height), format);
    }

    private static void DrawLine(Graphics g, Color color, float width, float x1, float y1, float x2, float y2)
    {
        using var p = new Pen(color, width); g.DrawLine(p, x1, y1, x2, y2);
    }

    private void StartNight()
    {
        _showMenu = false;
        _payoutForNight = _payoutPaidDay == _day ? 0 : NightlyPayout();
        _payoutCollectedThisNight = 0;
        _payoutAiBonusCollected = 0;
        _dayResetByDeath = false;
        _nightStarted = true; _nightOver = false; _jumpscareActive = false; _officeIntruderId = null; _jumpscareAnimatronicId = null; _seconds = 0; _strikes = 0; _score = 0; _vent = 100; _ventCooldown = 0; _doorPower = 100; _doorPowerDrainCounter = 0; _doorPowerRechargeCounter = 0; _leftDoorClosed = false; _rightDoorClosed = false; _maskOn = false; _listening = false; _lureSpent = false; _ventScanned = false; _lastResolved = "";
        _monitorOpen = false; _cameraMonitorMode = false; _selectedCamera = "1A"; _leftLightOn = false; _rightLightOn = false; _completedOfficeTasks.Clear(); _activeOfficeTaskId = null; _officeTaskSecondsRemaining = 0; _ventThreatSide = null;
        _message = ActiveThreatCount() == 0
            ? "No animatronics tonight. Raise the terminal monitor and finish all 11 office tasks."
            : $"Raise the terminal monitor and finish all 11 tasks. Survival earns ${_payoutForNight:N0}; loud printing can draw threats in.";
        _pendingUntil.Clear(); _nextAttack.Clear(); _resolved.Clear(); _animatronicCameras.Clear(); _animatronicRouteIndices.Clear();
        foreach (var a in _sheets.Animatronics.Where(a => _stageAnimatronics.Contains(a.Id) && _ai[a.Id] > 0))
        {
            _animatronicCameras[a.Id] = "1A";
            _animatronicRouteIndices[a.Id] = 0;
            _nextAttack[a.Id] = Math.Max(5, a.FirstAttackSeconds - _ai[a.Id] / 2);
        }
        _timer.Start(); SaveActiveSlot(); Invalidate();
    }

    private void AdvanceNight()
    {
        if (!_nightStarted || _nightOver) return;
        _seconds++;
        if (_activeOfficeTaskId is not null && --_officeTaskSecondsRemaining <= 0)
        {
            _completedOfficeTasks.Add(_activeOfficeTaskId);
            var finished = OfficeTasks.First(t => t.Id == _activeOfficeTaskId).Name;
            NightSounds.Play("task");
            _activeOfficeTaskId = null;
            _officeTaskSecondsRemaining = 0;
            _seconds = Math.Min(NightSteps, _seconds + 3);
            _message = $"{finished} complete. The shift clock moved ahead.";
            if (_completedOfficeTasks.Count == OfficeTasks.Length)
            {
                _message = "All 11 office tasks complete. Shift logged off.";
                FinishNight(true);
                return;
            }
        }
        var closedDoorCount = (_leftDoorClosed ? 1 : 0) + (_rightDoorClosed ? 1 : 0);
        var equipmentDraw = (_monitorOpen ? 1 : 0) + (_cameraMonitorMode ? 1 : 0) + (_leftLightOn ? 1 : 0) + (_rightLightOn ? 1 : 0);
        if (closedDoorCount > 0)
        {
            if (++_doorPowerDrainCounter >= 5)
            {
                _doorPower = Math.Max(0, _doorPower - 5 * closedDoorCount);
                _doorPowerDrainCounter = 0;
            }
            if (_doorPower == 0)
            {
                _leftDoorClosed = false; _rightDoorClosed = false;
                _doorPowerDrainCounter = 0;
                _message = "Door power depleted. Both doors opened.";
            }
        }
        else if (equipmentDraw == 0)
        {
            _doorPowerDrainCounter = 0;
            if (++_doorPowerRechargeCounter >= 3)
            {
                _doorPower = Math.Min(100, _doorPower + 3);
                _doorPowerRechargeCounter = 0;
            }
        }
        else { _doorPowerDrainCounter = 0; _doorPowerRechargeCounter = 0; }
        if (closedDoorCount > 0 || equipmentDraw > 0) _doorPowerRechargeCounter = 0;
        if (equipmentDraw > 0)
        {
            _doorPower = Math.Max(0, _doorPower - equipmentDraw);
            if (_doorPower == 0)
            {
                _leftDoorClosed = false; _rightDoorClosed = false;
                _leftLightOn = false; _rightLightOn = false;
                _monitorOpen = false; _cameraMonitorMode = false;
                _activeOfficeTaskId = null; _officeTaskSecondsRemaining = 0;
                _message = "Power depleted. Doors opened; monitor and doorway lights shut down.";
            }
        }
        if (_ventCooldown > 0) _ventCooldown--;
        if (_seconds % 4 == 0)
        {
            var ventThreatActive = (_stageAnimatronics.Contains("springtrap") && _ai["springtrap"] > 0) || (_stageAnimatronics.Contains("scrap-baby") && _ai["scrap-baby"] > 0);
            if (ventThreatActive) _vent = Math.Max(0, _vent - 1);
        }
        foreach (var a in _sheets.Animatronics.Where(a => _stageAnimatronics.Contains(a.Id) && _ai[a.Id] > 0))
        {
            if (_nextAttack.TryGetValue(a.Id, out var due) && due <= _seconds && !_pendingUntil.ContainsKey(a.Id))
            {
                var route = RouteFor(a.Id);
                // FNaF 1 uses movement opportunities: each check is an AI/20 roll.
                // The camera monitor pauses movement for threats visible on the feed.
                var cameraPaused = _cameraMonitorMode && _monitorOpen && a.Id.Equals("freddy", StringComparison.OrdinalIgnoreCase);
                if (!cameraPaused && _random.Next(1, 21) <= _ai[a.Id])
                {
                    var routeIndex = Math.Min(route.Length - 1, _animatronicRouteIndices.GetValueOrDefault(a.Id) + 1);
                    _animatronicRouteIndices[a.Id] = routeIndex;
                    _animatronicCameras[a.Id] = route[routeIndex];
                    if (routeIndex == route.Length - 1)
                    {
                        _pendingUntil[a.Id] = _seconds + _random.Next(3, 7);
                        _resolved.Remove(a.Id);
                        _message = WarningFor(a);
                        NightSounds.Play("warning");
                        if (a.Action == "scan") _ventThreatSide = route[^1] == "V1" ? "left" : "right";
                    }
                }
                _nextAttack[a.Id] = _seconds + MovementOpportunityInterval(a.Id);
            }
            if (_pendingUntil.TryGetValue(a.Id, out var until) && until <= _seconds && !_resolved.Contains(a.Id))
            {
                if (WasHandled(a)) { _score += 100 + _ai[a.Id] * 5; _lastResolved = $"{a.Name} moved away"; _message = _lastResolved + ". Stay alert."; _animatronicRouteIndices[a.Id] = Math.Max(0, _animatronicRouteIndices.GetValueOrDefault(a.Id) - 2); _animatronicCameras[a.Id] = RouteFor(a.Id)[_animatronicRouteIndices[a.Id]]; }
                else { _strikes++; _officeIntruderId = a.Id; _officeIntruderUntilUtc = DateTime.UtcNow.AddMilliseconds(1900); _lastResolved = $"{a.Name} entered the office"; _message = $"{_lastResolved}. Strike {_strikes} of 3."; NightSounds.Play("approach"); }
                _resolved.Add(a.Id); _pendingUntil.Remove(a.Id); _listening = false; _ventScanned = false;
                if (a.Action == "scan") _ventThreatSide = null;
            }
        }
        if (_strikes >= 3) { BeginJumpscare(_officeIntruderId); return; }
        else if (_seconds >= NightSteps) FinishNight(false);
        Invalidate();
    }

    private void ToggleOfficeMonitor()
    {
        if (!_monitorOpen && _doorPower <= 0)
        {
            _message = "No power available for the monitor.";
            Invalidate();
            return;
        }
        _monitorOpen = !_monitorOpen;
        if (_monitorOpen) { _cameraMonitorMode = false; _selectedCamera = "1A"; }
        NightSounds.Play(_monitorOpen ? "monitor-up" : "monitor-down");
        if (!_monitorOpen && _activeOfficeTaskId is not null)
        {
            _activeOfficeTaskId = null;
            _officeTaskSecondsRemaining = 0;
            _message = "Monitor powered off. Task cancelled.";
        }
        else _message = _monitorOpen ? "Terminal online. Select a task to begin." : "Monitor lowered.";
        Invalidate();
    }

    private static string[] RouteFor(string id) => AnimatronicRoutes.TryGetValue(id, out var route)
        ? route
        : ["1A", "1B", "4A", "4B"];

    private static int MovementOpportunityInterval(string id) => id.Equals("freddy", StringComparison.OrdinalIgnoreCase) ? 3 : 5;

    private static string? DoorSideFor(Animatronic a) => a.DoorSide ?? (RouteFor(a.Id)[^1] switch
    {
        "2B" => "left",
        "4B" => "right",
        _ => null
    });

    private void StartOfficeTask(string taskId)
    {
        if (!_monitorOpen || _activeOfficeTaskId is not null || _completedOfficeTasks.Contains(taskId)) return;
        var task = OfficeTasks.FirstOrDefault(t => t.Id == taskId);
        if (task.Id is null) return;
        _activeOfficeTaskId = task.Id;
        _officeTaskSecondsRemaining = task.Seconds;
        if (task.Noisy)
        {
            foreach (var a in _sheets.Animatronics.Where(a => _stageAnimatronics.Contains(a.Id) && _ai[a.Id] > 0))
            {
                if (_pendingUntil.ContainsKey(a.Id)) _pendingUntil[a.Id] = _seconds + _random.Next(3, 7);
                else if (_nextAttack.ContainsKey(a.Id)) _nextAttack[a.Id] = Math.Min(_nextAttack[a.Id], _seconds + _random.Next(5, 11));
            }
            _message = $"{task.Name} started. The printer is loud; animatronics may be drawn closer.";
        }
        else _message = $"{task.Name} started. Stay alert while it runs.";
        Invalidate();
    }

    private void BeginJumpscare(string? animatronicId)
    {
        _timer.Stop();
        NightSounds.Play("jumpscare");
        _jumpscareAnimatronicId = animatronicId;
        _jumpscareActive = true;
        _jumpscareTimer.Start();
        Invalidate();
    }

    private bool WasHandled(Animatronic a) => a.Action switch
    {
        "door" => IsDoorClosed(a.DoorSide), "mask" => _maskOn, "lure" => _lureSpent,
        "listen" => _listening && IsDoorClosed(a.DoorSide), "scan" => _ventScanned, _ => false
    };

    private bool IsDoorClosed(string? side) => side switch { "left" => _leftDoorClosed, "right" => _rightDoorClosed, _ => false };

    private string WarningFor(Animatronic a) => a.Action switch
    {
        "door" => $"Movement at the right doorway. Press {KeyLabel(_controlBindings["right-door"])} to close it.",
        "mask" => $"Toy Bonnie is in the hall. Press {KeyLabel(_controlBindings["mask"])} to put on the mask and stay still.",
        "lure" => $"Springtrap is near. Press {KeyLabel(_controlBindings["lure"])} to use the audio lure while ventilation is stable.",
        "listen" => $"Breathing at the left door. Press {KeyLabel(_controlBindings["listen"])} to listen, then {KeyLabel(_controlBindings["left-door"])} to close the left door.",
        "scan" => $"Scrap Baby is in the vents. Press {KeyLabel(_controlBindings["scan"])} to scan before she reaches the office.", _ => "Something is moving nearby."
    };

    private Animatronic? CurrentThreat() => _sheets.Animatronics.FirstOrDefault(a => _pendingUntil.TryGetValue(a.Id, out var until) && until > _seconds && !_resolved.Contains(a.Id));
    private int ActiveThreatCount() => _sheets.Animatronics.Count(a => _stageAnimatronics.Contains(a.Id) && _ai[a.Id] > 0);
    private bool ActionAvailable(string action) => action switch
    {
        "left-door" or "right-door" => true, "left-light" => _leftLightOn || _doorPower > 0, "right-light" => _rightLightOn || _doorPower > 0,
        "door" => true, "mask" => _owned.Contains("toy-bonnie"), "lure" => _owned.Contains("springtrap"),
        "listen" => _owned.Contains("nightmare-chica") && !_listening && _pendingUntil.TryGetValue("nightmare-chica", out var until) && until > _seconds && !_resolved.Contains("nightmare-chica"),
        "scan" => _owned.Contains("scrap-baby"), "vent" => _owned.Contains("springtrap") || _owned.Contains("scrap-baby"), _ => false
    };

    private void UseAction(string action)
    {
        switch (action)
        {
            case "left-door": case "door-left":
                if (_leftDoorClosed) { _leftDoorClosed = false; _message = "Left door open. Shared power is recharging."; }
                else if (_doorPower > 0) { _leftDoorClosed = true; _message = "Left door closed. Shared power drains while doors are shut."; }
                else _message = "No door power. Open the doors to recharge.";
                break;
            case "right-door": case "door":
                if (_rightDoorClosed) { _rightDoorClosed = false; _message = "Right door open. Shared power is recharging."; }
                else if (_doorPower > 0) { _rightDoorClosed = true; _message = "Right door closed. Shared power drains while doors are shut."; }
                else _message = "No door power. Open the doors to recharge.";
                break;
            case "left-light":
                _leftLightOn = !_leftLightOn;
                _message = _leftLightOn ? "Left doorway illuminated. Light uses 1% power each second." : "Left doorway light off.";
                NightSounds.Play("light");
                break;
            case "right-light":
                _rightLightOn = !_rightLightOn;
                _message = _rightLightOn ? "Right doorway illuminated. Light uses 1% power each second." : "Right doorway light off.";
                NightSounds.Play("light");
                break;
            case "mask": _maskOn = !_maskOn; _message = _maskOn ? "Mask on. Keep still." : "Mask off."; break;
            case "lure": if (!_lureSpent && _vent >= 25) { _lureSpent = true; _message = "Audio lure sent down the hall."; } else _message = "Lure needs 25% ventilation. Reset the vents first."; break;
            case "listen":
                if (_pendingUntil.TryGetValue("nightmare-chica", out var chicaUntil) && chicaUntil > _seconds && !_resolved.Contains("nightmare-chica"))
                {
                    _listening = true;
                    _message = "You hear Nightmare Chica breathing at the left door. Close it now.";
                }
                else _message = "No breathing at the left door. Wait for Nightmare Chica's warning, then listen.";
                break;
            case "scan": _ventScanned = true; _message = "Vent scan complete. Keep the ducts clear."; break;
            case "vent": if (_ventCooldown <= 0) { _vent = 100; _ventCooldown = 10; _lureSpent = false; _message = "Ventilation restored. Audio lure is ready."; } else _message = $"Vent reset cooling down: {_ventCooldown}s."; break;
        }
        if (action is "left-door" or "right-door") NightSounds.Play("door");
        Invalidate();
    }

    private void FinishNight(bool win, bool died = false)
    {
        _nightOver = true; _nightWon = win;
        _dayResetByDeath = died;
        if (win && _payoutPaidDay != _day)
        {
            var aiBonusBeforeStrikePenalty = (int)Math.Round(_payoutForNight * StagedAiPayoutMultiplier(), MidpointRounding.AwayFromZero);
            var grossPayout = _payoutForNight + aiBonusBeforeStrikePenalty;
            var strikePenalty = Math.Min(1d, _strikes * .25d);
            _payoutCollectedThisNight = (int)Math.Floor(grossPayout * (1d - strikePenalty));
            _payoutAiBonusCollected = (int)Math.Round(aiBonusBeforeStrikePenalty * (1d - strikePenalty), MidpointRounding.AwayFromZero);
            _funds += _payoutCollectedThisNight;
            _payoutPaidDay = _day;
        }
        else if (died)
        {
            RestoreDayStartSnapshot();
        }
        if (!died) CheckForLawsuits();
        _timer.Stop(); SaveActiveSlot(); Invalidate();
    }

    private void CheckForLawsuits()
    {
        _lawsuitsThisShift = 0;
        _lawsuitFeesThisShift = 0;
        if (_lawsuitCheckedDay == _day) return;
        _lawsuitCheckedDay = _day;
        var risk = Math.Clamp(PizzeriaRating().LiabilityRisk, 0, 50);
        for (var roll = 0; roll < 3; roll++)
        {
            if (_random.Next(1, 51) > risk) continue;
            _lawsuitsThisShift++;
            var fee = Math.Min(_funds, 100);
            _funds -= fee;
            _lawsuitFeesThisShift += fee;
        }
    }

    private int NightlyPayout()
    {
        var rating = PizzeriaRating();
        var investmentReturn = _sheets.Attractions.Where(IsAttractionEffectivelyPlaced).Sum(a => a.Cost / 4);
        return investmentReturn + rating.Atmosphere * 2 + rating.Entertainment * 3 + rating.BonusRevenue * 10;
    }

    private double StagedAiPayoutMultiplier()
    {
        return _stageAnimatronics.Where(id => _owned.Contains(id) && _ai.ContainsKey(id))
            .Sum(id => _ai[id] <= 5 ? 0d : 0.005d * (_ai[id] - 5) / 15d);
    }

    private int AttractionPayout(Attraction attraction) => attraction.Cost / 4 + attraction.Atmosphere * 2 + attraction.Entertainment * 3 + attraction.BonusRevenue * 10;

    private void DrawCheatToast(Graphics g)
    {
        if (_cheatToastUntilUtc == default || DateTime.UtcNow >= _cheatToastUntilUtc) return;
        var toast = new RectangleF(420, 96, 440, 38);
        using var fill = new SolidBrush(Color.FromArgb(238, 23, 37, 32));
        using var edge = new Pen(_gold, 1);
        g.FillRectangle(fill, toast); g.DrawRectangle(edge, toast.X, toast.Y, toast.Width, toast.Height);
        DrawText(g, _cheatToastMessage, _smallFont, _gold, toast.X, toast.Y + 10, toast.Width, 20, StringAlignment.Center);
    }

    private static bool IsKonamiKey(Keys key) => key is Keys.Up or Keys.Down or Keys.Left or Keys.Right or Keys.B or Keys.A;

    private bool ProcessKonamiKey(Keys key)
    {
        Keys[] sequence = [Keys.Up, Keys.Up, Keys.Down, Keys.Down, Keys.Left, Keys.Right, Keys.Left, Keys.Right, Keys.B, Keys.A];
        if (key == sequence[_konamiIndex]) _konamiIndex++;
        else _konamiIndex = key == sequence[0] ? 1 : 0;
        if (_konamiIndex < sequence.Length) return false;
        _konamiIndex = 0;
        _funds += 1000;
        _cheatToastMessage = "CHEAT ACCEPTED · +$1,000";
        _cheatToastUntilUtc = DateTime.UtcNow.AddSeconds(3);
        SaveActiveSlot();
        Invalidate();
        return true;
    }

    private bool ProcessNightSkipCheat(Keys key)
    {
        if (!_nightStarted || _nightOver || _jumpscareActive) { _nightSkipCheatIndex = 0; return false; }
        Keys[] sequence = [Keys.Left, Keys.Left, Keys.Down, Keys.Up, Keys.A, Keys.A];
        if (key == sequence[_nightSkipCheatIndex]) _nightSkipCheatIndex++;
        else _nightSkipCheatIndex = key == sequence[0] ? 1 : 0;
        if (_nightSkipCheatIndex < sequence.Length) return false;
        _nightSkipCheatIndex = 0;
        _completedOfficeTasks.UnionWith(OfficeTasks.Select(task => task.Id));
        _activeOfficeTaskId = null;
        _officeTaskSecondsRemaining = 0;
        _monitorOpen = false;
        _cheatToastMessage = "TEST CODE ACCEPTED · NIGHT SKIPPED";
        _cheatToastUntilUtc = DateTime.UtcNow.AddSeconds(3);
        _message = "Night skipped for testing.";
        FinishNight(true);
        return true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e); _mouse = ToDesign(e.Location);
        if (_draggingRosterScroll)
        {
            var thumbTravel = Math.Max(1, _rosterScrollTrackRect.Height - _rosterScrollThumbRect.Height);
            var thumbTop = Math.Clamp(_mouse.Y - _rosterScrollTrackRect.Y - _rosterDragGrabOffset, 0, thumbTravel);
            var contentTravel = Math.Max(0, _rosterContentHeight - _rosterViewportRect.Height);
            _rosterScrollOffset = (int)(thumbTop / thumbTravel * contentTravel);
        }
        Invalidate();
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        var p = ToDesign(e.Location);
        if (_pendingPurchaseKind != PurchaseKind.None) return;
        if (_prepScreen == PrepScreen.Builder && _catalogViewportRect.Contains(p))
        {
            _catalogScrollOffset = Math.Clamp(_catalogScrollOffset + (e.Delta < 0 ? 82 : -82), 0, Math.Max(0, _catalogContentHeight - (int)_catalogViewportRect.Height));
            Invalidate();
            return;
        }
        if (_prepScreen == PrepScreen.Blueprint && _blueprintInventoryViewportRect.Contains(p))
        {
            _blueprintInventoryScrollOffset = Math.Clamp(_blueprintInventoryScrollOffset + (e.Delta < 0 ? 48 : -48), 0, Math.Max(0, _sheets.Attractions.Count(a => _builtAttractions.Contains(a.Id) && !_placedAttractions.ContainsKey(a.Id)) * 48 - (int)_blueprintInventoryViewportRect.Height));
            Invalidate();
            return;
        }
        if (_prepScreen != PrepScreen.Roster) return;
        if (_rosterViewportRect.Contains(p))
        {
            foreach (var (id, rect) in _rosterCardRects)
            {
                if (!rect.Contains(p) || !_owned.Contains(id)) continue;
                _ai[id] = Math.Clamp(_ai[id] + (e.Delta > 0 ? 1 : -1), 0, _sheets.MaximumAiLevel);
                SaveActiveSlot();
                Invalidate();
                return;
            }
        }
        if (!_rosterViewportRect.Contains(p)) return;
        var maxScroll = Math.Max(0, _rosterContentHeight - (int)_rosterViewportRect.Height);
        _rosterScrollOffset = Math.Clamp(_rosterScrollOffset + (e.Delta < 0 ? 100 : -100), 0, maxScroll);
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        var p = ToDesign(e.Location);
        if (e.Button == MouseButtons.Left && HandleResultButton(p))
        {
            _suppressNextMouseClick = true;
            return;
        }
        if (_prepScreen == PrepScreen.Roster && _rosterScrollThumbRect.Contains(p))
        {
            _draggingRosterScroll = true;
            _rosterDragGrabOffset = p.Y - _rosterScrollThumbRect.Y;
            Capture = true;
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (!_draggingRosterScroll) return;
        _draggingRosterScroll = false;
        _suppressNextMouseClick = true;
        Capture = false;
        Invalidate();
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        var p = ToDesign(e.Location);
        if (_suppressNextMouseClick) { _suppressNextMouseClick = false; return; }
        if (_pendingDeleteSlot >= 0)
        {
            if (_deleteSaveConfirmRect.Contains(p)) DeleteSaveSlot(_pendingDeleteSlot);
            else if (_deleteSaveCancelRect.Contains(p)) { _pendingDeleteSlot = -1; Invalidate(); }
            return;
        }
        if (_pendingPurchaseKind != PurchaseKind.None)
        {
            if (_purchaseConfirmRect.Contains(p)) ConfirmPendingPurchase();
            else if (_purchaseCancelRect.Contains(p)) CancelPendingPurchase();
            return;
        }
        if (_nightStarted && _nightOver && HandleResultButton(p)) return;
        if (_showRatingGuide)
        {
            if (_ratingGuideCloseRect.Contains(p)) _showRatingGuide = false;
            Invalidate();
            return;
        }
        if (_nightStarted && _nightOver)
        {
            return;
        }
        if (_showMenu && !_settingsOpen)
        {
            if (_saveMenuOpen)
            {
                if (_saveMenuBackRect.Contains(p))
                {
                    _saveMenuOpen = false;
                    _pendingDeleteSlot = -1;
                    Invalidate();
                    return;
                }
                for (var slot = 0; slot < 3; slot++)
                {
                    if (_saveDeleteRects[slot].Contains(p) && _saveSlotFilesPresent[slot])
                    {
                        _pendingDeleteSlot = slot;
                        Invalidate();
                        return;
                    }
                    if (!_saveLoadRects[slot].Contains(p)) continue;
                    if (!_saveSlotFilesPresent[slot]) StartNewGameInSlot(slot);
                    else if (_saveSlots[slot] is not null) LoadGameFromSlot(slot);
                    else { _saveMenuMessage = "This save cannot be read. Delete it to clear the slot."; Invalidate(); }
                    return;
                }
            }
            else if (_menuStartRect.Contains(p))
            {
                RefreshSaveSlots();
                _saveMenuOpen = true;
                _saveMenuMessage = "Progress saves automatically in the selected slot.";
                Invalidate();
            }
            else if (_menuSettingsRect.Contains(p))
            {
                _settingsOpen = true;
                _waitingForBindingAction = null;
                Invalidate();
            }
            return;
        }
        if (_settingsOpen)
        {
            if (_settingsBackRect.Contains(p))
            {
                _settingsOpen = false;
                _waitingForBindingAction = null;
                Invalidate();
                return;
            }
            foreach (var (action, rect) in _settingsBindingRects)
                if (rect.Contains(p))
                {
                    _waitingForBindingAction = action;
                    _settingsMessage = "Press the key you want to use. Esc cancels.";
                    Invalidate();
                    return;
                }
            return;
        }
        if (_nightStarted && !_nightOver)
        {
            if (_monitorToggleRect.Contains(p)) { ToggleOfficeMonitor(); return; }
            if (_monitorOpen)
            {
                foreach (var (tabId, rect) in _officeMonitorTabRects)
                {
                    if (!rect.Contains(p)) continue;
                    _cameraMonitorMode = tabId == "cameras";
                    NightSounds.Play("task");
                    Invalidate();
                    return;
                }
                foreach (var (cameraId, rect) in _cameraSelectRects)
                {
                    if (!rect.Contains(p)) continue;
                    _selectedCamera = cameraId;
                    NightSounds.Play("task");
                    Invalidate();
                    return;
                }
                foreach (var (taskId, rect) in _officeTaskRects)
                    if (rect.Contains(p)) { StartOfficeTask(taskId); return; }
            }
            else
            {
                foreach (var (lightId, rect) in _officeLightRects)
                    if (rect.Contains(p)) { UseAction(lightId); return; }
            }
            foreach (var (action, rect) in _nightActionRects)
                if (rect.Contains(p) && ActionAvailable(action)) { UseAction(action); return; }
            return;
        }
        if (_buildTabRect.Contains(p)) { _prepScreen = PrepScreen.Builder; Invalidate(); return; }
        if (_blueprintTabRect.Contains(p)) { _prepScreen = PrepScreen.Blueprint; Invalidate(); return; }
        if (_rosterTabRect.Contains(p)) { _prepScreen = PrepScreen.Roster; Invalidate(); return; }
        if (_startNightRect.Contains(p)) { StartNight(); return; }
        if (_prepScreen == PrepScreen.Roster)
        {
            foreach (var (gameId, rect) in _animatronicCategoryUnlockRects)
                if (rect.Contains(p) && !_unlockedAnimatronicGames.Contains(gameId) && _sources[gameId].IsAvailable && _funds >= AnimatronicCategoryCost(gameId))
                {
                    _pendingPurchaseId = gameId;
                    _pendingPurchaseKind = PurchaseKind.AnimatronicCategory;
                    Invalidate();
                    return;
                }
            if (_rosterScrollTrackRect.Contains(p) && _rosterContentHeight > _rosterViewportRect.Height)
            {
                var relative = Math.Clamp((p.Y - _rosterScrollTrackRect.Y) / _rosterScrollTrackRect.Height, 0, 1);
                _rosterScrollOffset = (int)(relative * Math.Max(0, _rosterContentHeight - _rosterViewportRect.Height));
                Invalidate(); return;
            }
            foreach (var (gameId, rect) in _rosterGroupHeaderRects)
                if (rect.Contains(p))
                {
                    if (!_collapsedRosterGroups.Add(gameId)) _collapsedRosterGroups.Remove(gameId);
                    Invalidate(); return;
                }
            if (_rosterAiMinusRect.Contains(p) && _rosterHoveredAnimatronic is not null)
            {
                _ai[_rosterHoveredAnimatronic] = Math.Max(0, _ai[_rosterHoveredAnimatronic] - 1);
                SaveActiveSlot();
                Invalidate(); return;
            }
            if (_rosterAiPlusRect.Contains(p) && _rosterHoveredAnimatronic is not null)
            {
                _ai[_rosterHoveredAnimatronic] = Math.Min(_sheets.MaximumAiLevel, _ai[_rosterHoveredAnimatronic] + 1);
                SaveActiveSlot();
                Invalidate(); return;
            }
            if (_rosterViewportRect.Contains(p))
            {
                foreach (var (id, rect) in _rosterCardRects)
                {
                    if (!rect.Contains(p)) continue;
                    var a = _sheets.Animatronics.First(entry => entry.Id == id);
                    if (!_owned.Contains(id))
                    {
                        if (_sources[a.GameId].IsAvailable && _funds >= a.Cost)
                        {
                            _pendingPurchaseId = id;
                            _pendingPurchaseKind = PurchaseKind.Animatronic;
                            Invalidate();
                        }
                    }
                    else if (!_stageAnimatronics.Add(id)) _stageAnimatronics.Remove(id);
                    SaveActiveSlot();
                    Invalidate(); return;
                }
            }
            return;
        }
        if (_prepScreen == PrepScreen.Builder)
        {
            foreach (var (category, rect) in _catalogCategoryRects)
            {
                if (!rect.Contains(p)) continue;
                if (IsCatalogCategoryUnlocked(category))
                {
                    _selectedCatalogCategory = category;
                    _catalogScrollOffset = 0;
                    _catalogMessage = "Place purchases to raise your rating and unlock the next catalog.";
                }
                else _catalogMessage = $"This catalog unlocks at {CatalogRequirement(category)} pizzeria rating points. Place more purchases first.";
                Invalidate(); return;
            }
            if (_catalogScrollTrackRect.Contains(p) && _catalogContentHeight > _catalogViewportRect.Height)
            {
                var ratio = Math.Clamp((p.Y - _catalogScrollTrackRect.Y) / _catalogScrollTrackRect.Height, 0, 1);
                _catalogScrollOffset = (int)(ratio * Math.Max(0, _catalogContentHeight - _catalogViewportRect.Height));
                Invalidate(); return;
            }
            foreach (var attraction in _sheets.Attractions)
            {
                if (!_attractionBuyRects.TryGetValue(attraction.Id, out var rect) || !rect.Contains(p)) continue;
                if (!_builtAttractions.Contains(attraction.Id) && _funds >= attraction.Cost)
                {
                    _pendingPurchaseId = attraction.Id;
                    _pendingPurchaseKind = PurchaseKind.Attraction;
                    Invalidate();
                }
                Invalidate(); return;
            }
            return;
        }
        if (_prepScreen == PrepScreen.Blueprint)
        {
            if (_ratingGuideButtonRect.Contains(p)) { _showRatingGuide = true; Invalidate(); return; }
            if (_blueprintInventoryScrollTrackRect.Contains(p))
            {
                var count = _sheets.Attractions.Count(a => _builtAttractions.Contains(a.Id) && !_placedAttractions.ContainsKey(a.Id));
                var ratio = Math.Clamp((p.Y - _blueprintInventoryScrollTrackRect.Y) / _blueprintInventoryScrollTrackRect.Height, 0, 1);
                _blueprintInventoryScrollOffset = (int)(ratio * Math.Max(0, count * 48 - _blueprintInventoryViewportRect.Height));
                Invalidate(); return;
            }
            foreach (var (id, rect) in _stageAnimatronicRects)
                if (rect.Contains(p)) { _stageAnimatronics.Remove(id); SaveActiveSlot(); Invalidate(); return; }
            foreach (var (id, rect) in _blueprintInventoryRects)
                if (rect.Contains(p)) { _selectedAttraction = id; Invalidate(); return; }
            foreach (var (id, rect) in _placedAttractionRemoveRects)
                if (rect.Contains(p))
                {
                    _placedAttractions.Remove(id);
                    _selectedAttraction = null;
                    SaveActiveSlot();
                    Invalidate();
                    return;
                }
            if (_blueprintGridRect.Contains(p))
            {
                const int columns = 8;
                var col = Math.Clamp((int)((p.X - _blueprintGridRect.X) / (_blueprintGridRect.Width / columns)), 0, columns - 1);
                var row = Math.Clamp((int)((p.Y - _blueprintGridRect.Y) / (_blueprintGridRect.Height / 4)), 0, 3);
                var cell = new Point(col, row);
                if (IsDiningTableTile(cell)) { _selectedAttraction = null; Invalidate(); return; }
                var occupant = _placedAttractions.FirstOrDefault(item => item.Value == cell).Key;
                if (occupant is not null) _selectedAttraction = occupant;
                else if (_selectedAttraction is not null)
                {
                    _placedAttractions[_selectedAttraction] = cell;
                    _selectedAttraction = null;
                    SaveActiveSlot();
                }
                Invalidate(); return;
            }
            return;
        }
    }

    private PointF ToDesign(Point p) => new((p.X - _offsetX) / _scale, (p.Y - _offsetY) / _scale);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (_waitingForBindingAction is not null)
        {
            BindCapturedKey(e.KeyCode);
            e.Handled = true;
            e.SuppressKeyPress = true;
            return;
        }
        if (_pendingPurchaseKind != PurchaseKind.None && e.KeyCode == Keys.Escape)
        {
            CancelPendingPurchase();
            e.Handled = true;
            e.SuppressKeyPress = true;
            return;
        }
        if (_pendingDeleteSlot >= 0 && e.KeyCode == Keys.Escape)
        {
            _pendingDeleteSlot = -1;
            Invalidate();
            e.Handled = true;
            e.SuppressKeyPress = true;
            return;
        }
        if (e.KeyCode == Keys.F11)
        {
            ToggleDisplayMode();
            e.Handled = true;
            e.SuppressKeyPress = true;
            return;
        }
        if (IsKonamiKey(e.KeyCode))
        {
            var firstPress = _cheatKeysHeld.Add(e.KeyCode);
            if (!firstPress) return;
            if (ProcessKonamiKey(e.KeyCode)) { e.Handled = true; e.SuppressKeyPress = true; return; }
            if (ProcessNightSkipCheat(e.KeyCode)) { e.Handled = true; e.SuppressKeyPress = true; return; }
        }
        if (_nightStarted && _nightOver)
        {
            if (e.KeyCode is Keys.Enter or Keys.Space) ContinueAfterNight();
            else return;
            e.Handled = true;
            e.SuppressKeyPress = true;
            return;
        }
        if (_showMenu && !_settingsOpen && e.KeyCode == Keys.Enter)
        {
            if (!_saveMenuOpen)
            {
                RefreshSaveSlots();
                _saveMenuOpen = true;
                _saveMenuMessage = "Choose an empty slot or load an existing save.";
            }
            Invalidate();
            return;
        }
        if (e.KeyCode == Keys.Escape)
        {
            if (_settingsOpen) _settingsOpen = false;
            else if (_saveMenuOpen) _saveMenuOpen = false;
            else if (_nightStarted && !_nightOver && _monitorOpen) ToggleOfficeMonitor();
            else if (_nightStarted && !_nightOver) { _nightStarted = false; _prepScreen = PrepScreen.Builder; _timer.Stop(); SaveActiveSlot(); }
            else if (!_showMenu) { SaveActiveSlot(); _showMenu = true; }
            else Close();
            Invalidate(); return;
        }
        if (!_nightStarted || _nightOver) return;
        if (e.KeyCode == _controlBindings["monitor"]) { ToggleOfficeMonitor(); e.Handled = true; e.SuppressKeyPress = true; return; }
        if (_monitorOpen && _cameraMonitorMode && (e.KeyCode == _controlBindings["camera-prev"] || e.KeyCode == _controlBindings["camera-next"]))
        {
            var index = Array.FindIndex(FnafCameras, camera => camera.Id == _selectedCamera);
            var direction = e.KeyCode == _controlBindings["camera-prev"] ? -1 : 1;
            _selectedCamera = FnafCameras[(index + direction + FnafCameras.Length) % FnafCameras.Length].Id;
            NightSounds.Play("task"); Invalidate(); e.Handled = true; e.SuppressKeyPress = true; return;
        }
        var action = _controlBindings.FirstOrDefault(pair => pair.Value == e.KeyCode).Key;
        if (action is not null && ActionAvailable(action)) UseAction(action);
    }

    private void ToggleDisplayMode()
    {
        if (_isFullscreen)
        {
            FormBorderStyle = FormBorderStyle.Sizable;
            WindowState = FormWindowState.Normal;
            if (_windowedBounds.IsEmpty)
            {
                Size = new Size(1280, 720);
                CenterToScreen();
            }
            else Bounds = _windowedBounds;
            _isFullscreen = false;
        }
        else
        {
            _windowedBounds = Bounds;
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            _isFullscreen = true;
        }
        Invalidate();
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        _cheatKeysHeld.Remove(e.KeyCode);
        base.OnKeyUp(e);
    }

    private string ShortName(string name) => name switch
    {
        "Five Nights at Freddy's" => "FNAF 1",
        "Five Nights at Freddy's 2" => "FNAF 2",
        "Five Nights at Freddy's 3" => "FNAF 3",
        "Five Nights at Freddy's 4" => "FNAF 4",
        "Freddy Fazbear's Pizzeria Simulator" => "PIZZERIA SIM",
        _ => "UCN"
    };

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        SaveActiveSlot();
        _timer.Dispose(); _blinkTimer.Dispose(); _jumpscareTimer.Dispose();
        foreach (var image in _sourceArt.Values) image?.Dispose();
        foreach (var image in _portraitArt.Values) image.Dispose();
        foreach (var image in _catalogAttractionArt.Values) image.Dispose();
        _brandFont.Dispose(); _titleFont.Dispose(); _bodyFont.Dispose(); _smallFont.Dispose(); _cardTitleFont.Dispose(); _panelTitleFont.Dispose(); _messageFont.Dispose(); _aiFont.Dispose(); _noMovementFont.Dispose(); _keyFont.Dispose(); _menuTitleFont.Dispose(); _menuSubtitleFont.Dispose();
        foreach (var source in _sources.Values) source.Dispose();
        base.OnFormClosed(e);
    }
}
