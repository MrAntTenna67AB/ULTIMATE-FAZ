using System.Drawing.Drawing2D;
using System.Text.Json;
using System.Drawing.Imaging;

namespace FazbearPizzeria;

internal sealed class GameForm : Form
{
    private const float DesignWidth = 1280;
    private const float DesignHeight = 720;
    private readonly DesignSheets _sheets;
    private readonly Dictionary<string, SourceInstall> _sources;
    private readonly Dictionary<string, Bitmap?> _sourceArt = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Bitmap> _portraitArt = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Image> _jumpscareArt = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Bitmap> _catalogAttractionArt = new(StringComparer.OrdinalIgnoreCase);
    private Bitmap? _tennaModelArt;
    private Bitmap? _shadowOfficeArt;
    private bool _shadowInOffice;
    private int _shadowDeadline;
    private readonly HashSet<string> _owned = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _unlockedAnimatronicGames = new(StringComparer.OrdinalIgnoreCase) { "fnaf1" };
    private readonly HashSet<string> _purchasedCatalogCategories = new(StringComparer.OrdinalIgnoreCase) { "dumpster" };
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
    private static readonly HashSet<string> CatalogCategoryIds = new(StringComparer.OrdinalIgnoreCase) { "dumpster", "budget-tech", "smiles", "rare" };
    private static readonly (string Id, string Name, int Cost)[] CatalogCategories =
    [
        ("dumpster", "DUMPSTER DIVER WEEKLY", 0),
        ("budget-tech", "STAN'S BUDGET TECH", 500),
        ("smiles", "SMILES AND SERVOS, INC.", 1500),
        ("rare", "RARE FINDS AUCTION", 3000)
    ];
    private int _funds;
    private int _day = 1;
    private int _seconds;
    private int _strikes;
    private int _score;
    private int _vent = 100;
    private int _ventCooldown;
    private int _doorPower = 100;
    private int _doorPowerDrainCounter;
    private int _payoutPaidDay;
    private int _lawsuitCheckedDay;
    private int _payoutForNight;
    private int _payoutCollectedThisNight;
    private int _payoutAiBonusCollected;
    private int _payoutAnimatronicBonusCollected;
    private int _konamiIndex;
    private int _nightSkipCheatIndex;
    private bool _salvageRevealed;
    private readonly Dictionary<string, int> _wordCheatProgress = new(StringComparer.OrdinalIgnoreCase);
    private static readonly (string Code, string Target, string Toast)[] WordCheats =
    [
        ("UNCOVER", "reveal", "CHEAT ACCEPTED · HIDDEN SALVAGES REVEALED"),
        ("FIVENIGHTS", "fnaf1", "CHEAT ACCEPTED · ALL FNAF 1 ANIMATRONICS UNLOCKED"),
        ("WITHERED&TOYS", "fnaf2", "CHEAT ACCEPTED · ALL FNAF 2 ANIMATRONICS UNLOCKED"),
        ("IALWAYSCOMEBACK", "fnaf3", "CHEAT ACCEPTED · ALL FNAF 3 ANIMATRONICS UNLOCKED"),
        ("JUSTYOURIMAGINATION", "fnaf4", "CHEAT ACCEPTED · ALL FNAF 4 ANIMATRONICS UNLOCKED"),
        ("CONNECTIONTERMINATED", "ffps", "CHEAT ACCEPTED · ALL FFPS ANIMATRONICS UNLOCKED"),
        ("PURGATORY", "ucn", "CHEAT ACCEPTED · ALL UCN ANIMATRONICS UNLOCKED"),
        ("WHOISTHATBRUH", "fnaf-mades", "CHEAT ACCEPTED · ALL CUSTOM ANIMATRONICS UNLOCKED"),
    ];
    private int _unlockCategoriesCheatIndex;
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
    private bool _goldenFreddyReady;
    private bool _goldenFreddyEncounterTriggered;
    private int _puppetMusicBox = 100;
    private bool _puppetEscaped;
    private RectangleF _puppetWindRect;
    private bool _mangleOnCeiling;
    private int _mangleStaticSeconds;
    private bool _tennaQuizActive;
    private bool _charlieDebateActive;
    private int _charlieQuestionPosition;
    private int _charlieSecondsRemaining;
    private string[] _charlieShuffledAnswers = [];
    private int _tennaQuestionPosition;
    private int _tennaCorrectAnswers;
    private int _tennaFailedAnswers;
    private int _tennaSecondsRemaining;
    private int _tennaQuizCooldownUntil;
    private int _tennaHijackCountdown = -1;
    private bool _tennaFirstEncounterPending;
    private int[] _tennaQuestionOrder = [];
    private string[] _tennaShuffledAnswers = [];
    private readonly Dictionary<int, RectangleF> _tennaAnswerRects = [];
    private string? _salvageCandidateId;
    private bool _salvageOfferOpen;
    private bool _salvageActive;
    private bool _salvageDone;
    private int _salvageStage;
    private int _salvagePhase;
    private bool _salvageTwitch;
    private int _salvageTicksLeft;
    private string _salvageMessage = "";
    private RectangleF _salvageKeepRect = RectangleF.Empty;
    private RectangleF _salvageTossRect = RectangleF.Empty;
    private RectangleF _salvageTaseRect = RectangleF.Empty;
    private RectangleF _salvageCheckRect = RectangleF.Empty;
    private RectangleF _salvageDoneRect = RectangleF.Empty;
    private const int SalvageWatchTicks = 7;
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
    private bool _nightPaused;
    private bool _peeMonsterTintActive;
    private bool _dayResetByDeath;
    private bool _jumpscareActive;
    private string? _officeIntruderId;
    private string? _jumpscareAnimatronicId;
    private DateTime _officeIntruderUntilUtc;
    private bool _showMenu = true;
    private bool _settingsOpen;
    private bool _showRatingGuide;
    private bool _isFullscreen = true;
    private int _musicVolume = 100;
    private int _effectsVolume = 100;
    private bool _autoLaunchOnSignIn;
    private RectangleF _musicVolumeRect;
    private RectangleF _effectsVolumeRect;
    private RectangleF _displayModeRect;
    private RectangleF _autoLaunchRect;
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
    private RectangleF _rosterSellRect = RectangleF.Empty;
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
    private sealed record TennaQuestion(string Prompt, string[] Answers, string CorrectAnswer);
    private static readonly TennaQuestion[] TennaQuestions =
    [
        new("Finish the sentence! ‘WAS THAT HE BITE OF ???’", ["85", "83", "86", "87"], "87"),
        new("What's the name of the child in Haunting Foxy?", ["Fritz", "Bob", "Unknown", "Roxy"], "Fritz"),
        new("It been so ???", ["Long", "Short", "Midly long", "Soon"], "Long"),
        new("What's in the box?", ["The lore", "Nothing", "Better to keep it a secret"], "Better to keep it a secret"),
        new("Say it with me, folks!", ["ITS T-V TIME!", "ITS!! T-V!! TIME!!"], "ITS!! T-V!! TIME!!"),
        new("Which one's the best?", ["Xbox", "PS4", "Switch"], "Xbox"),
        new("Bonnie is...", ["Blue", "Purple", "Mixed"], "Blue"),
        new("Chica is what?", ["A fat ass", "Cutie", "Chicken"], "Cutie"),
        new("Will you buy Mr. (ant) Tenna merch (trademark pending)?", ["Yes we will!", "No.", "Yes daddy CRT"], "Yes we will!"),
        new("Do you love TV?", ["I LOVE TV!!!", "I HATE TV!!!"], "I LOVE TV!!!")
    ];
    private static readonly TennaQuestion[] CharlieQuestions =
    [
        new("Should society return to Hitler's regime?", ["No. That regime was horrific.", "Yes."], "No. That regime was horrific."),
        new("Is President Donald Trump acting in the interests of the American people?", ["I'm not convinced.", "Yes, absolutely."], "I'm not convinced."),
        new("Is Israel only defending itself?", ["That is too simple. Civilians everywhere deserve protection.", "Yes, only that."], "That is too simple. Civilians everywhere deserve protection.")
    ];
    private static readonly (string Id, string Name)[] FnafCameras =
    [
        ("1A", "SHOW STAGE"), ("1B", "DINING AREA"), ("1C", "COVE"),
        ("10", "GAME AREA"),
        ("2A", "WEST HALL"), ("2B", "WEST CORNER"), ("3", "SUPPLY CLOSET"),
        ("4A", "EAST HALL"), ("4B", "EAST CORNER"), ("5", "BACKSTAGE"),
        ("6", "KITCHEN"), ("7", "RESTROOMS"),
        ("F2-KC", "KID'S COVE"), ("F2-PC", "PRIZE CORNER"), ("F2-GA", "GAME AREA"), ("F2-MH", "MAIN HALL"),
        ("F2-OH", "OFFICE HALLWAY"), ("F2-PR1", "PARTY ROOM 1"), ("F2-PR2", "PARTY ROOM 2"), ("F2-RV", "RIGHT AIR VENT")
    ];
    private static readonly Dictionary<string, string[]> AnimatronicRoutes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["freddy"] = ["1A", "1B", "7", "6", "4A", "4B"],
        ["foxy"] = ["1C", "2A", "2B"],
        ["golden-freddy"] = ["2B"],
        ["bonnie"] = ["1A", "1B", "2A", "2B"],
        ["chica"] = ["1A", "1B", "7", "4A", "4B"],
        ["toy-bonnie"] = ["1A", "5", "1B", "2A", "2B"],
        ["toy-freddy"] = ["1A", "10", "1B", "2A", "2B"],
        ["toy-chica"] = ["1A", "5", "1B", "2A", "2B"],
        ["jay-jay"] = ["1A", "5", "1B", "2A", "2B"],
        ["withered-foxy"] = ["1A", "5", "1B", "2A", "2B"],
        ["withered-freddy"] = ["1A", "5", "1B", "2A", "2B"],
        ["balloon-boy"] = ["1A", "5", "1B", "2A", "2B"],
        ["withered-bonnie"] = ["1A", "5", "1B", "2A", "2B"],
        ["withered-chica"] = ["1A", "5", "1B", "2A", "2B"],
        ["mangle"] = ["F2-KC", "F2-PC", "F2-GA", "F2-MH", "F2-OH", "F2-PR1", "F2-PR2", "F2-RV"],
        ["charlie-kirk"] = ["1A", "1B", "2A", "2B"],
        ["pee-monster"] = ["1A", "7", "V1"],
        ["puppet"] = ["1A"],
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

    private sealed record GamePreferences(int MusicVolume = 100, int EffectsVolume = 100,
        bool Fullscreen = true, bool AutoLaunchOnSignIn = false);

    private enum PrepScreen { Builder, Blueprint, Roster }
    private enum PurchaseKind { None, Attraction, Animatronic, AnimatronicCategory, CatalogCategory }
    private sealed record DayStartSnapshot(int Day, int Funds, string[] OwnedAnimatronics, string[] StagedAnimatronics,
        string[] BuiltAttractions, Dictionary<string, int> AttractionTiles, Dictionary<string, int> AiLevels,
        string[] UnlockedAnimatronicGames, int PayoutPaidDay, int LawsuitCheckedDay, string[]? PurchasedCatalogCategories = null);
    private sealed record SaveGameData(int Version, DateTime SavedAt, int Day, int Funds, string[] OwnedAnimatronics,
        string[] StagedAnimatronics, string[] BuiltAttractions, Dictionary<string, int> AttractionTiles,
        Dictionary<string, int> AiLevels, string[] UnlockedAnimatronicGames, int PayoutPaidDay, int LawsuitCheckedDay,
        int PrepScreen, bool NightOver, bool NightWon, int PayoutCollectedThisNight, int LawsuitsThisShift, int LawsuitFeesThisShift,
        int AiPayoutBonus = 0, DayStartSnapshot? DayStartState = null, int NightStrikes = 0, bool DayResetByDeath = false,
        int AnimatronicPayoutBonus = 0, string[]? PurchasedCatalogCategories = null);
    private DayStartSnapshot? _dayStartSnapshot;

    public GameForm(DesignSheets sheets, Dictionary<string, SourceInstall> sources, bool demo)
    {
        _sheets = sheets;
        _sources = sources;
        _demo = demo;
        _funds = sheets.StartingFunds;
        LoadPreferences();
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
        var jumpscareDirectory = Path.Combine(AppContext.BaseDirectory, "assets", "jumpscares");
        if (Directory.Exists(jumpscareDirectory))
        foreach (var path in Directory.GetFiles(jumpscareDirectory).Where(path => Path.GetExtension(path).Equals(".gif", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(path).Equals(".png", StringComparison.OrdinalIgnoreCase)))
            try { _jumpscareArt[Path.GetFileNameWithoutExtension(path)] = Image.FromFile(path); } catch { }
        var tennaModelPath = Path.Combine(portraitDirectory, "mr-ant-tenna-model.png");
        if (File.Exists(tennaModelPath))
            try { using var image = Image.FromFile(tennaModelPath); _tennaModelArt = new Bitmap(image); } catch { }
        var shadowOfficePath = Path.Combine(portraitDirectory, "shadow-bonnie-office.png");
        if (File.Exists(shadowOfficePath))
            try { using var officeImage = Image.FromFile(shadowOfficePath); _shadowOfficeArt = new Bitmap(officeImage); } catch { }
        var attractionArtDirectory = Path.Combine(AppContext.BaseDirectory, "assets", "attractions");
        foreach (var attraction in sheets.Attractions)
        {
            var path = Path.Combine(attractionArtDirectory, attraction.Id + ".png");
            if (!File.Exists(path)) continue;
            try { using var image = Image.FromFile(path); _catalogAttractionArt[attraction.Id] = new Bitmap(image); } catch { }
        }
        Text = "Ultimate Fazbear";
        FormBorderStyle = _isFullscreen ? FormBorderStyle.None : FormBorderStyle.Sizable;
        WindowState = _isFullscreen ? FormWindowState.Maximized : FormWindowState.Normal;
        if (!_isFullscreen) Size = new Size(1280, 720);
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
            if (_jumpscareAnimatronicId is not null && _jumpscareArt.TryGetValue(_jumpscareAnimatronicId, out var scareImage))
                ImageAnimator.StopAnimate(scareImage, OnJumpscareFrameChanged);
            _jumpscareActive = false;
            FinishNight(false, died: true);
        };
        _blinkTimer.Tick += (_, _) =>
        {
            if (_nightStarted && !_nightOver) Invalidate();
            if (_salvageActive && !_salvageDone) AdvanceSalvage();
            if (_cheatToastUntilUtc != default)
            {
                if (DateTime.UtcNow >= _cheatToastUntilUtc) _cheatToastUntilUtc = default;
                Invalidate();
            }
        };
        _blinkTimer.Start();
        NightSounds.StartMenuMusic();
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
        if (!_nightStarted)
        {
            if (_settingsOpen || _saveMenuOpen) NightSounds.StartTheme("settings and saves screen theme.wav");
            else if (_showMenu) NightSounds.StartMenuMusic();
            else if (_prepScreen == PrepScreen.Blueprint) NightSounds.StartTheme("blue print mode theme.wav");
            else NightSounds.StartTheme("catalogs theme.wav");
        }
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
        else if (_tennaQuizActive) DrawTennaQuiz(g);
        else if (_charlieDebateActive) DrawCharlieDebate(g);
        else if (_nightStarted && _nightOver) { DrawResult(g); if (_salvageOfferOpen || _salvageActive || _salvageDone) DrawSalvage(g); }
        else if (_nightStarted) { DrawNight(g); if (_peeMonsterTintActive && !_nightOver) DrawPeeMonsterTint(g); if (_nightPaused) DrawPauseOverlay(g); }
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
        DrawText(g, "ULTIMATE FAZBEAR", _smallFont, _gold, 48, 17);
        DrawText(g, "ULTIMATE FAZBEAR", _brandFont, _ink, 48, 34);
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
        DrawText(g, "ULTIMATE", _smallFont, _gold, 88, 179);
        DrawText(g, "FAZBEAR", _menuTitleFont, _ink, 82, 198, 720, 80);
        DrawText(g, "AFTER HOURS", _menuSubtitleFont, _teal, 88, 273, 600, 48);
        DrawText(g, "BUILD A FAMILY PIZZERIA BY DAY.", _panelTitleFont, _ink, 90, 365, 640, 30);
        DrawText(g, "BUY ATTRACTIONS. ADD ANIMATRONICS. SURVIVE THE NIGHT.", _bodyFont, _muted, 90, 401, 800, 25);
        _menuStartRect = new RectangleF(88, 485, 330, 58);
        DrawButton(g, _menuStartRect, "SAVE SLOTS", "→", _gold, Color.FromArgb(47, 35, 19), true);
        _menuSettingsRect = new RectangleF(434, 485, 220, 58);
        DrawButton(g, _menuSettingsRect, "SETTINGS", "⚙", _teal, Color.FromArgb(22, 28, 29), true);
        DrawText(g, "A SOLO FNAF MASHUP", _smallFont, _muted, 90, 568);
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
        DrawText(g, "AUDIO", _smallFont, _gold, 730, 190, 450, 20);
        DrawVolumeSetting(g, "MUSIC", _musicVolume, 730, 226, out _musicVolumeRect);
        DrawVolumeSetting(g, "SOUND EFFECTS", _effectsVolume, 730, 314, out _effectsVolumeRect);
        DrawText(g, "STARTUP DISPLAY", _smallFont, _gold, 730, 407, 450, 20);
        _displayModeRect = new RectangleF(730, 433, 450, 42);
        DrawButton(g, _displayModeRect, "DISPLAY MODE", _isFullscreen ? "FULLSCREEN" : "WINDOWED", _teal, Color.FromArgb(13, 20, 21), true);
        DrawText(g, "WINDOWS STARTUP", _smallFont, _gold, 730, 496, 450, 20);
        _autoLaunchRect = new RectangleF(730, 522, 450, 42);
        DrawButton(g, _autoLaunchRect, "LAUNCH ON SIGN-IN", _autoLaunchOnSignIn ? "ON" : "OFF", _autoLaunchOnSignIn ? _teal : _muted, Color.FromArgb(13, 20, 21), true);
        _settingsBackRect = new RectangleF(88, 625, 260, 42);
        DrawButton(g, _settingsBackRect, "BACK TO MENU", "←", _gold, Color.FromArgb(47, 35, 19), true);
        DrawText(g, "F11  WINDOWED / FULLSCREEN", _smallFont, Color.FromArgb(106, 115, 110), 1010, 694, 220, 18, StringAlignment.Far);
    }

    private void DrawVolumeSetting(Graphics g, string label, int volume, float x, float y, out RectangleF slider)
    {
        DrawText(g, label, _smallFont, _ink, x, y, 320, 18);
        DrawText(g, $"{volume}%", _smallFont, _teal, x + 370, y, 80, 18, StringAlignment.Far);
        slider = new RectangleF(x, y + 25, 450, 22);
        using var track = new SolidBrush(Color.FromArgb(30, 38, 37));
        using var fill = new SolidBrush(Color.FromArgb(190, _teal));
        using var edge = new Pen(Color.FromArgb(76, _teal));
        g.FillRectangle(track, slider);
        g.FillRectangle(fill, slider.X, slider.Y, slider.Width * volume / 100f, slider.Height);
        g.DrawRectangle(edge, slider.X, slider.Y, slider.Width, slider.Height);
        var knobX = slider.X + slider.Width * volume / 100f;
        using var knob = new SolidBrush(_ink);
        g.FillEllipse(knob, knobX - 6, slider.Y - 3, 12, slider.Height + 6);
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
    private static string PreferencesPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FazbearPizzeriaAfterHours", "settings.json");
    private const string StartupRunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string StartupValueName = "FazbearPizzeriaAfterHours";

    private void LoadPreferences()
    {
        try
        {
            if (!File.Exists(PreferencesPath)) return;
            var preferences = JsonSerializer.Deserialize<GamePreferences>(File.ReadAllText(PreferencesPath));
            if (preferences is null) return;
            _musicVolume = Math.Clamp(preferences.MusicVolume, 0, 100);
            _effectsVolume = Math.Clamp(preferences.EffectsVolume, 0, 100);
            _isFullscreen = preferences.Fullscreen;
            _autoLaunchOnSignIn = preferences.AutoLaunchOnSignIn;
            NightSounds.SetVolumes(_musicVolume, _effectsVolume);
        }
        catch { }
    }

    private void SavePreferences()
    {
        try
        {
            var directory = Path.GetDirectoryName(PreferencesPath)!;
            Directory.CreateDirectory(directory);
            File.WriteAllText(PreferencesPath, JsonSerializer.Serialize(new GamePreferences(_musicVolume, _effectsVolume, _isFullscreen, _autoLaunchOnSignIn)));
        }
        catch { _settingsMessage = "Could not save settings. They may reset next launch."; }
    }

    private void SetAutoLaunchOnSignIn(bool enabled)
    {
        try
        {
            using var runKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(StartupRunKey, writable: true);
            if (enabled)
            {
                var executable = Environment.ProcessPath ?? Application.ExecutablePath;
                runKey.SetValue(StartupValueName, $"\"{executable}\"");
            }
            else runKey.DeleteValue(StartupValueName, throwOnMissingValue: false);
            _autoLaunchOnSignIn = enabled;
            SavePreferences();
            _settingsMessage = enabled ? "The game will launch when you sign in to Windows." : "Automatic Windows sign-in launch is off.";
        }
        catch
        {
            _settingsMessage = "Windows could not update the launch-at-sign-in setting.";
        }
    }

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
        catch { _settingsMessage = "Could not save controls. They will reset next launch."; }
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
                if (slot is { Version: >= 1, OwnedAnimatronics: not null, StagedAnimatronics: not null,
                    BuiltAttractions: not null, AttractionTiles: not null, AiLevels: not null,
                    UnlockedAnimatronicGames: not null }) _saveSlots[i] = slot;
            }
            catch { }
        }
    }

    private void StartNewGameInSlot(int slot)
    {
        if (_saveSlotFilesPresent[slot]) return;
        NightSounds.StopMenuMusic();
        _timer.Stop();
        _activeSaveSlot = slot;
        _funds = _sheets.StartingFunds;
        _day = 1;
        _payoutPaidDay = 0;
        _lawsuitCheckedDay = 0;
        _payoutCollectedThisNight = 0;
        _payoutAiBonusCollected = 0;
        _payoutAnimatronicBonusCollected = 0;
        _lawsuitsThisShift = 0;
        _lawsuitFeesThisShift = 0;
        _owned.Clear();
        _owned.Add("freddy");
        _unlockedAnimatronicGames.Clear();
        _unlockedAnimatronicGames.Add("fnaf1");
        _purchasedCatalogCategories.Clear(); _purchasedCatalogCategories.Add("dumpster");
        _stageAnimatronics.Clear();
        _builtAttractions.Clear();
        _placedAttractions.Clear();
        _ai.Clear();
        foreach (var animatronic in _sheets.Animatronics) _ai[animatronic.Id] = _sheets.DefaultAiLevel;
        _nightStarted = false;
        _nightOver = false;
        _nightPaused = false;
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
        NightSounds.StopMenuMusic();
        _activeSaveSlot = slot;
        _funds = Math.Max(0, data.Funds);
        _day = Math.Max(1, data.Day);
        _payoutPaidDay = Math.Max(0, data.PayoutPaidDay);
        _lawsuitCheckedDay = Math.Max(0, data.LawsuitCheckedDay);
        _payoutCollectedThisNight = Math.Max(0, data.PayoutCollectedThisNight);
        _payoutAiBonusCollected = Math.Max(0, data.AiPayoutBonus);
        _payoutAnimatronicBonusCollected = Math.Max(0, data.AnimatronicPayoutBonus);
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
        _unlockedAnimatronicGames.Add("fnaf1");
        var savedCategoryIds = (data.UnlockedAnimatronicGames ?? []).Where(id => _sheets.Games.Any(game => game.Id == id));
        // UCN became a paid category in save version 3. Older saves may contain it
        // because the category-unlock cheat used to unlock every category.
        _unlockedAnimatronicGames.UnionWith(savedCategoryIds.Where(id => !(id.Equals("ucn", StringComparison.OrdinalIgnoreCase) && data.Version < 3)));
        _purchasedCatalogCategories.Clear(); _purchasedCatalogCategories.Add("dumpster");
        _purchasedCatalogCategories.UnionWith((data.PurchasedCatalogCategories ?? []).Where(CatalogCategoryIds.Contains));
        _dayStartSnapshot = data.DayStartState;
        if (data.Version < 3 && _dayStartSnapshot is { } oldSnapshot)
            _dayStartSnapshot = oldSnapshot with { UnlockedAnimatronicGames = oldSnapshot.UnlockedAnimatronicGames.Where(id => !id.Equals("ucn", StringComparison.OrdinalIgnoreCase)).ToArray() };
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
            var data = new SaveGameData(3, DateTime.UtcNow, _day, _funds,
                _owned.Order(StringComparer.OrdinalIgnoreCase).ToArray(), _stageAnimatronics.Order(StringComparer.OrdinalIgnoreCase).ToArray(),
                _builtAttractions.Order(StringComparer.OrdinalIgnoreCase).ToArray(),
                _placedAttractions.ToDictionary(pair => pair.Key, pair => pair.Value.Y * 8 + pair.Value.X, StringComparer.OrdinalIgnoreCase),
                new Dictionary<string, int>(_ai, StringComparer.OrdinalIgnoreCase),
                _unlockedAnimatronicGames.Order(StringComparer.OrdinalIgnoreCase).ToArray(), _payoutPaidDay, _lawsuitCheckedDay,
                (int)_prepScreen, _nightOver, _nightWon, _payoutCollectedThisNight, _lawsuitsThisShift, _lawsuitFeesThisShift,
                _payoutAiBonusCollected, _dayStartSnapshot, _strikes, _dayResetByDeath, _payoutAnimatronicBonusCollected,
                _purchasedCatalogCategories.Order(StringComparer.OrdinalIgnoreCase).ToArray());
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
            _unlockedAnimatronicGames.Order(StringComparer.OrdinalIgnoreCase).ToArray(), _payoutPaidDay, _lawsuitCheckedDay,
            _purchasedCatalogCategories.Order(StringComparer.OrdinalIgnoreCase).ToArray());
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
        _unlockedAnimatronicGames.Clear(); _unlockedAnimatronicGames.Add("fnaf1");
        _unlockedAnimatronicGames.UnionWith(snapshot.UnlockedAnimatronicGames.Where(id => _sheets.Games.Any(game => game.Id == id)));
        _purchasedCatalogCategories.Clear(); _purchasedCatalogCategories.Add("dumpster");
        _purchasedCatalogCategories.UnionWith((snapshot.PurchasedCatalogCategories ?? []).Where(CatalogCategoryIds.Contains));
        _payoutCollectedThisNight = 0;
        _payoutAiBonusCollected = 0;
        _payoutAnimatronicBonusCollected = 0;
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
        DrawText(g, $"Pizzeria rating {points} · Buy catalog access with available cash, then purchase individual attractions.", _bodyFont, _muted, 50, 178, 1172, 24);
        _catalogCategoryRects.Clear();
        for (var i = 0; i < CatalogCategories.Length; i++)
        {
            var category = CatalogCategories[i];
            var rect = new RectangleF(48 + i * 296, 207, 284, 32);
            _catalogCategoryRects[category.Id] = rect;
            var unlocked = IsCatalogCategoryUnlocked(category.Id);
            var label = unlocked ? category.Name : $"LOCKED · ${category.Cost:N0}";
            DrawButton(g, rect, label, unlocked ? category.Id == _selectedCatalogCategory ? "✓" : "→" : "BUY", !unlocked ? _muted : category.Id == _selectedCatalogCategory ? _gold : _teal, category.Id == _selectedCatalogCategory ? Color.FromArgb(47, 35, 19) : _panel, unlocked || _funds >= category.Cost);
        }
        if (!IsCatalogCategoryUnlocked(_selectedCatalogCategory)) _selectedCatalogCategory = "dumpster";
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
            var replaced = IsTablewareUpgradeSuperseded(attraction.Id);
            var canBuy = !_builtAttractions.Contains(attraction.Id) && !replaced && _funds >= attraction.Cost;
            var stateLabel = _builtAttractions.Contains(attraction.Id) ? "BUILT" : replaced ? "REPLACED" : "BUY";
            DrawButton(g, buy, stateLabel, _builtAttractions.Contains(attraction.Id) ? "✓" : $"${attraction.Cost:N0}", canBuy ? _teal : _muted, Color.FromArgb(13, 28, 28), canBuy);
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
        var stageCrew = _sheets.Animatronics.Where(a => _stageAnimatronics.Contains(a.Id) && IsAnimatronicSourceAvailable(a)).ToArray();
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
        DrawText(g, "FIXED TABLES · TABLEWARE SET FITS BOTH · STATION BY STAGE", _smallFont, _muted, floor.X + 16, floor.Bottom - 30, floor.Width - 32, 20);
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
        var purchased = _sheets.Attractions.Where(a => _builtAttractions.Contains(a.Id) && !IsAttractionEffectivelyPlaced(a) && !IsTablewareUpgradeSuperseded(a.Id)).ToArray();
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
        {
            var setId = _builtAttractions.Contains("neon-party-set") ? "neon-party-set"
                : _builtAttractions.Contains("foil-party-set") ? "foil-party-set" : "paper-cups";
            var set = _sheets.Attractions.FirstOrDefault(item => item.Id == setId);
            var style = set?.Style ?? "cups";
            DrawAttractionIcon(g, style, new RectangleF(table.X + 19, table.Y + 14, table.Width - 38, table.Height - 16), false, setId);
        }
        else if (_builtAttractions.Contains("neon-party-set") || _builtAttractions.Contains("foil-party-set"))
        {
            var setId = _builtAttractions.Contains("neon-party-set") ? "neon-party-set" : "foil-party-set";
            var set = _sheets.Attractions.First(item => item.Id == setId);
            DrawAttractionIcon(g, set.Style, new RectangleF(table.X + 19, table.Y + 14, table.Width - 38, table.Height - 16), false, setId);
        }
    }

    private static bool IsDiningTableTile(Point cell) => cell.Y == 1 && cell.X is 1 or 2 or 4 or 5;

    private bool IsAttractionEffectivelyPlaced(Attraction attraction)
    {
        if (!_builtAttractions.Contains(attraction.Id)) return false;
        var upgradedTableware = _builtAttractions.Contains("neon-party-set") || _builtAttractions.Contains("foil-party-set");
        if (attraction.Id == "paper-cups") return !upgradedTableware;
        if (attraction.Id == "foil-party-set") return !_builtAttractions.Contains("neon-party-set");
        if (attraction.Id is "sanitation-station" or "foil-party-set" or "neon-party-set") return true;
        return _placedAttractions.ContainsKey(attraction.Id);
    }

    private bool IsTablewareUpgradeSuperseded(string id) =>
        id == "paper-cups" && (_builtAttractions.Contains("foil-party-set") || _builtAttractions.Contains("neon-party-set")) ||
        id == "foil-party-set" && _builtAttractions.Contains("neon-party-set");

    private (int Atmosphere, int HealthSafety, int Entertainment, int BonusRevenue, int LiabilityRisk) PizzeriaRating()
    {
        var placed = _sheets.Attractions.Where(IsAttractionEffectivelyPlaced).ToArray();
        var performers = _sheets.Animatronics.Where(a => _stageAnimatronics.Contains(a.Id) && IsAnimatronicSourceAvailable(a)).ToArray();
        return (
            placed.Sum(a => a.Atmosphere) + performers.Sum(a => a.Atmosphere),
            placed.Sum(a => a.HealthSafety) + performers.Sum(a => a.HealthSafety),
            placed.Sum(a => a.Entertainment) + performers.Sum(a => a.Entertainment),
            placed.Sum(a => a.BonusRevenue) + performers.Sum(a => a.BonusRevenue),
            Math.Max(0, placed.Sum(a => a.LiabilityRisk) - placed.Sum(a => a.HealthSafety) - performers.Sum(a => a.HealthSafety)));
    }

    private static int CatalogCategoryCost(string category) => CatalogCategories.FirstOrDefault(entry => entry.Id.Equals(category, StringComparison.OrdinalIgnoreCase)).Cost;

    private int AnimatronicCategoryCost(string gameId) =>
        _sheets.Games.FirstOrDefault(game => game.Id.Equals(gameId, StringComparison.OrdinalIgnoreCase))?.AnimatronicUnlockCost ?? int.MaxValue;

    private bool IsAnimatronicSourceAvailable(Animatronic a) => _sources.TryGetValue(a.GameId, out var source) && source.IsAvailable;

    private bool IsAnimatronicSourceAvailable(string id)
    {
        var animatronic = _sheets.Animatronics.FirstOrDefault(a => a.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        return animatronic is not null && IsAnimatronicSourceAvailable(animatronic);
    }

    private bool IsCatalogCategoryUnlocked(string category, int? points = null) => _purchasedCatalogCategories.Contains(category);

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
        _rosterViewportRect = new RectangleF(48, 319, 1174, 280);
        _rosterGroupHeaderRects.Clear();
        _animatronicCategoryUnlockRects.Clear();
        _rosterCardRects.Clear();
        _rosterAiMinusRect = RectangleF.Empty;
        _rosterAiPlusRect = RectangleF.Empty;
        _rosterSellRect = RectangleF.Empty;

        const int slotsPerRow = 12;
        const float cardW = 88, cardH = 98, cardGap = 5, groupGap = 10, headerH = 27, cardTopGap = 5;
        var fullHeight = 0;
        foreach (var game in _sheets.Games)
        {
            var gameRows = Math.Max(1, (game.RosterSlots + slotsPerRow - 1) / slotsPerRow);
            var rowHeight = _sources[game.Id].IsAvailable && _unlockedAnimatronicGames.Contains(game.Id) ? gameRows * (cardH + cardTopGap) : 76;
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
            var categoryStatus = !source.IsAvailable ? "SOURCE GAME REQUIRED" : categoryUnlocked ? $"CATEGORY OPEN · {boughtCount} OWNED" : $"LOCKED · UNLOCK ${categoryCost:N0}";
            DrawText(g, categoryStatus, _smallFont, source.IsAvailable ? categoryUnlocked ? _teal : _gold : Color.FromArgb(210, 125, 105), header.X + 250, header.Y + 6, 380, 18);
            DrawText(g, $"{game.RosterSlots} PORTRAIT SLOTS", _smallFont, _muted, header.Right - 195, header.Y + 6, 180, 18, StringAlignment.Far);
            y += headerH + groupGap;
            if (collapsed) { y += groupGap; continue; }

            if (!source.IsAvailable)
            {
                var lockedPanel = new RectangleF(_rosterViewportRect.X + 19, y, 1095, 66);
                DrawPanel(g, lockedPanel, Color.FromArgb(20, 17, 23));
                DrawText(g, "download the corresponding game to unlock its animatronics", _smallFont, Color.FromArgb(210, 125, 105), lockedPanel.X + 14, lockedPanel.Y + 23, lockedPanel.Width - 28, 20, StringAlignment.Center);
                y += 76;
                continue;
            }

            if (!categoryUnlocked)
            {
                var unlockPanel = new RectangleF(_rosterViewportRect.X + 19, y, 1095, 66);
                DrawPanel(g, unlockPanel, Color.FromArgb(20, 17, 23));
                DrawText(g, $"{ShortName(game.Name).ToUpperInvariant()} CHARACTER FILE", _smallFont, _ink, unlockPanel.X + 14, unlockPanel.Y + 10, 330, 18);
                DrawText(g, "One-time category unlock; characters still cost extra.", _smallFont, _muted, unlockPanel.X + 14, unlockPanel.Y + 34, 610, 18);
                var unlockButton = new RectangleF(unlockPanel.Right - 250, unlockPanel.Y + 12, 234, 42);
                _animatronicCategoryUnlockRects[game.Id] = unlockButton;
                DrawButton(g, unlockButton, "UNLOCK CATEGORY", $"${categoryCost:N0}", _gold, Color.FromArgb(47, 35, 19), _funds >= categoryCost);
                y += 76;
                continue;
            }

            var slots = Math.Max(slotsPerRow, game.RosterSlots);
            var cardX = _rosterViewportRect.X + 19;
            var ordered = new Animatronic?[slots];
            var placed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var a in performers.Where(a => a.RosterSlot >= 0 && a.RosterSlot < slots))
            {
                if (ordered[a.RosterSlot] is null) { ordered[a.RosterSlot] = a; placed.Add(a.Id); }
            }
            var cursor = 0;
            foreach (var a in performers)
            {
                if (placed.Contains(a.Id)) continue;
                while (cursor < slots && ordered[cursor] is not null) cursor++;
                if (cursor >= slots) break;
                ordered[cursor] = a;
                placed.Add(a.Id);
                cursor++;
            }
            for (var slot = 0; slot < slots; slot++)
            {
                var card = new RectangleF(cardX + (slot % slotsPerRow) * (cardW + cardGap), y + (slot / slotsPerRow) * (cardH + cardTopGap), cardW, cardH);
                if (ordered[slot] is not { } animatronic)
                {
                    DrawPanel(g, card, Color.FromArgb(12, 14, 15));
                    DrawText(g, "EMPTY", _smallFont, Color.FromArgb(82, 91, 91), card.X + 5, card.Y + 40, card.Width - 10, 18, StringAlignment.Center);
                    continue;
                }
                if (animatronic.SalvageOnly && !_owned.Contains(animatronic.Id) && !_salvageRevealed)
                {
                    DrawPanel(g, card, Color.FromArgb(12, 14, 15));
                    using (var black = new SolidBrush(Color.Black))
                        g.FillRectangle(black, card.X + 13, card.Y + 5, 62, 62);
                    var hiddenBadge = new RectangleF(card.X + 5, card.Bottom - 25, card.Width - 10, 19);
                    using (var fill = new SolidBrush(Color.FromArgb(13, 18, 18))) g.FillRectangle(fill, hiddenBadge);
                    DrawText(g, "SALVAGE ONLY", _smallFont, _gold, hiddenBadge.X + 2, hiddenBadge.Y + 2, hiddenBadge.Width - 4, 16, StringAlignment.Center);
                    continue;
                }
                _rosterCardRects[animatronic.Id] = card;
                if (card.Contains(_mouse)) _rosterHoveredAnimatronic = animatronic.Id;
                DrawRosterPortraitCard(g, animatronic, card, _stageAnimatronics.Contains(animatronic.Id));
            }
            y += (slots + slotsPerRow - 1) / slotsPerRow * (cardH + cardTopGap) + groupGap;
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
        DrawText(g, bought ? $"AI {_ai[a.Id]:00}" : a.SalvageOnly ? "SALVAGE ONLY" : $"$ {a.Cost:N0}", _smallFont, bought ? staged ? _gold : _teal : _gold, badge.X + 2, badge.Y + 2, badge.Width - 4, 16, StringAlignment.Center);
    }

    private void DrawRosterTooltip(Graphics g)
    {
        var panel = new RectangleF(48, 209, 1174, 101);
        DrawPanel(g, panel, Color.FromArgb(15, 20, 20));
        if (_rosterHoveredAnimatronic is null)
        {
            _rosterAiMinusRect = RectangleF.Empty; _rosterAiPlusRect = RectangleF.Empty; _rosterSellRect = RectangleF.Empty;
            DrawText(g, "HOVER A PORTRAIT FOR ITS RULES AND PIZZERIA RATINGS", _smallFont, _muted, panel.X + 14, panel.Y + 38, panel.Width - 28, 20);
            return;
        }
        var a = _sheets.Animatronics.First(entry => entry.Id == _rosterHoveredAnimatronic);
        DrawText(g, a.Name, _panelTitleFont, _ink, panel.X + 14, panel.Y + 5, 330, 22);
        DrawText(g, $"ATM {a.Atmosphere} · H&S {a.HealthSafety} · ENT {a.Entertainment} · REV {a.BonusRevenue}", _smallFont, _teal, panel.X + 352, panel.Y + 8, panel.Width - 366, 18, StringAlignment.Far);
        DrawText(g, a.Rule, _smallFont, _muted, panel.X + 14, panel.Y + 34, panel.Width - 28, 38);
        if (!_owned.Contains(a.Id))
        {
            _rosterAiMinusRect = RectangleF.Empty; _rosterAiPlusRect = RectangleF.Empty; _rosterSellRect = RectangleF.Empty;
            DrawText(g, a.SalvageOnly ? "SALVAGE ONLY · EARNED IN THE BACK ALLEY" : $"PRICE ${a.Cost:N0} · CLICK TO BUY", _smallFont, _gold, panel.X + 746, panel.Y + 76, 410, 18, StringAlignment.Far);
            return;
        }
        DrawText(g, _stageAnimatronics.Contains(a.Id) ? "ON STAGE" : "OFF STAGE", _smallFont, _stageAnimatronics.Contains(a.Id) ? _gold : _muted, panel.X + 746, panel.Y + 76, 90, 18, StringAlignment.Far);
        DrawText(g, "AI", _smallFont, _muted, panel.X + 850, panel.Y + 76, 22, 18);
        _rosterAiMinusRect = new RectangleF(panel.X + 878, panel.Y + 72, 24, 24);
        _rosterAiPlusRect = new RectangleF(panel.X + 974, panel.Y + 72, 24, 24);
        DrawAiStepButton(g, _rosterAiMinusRect, "−", _ai[a.Id] > 0);
        DrawText(g, _ai[a.Id].ToString("00"), _smallFont, _gold, panel.X + 910, panel.Y + 76, 56, 17, StringAlignment.Center);
        DrawAiStepButton(g, _rosterAiPlusRect, "+", _ai[a.Id] < _sheets.MaximumAiLevel);
        _rosterSellRect = new RectangleF(panel.X + 1010, panel.Y + 72, 140, 24);
        DrawButton(g, _rosterSellRect, "SELL", $"+${a.Cost / 2:N0}", _gold, Color.FromArgb(47, 35, 19), true);
    }

    private void SellHoveredAnimatronic()
    {
        var id = _rosterHoveredAnimatronic;
        if (id is null || !_owned.Contains(id)) return;
        var a = _sheets.Animatronics.FirstOrDefault(entry => entry.Id == id);
        if (a is null) return;
        _owned.Remove(id);
        _stageAnimatronics.Remove(id);
        _funds += a.Cost / 2;
        SaveActiveSlot();
        Invalidate();
    }

    private void DrawPrepFooter(Graphics g)
    {
        _startNightRect = new RectangleF(936, 624, 296, 43);
        var canStartNight = ActiveThreatCount() > 0;
        DrawButton(g, _startNightRect, "OPEN FOR THE NIGHT", canStartNight ? "→" : "STAGE 1+", _gold, Color.FromArgb(47, 35, 19), canStartNight);
        if (_prepScreen is PrepScreen.Builder or PrepScreen.Blueprint)
        {
            var payout = _payoutPaidDay == _day ? 0 : NightlyPayout();
            var multiplier = StagedAiPayoutMultiplier();
            var aiBonus = (int)Math.Round(payout * multiplier, MidpointRounding.AwayFromZero);
            DrawText(g, $"SHIFT PAYOUT: +${payout:N0}  ·  STAGE AI +{multiplier:P2}  (EST. +${aiBonus:N0})", _smallFont, _teal, 48, 640, 1120);
        }
        else
        {
            var linked = _sources.Values.Count(s => s.Game.Id != "fnaf-mades" && s.IsAvailable);
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
        else if (_pendingPurchaseKind == PurchaseKind.CatalogCategory && _pendingPurchaseId is not null)
        {
            var category = CatalogCategories.FirstOrDefault(entry => entry.Id == _pendingPurchaseId);
            if (category.Id is not null) { name = $"{category.Name} CATALOG"; cost = category.Cost; }
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
            if (item is not null && !item.SalvageOnly && IsAnimatronicSourceAvailable(item) && !_owned.Contains(item.Id) && _unlockedAnimatronicGames.Contains(item.GameId) && _funds >= item.Cost)
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
                if (item.Id == "foil-party-set") _builtAttractions.Remove("paper-cups");
                else if (item.Id == "neon-party-set")
                {
                    _builtAttractions.Remove("paper-cups");
                    _builtAttractions.Remove("foil-party-set");
                }
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
        else if (_pendingPurchaseKind == PurchaseKind.CatalogCategory && _pendingPurchaseId is not null)
        {
            var category = _pendingPurchaseId;
            var cost = CatalogCategoryCost(category);
            if (CatalogCategoryIds.Contains(category) && !_purchasedCatalogCategories.Contains(category) && _funds >= cost)
            {
                _funds -= cost;
                _purchasedCatalogCategories.Add(category);
                _selectedCatalogCategory = category;
                _catalogMessage = "Catalog purchased. Browse its attractions below.";
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
        DrawText(g, source.IsAvailable ? "SOURCE CONNECTED" : "download the corresponding game to unlock its animatronics", _smallFont, source.IsAvailable ? Color.FromArgb(123, 186, 143) : Color.FromArgb(210, 125, 105), r.X + 108, r.Y + 40, r.Width - 125, 42);
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
            if (!source.IsAvailable) return;
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
        DrawText(g, "DRAIN WHEN ON: MON 1%/s · CAMS 1%/s · LIGHTS 1%/s", _smallFont, _muted, right.X + 22, right.Y + 251, right.Width - 44, 15);
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

    private void DrawPauseOverlay(Graphics g)
    {
        using var shade = new SolidBrush(Color.FromArgb(190, 0, 0, 0));
        g.FillRectangle(shade, 0, 88, DesignWidth, 592);
        var panel = new RectangleF(330, 286, 620, 122);
        DrawPanel(g, panel, Color.FromArgb(22, 17, 23));
        using var border = new Pen(_gold, 2);
        g.DrawRectangle(border, panel.X, panel.Y, panel.Width, panel.Height);
        DrawText(g, "GAME PAUSED", _titleFont, _gold, panel.X + 24, panel.Y + 22, panel.Width - 48, 44, StringAlignment.Center);
        DrawText(g, "PRESS P TO RESUME", _smallFont, _ink, panel.X + 24, panel.Y + 77, panel.Width - 48, 20, StringAlignment.Center);
    }

    private static void DrawPeeMonsterTint(Graphics g)
    {
        using var tint = new SolidBrush(Color.FromArgb(72, 255, 218, 61));
        g.FillRectangle(tint, 0, 0, DesignWidth, DesignHeight);
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
        DrawText(g, "ULTIMATE FAZBEAR", _smallFont, Color.FromArgb(105, 122, 112), r.X + 22, r.Bottom - 35);
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
        if (_mangleOnCeiling && _portraitArt.TryGetValue("mangle", out var ceilingMangle))
        {
            var ceilingImage = new RectangleF(r.X + r.Width * .43f, r.Y + 45, r.Width * .15f, 105);
            g.DrawImage(ceilingMangle, ceilingImage);
            DrawText(g, "MANGLE ON THE CEILING", _smallFont, _red, ceilingImage.X - 40, ceilingImage.Bottom + 4, ceilingImage.Width + 80, 18, StringAlignment.Center);
        }
        if (_shadowInOffice && _shadowOfficeArt is not null)
        {
            var shadowFigure = FitImage(_shadowOfficeArt, new RectangleF(r.X + r.Width * .33f, r.Y + 90, r.Width * .34f, 330));
            g.DrawImage(_shadowOfficeArt, shadowFigure);
            DrawText(g, "SHADOW BONNIE — MASK!", _smallFont, _red, shadowFigure.X - 60, shadowFigure.Bottom + 4, shadowFigure.Width + 120, 18, StringAlignment.Center);
        }
        if (threat is null && !intruderVisible && !_mangleOnCeiling && !_shadowInOffice)
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
        _puppetWindRect = RectangleF.Empty;
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
        var contentY = panel.Y + 62;
        var contentHeight = panel.Height - 80;
        var feed = new RectangleF(panel.X + 18, contentY, panel.Width * .47f, contentHeight);
        using (var fill = new SolidBrush(Color.FromArgb(18, 25, 24))) g.FillRectangle(fill, feed);
        using (var border = new Pen(_edge)) g.DrawRectangle(border, feed.X, feed.Y, feed.Width, feed.Height);
        var camera = FnafCameras.FirstOrDefault(c => c.Id == _selectedCamera);
        DrawText(g, $"CAM {camera.Id}  /  {camera.Name}", _smallFont, _gold, feed.X + 14, feed.Y + 12, feed.Width - 28, 18);
        var room = new RectangleF(feed.X + 12, feed.Y + 42, feed.Width - 24, Math.Min(250, feed.Height * .52f));
        using (var roomFill = new LinearGradientBrush(room, Color.FromArgb(31, 41, 39), Color.FromArgb(8, 12, 12), 90f)) g.FillRectangle(roomFill, room);
        DrawLine(g, Color.FromArgb(85, 110, 103), 1, room.X + 12, room.Y + room.Height * .67f, room.Right - 12, room.Y + room.Height * .67f);
        DrawLine(g, Color.FromArgb(85, 110, 103), 1, room.X + 24, room.Bottom - 18, room.Right - 24, room.Bottom - 18);
        var occupants = _animatronicCameras.Where(x => x.Value == camera.Id)
            .Select(x => _sheets.Animatronics.FirstOrDefault(a => a.Id == x.Key)).Where(a => a is not null).ToArray();
        var goldenPoster = camera.Id == "2B" && _goldenFreddyReady && _portraitArt.ContainsKey("golden-freddy");
        if (goldenPoster && _portraitArt.TryGetValue("golden-freddy", out var goldenPortraitImage))
        {
            var poster = new RectangleF(room.X + room.Width / 2 - 78, room.Y + 8, 156, room.Height - 25);
            g.DrawImage(goldenPortraitImage, poster);
            DrawText(g, "POSTER CHANGED · LOWER MONITOR", _smallFont, _gold, room.X + 8, room.Bottom - 20, room.Width - 16, 18, StringAlignment.Center);
        }
        else if (occupants.Length == 0) DrawText(g, "NO MOVEMENT", _bodyFont, _muted, room.X + 28, room.Y + 84, room.Width - 56, 28, StringAlignment.Center);
        else
        {
            var visibleCount = Math.Min(4, occupants.Length);
            var portraitWidth = Math.Min(160, (room.Width - 42f) / visibleCount - 12);
            var portraitHeight = Math.Min(174, room.Height - 49);
            var groupWidth = visibleCount * portraitWidth + (visibleCount - 1) * 12;
            var startX = room.X + (room.Width - groupWidth) / 2;
            for (var i = 0; i < visibleCount; i++)
            {
                var portrait = new RectangleF(startX + i * (portraitWidth + 12), room.Y + 8, portraitWidth, portraitHeight);
                DrawPortraitFrame(g, occupants[i]!, portrait);
                DrawText(g, occupants[i]!.Name.ToUpperInvariant(), _smallFont, _ink, portrait.X - 2, portrait.Bottom + 4, portrait.Width + 4, 18, StringAlignment.Center);
            }
            if (occupants.Length > visibleCount) DrawText(g, $"+{occupants.Length - visibleCount} MORE ON THIS CAMERA", _smallFont, _gold, room.X, room.Bottom - 20, room.Width, 18, StringAlignment.Center);
        }
        if (_mangleStaticSeconds > 0) DrawMangleStatic(g, room);
        DrawText(g, "LIVE CAMERA FEED · ANIMATRONIC PORTRAITS", _smallFont, _teal, feed.X + 12, room.Bottom + 17, feed.Width - 24, 18);
        if (IsAnimatronicSourceAvailable("puppet") && _stageAnimatronics.Contains("puppet") && _ai.GetValueOrDefault("puppet") > 0)
        {
            var boxLabel = _puppetEscaped ? "MUSIC BOX EMPTY · PUPPET LOOSE" : $"MUSIC BOX  {_puppetMusicBox}%";
            DrawText(g, boxLabel, _smallFont, _puppetEscaped ? _red : _gold, feed.X + 12, feed.Bottom - 77, feed.Width - 24, 18, StringAlignment.Center);
            var meter = new RectangleF(feed.X + 18, feed.Bottom - 55, feed.Width - 36, 8);
            using (var track = new SolidBrush(Color.FromArgb(38, 46, 43))) g.FillRectangle(track, meter);
            if (!_puppetEscaped && _puppetMusicBox > 0)
            {
                using var fill = new SolidBrush(_puppetMusicBox > 25 ? _teal : _red);
                g.FillRectangle(fill, new RectangleF(meter.X, meter.Y, meter.Width * _puppetMusicBox / 100f, meter.Height));
            }
            if (!_puppetEscaped)
            {
                _puppetWindRect = new RectangleF(feed.X + 18, feed.Bottom - 42, feed.Width - 36, 30);
                DrawButton(g, _puppetWindRect, "WIND MUSIC BOX  +25%", "CLICK", _gold, Color.FromArgb(29, 26, 18), true);
            }
        }

        var map = new RectangleF(feed.Right + 12, contentY, panel.Right - feed.Right - 30, contentHeight);
        using (var fill = new SolidBrush(Color.FromArgb(12, 17, 17))) g.FillRectangle(fill, map);
        using (var border = new Pen(_edge)) g.DrawRectangle(border, map.X, map.Y, map.Width, map.Height);
        DrawText(g, "CAMERA MAP · SELECT A FEED", _smallFont, _teal, map.X + 12, map.Y + 8, map.Width - 24, 18);
        var placements = new Dictionary<string, PointF>
        {
            ["1A"] = new(.50f, .07f), ["1B"] = new(.50f, .27f), ["1C"] = new(.15f, .07f), ["10"] = new(.85f, .07f),
            ["2A"] = new(.16f, .41f), ["2B"] = new(.16f, .53f), ["3"] = new(.39f, .41f), ["4A"] = new(.68f, .41f),
            ["4B"] = new(.68f, .53f), ["5"] = new(.50f, .17f), ["6"] = new(.85f, .53f), ["7"] = new(.85f, .41f),
            ["F2-KC"] = new(.14f, .66f), ["F2-PC"] = new(.38f, .66f), ["F2-GA"] = new(.62f, .66f), ["F2-MH"] = new(.86f, .66f),
            ["F2-RV"] = new(.14f, .80f), ["F2-PR2"] = new(.38f, .80f), ["F2-PR1"] = new(.62f, .80f), ["F2-OH"] = new(.86f, .80f)
        };
        PointF CenterOf(string id) => new(map.X + map.Width * placements[id].X, map.Y + map.Height * placements[id].Y + 15);
        var links = new[] { ("1A", "5"), ("5", "1B"), ("1A", "1C"), ("1A", "10"), ("10", "1B"), ("1B", "2A"), ("2A", "2B"), ("3", "2A"), ("1B", "7"), ("7", "6"), ("1B", "4A"), ("4A", "4B"),
            ("F2-KC", "F2-PC"), ("F2-PC", "F2-GA"), ("F2-GA", "F2-MH"), ("F2-MH", "F2-OH"), ("F2-OH", "F2-PR1"), ("F2-PR1", "F2-PR2"), ("F2-PR2", "F2-RV") };
        using (var corridor = new Pen(Color.FromArgb(85, 95, 83, 65), 3))
            foreach (var (from, to) in links) g.DrawLine(corridor, CenterOf(from), CenterOf(to));
        var nodeW = Math.Min(94, map.Width * .22f);
        const float nodeH = 31;
        foreach (var (id, point) in placements)
        {
            var node = new RectangleF(map.X + map.Width * point.X - nodeW / 2, map.Y + map.Height * point.Y, nodeW, nodeH);
            _cameraSelectRects[id] = node;
            var selected = id == _selectedCamera;
            var occupied = _animatronicCameras.Values.Contains(id, StringComparer.OrdinalIgnoreCase);
            using var nodeFill = new SolidBrush(selected ? Color.FromArgb(53, 39, 21) : Color.FromArgb(24, 31, 30));
            using var nodeEdge = new Pen(occupied ? _red : selected ? _gold : _edge, selected || occupied ? 2 : 1);
            g.FillRectangle(nodeFill, node); g.DrawRectangle(nodeEdge, node.X, node.Y, node.Width, node.Height);
            DrawText(g, id.StartsWith("F2-", StringComparison.OrdinalIgnoreCase) ? id : $"CAM {id}", _smallFont, occupied ? _red : selected ? _gold : _ink, node.X, node.Y + 7, node.Width, 17, StringAlignment.Center);
        }
        var office = new RectangleF(map.X + map.Width * .28f, map.Bottom - 42, map.Width * .44f, 28);
        using var officeFill = new SolidBrush(Color.FromArgb(61, 28, 26)); g.FillRectangle(officeFill, office);
        DrawText(g, "OFFICE", _smallFont, _red, office.X, office.Y + 6, office.Width, 17, StringAlignment.Center);
    }

    private void DrawMangleStatic(Graphics g, RectangleF bounds)
    {
        using var wash = new SolidBrush(Color.FromArgb(150, 178, 188, 187));
        g.FillRectangle(wash, bounds);
        var frame = Environment.TickCount64 / 35;
        for (var i = 0; i < 20; i++)
        {
            var y = bounds.Y + (float)((i * 37 + frame * 13) % Math.Max(1, (int)bounds.Height));
            var xOffset = (float)(Math.Sin(frame * .7 + i * 2.3) * bounds.Width * .24);
            var alpha = 80 + (i * 23 % 150);
            using var noise = new Pen(Color.FromArgb(alpha, 225, 235, 233), i % 5 == 0 ? 3 : 1);
            g.DrawLine(noise, bounds.X + Math.Max(0, xOffset), y, bounds.Right - Math.Max(0, -xOffset), y);
        }
        DrawText(g, "SIGNAL INTERFERENCE", _smallFont, Color.White, bounds.X + 8, bounds.Y + 8, bounds.Width - 16, 20, StringAlignment.Center);
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
        if (_jumpscareAnimatronicId is not null && _jumpscareArt.TryGetValue(_jumpscareAnimatronicId, out var scareImage))
        {
            ImageAnimator.UpdateFrames(scareImage);
            var target = FitImage(scareImage, new RectangleF(70, 20, 1140, 680));
            g.DrawImage(scareImage, target);
            using var edge = new Pen(Color.FromArgb(flash ? 230 : 110, 190, 20, 28), flash ? 12 : 5);
            g.DrawRectangle(edge, target.X, target.Y, target.Width, target.Height);
        }
        else if (_jumpscareAnimatronicId == "mr-ant-tenna" && _tennaModelArt is not null)
        {
            var shake = (int)(Environment.TickCount64 / 35 % 31) - 15;
            var target = FitImage(_tennaModelArt, new RectangleF(390 + shake, 45, 500, 600));
            g.DrawImage(_tennaModelArt, target);
        }
        else if (_jumpscareAnimatronicId is not null && _portraitArt.TryGetValue(_jumpscareAnimatronicId, out var portrait))
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
                ? $"Payout +${_payoutCollectedThisNight:N0} (AI +${_payoutAiBonusCollected:N0}, threat bonus +${_payoutAnimatronicBonusCollected:N0}, {_strikes} strikes)."
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
        _puppetMusicBox = 100; _puppetEscaped = false; _puppetWindRect = RectangleF.Empty;
        _rosterHoveredAnimatronic = null;
        _salvageCandidateId = null; _salvageOfferOpen = false; _salvageActive = false; _salvageDone = false;
        _shadowInOffice = false;
        SaveActiveSlot();
        Invalidate();
    }

    private bool HandleResultButton(PointF point)
    {
        if (!_nightStarted || !_nightOver) return false;
        if (_salvageOfferOpen || _salvageActive || _salvageDone) return HandleSalvageClick(point);
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
        var valueWidth = Math.Clamp(g.MeasureString(value, _smallFont).Width + 8, 36, Math.Max(36, r.Width * .42f));
        var labelWidth = Math.Max(0, r.Width - valueWidth - 34);
        DrawText(g, text, _smallFont, enabled ? fore : Color.FromArgb(92, 99, 96), r.X + 13, r.Y + r.Height / 2 - 8, labelWidth, 18);
        DrawText(g, value, _smallFont, enabled ? fore : Color.FromArgb(92, 99, 96), r.Right - valueWidth - 12, r.Y + r.Height / 2 - 8, valueWidth, 18, StringAlignment.Far);
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

    private static RectangleF FitImage(Image image, RectangleF bounds)
    {
        var scale = Math.Min(bounds.Width / image.Width, bounds.Height / image.Height);
        var width = image.Width * scale;
        var height = image.Height * scale;
        return new RectangleF(bounds.X + (bounds.Width - width) / 2, bounds.Y + (bounds.Height - height) / 2, width, height);
    }

    private static void DrawLine(Graphics g, Color color, float width, float x1, float y1, float x2, float y2)
    {
        using var p = new Pen(color, width); g.DrawLine(p, x1, y1, x2, y2);
    }

    private void StartNight()
    {
        if (ActiveThreatCount() == 0)
        {
            _prepScreen = PrepScreen.Roster;
            _rosterScrollOffset = 0;
            Invalidate();
            return;
        }
        NightSounds.StopMenuMusic();
        _showMenu = false;
        _payoutForNight = _payoutPaidDay == _day ? 0 : NightlyPayout();
        _payoutCollectedThisNight = 0;
        _payoutAiBonusCollected = 0;
        _payoutAnimatronicBonusCollected = 0;
        _dayResetByDeath = false;
        _tennaQuizActive = false; _charlieDebateActive = false; _charlieQuestionPosition = 0; _charlieShuffledAnswers = []; _tennaFailedAnswers = 0; _tennaQuestionOrder = []; _tennaShuffledAnswers = []; _tennaQuizCooldownUntil = 0;
        _tennaFirstEncounterPending = IsAnimatronicSourceAvailable("mr-ant-tenna") && _stageAnimatronics.Contains("mr-ant-tenna") && _ai.GetValueOrDefault("mr-ant-tenna") > 0;
        _tennaHijackCountdown = -1;
        _goldenFreddyReady = false; _goldenFreddyEncounterTriggered = false;
        _nightStarted = true; _nightOver = false; _nightPaused = false; _peeMonsterTintActive = false; _shadowInOffice = false; _mangleOnCeiling = false; _mangleStaticSeconds = 0; _jumpscareActive = false; _officeIntruderId = null; _jumpscareAnimatronicId = null; _seconds = 0; _strikes = 0; _score = 0; _vent = 100; _ventCooldown = 0; _doorPower = 100; _doorPowerDrainCounter = 0; _leftDoorClosed = false; _rightDoorClosed = false; _maskOn = false; _listening = false; _lureSpent = false; _ventScanned = false; _lastResolved = "";
        _monitorOpen = false; _cameraMonitorMode = false; _selectedCamera = "1A"; _leftLightOn = false; _rightLightOn = false; _completedOfficeTasks.Clear(); _activeOfficeTaskId = null; _officeTaskSecondsRemaining = 0; _ventThreatSide = null;
        _message = ActiveThreatCount() == 0
            ? "No animatronics tonight. Raise the terminal monitor and finish all 11 office tasks."
            : $"Raise the terminal monitor and finish all 11 tasks. Survival earns ${_payoutForNight:N0}; loud printing can draw threats in.";
        _pendingUntil.Clear(); _nextAttack.Clear(); _resolved.Clear(); _animatronicCameras.Clear(); _animatronicRouteIndices.Clear();
        foreach (var a in _sheets.Animatronics.Where(a => IsAnimatronicSourceAvailable(a) && _stageAnimatronics.Contains(a.Id) && _ai[a.Id] > 0))
        {
            if (a.Action is "puppet" or "shadow") continue;
            _animatronicCameras[a.Id] = RouteFor(a.Id)[0];
            _animatronicRouteIndices[a.Id] = 0;
            if (!a.Id.Equals("golden-freddy", StringComparison.OrdinalIgnoreCase) && !a.Id.Equals("mr-ant-tenna", StringComparison.OrdinalIgnoreCase))
                _nextAttack[a.Id] = Math.Max(5, a.FirstAttackSeconds - _ai[a.Id] / 2);
        }
        NightSounds.StartAmbience();
        _timer.Start(); SaveActiveSlot(); Invalidate();
    }

    private void ToggleNightPause()
    {
        if (!_nightStarted || _nightOver || _jumpscareActive) return;
        _nightPaused = !_nightPaused;
        if (_nightPaused)
        {
            _timer.Stop();
            NightSounds.StopAmbience();
        }
        else
        {
            NightSounds.StartAmbience();
            _timer.Start();
        }
        Invalidate();
    }

    private void AdvanceNight()
    {
        if (!_nightStarted || _nightOver || _nightPaused) return;
        if (_tennaQuizActive)
        {
            if (--_tennaSecondsRemaining <= 0) SubmitTennaAnswer(null);
            Invalidate();
            return;
        }
        if (_charlieDebateActive)
        {
            if (--_charlieSecondsRemaining <= 0) SubmitCharlieAnswer(null);
            Invalidate();
            return;
        }
        if (_shadowInOffice)
        {
            if (_maskOn)
            {
                _shadowInOffice = false;
                _message = "The Freddy mask drove Shadow Bonnie back into the dark.";
                ResetAnimatronicToStage("shadow-bonnie");
            }
            else if (_seconds >= _shadowDeadline) { FailNightToShadow(); return; }
            Invalidate();
        }
        _seconds++;
        if (_mangleStaticSeconds > 0) _mangleStaticSeconds--;
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
        var equipmentDraw = (_monitorOpen ? 1 : 0) + (_monitorOpen && _cameraMonitorMode ? 1 : 0) + (_leftLightOn ? 1 : 0) + (_rightLightOn ? 1 : 0);
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
                NightSounds.Play("powerdown");
            }
        }
        else if (equipmentDraw == 0)
        {
            _doorPowerDrainCounter = 0;
            _doorPower = Math.Min(100, _doorPower + 1);
        }
        else _doorPowerDrainCounter = 0;
        if (equipmentDraw > 0)
        {
            _doorPower = Math.Max(0, _doorPower - equipmentDraw);
            if (_doorPower == 0)
            {
                _leftDoorClosed = false; _rightDoorClosed = false;
                _leftLightOn = false; _rightLightOn = false;
                _monitorOpen = false; _cameraMonitorMode = false;
                _activeOfficeTaskId = null; _officeTaskSecondsRemaining = 0;
                _message = "Power depleted. Doors opened. Monitor and doorway lights shut down.";
                NightSounds.Play("powerdown");
            }
        }
        if (_monitorOpen && IsAnimatronicSourceAvailable("mr-ant-tenna") && _stageAnimatronics.Contains("mr-ant-tenna") && _ai.GetValueOrDefault("mr-ant-tenna") > 0 &&
            (_tennaHijackCountdown >= 0 || (_seconds >= _tennaQuizCooldownUntil && _random.Next(60) == 0)))
        {
            if (_tennaHijackCountdown > 0) _tennaHijackCountdown--;
            if (_tennaHijackCountdown == 0 || (_tennaHijackCountdown < 0 && _seconds >= _tennaQuizCooldownUntil))
            {
                _tennaHijackCountdown = -1;
                StartTennaQuiz();
                Invalidate();
                return;
            }
        }
        if (IsAnimatronicSourceAvailable("puppet") && _stageAnimatronics.Contains("puppet") && _ai.GetValueOrDefault("puppet") > 0)
        {
            if (!_puppetEscaped)
            {
                var drain = (_ai["puppet"] / 10 + 1) * 2;
                _puppetMusicBox = Math.Max(0, _puppetMusicBox - drain);
                if (_puppetMusicBox == 0)
                {
                    _puppetEscaped = true;
                    _message = "The music box stopped. The Puppet is loose. Lower the monitor to avoid her attack.";
                }
            }
            else if (_monitorOpen && _random.Next(3) == 0)
            {
                _message = "The Puppet attacked from the music box!";
                BeginJumpscare("puppet");
                return;
            }
        }
        if (_ventCooldown > 0) _ventCooldown--;
        if (_seconds % 4 == 0)
        {
            var ventThreatActive = (IsAnimatronicSourceAvailable("springtrap") && _stageAnimatronics.Contains("springtrap") && _ai["springtrap"] > 0) || (IsAnimatronicSourceAvailable("scrap-baby") && _stageAnimatronics.Contains("scrap-baby") && _ai["scrap-baby"] > 0);
            if (ventThreatActive) _vent = Math.Max(0, _vent - 1);
        }
        foreach (var a in _sheets.Animatronics.Where(a => IsAnimatronicSourceAvailable(a) && _stageAnimatronics.Contains(a.Id) && _ai[a.Id] > 0))
        {
            if (a.Action is "puppet" or "shadow") continue;
            if (a.Id.Equals("mangle", StringComparison.OrdinalIgnoreCase) && _mangleOnCeiling) continue;
            if (_nextAttack.TryGetValue(a.Id, out var due) && due <= _seconds && !_pendingUntil.ContainsKey(a.Id))
            {
                var route = RouteFor(a.Id);
                // FNaF 1 uses movement opportunities: each check is an AI/20 roll.
                // Checking cameras holds Freddy in place and slows Foxy's advance from Pirate Cove.
                var cameraPaused = _cameraMonitorMode && _monitorOpen && (a.Id.Equals("freddy", StringComparison.OrdinalIgnoreCase) || a.Id.Equals("foxy", StringComparison.OrdinalIgnoreCase));
                if (!cameraPaused && _random.Next(1, 21) <= _ai[a.Id])
                {
                    var previousCamera = route[_animatronicRouteIndices.GetValueOrDefault(a.Id)];
                    MoveAnimatronicOneCamera(a.Id, 1);
                    var routeIndex = _animatronicRouteIndices.GetValueOrDefault(a.Id);
                    var currentCamera = route[routeIndex];
                    if (a.Id.Equals("mangle", StringComparison.OrdinalIgnoreCase) && previousCamera != currentCamera && _monitorOpen && _cameraMonitorMode && (_selectedCamera == previousCamera || _selectedCamera == currentCamera))
                        _mangleStaticSeconds = 2;
                    NightSounds.Play("movement");
                    if (routeIndex == route.Length - 1)
                    {
                        _pendingUntil[a.Id] = _seconds + _random.Next(3, 7);
                        _resolved.Remove(a.Id);
                        _message = WarningFor(a);
                        if (a.Action == "scan") _ventThreatSide = route[^1] == "V1" ? "left" : "right";
                    }
                }
                _nextAttack[a.Id] = _seconds + MovementOpportunityInterval(a.Id);
            }
            if (_pendingUntil.TryGetValue(a.Id, out var until) && until <= _seconds && !_resolved.Contains(a.Id))
            {
                if (a.Id.Equals("mangle", StringComparison.OrdinalIgnoreCase))
                {
                    _pendingUntil.Remove(a.Id);
                    _resolved.Add(a.Id);
                    _listening = false;
                    if (_maskOn)
                    {
                        var mainHallIndex = Array.IndexOf(RouteFor(a.Id), "F2-MH");
                        _animatronicRouteIndices[a.Id] = mainHallIndex;
                        _animatronicCameras[a.Id] = "F2-MH";
                        _nextAttack[a.Id] = _seconds + MovementOpportunityInterval(a.Id);
                        _message = "The Freddy head drove Mangle out of the right vent and back to the Main Hall.";
                    }
                    else if (_officeIntruderId is not null && !_officeIntruderId.Equals("mangle", StringComparison.OrdinalIgnoreCase) && DateTime.UtcNow < _officeIntruderUntilUtc)
                    {
                        _mangleOnCeiling = true;
                        _animatronicCameras.Remove(a.Id);
                        _message = "Mangle climbed into the office ceiling. The mask cannot repel them now. Lowering the monitor may trigger an attack.";
                    }
                    else
                    {
                        _nextAttack[a.Id] = _seconds + MovementOpportunityInterval(a.Id);
                        _message = "Mangle is waiting in the right air vent for another animatronic to enter the office.";
                    }
                    Invalidate();
                    continue;
                }
                if (a.Id.Equals("charlie-kirk", StringComparison.OrdinalIgnoreCase))
                {
                    _resolved.Add(a.Id);
                    _pendingUntil.Remove(a.Id);
                    StartCharlieDebate();
                    Invalidate();
                    return;
                }
                if (a.Id.Equals("balloon-boy", StringComparison.OrdinalIgnoreCase) && !WasHandled(a))
                {
                    _doorPower = 0;
                    _lastResolved = "Balloon Boy stole all your power";
                    _message = _lastResolved + ". Doors, monitor, and lights are down until power recovers.";
                    NightSounds.Play("powerdown");
                    ResetAnimatronicToStage(a.Id);
                    _resolved.Add(a.Id); _pendingUntil.Remove(a.Id); _listening = false; _ventScanned = false;
                    Invalidate();
                    continue;
                }
                if (WasHandled(a)) { _score += 100 + _ai[a.Id] * 5; _lastResolved = $"{a.Name} returned to the stage"; _message = _lastResolved + ". Stay alert."; ResetAnimatronicToStage(a.Id); }
                else
                {
                    _strikes++; _officeIntruderId = a.Id; _officeIntruderUntilUtc = DateTime.UtcNow.AddMilliseconds(1900); _lastResolved = $"{a.Name} entered the office"; _message = $"{_lastResolved}. Strike {_strikes} of 3.";
                    if (a.Id.Equals("pee-monster", StringComparison.OrdinalIgnoreCase))
                    {
                        _peeMonsterTintActive = true;
                        _message += " A yellow tint will remain until the night ends.";
                    }
                }
                _resolved.Add(a.Id); _pendingUntil.Remove(a.Id); _listening = false; _ventScanned = false;
                if (a.Action == "scan") _ventThreatSide = null;
            }
        }
        if (_strikes >= 3) { BeginJumpscare(_officeIntruderId); return; }
        else if (_seconds >= NightSteps) FinishNight(true);
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
        var loweringMonitor = _monitorOpen;
        _monitorOpen = !_monitorOpen;
        var wasWatchingCameras = loweringMonitor && _cameraMonitorMode;
        if (_monitorOpen) { _cameraMonitorMode = false; _selectedCamera = "1A"; }
        else _cameraMonitorMode = false;
        if (wasWatchingCameras) MaybeSpawnShadow();
        if (_monitorOpen && IsAnimatronicSourceAvailable("mr-ant-tenna") && _tennaFirstEncounterPending && _stageAnimatronics.Contains("mr-ant-tenna") && _ai.GetValueOrDefault("mr-ant-tenna") > 0)
        {
            _tennaFirstEncounterPending = false;
            _tennaHijackCountdown = 5;
            _message = "A signal is forcing its way onto the monitor...";
        }
        NightSounds.Play(_monitorOpen ? "monitor-up" : "monitor-down");
        if (loweringMonitor && _mangleOnCeiling)
        {
            if (_random.Next(1, 21) <= _ai.GetValueOrDefault("mangle"))
            {
                _message = "Mangle dropped from the office ceiling!";
                BeginJumpscare("mangle");
                return;
            }
            _message = "Mangle is still on the ceiling. The monitor cycle did not trigger an attack this time.";
            Invalidate();
            return;
        }
        if (_monitorOpen && _pendingUntil.ContainsKey("golden-freddy"))
        {
            _pendingUntil.Remove("golden-freddy"); _resolved.Add("golden-freddy");
            _officeIntruderId = null; _goldenFreddyReady = false;
            _message = "Golden Freddy vanished when you raised the monitor.";
            Invalidate(); return;
        }
        if (loweringMonitor && _goldenFreddyReady)
        {
            _goldenFreddyReady = false;
            _officeIntruderId = "golden-freddy"; _officeIntruderUntilUtc = DateTime.UtcNow.AddSeconds(5);
            _pendingUntil["golden-freddy"] = _seconds + 5; _resolved.Remove("golden-freddy");
            _message = "Golden Freddy is in the office! Raise the monitor now!";
            Invalidate(); return;
        }
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

    private void MoveAnimatronicOneCamera(string id, int direction)
    {
        var route = RouteFor(id);
        var current = _animatronicRouteIndices.GetValueOrDefault(id);
        var next = Math.Clamp(current + Math.Sign(direction), 0, route.Length - 1);
        if (next == current) return;
        _animatronicRouteIndices[id] = next;
        _animatronicCameras[id] = route[next];
    }

    private void ResetAnimatronicToStage(string id)
    {
        _animatronicRouteIndices[id] = 0;
        _animatronicCameras[id] = RouteFor(id)[0];
    }

    private void MaybeSpawnShadow()
    {
        if (!_nightStarted || _nightOver || _nightPaused || _shadowInOffice) return;
        var a = _sheets.Animatronics.FirstOrDefault(x => x.Id == "shadow-bonnie");
        if (a is null || !IsAnimatronicSourceAvailable(a) || !_stageAnimatronics.Contains(a.Id) || _ai.GetValueOrDefault(a.Id) <= 0) return;
        if (_random.Next(1, 21) > _ai[a.Id]) return;
        _shadowInOffice = true;
        _shadowDeadline = _seconds + Math.Max(1, a.WarningSeconds);
        _message = "Something is standing in the middle of the office. Put on the Freddy mask!";
        NightSounds.Play("warning");
        Invalidate();
    }

    private void FailNightToShadow()
    {
        _shadowInOffice = false;
        FinishNight(false);
        SaveActiveSlot();
        Close();
    }

    private void SelectCamera(string cameraId)
    {
        _selectedCamera = cameraId;
        if (cameraId == "2B" && IsAnimatronicSourceAvailable("golden-freddy") && _stageAnimatronics.Contains("golden-freddy") && _ai.GetValueOrDefault("golden-freddy") > 0 && !_goldenFreddyEncounterTriggered)
        {
            _goldenFreddyReady = true;
            _goldenFreddyEncounterTriggered = true;
            _message = "The CAM 2B poster changed. Lower the monitor to bring Golden Freddy into the office.";
        }
    }

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
            foreach (var a in _sheets.Animatronics.Where(a => IsAnimatronicSourceAvailable(a) && _stageAnimatronics.Contains(a.Id) && _ai[a.Id] > 0))
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
        _nightPaused = false;
        _nightOver = true;
        _tennaQuizActive = false;
        _charlieDebateActive = false;
        NightSounds.StopAmbience();
        NightSounds.Play(animatronicId switch
        {
            "golden-freddy" => "golden-jumpscare",
            "pee-monster" => "pee-monster-jumpscare",
            "charlie-kirk" => "charlie-kirk-jumpscare",
            _ => "jumpscare"
        });
        _jumpscareAnimatronicId = animatronicId;
        _jumpscareTimer.Interval = animatronicId?.Equals("charlie-kirk", StringComparison.OrdinalIgnoreCase) == true ? 12_000 : 1_100;
        if (animatronicId is not null && _jumpscareArt.TryGetValue(animatronicId, out var scareImage) && ImageAnimator.CanAnimate(scareImage))
            ImageAnimator.Animate(scareImage, OnJumpscareFrameChanged);
        _jumpscareActive = true;
        _jumpscareTimer.Start();
        Invalidate();
    }

    private void OnJumpscareFrameChanged(object? sender, EventArgs e)
    {
        if (IsDisposed || !IsHandleCreated) return;
        try { if (InvokeRequired) BeginInvoke((Action)Invalidate); else Invalidate(); }
        catch (InvalidOperationException) { }
    }

    private void StartTennaQuiz()
    {
        _tennaQuestionOrder = Enumerable.Range(0, TennaQuestions.Length).OrderBy(_ => _random.Next()).Take(3).ToArray();
        _tennaQuestionPosition = 0;
        _tennaCorrectAnswers = 0;
        _tennaQuizCooldownUntil = _seconds + 45;
        _tennaQuizActive = true;
        PrepareTennaQuestion();
        _message = "Mr. (ant) Tenna hijacked the monitor! Answer his quiz.";
    }

    private void PrepareTennaQuestion()
    {
        var question = TennaQuestions[_tennaQuestionOrder[_tennaQuestionPosition]];
        _tennaShuffledAnswers = question.Answers.OrderBy(_ => _random.Next()).ToArray();
        _tennaSecondsRemaining = 7;
    }

    private void SubmitTennaAnswer(string? answer)
    {
        if (!_tennaQuizActive) return;
        var question = TennaQuestions[_tennaQuestionOrder[_tennaQuestionPosition]];
        if (answer is not null && answer.Equals(question.CorrectAnswer, StringComparison.OrdinalIgnoreCase))
            _tennaCorrectAnswers++;
        else
        {
            _tennaFailedAnswers++;
            var powerLoss = (int)Math.Ceiling(_doorPower * .20);
            _doorPower = Math.Max(0, _doorPower - powerLoss);
            if (_tennaFailedAnswers >= 3)
            {
                _tennaQuizActive = false;
                BeginJumpscare("mr-ant-tenna");
                return;
            }
        }
        _tennaQuestionPosition++;
        if (_tennaQuestionPosition < _tennaQuestionOrder.Length)
        {
            PrepareTennaQuestion();
            return;
        }
        _tennaQuizActive = false;
        _message = _tennaCorrectAnswers >= 2
            ? "Mr. (ant) Tenna accepted your answers and left the monitor."
            : "Quiz failed. Tenna may return; wrong answers cost 20% battery each.";
    }

    private void StartCharlieDebate()
    {
        _charlieQuestionPosition = 0;
        _charlieDebateActive = true;
        PrepareCharlieQuestion();
        _message = "Charlie Kirk is debating you. Answer all three questions before time runs out.";
    }

    private void PrepareCharlieQuestion()
    {
        var question = CharlieQuestions[_charlieQuestionPosition];
        _charlieShuffledAnswers = question.Answers.OrderBy(_ => _random.Next()).ToArray();
        _charlieSecondsRemaining = 7;
    }

    private void SubmitCharlieAnswer(string? answer)
    {
        if (!_charlieDebateActive) return;
        var question = CharlieQuestions[_charlieQuestionPosition];
        if (answer is null || !answer.Equals(question.CorrectAnswer, StringComparison.OrdinalIgnoreCase))
        {
            _charlieDebateActive = false;
            _message = "Debate failed. The shift ended.";
            BeginJumpscare("charlie-kirk");
            return;
        }
        _charlieQuestionPosition++;
        if (_charlieQuestionPosition < CharlieQuestions.Length)
        {
            PrepareCharlieQuestion();
            return;
        }
        _charlieDebateActive = false;
        _message = "Debate won. Charlie Kirk returned to the stage.";
        ResetAnimatronicToStage("charlie-kirk");
        Invalidate();
    }

    private void OfferSalvage()
    {
        _salvageCandidateId = null; _salvageOfferOpen = false; _salvageActive = false; _salvageDone = false;
        var options = _sheets.Animatronics
            .Where(a => !_owned.Contains(a.Id) && _unlockedAnimatronicGames.Contains(a.GameId))
            .ToArray();
        if (options.Length == 0) return;
        _salvageCandidateId = options[_random.Next(options.Length)].Id;
        _salvageOfferOpen = true;
    }

    private void StartSalvage()
    {
        if (!_salvageOfferOpen || _salvageCandidateId is null) return;
        _salvageOfferOpen = false; _salvageActive = true; _salvageDone = false;
        _salvageStage = 0; _salvagePhase = 0; _salvageTwitch = false;
        _salvageTicksLeft = SalvageWatchTicks;
        _salvageMessage = "Observe the animatronic. Do not touch anything yet.";
        NightSounds.Play("task");
        Invalidate();
    }

    private void TossSalvage()
    {
        if (!_salvageOfferOpen) return;
        _salvageOfferOpen = false; _salvageCandidateId = null;
        _message = "You threw the alley find back out. Nothing gained.";
        Invalidate();
    }

    private void AdvanceSalvage()
    {
        if (!_salvageActive || _salvageDone) return;
        if (--_salvageTicksLeft > 0) { Invalidate(); return; }
        if (_salvagePhase == 0)
        {
            _salvagePhase = 1;
            _salvageTwitch = _random.Next(100) < 60;
            _salvageTicksLeft = _random.Next(7, 14);
            _salvageMessage = _salvageTwitch ? "IT MOVED! Tase it before it lunges!" : "It is still. Check it off the list.";
        }
        else FailSalvage("Too slow. It lunged while you hesitated.");
        Invalidate();
    }

    private void SalvageTase()
    {
        if (!_salvageActive || _salvageDone || _salvagePhase != 1) return;
        if (!_salvageTwitch) { FailSalvage("You shocked a still animatronic. It thrashed free."); return; }
        CompleteSalvageStage("Tased. It slumped back onto the table.");
    }

    private void SalvageCheck()
    {
        if (!_salvageActive || _salvageDone || _salvagePhase != 1) return;
        if (_salvageTwitch) { FailSalvage("You stared at it while it moved."); return; }
        CompleteSalvageStage("Checked off. No movement.");
    }

    private void CompleteSalvageStage(string note)
    {
        NightSounds.Play("task");
        _salvageStage++;
        if (_salvageStage >= 3) { WinSalvage(); return; }
        _salvagePhase = 0; _salvageTwitch = false;
        _salvageTicksLeft = SalvageWatchTicks;
        _salvageMessage = note + " Keep observing.";
        Invalidate();
    }

    private void WinSalvage()
    {
        var a = _sheets.Animatronics.FirstOrDefault(x => x.Id == _salvageCandidateId);
        if (a is not null && !_owned.Contains(a.Id))
        {
            _owned.Add(a.Id);
            _ai[a.Id] = _sheets.DefaultAiLevel;
            _funds += 150;
            _salvageMessage = $"SALVAGED: {a.Name} joined your roster, +$150 salvage bonus.";
        }
        else _salvageMessage = "Salvage complete, but there was nothing to take.";
        _salvageActive = false; _salvageDone = true;
        SaveActiveSlot(); Invalidate();
    }

    private void FailSalvage(string why)
    {
        NightSounds.Play("jumpscare");
        var fee = Math.Min(_funds, 100);
        _funds -= fee;
        _salvageMessage = $"SALVAGE FAILED: {why} It had to be thrown back out. (-${fee} parts damage)";
        _salvageActive = false; _salvageDone = true;
        SaveActiveSlot(); Invalidate();
    }

    private void CloseSalvage()
    {
        _salvageOfferOpen = false; _salvageActive = false; _salvageDone = false; _salvageCandidateId = null;
        Invalidate();
    }

    private bool HandleSalvageClick(PointF point)
    {
        if (_salvageOfferOpen)
        {
            if (_salvageKeepRect.Contains(point)) StartSalvage();
            else if (_salvageTossRect.Contains(point)) TossSalvage();
            return true;
        }
        if (_salvageActive && !_salvageDone)
        {
            if (_salvageTaseRect.Contains(point)) SalvageTase();
            else if (_salvageCheckRect.Contains(point)) SalvageCheck();
            return true;
        }
        if (_salvageDone && _salvageDoneRect.Contains(point)) CloseSalvage();
        return true;
    }

    private void WindPuppetMusicBox()
    {
        if (!_monitorOpen || !_cameraMonitorMode || _puppetEscaped || !IsAnimatronicSourceAvailable("puppet") || !_stageAnimatronics.Contains("puppet")) return;
        _puppetMusicBox = Math.Min(100, _puppetMusicBox + 25);
        _message = $"Music box wound to {_puppetMusicBox}%. Keep it wound to hold the Puppet back.";
        Invalidate();
    }

    private void DrawTennaQuiz(Graphics g)
    {
        using var shade = new SolidBrush(Color.FromArgb(248, 4, 5, 8));
        g.FillRectangle(shade, 0, 88, DesignWidth, 592);
        var questionPanel = new RectangleF(42, 112, 890, 526);
        var tennaPanel = new RectangleF(950, 112, 282, 526);
        DrawPanel(g, questionPanel, Color.FromArgb(18, 15, 24));
        DrawPanel(g, tennaPanel, Color.FromArgb(22, 16, 29));
        using (var edge = new Pen(_gold, 2))
        {
            g.DrawRectangle(edge, questionPanel.X, questionPanel.Y, questionPanel.Width, questionPanel.Height);
            g.DrawRectangle(edge, tennaPanel.X, tennaPanel.Y, tennaPanel.Width, tennaPanel.Height);
        }
        var q = TennaQuestions[_tennaQuestionOrder[_tennaQuestionPosition]];
        DrawText(g, $"QUESTION {_tennaQuestionPosition + 1} / 3", _smallFont, _gold, questionPanel.X + 28, questionPanel.Y + 22, 260, 22);
        DrawText(g, $"CORRECT {_tennaCorrectAnswers} / 2 NEEDED", _smallFont, _teal, questionPanel.X + 300, questionPanel.Y + 22, 360, 22);
        DrawText(g, $"TIME 00:{_tennaSecondsRemaining:00}", _smallFont, _red, questionPanel.Right - 190, questionPanel.Y + 22, 160, 22, StringAlignment.Far);
        DrawLine(g, _edge, 2, questionPanel.X + 24, questionPanel.Y + 58, questionPanel.Right - 24, questionPanel.Y + 58);
        DrawText(g, q.Prompt, _menuSubtitleFont, _ink, questionPanel.X + 36, questionPanel.Y + 82, questionPanel.Width - 72, 100);
        var columns = _tennaShuffledAnswers.Length == 4 ? 2 : _tennaShuffledAnswers.Length;
        var rows = (int)Math.Ceiling(_tennaShuffledAnswers.Length / (double)columns);
        var gapX = 18f; var gapY = 18f;
        var buttonW = (questionPanel.Width - 72 - gapX * (columns - 1)) / columns;
        var buttonH = rows == 1 ? 118f : 104f;
        _tennaAnswerRects.Clear();
        for (var i = 0; i < _tennaShuffledAnswers.Length; i++)
        {
            var col = i % columns;
            var row = i / columns;
            var x = questionPanel.X + 36 + col * (buttonW + gapX);
            var y = questionPanel.Y + 215 + row * (buttonH + gapY);
            var rect = new RectangleF(x, y, buttonW, buttonH);
            _tennaAnswerRects[i] = rect;
            DrawButton(g, rect, _tennaShuffledAnswers[i], ((char)('A' + i)).ToString(), _ink, Color.FromArgb(26, 21, 34), true);
        }
        DrawText(g, "THREE RANDOM FNAF QUESTIONS · 7 SECONDS EACH · 3 WRONG ANSWERS = GAME OVER", _smallFont, _muted,
            questionPanel.X + 28, questionPanel.Bottom - 34, questionPanel.Width - 56, 18, StringAlignment.Center);
        DrawText(g, "MR. (ANT) TENNA", _cardTitleFont, _gold, tennaPanel.X + 15, tennaPanel.Y + 18, tennaPanel.Width - 30, 25, StringAlignment.Center);
        if (_tennaModelArt is not null)
        {
            var model = FitImage(_tennaModelArt, new RectangleF(tennaPanel.X + 18, tennaPanel.Y + 62, tennaPanel.Width - 36, 380));
            g.DrawImage(_tennaModelArt, model);
        }
        DrawText(g, $"BATTERY  {_doorPower}%", _smallFont, _doorPower > 25 ? _gold : _red, tennaPanel.X + 18, tennaPanel.Bottom - 58, tennaPanel.Width - 36, 20, StringAlignment.Center);
        DrawText(g, $"MISSED ANSWERS  {_tennaFailedAnswers} / 3", _smallFont, _muted, tennaPanel.X + 18, tennaPanel.Bottom - 32, tennaPanel.Width - 36, 18, StringAlignment.Center);
    }

    private void DrawCharlieDebate(Graphics g)
    {
        using var shade = new SolidBrush(Color.FromArgb(248, 4, 5, 8));
        g.FillRectangle(shade, 0, 88, DesignWidth, 592);
        var questionPanel = new RectangleF(42, 112, 890, 526);
        var charliePanel = new RectangleF(950, 112, 282, 526);
        DrawPanel(g, questionPanel, Color.FromArgb(18, 15, 24));
        DrawPanel(g, charliePanel, Color.FromArgb(22, 16, 29));
        using (var edge = new Pen(_gold, 2))
        {
            g.DrawRectangle(edge, questionPanel.X, questionPanel.Y, questionPanel.Width, questionPanel.Height);
            g.DrawRectangle(edge, charliePanel.X, charliePanel.Y, charliePanel.Width, charliePanel.Height);
        }
        var q = CharlieQuestions[_charlieQuestionPosition];
        DrawText(g, $"QUESTION {_charlieQuestionPosition + 1} / {CharlieQuestions.Length}", _smallFont, _gold, questionPanel.X + 28, questionPanel.Y + 22, 300, 22);
        DrawText(g, $"TIME 00:{_charlieSecondsRemaining:00}", _smallFont, _red, questionPanel.Right - 190, questionPanel.Y + 22, 160, 22, StringAlignment.Far);
        DrawLine(g, _edge, 2, questionPanel.X + 24, questionPanel.Y + 58, questionPanel.Right - 24, questionPanel.Y + 58);
        DrawText(g, q.Prompt, _menuSubtitleFont, _ink, questionPanel.X + 36, questionPanel.Y + 82, questionPanel.Width - 72, 100);
        var gapX = 18f;
        var buttonW = (questionPanel.Width - 72 - gapX) / 2;
        _tennaAnswerRects.Clear();
        for (var i = 0; i < _charlieShuffledAnswers.Length; i++)
        {
            var rect = new RectangleF(questionPanel.X + 36 + i * (buttonW + gapX), questionPanel.Y + 215, buttonW, 118);
            _tennaAnswerRects[i] = rect;
            DrawButton(g, rect, _charlieShuffledAnswers[i], ((char)('A' + i)).ToString(), _ink, Color.FromArgb(26, 21, 34), true);
        }
        DrawText(g, "ANSWER CORRECTLY BEFORE THE TIMER EXPIRES", _smallFont, _muted,
            questionPanel.X + 28, questionPanel.Bottom - 34, questionPanel.Width - 56, 18, StringAlignment.Center);
        DrawText(g, "CHARLIE KIRK", _cardTitleFont, _gold, charliePanel.X + 15, charliePanel.Y + 18, charliePanel.Width - 30, 25, StringAlignment.Center);
        if (_portraitArt.TryGetValue("charlie-kirk", out var portrait))
        {
            var target = FitImage(portrait, new RectangleF(charliePanel.X + 18, charliePanel.Y + 62, charliePanel.Width - 36, 380));
            g.DrawImage(portrait, target);
        }
        DrawText(g, "DEBATE IN PROGRESS", _smallFont, _teal, charliePanel.X + 18, charliePanel.Bottom - 42, charliePanel.Width - 36, 20, StringAlignment.Center);
    }

    private void DrawSalvage(Graphics g)
    {
        var a = _sheets.Animatronics.FirstOrDefault(x => x.Id == _salvageCandidateId);
        if (a is null) { CloseSalvage(); return; }
        using var shade = new SolidBrush(Color.FromArgb(248, 4, 5, 8));
        g.FillRectangle(shade, 0, 88, DesignWidth, 592);
        var panel = new RectangleF(196, 118, 880, 512);
        DrawPanel(g, panel, Color.FromArgb(20, 16, 24));
        using (var edge = new Pen(_gold, 2)) g.DrawRectangle(edge, panel.X, panel.Y, panel.Width, panel.Height);
        DrawText(g, "BACK ALLEY SALVAGE", _smallFont, _gold, panel.X + 28, panel.Y + 18, 400, 22);
        var artRect = new RectangleF(panel.X + 28, panel.Y + 56, 300, 330);
        Image? scare = null;
        var twitching = _salvageActive && !_salvageDone && _salvagePhase == 1 && _salvageTwitch
            && _jumpscareArt.TryGetValue(a.Id, out scare);
        if (twitching && scare is not null) g.DrawImage(scare, FitImage(scare, artRect));
        else DrawPortraitFrame(g, a, artRect);
        var textX = panel.X + 352; var textW = panel.Width - 380;
        DrawText(g, a.Name.ToUpperInvariant(), _panelTitleFont, _ink, textX, panel.Y + 56, textW, 30);
        var gameName = ShortName(_sheets.Games.FirstOrDefault(gm => gm.Id.Equals(a.GameId, StringComparison.OrdinalIgnoreCase))?.Name ?? a.GameId);
        DrawText(g, $"{gameName}  ·  NORMALLY ${a.Cost:N0}", _smallFont, _muted, textX, panel.Y + 94, textW, 20);
        DrawText(g, $"ATM {a.Atmosphere}  ·  H&S {a.HealthSafety}  ·  ENT {a.Entertainment}  ·  BONUS {a.BonusRevenue}", _smallFont, _teal, textX, panel.Y + 118, textW, 20);
        if (_salvageDone)
        {
            DrawText(g, _salvageMessage, _bodyFont, _ink, textX, panel.Y + 170, textW, 120);
            _salvageDoneRect = new RectangleF(textX, panel.Bottom - 96, 340, 56);
            DrawButton(g, _salvageDoneRect, "CONTINUE", "→", _gold, Color.FromArgb(47, 35, 19), true);
        }
        else if (_salvageActive)
        {
            var secs = Math.Max(0, (_salvageTicksLeft + 2) / 3);
            DrawText(g, $"CHECKLIST  {_salvageStage} / 3 DONE   ·   TIME 00:{secs:00}", _smallFont, _gold, textX, panel.Y + 170, textW, 22);
            DrawText(g, _salvagePhase == 0 ? "OBSERVING..." : _salvageTwitch ? "IT IS MOVING!" : "IT IS STILL.", _smallFont, _salvageTwitch ? _red : _teal, textX, panel.Y + 198, textW, 22);
            DrawText(g, _salvageMessage, _bodyFont, _ink, textX, panel.Y + 228, textW, 110);
            var buttonW = (textW - 16) / 2;
            _salvageTaseRect = new RectangleF(textX, panel.Bottom - 96, buttonW, 56);
            _salvageCheckRect = new RectangleF(textX + buttonW + 16, panel.Bottom - 96, buttonW, 56);
            var reacting = _salvagePhase == 1;
            DrawButton(g, _salvageTaseRect, "TASER", "ZAP", _gold, Color.FromArgb(47, 35, 19), reacting);
            DrawButton(g, _salvageCheckRect, "CHECK OFF", "OK", _teal, Color.FromArgb(22, 28, 29), reacting);
        }
        else
        {
            DrawText(g, "Found wandering the alley after the shift. Salvage it for your roster and a +$150 bonus, or throw it back out.", _bodyFont, _ink, textX, panel.Y + 170, textW, 110);
            DrawText(g, a.Rule, _smallFont, _muted, textX, panel.Y + 280, textW, 80);
            var buttonW = (textW - 16) / 2;
            _salvageKeepRect = new RectangleF(textX, panel.Bottom - 96, buttonW, 56);
            _salvageTossRect = new RectangleF(textX + buttonW + 16, panel.Bottom - 96, buttonW, 56);
            DrawButton(g, _salvageKeepRect, "SALVAGE", "+$150", _gold, Color.FromArgb(47, 35, 19), true);
            DrawButton(g, _salvageTossRect, "THROW BACK", "SKIP", _muted, Color.FromArgb(22, 28, 29), true);
        }
    }

    private bool WasHandled(Animatronic a) => a.Action switch
    {
        "door" => IsDoorClosed(a.DoorSide), "foxy" => _leftDoorClosed, "mask" => _maskOn, "mangle" => _maskOn && !_mangleOnCeiling, "lure" => _lureSpent,
        "golden" => _monitorOpen,
        "listen" => _listening && IsDoorClosed(a.DoorSide), "scan" => _ventScanned, _ => false
    };

    private bool IsDoorClosed(string? side) => side switch { "left" => _leftDoorClosed, "right" => _rightDoorClosed, _ => false };

    private string WarningFor(Animatronic a) => a.Action switch
    {
        "door" => $"Movement at the right doorway. Press {KeyLabel(_controlBindings["right-door"])} to close it.",
        "foxy" => $"Foxy is charging from Pirate Cove! Press {KeyLabel(_controlBindings["left-door"])} to shut the left door.",
        "golden" => $"Golden Freddy is in the office! Press {KeyLabel(_controlBindings["monitor"])} to raise the monitor and dismiss him.",
        "mask" => $"{a.Name} is approaching. Press {KeyLabel(_controlBindings["mask"])} to put on the mask and stay still.",
        "mangle" => $"Mangle is in the Right Air Vent! Put on the Freddy head ({KeyLabel(_controlBindings["mask"])}) before they reach the office.",
        "debate" => "Charlie Kirk is approaching. Get ready to answer his debate questions.",
        "lure" => $"Springtrap is near. Press {KeyLabel(_controlBindings["lure"])} to use the audio lure while ventilation is stable.",
        "listen" => $"Breathing at the left door. Press {KeyLabel(_controlBindings["listen"])} to listen, then {KeyLabel(_controlBindings["left-door"])} to close the left door.",
        "scan" => a.Id.Equals("pee-monster", StringComparison.OrdinalIgnoreCase)
            ? $"Pee Monster is in the vents! Press {KeyLabel(_controlBindings["scan"])} to scan before he reaches the office."
            : $"Scrap Baby is in the vents. Press {KeyLabel(_controlBindings["scan"])} to scan before she reaches the office.", _ => "Something is moving nearby."
    };

    private Animatronic? CurrentThreat() => _sheets.Animatronics.FirstOrDefault(a => _pendingUntil.TryGetValue(a.Id, out var until) && until > _seconds && !_resolved.Contains(a.Id));
    private int ActiveThreatCount() => _sheets.Animatronics.Count(a => IsAnimatronicSourceAvailable(a) && _stageAnimatronics.Contains(a.Id) && _ai[a.Id] > 0);
    private bool ActionAvailable(string action) => action switch
    {
        "left-door" or "right-door" => true, "left-light" => _leftLightOn || _doorPower > 0, "right-light" => _rightLightOn || _doorPower > 0,
        "door" => true, "mask" => _owned.Contains("toy-bonnie") || _owned.Contains("toy-freddy") || _owned.Contains("toy-chica") || _owned.Contains("balloon-boy") || _owned.Contains("jay-jay") || _owned.Contains("withered-freddy") || _owned.Contains("withered-foxy") || _owned.Contains("shadow-bonnie") || _owned.Contains("withered-bonnie") || _owned.Contains("withered-chica") || _owned.Contains("mangle"), "lure" => _owned.Contains("springtrap"),
        "listen" => _owned.Contains("nightmare-chica") && !_listening && _pendingUntil.TryGetValue("nightmare-chica", out var until) && until > _seconds && !_resolved.Contains("nightmare-chica"),
        "scan" => _owned.Contains("scrap-baby") || _owned.Contains("pee-monster"), "vent" => _owned.Contains("springtrap") || _owned.Contains("scrap-baby") || _owned.Contains("pee-monster"), _ => false
    };

    private void UseAction(string action)
    {
        var closesDoor = (action == "left-door" && !_leftDoorClosed) || (action == "right-door" && !_rightDoorClosed);
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
                if (_leftLightOn) _rightLightOn = false;
                _message = _leftLightOn ? "Left doorway illuminated. Light uses 1% power each second." : "Left doorway light off.";
                if (_leftLightOn) NightSounds.Play(ThreatWaitingAt("left") ? "light-spotted" : "light");
                else NightSounds.StopDoorLightSound();
                break;
            case "right-light":
                _rightLightOn = !_rightLightOn;
                if (_rightLightOn) _leftLightOn = false;
                _message = _rightLightOn ? "Right doorway illuminated. Light uses 1% power each second." : "Right doorway light off.";
                if (_rightLightOn) NightSounds.Play(ThreatWaitingAt("right") ? "light-spotted" : "light");
                else NightSounds.StopDoorLightSound();
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
        if (closesDoor) NightSounds.Play("door");
        Invalidate();
    }

    private bool ThreatWaitingAt(string side) => _sheets.Animatronics.Any(a =>
        _pendingUntil.TryGetValue(a.Id, out var until) && until > _seconds && !_resolved.Contains(a.Id) && DoorSideFor(a) == side);

    private void FinishNight(bool win, bool died = false)
    {
        NightSounds.StopAmbience();
        if (win) NightSounds.PlayVictorySequence();
        _nightOver = true; _nightWon = win;
        _dayResetByDeath = died;
        if (win && _payoutPaidDay != _day)
        {
            var aiBonusBeforeStrikePenalty = (int)Math.Round(_payoutForNight * StagedAiPayoutMultiplier(), MidpointRounding.AwayFromZero);
            var animatronicBonusBeforeStrikePenalty = _sheets.Animatronics
                .Where(a => IsAnimatronicSourceAvailable(a) && _stageAnimatronics.Contains(a.Id) && _owned.Contains(a.Id) && _ai[a.Id] > 0)
                .Sum(_ => Random.Shared.Next(1, 11));
            var grossPayout = _payoutForNight + aiBonusBeforeStrikePenalty + animatronicBonusBeforeStrikePenalty;
            var strikePenalty = Math.Min(1d, _strikes * .25d);
            _payoutCollectedThisNight = (int)Math.Floor(grossPayout * (1d - strikePenalty));
            _payoutAiBonusCollected = (int)Math.Round(aiBonusBeforeStrikePenalty * (1d - strikePenalty), MidpointRounding.AwayFromZero);
            _payoutAnimatronicBonusCollected = (int)Math.Floor(animatronicBonusBeforeStrikePenalty * (1d - strikePenalty));
            _funds += _payoutCollectedThisNight;
            _payoutPaidDay = _day;
        }
        if (win) OfferSalvage();
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
        return _stageAnimatronics.Where(id => IsAnimatronicSourceAvailable(id) && _owned.Contains(id) && _ai.ContainsKey(id))
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
        _cheatToastMessage = "CHEAT CODE ACCEPTED · NIGHT SKIPPED";
        _cheatToastUntilUtc = DateTime.UtcNow.AddSeconds(3);
        _message = "Night skipped.";
        FinishNight(true);
        return true;
    }

    private bool ProcessUnlockCategoriesCheat(Keys key)
    {
        Keys[] sequence = [Keys.Up, Keys.Up, Keys.Down, Keys.Right, Keys.Left, Keys.A, Keys.B];
        if (key == sequence[_unlockCategoriesCheatIndex]) _unlockCategoriesCheatIndex++;
        else _unlockCategoriesCheatIndex = key == sequence[0] ? 1 : 0;
        if (_unlockCategoriesCheatIndex < sequence.Length) return false;
        _unlockCategoriesCheatIndex = 0;
        _unlockedAnimatronicGames.UnionWith(_sheets.Games.Select(game => game.Id));
        _cheatToastMessage = "CHEAT ACCEPTED · ALL CHARACTER CATEGORIES UNLOCKED";
        _cheatToastUntilUtc = DateTime.UtcNow.AddSeconds(3);
        SaveActiveSlot();
        Invalidate();
        return true;
    }

    private bool ProcessWordCheatChar(char c)
    {
        if (c is < 'A' or > 'Z' && c != '&') return false;
        foreach (var (code, target, toast) in WordCheats)
        {
            var progress = _wordCheatProgress.GetValueOrDefault(code);
            if (progress >= code.Length) progress = 0;
            if (c == code[progress]) progress++;
            else progress = c == code[0] ? 1 : 0;
            if (progress < code.Length) { _wordCheatProgress[code] = progress; continue; }
            _wordCheatProgress[code] = 0;
            FireWordCheat(target, toast);
            return true;
        }
        return false;
    }

    protected override void OnKeyPress(KeyPressEventArgs e)
    {
        base.OnKeyPress(e);
        if (_nightPaused || _jumpscareActive) return;
        if (_tennaQuizActive || _charlieDebateActive) return;
        if (_waitingForBindingAction is not null) return;
        if (ProcessWordCheatChar(char.ToUpperInvariant(e.KeyChar))) e.Handled = true;
    }

    private void FireWordCheat(string target, string toast)
    {
        if (target == "reveal") _salvageRevealed = true;
        else
        {
            _unlockedAnimatronicGames.Add(target);
            foreach (var id in _sheets.Animatronics.Where(a => a.GameId == target).Select(a => a.Id)) _owned.Add(id);
        }
        _cheatToastMessage = toast;
        _cheatToastUntilUtc = DateTime.UtcNow.AddSeconds(3);
        SaveActiveSlot();
        Invalidate();
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
        if (_tennaQuizActive || _charlieDebateActive) return;
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
        if (_tennaQuizActive || _charlieDebateActive || _salvageOfferOpen || _salvageActive || _salvageDone) return;
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
        if (_nightPaused || _jumpscareActive) return;
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
        if (_tennaQuizActive || _charlieDebateActive)
        {
            foreach (var (index, rect) in _tennaAnswerRects)
                if (rect.Contains(p))
                {
                    if (_tennaQuizActive) SubmitTennaAnswer(_tennaShuffledAnswers[index]);
                    else SubmitCharlieAnswer(_charlieShuffledAnswers[index]);
                    return;
                }
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
            if (_musicVolumeRect.Contains(p))
            {
                _musicVolume = VolumeAt(p.X, _musicVolumeRect);
                NightSounds.SetVolumes(_musicVolume, _effectsVolume);
                SavePreferences(); Invalidate(); return;
            }
            if (_effectsVolumeRect.Contains(p))
            {
                _effectsVolume = VolumeAt(p.X, _effectsVolumeRect);
                NightSounds.SetVolumes(_musicVolume, _effectsVolume);
                SavePreferences(); Invalidate(); return;
            }
            if (_displayModeRect.Contains(p))
            {
                ToggleDisplayMode();
                _settingsMessage = _isFullscreen ? "Startup display set to fullscreen." : "Startup display set to windowed.";
                Invalidate(); return;
            }
            if (_autoLaunchRect.Contains(p))
            {
                SetAutoLaunchOnSignIn(!_autoLaunchOnSignIn);
                Invalidate(); return;
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
                if (_puppetWindRect.Contains(p)) { WindPuppetMusicBox(); return; }
                foreach (var (tabId, rect) in _officeMonitorTabRects)
                {
                    if (!rect.Contains(p)) continue;
                    var wasWatchingCameras = _cameraMonitorMode && tabId != "cameras";
                    _cameraMonitorMode = tabId == "cameras";
                    if (wasWatchingCameras) MaybeSpawnShadow();
                    NightSounds.Play(tabId == "cameras" ? "camera" : "task");
                    Invalidate();
                    return;
                }
                foreach (var (cameraId, rect) in _cameraSelectRects)
                {
                    if (!rect.Contains(p)) continue;
                    SelectCamera(cameraId);
                    NightSounds.Play("camera");
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
        if (_startNightRect.Contains(p))
        {
            if (ActiveThreatCount() > 0) StartNight();
            else { _prepScreen = PrepScreen.Roster; _rosterScrollOffset = 0; Invalidate(); }
            return;
        }
        if (_prepScreen == PrepScreen.Roster)
        {
            foreach (var (gameId, rect) in _animatronicCategoryUnlockRects)
                if (rect.Contains(p) && _sources[gameId].IsAvailable && !_unlockedAnimatronicGames.Contains(gameId) && _funds >= AnimatronicCategoryCost(gameId))
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
            if (_rosterSellRect.Contains(p) && _rosterHoveredAnimatronic is not null)
            {
                SellHoveredAnimatronic();
                return;
            }
            if (_rosterViewportRect.Contains(p))
            {
                foreach (var (id, rect) in _rosterCardRects)
                {
                    if (!rect.Contains(p)) continue;
                    var a = _sheets.Animatronics.First(entry => entry.Id == id);
                    if (!IsAnimatronicSourceAvailable(a)) return;
                    if (!_owned.Contains(id))
                    {
                        if (a.SalvageOnly) { Invalidate(); return; }
                        if (_funds >= a.Cost)
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
                    _catalogMessage = "Catalog open. Buy attractions here, then place them on the blueprint.";
                }
                else if (_funds >= CatalogCategoryCost(category))
                {
                    _pendingPurchaseId = category;
                    _pendingPurchaseKind = PurchaseKind.CatalogCategory;
                }
                else _catalogMessage = $"This catalog costs ${CatalogCategoryCost(category):N0}. Available cash: ${_funds:N0}.";
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

    private static int VolumeAt(float x, RectangleF slider) => Math.Clamp(
        (int)Math.Round(Math.Clamp((x - slider.X) / slider.Width, 0, 1) * 20, MidpointRounding.AwayFromZero) * 5, 0, 100);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.KeyCode == Keys.P && _nightStarted && !_nightOver && !_jumpscareActive)
        {
            ToggleNightPause();
            e.Handled = true;
            e.SuppressKeyPress = true;
            return;
        }
        if (_nightPaused || _jumpscareActive)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
            return;
        }
        if (_tennaQuizActive || _charlieDebateActive) { e.Handled = true; e.SuppressKeyPress = true; return; }
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
            if (ProcessUnlockCategoriesCheat(e.KeyCode)) { e.Handled = true; e.SuppressKeyPress = true; return; }
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
            else if (_nightStarted && !_nightOver) { _nightStarted = false; _nightPaused = false; _prepScreen = PrepScreen.Builder; _timer.Stop(); NightSounds.StopAmbience(); SaveActiveSlot(); }
            else if (!_showMenu) { SaveActiveSlot(); _showMenu = true; NightSounds.StartMenuMusic(); }
            else Close();
            Invalidate(); return;
        }
        if (!_nightStarted || _nightOver) return;
        if (e.KeyCode == _controlBindings["monitor"]) { ToggleOfficeMonitor(); e.Handled = true; e.SuppressKeyPress = true; return; }
        if (_monitorOpen && _cameraMonitorMode && (e.KeyCode == _controlBindings["camera-prev"] || e.KeyCode == _controlBindings["camera-next"]))
        {
            var index = Array.FindIndex(FnafCameras, camera => camera.Id == _selectedCamera);
            var direction = e.KeyCode == _controlBindings["camera-prev"] ? -1 : 1;
            SelectCamera(FnafCameras[(index + direction + FnafCameras.Length) % FnafCameras.Length].Id);
            NightSounds.Play("camera"); Invalidate(); e.Handled = true; e.SuppressKeyPress = true; return;
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
        SavePreferences();
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
        "FNAF Mades" => "FNAF Customs",
        _ => "UCN"
    };

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        NightSounds.StopMenuMusic();
        NightSounds.StopAmbience();
        SaveActiveSlot();
        _timer.Dispose(); _blinkTimer.Dispose(); _jumpscareTimer.Dispose();
        foreach (var image in _sourceArt.Values) image?.Dispose();
        foreach (var image in _portraitArt.Values) image.Dispose();
        foreach (var image in _jumpscareArt.Values) { ImageAnimator.StopAnimate(image, OnJumpscareFrameChanged); image.Dispose(); }
        foreach (var image in _catalogAttractionArt.Values) image.Dispose();
        _tennaModelArt?.Dispose(); _shadowOfficeArt?.Dispose();
        _brandFont.Dispose(); _titleFont.Dispose(); _bodyFont.Dispose(); _smallFont.Dispose(); _cardTitleFont.Dispose(); _panelTitleFont.Dispose(); _messageFont.Dispose(); _aiFont.Dispose(); _noMovementFont.Dispose(); _keyFont.Dispose(); _menuTitleFont.Dispose(); _menuSubtitleFont.Dispose();
        foreach (var source in _sources.Values) source.Dispose();
        base.OnFormClosed(e);
    }
}
