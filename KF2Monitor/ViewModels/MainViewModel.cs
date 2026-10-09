using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KF2Monitor.ViewModels;

public class LanguageItem
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
}

public class RemoteServerConfig
{
    public string Name { get; set; } = string.Empty;
    public string IP { get; set; } = string.Empty;
    public int Port { get; set; }
}

public class ChangelogEntry
{
    public bool IsHeader { get; set; }
    public bool IsBullet { get; set; }
    public bool IsText { get; set; }
    public string Text { get; set; } = "";
}

public class VersionItem
{
    public string Version { get; set; } = "";
    public string Suffix { get; set; } = "";
}

public delegate void SaveSettingsDelegate(bool hide, string hex, int alpha, int interval, string langCode, bool[] srvs, bool hasBgImage, int radius, int opacity, float offsetY, bool swapLR, bool swapWP, bool showMap, bool showWave, bool showPlayers, bool swapTopBar, string locWave, string locUpdated, string locSaved, bool enableNotifs, string trackedPlayers, bool enableCountNotifs, int countThreshold, bool[] countSrvs, bool fullSize, bool fadeBg, string locNotifPlayerTitle, string locNotifPlayerBody, string locNotifPopTitle, string locNotifPopBody, string locNotifPlayerLeftTitle, string locNotifPlayerLeftBody, string locNotifUpdateTitle, string locNotifUpdateBody, string appVer, string locStatus, string locPhrase1, string locPhrase2, string locPhrase3, string locPhrase4, string locPhrase5);
public delegate void LoadSettingsCallbackDelegate(bool hide, string hex, int alpha, int interval, string langCode, bool[] srvs, bool hasBgImage, int radius, int opacity, float offsetY, bool swapLR, bool swapWP, bool showMap, bool showWave, bool showPlayers, bool swapTopBar, bool enableNotifs, string trackedPlayers, bool enableCountNotifs, int countThreshold, bool[] countSrvs, List<string> onlinePlayers, bool fullSize, bool fadeBg);

public partial class MainViewModel : ViewModelBase
{
    public static MainViewModel? Instance { get; private set; }
    public static SaveSettingsDelegate? OnSaveSettings;
    public static Action<LoadSettingsCallbackDelegate>? RequestLoadSettings;
    public static Action<string>? OpenUrlAction;
    public static Func<Task<bool>>? PickImageAction;
    public static Action? VibrateAction;
    public static Action<string>? ShowToastAction;
    public static Action? RequestPermissionsAction;
    public static Action<string>? InstallApkAction;

    public const string DeveloperSteamUrl = "https://steamcommunity.com/profiles/76561198835178979";

    [ObservableProperty] private string _appVersion = "";
    [ObservableProperty] private string _latestVer = "1.0.0";
    [ObservableProperty] private string _updateMessage = "";
    [ObservableProperty] private bool _hasUpdate = false;
    private string _updateApkUrl = "https://github.com/ext4n/KF2Monitor/releases/latest/download/KF2Monitor.apk";

    private string _cachedChangelogText = "";
    private DateTime _lastChangelogFetch = DateTime.MinValue;
    
    [ObservableProperty] private ObservableCollection<VersionItem> _availableVersions = new();
    [ObservableProperty] private VersionItem? _selectedVersion;
    
    [ObservableProperty] private ObservableCollection<ChangelogEntry> _currentChangelogList = new();
    [ObservableProperty] private ObservableCollection<ChangelogEntry> _updateChangelogList = new();
    [ObservableProperty] private ObservableCollection<ChangelogEntry> _whatsNewChangelogList = new();
    public bool HasCurrentChangelog => CurrentChangelogList.Count > 0;
    public bool HasUpdateChangelog => UpdateChangelogList.Count > 0;

    [ObservableProperty] private bool _isUpdateModalOpen;
    [ObservableProperty] private bool _isWhatsNewModalOpen;
    [ObservableProperty] private bool _isDownloading;
    [ObservableProperty] private bool _isDownloadFinished;
    [ObservableProperty] private double _downloadProgress;
    [ObservableProperty] private string _downloadProgressText = "";

    [ObservableProperty] private bool _hideEmpty;
    [ObservableProperty] private Color _appThemeColor = Color.Parse("#059669");
    [ObservableProperty] private int _selectedInterval = 15;
    [ObservableProperty] private int _selectedMenuIndex = 0;

    [ObservableProperty] private int _cornerRadius = 12;
    [ObservableProperty] private int _imageOpacity = 100;
    
    [ObservableProperty] private double _imageOffsetY = 0;
    public double ImageOpacityDouble => ImageOpacity / 100.0;
    public TranslateTransform ImageOffsetTransform => new TranslateTransform(0, ImageOffsetY);

    [ObservableProperty] private bool _showSrv1 = true;
    [ObservableProperty] private bool _showSrv2 = true;
    [ObservableProperty] private bool _showSrv3 = true;
    [ObservableProperty] private bool _showSrv4 = true;
    [ObservableProperty] private bool _showSrv5 = true;

    [ObservableProperty] private bool _countSrv1 = true;
    [ObservableProperty] private bool _countSrv2 = true;
    [ObservableProperty] private bool _countSrv3 = true;
    [ObservableProperty] private bool _countSrv4 = true;
    [ObservableProperty] private bool _countSrv5 = true;

    [ObservableProperty] private bool _hasBackgroundImage = false;
    [ObservableProperty] private Bitmap? _widgetPreviewImage;

    [ObservableProperty] private bool _isEditorOpen;
    [ObservableProperty] private bool _swapLeftRight;
    [ObservableProperty] private bool _swapWavePlayers;
    [ObservableProperty] private bool _showMapBlock = true;
    [ObservableProperty] private bool _showWaveBlock = true;
    [ObservableProperty] private bool _showPlayersBlock = true;
    [ObservableProperty] private bool _swapTopBar = false;

    [ObservableProperty] private bool _hasCustomServers = false;
    
    [ObservableProperty] private bool _enableNotifications = false;
    [ObservableProperty] private string _trackedPlayers = "";
    [ObservableProperty] private ObservableCollection<string> _suggestedPlayers = new();
    public bool HasSuggestedPlayers => SuggestedPlayers != null && SuggestedPlayers.Count > 0;

    [ObservableProperty] private bool _enableCountNotifications = false;
    [ObservableProperty] private int _playerCountThreshold = 6;

    [ObservableProperty] private bool _fullSizeImage = true;
    [ObservableProperty] private bool _fadeBackground = false;

    [ObservableProperty] private ObservableCollection<FaqItem> _faqList = new();

    public bool NotSwapLeftRight => !SwapLeftRight;
    public bool NotSwapTopBar => !SwapTopBar;

    public string PreviewTopPlainText => "Mod-EU | ExempleTestServer #0";
    public string PreviewTopBgText => SwapWavePlayers ? $"{LocWave} 4 / 50" : "8";
    public string PreviewBottomPlainText => ShowMapBlock ? "KF-ExempleWidgetMap" : "";
    public string PreviewBottomBgText => SwapWavePlayers ? "8" : $"{LocWave} 4 / 50";

    public bool ShowTopBg => SwapWavePlayers ? ShowWaveBlock : ShowPlayersBlock;
    public bool ShowBottomBg => SwapWavePlayers ? ShowPlayersBlock : ShowWaveBlock;
    public bool ShowTopPlain => true;
    public bool ShowBottomPlain => ShowMapBlock;

    public SolidColorBrush PreviewTopBgColor => SwapWavePlayers ? AccentBrush2 : AccentBrush;
    public SolidColorBrush PreviewBottomBgColor => SwapWavePlayers ? AccentBrush : AccentBrush2;

    public List<int> Intervals { get; } = new() { 5, 10, 15, 20, 25, 30, 45, 60, 120, 180, 360 };

    public List<LanguageItem> Languages { get; } = new()
    {
        new LanguageItem { Code = "en", Name = "🇬🇧 English" },
        new LanguageItem { Code = "et", Name = "🇪🇪 Eesti" },
        new LanguageItem { Code = "sk", Name = "🇸🇰 Slovenčina" },
        new LanguageItem { Code = "lt", Name = "🇱🇹 Lietuvių" },
        new LanguageItem { Code = "lv", Name = "🇱🇻 Latviešu" },
        new LanguageItem { Code = "fr", Name = "🇫🇷 Français" },
        new LanguageItem { Code = "pl", Name = "🇵🇱 Polski" },
        new LanguageItem { Code = "de", Name = "🇩🇪 Deutsch" },
        new LanguageItem { Code = "ja", Name = "🇯🇵 日本語" }
    };

    [ObservableProperty] private LanguageItem? _selectedLanguage;

    public SolidColorBrush AccentBrush => new(Color.FromRgb(
        (byte)Math.Min(255, AppThemeColor.R + 80),
        (byte)Math.Min(255, AppThemeColor.G + 80),
        (byte)Math.Min(255, AppThemeColor.B + 80)));

    public SolidColorBrush AccentBrushHover => new(Color.FromRgb(
        (byte)Math.Max(0, AccentBrush.Color.R - 35),
        (byte)Math.Max(0, AccentBrush.Color.G - 35),
        (byte)Math.Max(0, AccentBrush.Color.B - 35)));

    public SolidColorBrush AccentBrushPressed => new(Color.FromRgb(
        (byte)Math.Max(0, AccentBrush.Color.R - 70),
        (byte)Math.Max(0, AccentBrush.Color.G - 70),
        (byte)Math.Max(0, AccentBrush.Color.B - 70)));

    public SolidColorBrush AccentBrush2 => new(Color.FromRgb(
        (byte)Math.Min(255, AppThemeColor.R + 130),
        (byte)Math.Min(255, AppThemeColor.G + 130),
        (byte)Math.Min(255, AppThemeColor.B + 130)));

    public SolidColorBrush AccentBrushLighter => new(Color.FromRgb(
        (byte)Math.Min(255, AppThemeColor.R + 130),
        (byte)Math.Min(255, AppThemeColor.G + 130),
        (byte)Math.Min(255, AppThemeColor.B + 130)));


    [ObservableProperty] private string _locTabGeneral = "";
    [ObservableProperty] private string _locTabWidget = "";
    [ObservableProperty] private string _locTabFaq = "";
    [ObservableProperty] private string _locTabUpdates = "";
    [ObservableProperty] private string _locTabAbout = "";

    [ObservableProperty] private string _locGeneralSettings = "";
    [ObservableProperty] private string _locLanguage = "";
    [ObservableProperty] private string _locHideEmpty = "";
    [ObservableProperty] private string _locHideEmptyDesc = "";
    [ObservableProperty] private string _locInterval = "";
    [ObservableProperty] private string _locIntervalDesc = "";
    [ObservableProperty] private string _locServerVisibility = "";
    [ObservableProperty] private string _locServerVisibilityDesc = "";
    [ObservableProperty] private string _locSaveBtn = "";

    [ObservableProperty] private string _locWidgetApp = "";
    [ObservableProperty] private string _locCornerRadius = "";
    [ObservableProperty] private string _locThemeColor = "";
    [ObservableProperty] private string _locThemeColorDesc = "";
    [ObservableProperty] private string _locBackgroundImage = "";
    [ObservableProperty] private string _locBackgroundImageDesc = "";
    [ObservableProperty] private string _locSelectImage = "";
    [ObservableProperty] private string _locImageOpacity = "";
    [ObservableProperty] private string _locLivePreview = "";
    [ObservableProperty] private string _locPreviewDisclaimer = "";

    [ObservableProperty] private string _locAppVersion = "";
    [ObservableProperty] private string _locCheckUpdateBtn = "";
    [ObservableProperty] private string _locDownloadBtn = "";

    [ObservableProperty] private string _locUpdatedJustNow = "";
    [ObservableProperty] private string _locAboutDesc = "";
    [ObservableProperty] private string _locSpecialThanks = "";
    [ObservableProperty] private string _locCreatedBy = "";

    [ObservableProperty] private string _locUpdateBadge = "";
    [ObservableProperty] private string _locWidgetEditorTitle = "";
    [ObservableProperty] private string _locBgPosition = "";
    [ObservableProperty] private string _locSwapLR = "";
    [ObservableProperty] private string _locSwapWP = "";
    [ObservableProperty] private string _locShowMap = "";
    [ObservableProperty] private string _locShowPlayers = "";
    [ObservableProperty] private string _locShowWave = "";
    [ObservableProperty] private string _locSwapTopBarText = "";
    [ObservableProperty] private string _locFullSizeImage = "";
    [ObservableProperty] private string _locFadeBackground = "";
    [ObservableProperty] private string _locDone = "";
    [ObservableProperty] private string _locReset = "";
    
    [ObservableProperty] private string _locDiscordBtn = "";
    [ObservableProperty] private string _locRepoBtn = "";

    [ObservableProperty] private string _locRemoteUpdateTitle = "";
    [ObservableProperty] private string _locRemoteUpdateDesc = "";
    [ObservableProperty] private string _locBtnUpdateServers = "";
    
    [ObservableProperty] private string _locMsgNoUpdates = "";
    [ObservableProperty] private string _locMsgUpdated = "";
    [ObservableProperty] private string _locMsgReset = "";

    [ObservableProperty] private string _locEnableNotifications = "";
    [ObservableProperty] private string _locTrackedPlayersHint = "";
    [ObservableProperty] private string _locSuggestedPlayers = "";

    [ObservableProperty] private string _locEnableCountNotifications = "";
    [ObservableProperty] private string _locPlayerCountThreshold = "";
    [ObservableProperty] private string _locPopWarning = "";

    [ObservableProperty] private string _locNotifPlayerTitle = "";
    [ObservableProperty] private string _locNotifPlayerBody = "";
    [ObservableProperty] private string _locNotifPopTitle = "";
    [ObservableProperty] private string _locNotifPopBody = "";
    
    [ObservableProperty] private string _locNotifPlayerLeftTitle = "";
    [ObservableProperty] private string _locNotifPlayerLeftBody = "";

    [ObservableProperty] private string _locNotifUpdateTitle = "";
    [ObservableProperty] private string _locNotifUpdateBody = "";

    [ObservableProperty] private string _locCurrentVersionBlock = "";
    [ObservableProperty] private string _locUpdateVersionBlock = "";
    
    [ObservableProperty] private string _locUpdateModalTitle = "";
    [ObservableProperty] private string _locInstallBtn = "";
    [ObservableProperty] private string _locCancelBtn = "";
    [ObservableProperty] private string _locReadyToInstall = "";

    [ObservableProperty] private string _locStatus = "";
    [ObservableProperty] private string _locPhrase1 = "";
    [ObservableProperty] private string _locPhrase2 = "";
    [ObservableProperty] private string _locPhrase3 = "";
    [ObservableProperty] private string _locPhrase4 = "";
    [ObservableProperty] private string _locPhrase5 = "";

    private string _locCurrentSuffix = "";
    private string _locPreviousSuffix = "";

    public string LocWave { get; set; } = "Wave:";
    public string LocUpdated { get; set; } = "Updated:";
    public string LocSettingsSaved { get; set; } = "Settings saved successfully!";

    public MainViewModel()
    {
        Instance = this;
         
        var version = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version;
        AppVersion = version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "1.0.0";

        LatestVer = AppVersion;

        CleanupOldUpdates();
        InitializeLanguage();
        CheckCustomServers();
    }

    private void CleanupOldUpdates()
    {
        try
        {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Updates");
            if (Directory.Exists(dir))
            {
                foreach (var file in Directory.GetFiles(dir, "*.apk"))
                {
                    File.Delete(file);
                }
            }
        }
        catch { }
    }

    private async void CheckIfUpdated()
    {
        try
        {
            string verFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "last_version.txt");
            string lastVer = File.Exists(verFile) ? File.ReadAllText(verFile) : "";

            if (string.IsNullOrEmpty(lastVer))
            {
                File.WriteAllText(verFile, AppVersion);
                return;
            }

            if (lastVer != AppVersion)
            {
                File.WriteAllText(verFile, AppVersion);
                
                if (string.IsNullOrEmpty(_cachedChangelogText))
                {
                    await FetchChangelogAsync(false);
                }
                
                if (!string.IsNullOrEmpty(_cachedChangelogText))
                {
                    string langCode = SelectedLanguage?.Code ?? "en";
                    ExtractSpecificChangelog(_cachedChangelogText, langCode, AppVersion, WhatsNewChangelogList);
                    
                    if (WhatsNewChangelogList.Count > 0)
                    {
                        IsWhatsNewModalOpen = true;
                    }
                }
            }
        }
        catch { }
    }

    [RelayCommand]
    private void CloseWhatsNewModal()
    {
        IsWhatsNewModalOpen = false;
    }

    private void CheckCustomServers()
    {
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "servers.json");
        HasCustomServers = File.Exists(path);
    }

    private void InitializeLanguage()
    {
        string sysLang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName.ToLower();
        SelectedLanguage = Languages.FirstOrDefault(l => l.Code == sysLang) ?? Languages.First();
    }

    partial void OnSelectedLanguageChanged(LanguageItem? value)
    {
        if (value != null) 
        {
            UpdateLocalization(value.Code);
            RefreshChangelogUI();
        }
    }

    partial void OnEnableNotificationsChanged(bool value)
    {
        if (value)
            RequestPermissionsAction?.Invoke();
    }

    partial void OnEnableCountNotificationsChanged(bool value)
    {
        if (value)
            RequestPermissionsAction?.Invoke();
    }

    partial void OnAppThemeColorChanged(Color value)
    {
        OnPropertyChanged(nameof(AccentBrush));
        OnPropertyChanged(nameof(AccentBrushHover));
        OnPropertyChanged(nameof(AccentBrushPressed));
        OnPropertyChanged(nameof(AccentBrush2));
        OnPropertyChanged(nameof(AccentBrushLighter));
        OnPropertyChanged(nameof(PreviewTopBgColor));
        OnPropertyChanged(nameof(PreviewBottomBgColor));
    }

    partial void OnImageOpacityChanged(int value)
    {
        OnPropertyChanged(nameof(ImageOpacityDouble));
    }

    partial void OnImageOffsetYChanged(double value)
    {
        OnPropertyChanged(nameof(ImageOffsetTransform));
    }

    partial void OnSwapTopBarChanged(bool value)
    {
        OnPropertyChanged(nameof(NotSwapTopBar));
    }

    partial void OnSwapLeftRightChanged(bool value)
    {
        OnPropertyChanged(nameof(NotSwapLeftRight));
    }

    partial void OnSwapWavePlayersChanged(bool value)
    {
        OnPropertyChanged(nameof(PreviewTopBgText));
        OnPropertyChanged(nameof(PreviewBottomBgText));
        OnPropertyChanged(nameof(ShowTopBg));
        OnPropertyChanged(nameof(ShowBottomBg));
        OnPropertyChanged(nameof(PreviewTopBgColor));
        OnPropertyChanged(nameof(PreviewBottomBgColor));
    }

    partial void OnShowMapBlockChanged(bool value)
    {
        OnPropertyChanged(nameof(PreviewBottomPlainText));
        OnPropertyChanged(nameof(ShowBottomPlain));
    }

    partial void OnShowWaveBlockChanged(bool value)
    {
        OnPropertyChanged(nameof(ShowTopBg));
        OnPropertyChanged(nameof(ShowBottomBg));
    }

    partial void OnShowPlayersBlockChanged(bool value)
    {
        OnPropertyChanged(nameof(ShowTopBg));
        OnPropertyChanged(nameof(ShowBottomBg));
    }

    partial void OnSelectedVersionChanged(VersionItem? value)
    {
        if (value != null && !string.IsNullOrEmpty(_cachedChangelogText))
        {
            ExtractSpecificChangelog(_cachedChangelogText, SelectedLanguage?.Code ?? "en", value.Version, CurrentChangelogList);
            OnPropertyChanged(nameof(HasCurrentChangelog));
        }
    }

    public void LoadData(bool hide, string hex, int alpha, int interval, string langCode, bool[] srvs, bool hasBgImage, int radius, int opacity, float offsetY, bool swapLR, bool swapWP, bool showMap, bool showWave, bool showPlayers, bool swapTopBar, bool enableNotifs, string trackedPlayers, bool enableCountNotifs, int countThreshold, bool[] countSrvs, List<string> onlinePlayers, bool fullSize, bool fadeBg)
    {
        HideEmpty = hide;
        SelectedInterval = interval;
        HasBackgroundImage = hasBgImage;
        CornerRadius = radius;
        ImageOpacity = opacity;
        ImageOffsetY = offsetY;
        SwapLeftRight = swapLR;
        SwapWavePlayers = swapWP;
        ShowMapBlock = showMap;
        ShowWaveBlock = showWave;
        ShowPlayersBlock = showPlayers;
        SwapTopBar = swapTopBar;
        
        EnableNotifications = enableNotifs;
        TrackedPlayers = trackedPlayers;
        
        EnableCountNotifications = enableCountNotifs;
        PlayerCountThreshold = countThreshold < 2 ? 6 : countThreshold;

        FullSizeImage = fullSize;
        FadeBackground = fadeBg;

        SuggestedPlayers.Clear();
        if (onlinePlayers != null)
        {
            foreach (var p in onlinePlayers)
            {
                SuggestedPlayers.Add(p);
            }
        }
        OnPropertyChanged(nameof(HasSuggestedPlayers));

        if (srvs != null && srvs.Length >= 5)
        {
            ShowSrv1 = srvs[0];
            ShowSrv2 = srvs[1];
            ShowSrv3 = srvs[2];
            ShowSrv4 = srvs[3];
            ShowSrv5 = srvs[4];
        }
        
        if (countSrvs != null && countSrvs.Length >= 5)
        {
            CountSrv1 = countSrvs[0];
            CountSrv2 = countSrvs[1];
            CountSrv3 = countSrvs[2];
            CountSrv4 = countSrvs[3];
            CountSrv5 = countSrvs[4];
        }

        if (!string.IsNullOrEmpty(langCode))
        {
            var lang = Languages.FirstOrDefault(l => l.Code == langCode);
            if (lang != null) SelectedLanguage = lang;
        }

        try
        {
            var c = Color.Parse(hex);
            AppThemeColor = Color.FromArgb((byte)alpha, c.R, c.G, c.B);
        }
        catch { }

        ReloadPreviewImage();
        _ = FetchChangelogAsync(false);
        CheckIfUpdated();
    }

    public void ReloadPreviewImage()
    {
        if (HasBackgroundImage)
        {
            try
            {
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "widget_bg.png");
                if (File.Exists(path))
                {
                    using var fs = File.OpenRead(path);
                    WidgetPreviewImage = Bitmap.DecodeToWidth(fs, 800);
                    return;
                }
            }
            catch (Exception) { }
        }
        WidgetPreviewImage = null;
        HasBackgroundImage = false;
        ImageOffsetY = 0;
    }

    private async Task FetchChangelogAsync(bool force)
    {
        if (!force && (DateTime.Now - _lastChangelogFetch).TotalHours < 24 && !string.IsNullOrEmpty(_cachedChangelogText))
        {
            RefreshChangelogUI();
            return;
        }

        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            
            string latestVer = await client.GetStringAsync("https://raw.githubusercontent.com/ext4n/KF2Monitor/main/version/android/version");
            LatestVer = latestVer.Trim();

            if (LatestVer != AppVersion && !string.IsNullOrEmpty(LatestVer)) HasUpdate = true;

            _cachedChangelogText = await client.GetStringAsync("https://raw.githubusercontent.com/ext4n/KF2Monitor/main/version/android/changes");
            _lastChangelogFetch = DateTime.Now;
            
            RefreshChangelogUI();
        }
        catch 
        { 
            if (!string.IsNullOrEmpty(_cachedChangelogText)) RefreshChangelogUI();
        }
    }

    private void RefreshChangelogUI()
    {
        if (string.IsNullOrEmpty(_cachedChangelogText)) return;

        string langCode = SelectedLanguage?.Code ?? "en";
        string langBlock = GetLangBlock(_cachedChangelogText, langCode);
        if (string.IsNullOrEmpty(langBlock) && langCode != "en")
        {
            langBlock = GetLangBlock(_cachedChangelogText, "en");
        }
        if (string.IsNullOrEmpty(langBlock)) return;

        var matches = Regex.Matches(langBlock, @"\[(\d+(?:\.\d+)+)\]");
        var extractedVersions = new List<string>();
        foreach (Match m in matches) 
        {
            string v = m.Groups[1].Value;
            if (!extractedVersions.Contains(v)) extractedVersions.Add(v);
        }

        var prevSelected = SelectedVersion?.Version;

        AvailableVersions.Clear();
        foreach(var v in extractedVersions)
        {
            string suffix = v == AppVersion ? _locCurrentSuffix : _locPreviousSuffix;
            AvailableVersions.Add(new VersionItem { Version = v, Suffix = suffix });
        }

        var toSelect = AvailableVersions.FirstOrDefault(x => x.Version == prevSelected) ??
                       AvailableVersions.FirstOrDefault(x => x.Version == AppVersion) ??
                       AvailableVersions.FirstOrDefault();
                       
        SelectedVersion = toSelect;

        if (HasUpdate && LatestVer != AppVersion)
        {
            ExtractSpecificChangelog(_cachedChangelogText, langCode, LatestVer, UpdateChangelogList);
            OnPropertyChanged(nameof(HasUpdateChangelog));
        }
        else
        {
            UpdateChangelogList.Clear();
            OnPropertyChanged(nameof(HasUpdateChangelog));
        }
    }

    private string GetLangBlock(string fullText, string langCode)
    {
        int langStart = fullText.IndexOf($"[{langCode}]");
        if (langStart == -1) return "";
        
        int nextLangStart = fullText.Length;
        foreach (var l in Languages) {
            if (l.Code == langCode) continue;
            int idx = fullText.IndexOf($"[{l.Code}]", langStart + 1);
            if (idx != -1 && idx < nextLangStart) nextLangStart = idx;
        }

        return fullText.Substring(langStart, nextLangStart - langStart);
    }

    private void ExtractSpecificChangelog(string fullText, string langCode, string version, ObservableCollection<ChangelogEntry> targetList)
    {
        targetList.Clear();

        string langBlock = GetLangBlock(fullText, langCode);
        if (string.IsNullOrEmpty(langBlock) && langCode != "en")
        {
            langBlock = GetLangBlock(fullText, "en");
        }
        if (string.IsNullOrEmpty(langBlock)) return;

        int verStart = langBlock.IndexOf($"[{version}]");
        if (verStart == -1) return;
        verStart += $"[{version}]".Length;
        
        int nextVerStart = langBlock.IndexOf("\n[", verStart);
        if (nextVerStart == -1) nextVerStart = langBlock.Length;
        
        string verBlock = langBlock.Substring(verStart, nextVerStart - verStart).Trim();

        var lines = verBlock.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            string trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;

            if (trimmed.StartsWith("#"))
            {
                targetList.Add(new ChangelogEntry { IsHeader = true, Text = trimmed.TrimStart('#', ' ') });
            }
            else if (trimmed.StartsWith("-") || trimmed.StartsWith("*"))
            {
                targetList.Add(new ChangelogEntry { IsBullet = true, Text = trimmed.TrimStart('-', '*', ' ') });
            }
            else
            {
                targetList.Add(new ChangelogEntry { IsText = true, Text = trimmed });
            }
        }
    }

    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task PickImage()
    {
        if (PickImageAction != null)
        {
            bool success = await PickImageAction();
            if (success)
            {
                HasBackgroundImage = true;
                ImageOffsetY = 0;
                ReloadPreviewImage();
                Save(); 
            }
        }
    }

    [RelayCommand]
    private void ClearImage()
    {
        HasBackgroundImage = false;
        WidgetPreviewImage = null;
        ImageOffsetY = 0;

        try
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "widget_bg.png");
            if (File.Exists(path)) File.Delete(path);
        }
        catch { }

        Save();
    }

    [RelayCommand]
    private void CloseEditor()
    {
        IsEditorOpen = false;
        SaveCommand.Execute(null);
    }

    [RelayCommand]
    private void ResetWidgetSettings()
    {
        ImageOffsetY = 0;
        SwapLeftRight = false;
        SwapWavePlayers = false;
        ShowMapBlock = true;
        ShowPlayersBlock = true;
        ShowWaveBlock = true;
        SwapTopBar = false;
        FullSizeImage = true;
        FadeBackground = false;
    }

    [RelayCommand]
    public void Save()
    {
        string hex = $"#{AppThemeColor.R:X2}{AppThemeColor.G:X2}{AppThemeColor.B:X2}";
        int alpha = AppThemeColor.A;

        bool[] srvs = new[] { ShowSrv1, ShowSrv2, ShowSrv3, ShowSrv4, ShowSrv5 };
        bool[] countSrvs = new[] { CountSrv1, CountSrv2, CountSrv3, CountSrv4, CountSrv5 };

        OnSaveSettings?.Invoke(HideEmpty, hex, alpha, SelectedInterval, SelectedLanguage?.Code ?? "en", srvs, HasBackgroundImage, CornerRadius, ImageOpacity, (float)ImageOffsetY, SwapLeftRight, SwapWavePlayers, ShowMapBlock, ShowWaveBlock, ShowPlayersBlock, SwapTopBar, LocWave, LocUpdated, LocSettingsSaved, EnableNotifications, TrackedPlayers, EnableCountNotifications, PlayerCountThreshold, countSrvs, FullSizeImage, FadeBackground, LocNotifPlayerTitle, LocNotifPlayerBody, LocNotifPopTitle, LocNotifPopBody, LocNotifPlayerLeftTitle, LocNotifPlayerLeftBody, LocNotifUpdateTitle, LocNotifUpdateBody, AppVersion, LocStatus, LocPhrase1, LocPhrase2, LocPhrase3, LocPhrase4, LocPhrase5);
    }

    [RelayCommand]
    private void AddSuggestedPlayer(string name)
    {
        if (string.IsNullOrWhiteSpace(TrackedPlayers))
        {
            TrackedPlayers = name;
        }
        else
        {
            var current = TrackedPlayers.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
            if (!current.Contains(name))
            {
                TrackedPlayers = TrackedPlayers.Trim();
                if (!TrackedPlayers.EndsWith(",")) TrackedPlayers += ", ";
                TrackedPlayers += name;
            }
        }
    }

    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task CheckUpdates()
    {
        HasUpdate = false;
        UpdateMessage = SelectedLanguage?.Code switch {
            "et" => "Kontrollin...", "sk" => "Kontrolujem...", "lt" => "Tikrinama...", 
            "lv" => "Pārbauda...", "fr" => "Vérification...", "pl" => "Sprawdzanie...", 
            "de" => "Überprüfung...", "ja" => "確認中...", _ => "Checking..."
        };

        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            string latestVer = await client.GetStringAsync("https://raw.githubusercontent.com/ext4n/KF2Monitor/main/version/android/version");
            LatestVer = latestVer.Trim();

            if (LatestVer != AppVersion && !string.IsNullOrEmpty(LatestVer))
            {
                UpdateMessage = SelectedLanguage?.Code switch
                {
                    "et" => $"Uus versioon on saadaval: {LatestVer}",
                    "sk" => $"Nová verzia k dispozícii: {LatestVer}",
                    "lt" => $"Yra nauja versija: {LatestVer}",
                    "lv" => $"Pieejama jauna versija: {LatestVer}",
                    "fr" => $"Nouvelle version disponible: {LatestVer}",
                    "pl" => $"Dostępna nowa wersja: {LatestVer}",
                    "de" => $"Neue Version verfügbar: {LatestVer}",
                    "ja" => $"新しいバージョンが利用可能です: {LatestVer}",
                    _ => $"New Version Available: {LatestVer}"
                };
                HasUpdate = true;
                await FetchChangelogAsync(true);
            }
            else
            {
                UpdateMessage = SelectedLanguage?.Code switch
                {
                    "et" => "Teil on uusim versioon.",
                    "sk" => "Máte najnovšiu verziu.",
                    "lt" => "Jūs turite naujausią versiją.",
                    "lv" => "Jums ir jaunākā versija.",
                    "fr" => "Vous avez la dernière version.",
                    "pl" => "Masz najnowszą wersję.",
                    "de" => "Sie haben die neueste Version.",
                    "ja" => "最新バージョンを使用しています。",
                    _ => "You are on the latest version."
                };
                await FetchChangelogAsync(true);
            }
        }
        catch
        {
            UpdateMessage = SelectedLanguage?.Code switch {
                "et" => "Viga kontrollimisel.", "sk" => "Chyba pri kontrole.", "lt" => "Klaida tikrinant.", 
                "lv" => "Kļūda pārbaudot.", "fr" => "Erreur de vérification.", "pl" => "Błąd sprawdzania.", 
                "de" => "Fehler bei der Überprüfung.", "ja" => "確認エラー。", _ => "Error checking updates."
            };
        }
    }

    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task UpdateServersFromCloud()
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            
            string url = "https://raw.githubusercontent.com/ext4n/KF2Monitor/main/servers/mod-eu.json";
            string newJson = (await client.GetStringAsync(url)).Trim();

            List<RemoteServerConfig>? fetchedServers = null;
            try
            {
                fetchedServers = JsonSerializer.Deserialize<List<RemoteServerConfig>>(newJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch { }

            if (fetchedServers == null) return;

            var defaultServers = new List<RemoteServerConfig>
            {
                new RemoteServerConfig { Name = "Mod-EU | Normal #1", IP = "78.58.223.116", Port = 27030 },
                new RemoteServerConfig { Name = "Mod-EU | Hard #2", IP = "78.58.223.116", Port = 27031 },
                new RemoteServerConfig { Name = "Mod-EU | Suicidal #3", IP = "78.58.223.116", Port = 27032 },
                new RemoteServerConfig { Name = "Mod-EU | HOE #4", IP = "78.58.223.116", Port = 27033 },
                new RemoteServerConfig { Name = "Mod-EU | Extreme #5", IP = "78.58.223.116", Port = 27034 }
            };

            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "servers.json");
            
            List<RemoteServerConfig> currentServers = defaultServers;
            if (File.Exists(path))
            {
                try
                {
                    string currentJson = await File.ReadAllTextAsync(path);
                    var parsed = JsonSerializer.Deserialize<List<RemoteServerConfig>>(currentJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (parsed != null && parsed.Count > 0) currentServers = parsed;
                }
                catch { }
            }

            bool isDifferent = false;
            if (currentServers.Count != fetchedServers.Count)
            {
                isDifferent = true;
            }
            else
            {
                for (int i = 0; i < currentServers.Count; i++)
                {
                    if (currentServers[i].IP != fetchedServers[i].IP || currentServers[i].Port != fetchedServers[i].Port)
                    {
                        isDifferent = true;
                        break;
                    }
                }
            }

            if (isDifferent)
            {
                await File.WriteAllTextAsync(path, newJson);
                HasCustomServers = true;
                ShowToastAction?.Invoke(LocMsgUpdated);
                SaveCommand.Execute(null); 
            }
            else
            {
                ShowToastAction?.Invoke(LocMsgNoUpdates);
            }
        }
        catch (Exception)
        {
            ShowToastAction?.Invoke("Error");
        }
    }

    [RelayCommand]
    private void ResetServers()
    {
        try
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "servers.json");
            if (File.Exists(path))
            {
                File.Delete(path);
                HasCustomServers = false;
                ShowToastAction?.Invoke(LocMsgReset);
                SaveCommand.Execute(null); 
            }
        }
        catch { }
    }

    [RelayCommand]
    private void OpenUpdateModal()
    {
        IsUpdateModalOpen = true;
        CheckDownloadedApk();
    }
    
    private void CheckDownloadedApk()
    {
        string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Updates");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        
        string expectedFile = Path.Combine(dir, $"KF2Monitor_v{LatestVer}.apk");
        
        foreach(var file in Directory.GetFiles(dir, "*.apk"))
        {
            if (file != expectedFile)
            {
                try { File.Delete(file); } catch { }
            }
        }
        
        if (File.Exists(expectedFile))
        {
            IsDownloading = false;
            IsDownloadFinished = true;
            DownloadProgress = 100;
            DownloadProgressText = LocReadyToInstall; 
        }
        else
        {
            IsDownloading = false;
            IsDownloadFinished = false;
            DownloadProgress = 0;
            DownloadProgressText = "";
        }
    }

    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task DownloadAndInstall()
    {
        string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Updates");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        string expectedFile = Path.Combine(dir, $"KF2Monitor_v{LatestVer}.apk");
        
        if (IsDownloadFinished && File.Exists(expectedFile))
        {
            InstallApkAction?.Invoke(expectedFile);
            return;
        }
        
        IsDownloading = true;
        IsDownloadFinished = false;
        DownloadProgress = 0;
        DownloadProgressText = "0.0 MB / 0.0 MB";
        
        try
        {
            using var client = new HttpClient();
            using var response = await client.GetAsync(_updateApkUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            
            var totalBytes = response.Content.Headers.ContentLength ?? -1L;

            using var stream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(expectedFile, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
            
            var buffer = new byte[8192];
            long totalRead = 0;
            int bytesRead;
            
            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead);
                totalRead += bytesRead;
                
                if (totalBytes != -1)
                {
                    double mbRead = totalRead / 1048576.0;
                    double mbTotal = totalBytes / 1048576.0;

                    DownloadProgress = (double)totalRead / totalBytes * 100;
                    DownloadProgressText = $"{mbRead:F1} MB / {mbTotal:F1} MB";
                }
            }
            
            IsDownloading = false;
            IsDownloadFinished = true;
            DownloadProgress = 100;
            DownloadProgressText = LocReadyToInstall;

            InstallApkAction?.Invoke(expectedFile);
        }
        catch (Exception ex)
        {
            IsDownloading = false;
            DownloadProgressText = "Error: " + ex.GetBaseException().Message;
        }
    }

    [RelayCommand]
    private void CloseUpdateModal()
    {
        IsUpdateModalOpen = false;
    }

    [RelayCommand]
    private void OpenExtasy() => OpenUrlAction?.Invoke("https://extasy.es");

    [RelayCommand]
    private void OpenSteam() => OpenUrlAction?.Invoke(DeveloperSteamUrl);

    [RelayCommand]
    private void OpenGithub() => OpenUrlAction?.Invoke("https://github.com/ext4n");
    
    [RelayCommand]
    private void OpenRepo() => OpenUrlAction?.Invoke("https://github.com/ext4n/KF2Monitor/");

    [RelayCommand]
    private void OpenLastFm() => OpenUrlAction?.Invoke("https://www.last.fm/user/GALAXYSLOT");

    [RelayCommand]
    private void OpenEmail() => OpenUrlAction?.Invoke("mailto:ext4n.dev@gmail.com");

    [RelayCommand]
    private void OpenDiscord() => OpenUrlAction?.Invoke("https://discordapp.com/invite/Ps3jqRr");

    private void UpdateLocalization(string code)
    {
        UpdateMessage = "";

        FaqList.Clear();
        foreach (var item in FaqManager.GetFaqs(code))
        {
            FaqList.Add(item);
        }

        switch (code)
        {
            case "et":
                LocUpdateBadge = "UUS";
                LocTabGeneral = "Üldine"; LocTabWidget = "Vidin"; LocTabFaq = "KKK"; LocTabUpdates = "Uuendused"; LocTabAbout = "Teave";
                LocGeneralSettings = "Üldseaded"; LocLanguage = "Keel";
                LocHideEmpty = "Tühjad serverid"; LocHideEmptyDesc = "Peida serverid, kus pole aktiivseid mängijaid";
                LocInterval = "Uuendamise intervall (minutites)"; LocIntervalDesc = "Kui tihti vidin uusi andmeid hangib";
                LocServerVisibility = "Serverite nähtavus"; LocServerVisibilityDesc = "Valige, milliseid servereid pingida ja kuvada";
                LocSaveBtn = "SALVESTA KONFIGURATSIOON"; LocWidgetApp = "Vidina välimus";
                LocCornerRadius = "Nurga Raadius"; LocThemeColor = "Teema värv ja läbipaistvus"; LocThemeColorDesc = "Valige tausta põhivärv";
                LocBackgroundImage = "Taustapilt"; LocBackgroundImageDesc = "Valige pilt oma galeriist"; LocSelectImage = "Vali Pilt"; LocImageOpacity = "Pildi läbipaistvus";
                LocLivePreview = "EELVAADE (Vajuta pikalt, et redigeerida)";
                LocPreviewDisclaimer = "See on ainult visuaalne eelvaade kujundamiseks, mitte funktsionaalne vidin. Tegeliku vidina lisamiseks kasutage oma seadme avakuva menüüd.";
                LocAppVersion = "Rakenduse versioon";
                LocCheckUpdateBtn = "Kontrolli"; LocDownloadBtn = "Laadi alla";
                LocAboutDesc = "Kohandatav avakuva vidin, mis näitab Killing Floor 2 MOD-EU serverite reaalajas olekut, pakkudes täpsemaid visuaalseid seadeid ja lisatööriistu.";
                LocSpecialThanks = "See projekt sai teoks tänu paljude suurepäraste inimeste tohutule toele. Suur ja südamlik aitäh kõigile, kes osalesid arenduses ja kasvamises! Eraldi ja piiritu tänu projekti asutajatele, administraatoritele ja juhtidele — Edvis ja Wyvern. Teie lõputu pühendumus ja usk teevad sellest kommuunist midagi tõeliselt elavat ja ägedat!";
                LocWidgetEditorTitle = "Vidina Redaktor"; LocBgPosition = "Tausta Positsioon";
                LocSwapLR = "Vaheta Vasak ja Parem"; LocSwapWP = "Vaheta Laine ja Mängijad";
                LocShowMap = "Näita Kaarti"; LocShowPlayers = "Näita Mängijate Arvu";
                LocShowWave = "Näita Praegust Lainet"; LocSwapTopBarText = "Vaheta päise elemendid"; 
                LocFullSizeImage = "Täissuuruses taustapilt"; LocFadeBackground = "Hajuta pilt tausta";
                LocDone = "Valmis"; LocReset = "Lähtesta";
                LocWave = "Laine:"; LocUpdated = "Uuendatud:"; LocSettingsSaved = "Seaded edukalt salvestatud!";
                LocDiscordBtn = "Ametlik MOD-EU Discordi server";
                LocRepoBtn = "Ametlik GitHubi repositoorium";
                LocRemoteUpdateTitle = "Pilveuuendus"; 
                LocRemoteUpdateDesc = "Kui serverid muutsid IP-d või porti, laadige alla uusim loend. Saate alati naasta algseadete juurde."; 
                LocBtnUpdateServers = "Värskenda pilvest"; 
                LocMsgNoUpdates = "Uusi värskendusi ei leitud.";
                LocMsgUpdated = "Serverid on edukalt värskendatud.";
                LocMsgReset = "Naasti algserverite juurde.";
                LocEnableNotifications = "Luba mängijate jälgimise teated";
                LocTrackedPlayersHint = "Sisesta hüüdnimed (komadega eraldatud)";
                LocSuggestedPlayers = "Mängijad võrgus (puuduta lisamiseks):";
                LocEnableCountNotifications = "Teavita, kui server täitub"; LocPlayerCountThreshold = "Mängijate piir:";
                LocPopWarning = "Teavitused töötavad ainult nende serverite puhul, mis on sisse lülitatud jaotises 'Serverite nähtavus'.";
                LocNotifPlayerTitle = "Mängija on võrgus!"; LocNotifPlayerBody = "{0} liitus serveriga {1}";
                LocNotifPopTitle = "Server täitub!"; LocNotifPopBody = "{0} serveris on nüüd {1} mängijat.";
                LocNotifPlayerLeftTitle = "Mängija lahkus!"; LocNotifPlayerLeftBody = "{0} lahkus serverist {1}";
                LocNotifUpdateTitle = "Uus uuendus saadaval!"; LocNotifUpdateBody = "Versioon {0} on allalaadimiseks valmis.";
                LocCreatedBy = "Idee ja arendus: EXT4N (09.2026)";
                LocCurrentVersionBlock = "Versiooniajalugu"; LocUpdateVersionBlock = "Uue versiooni muudatused";
                LocUpdateModalTitle = "Rakenduse uuendamine"; LocInstallBtn = "Installi"; LocCancelBtn = "Tühista"; LocReadyToInstall = "Valmis installimiseks";
                _locCurrentSuffix = "(Praegune)"; _locPreviousSuffix = "(Eelmine)";
                
                LocStatus = "Olek";
                LocPhrase1 = "Kangelaste ootel...";
                LocPhrase2 = "Vaikus rajatises...";
                LocPhrase3 = "Ühtegi zedi pole veel tapetud...";
                LocPhrase4 = "Ala puhas, esialgu...";
                LocPhrase5 = "Valmis kasutuselevõtuks.";
                break;
            case "sk":
                LocUpdateBadge = "NOVÉ";
                LocTabGeneral = "Hlavné"; LocTabWidget = "Widget"; LocTabFaq = "FAQ"; LocTabUpdates = "Update"; LocTabAbout = "Info";
                LocGeneralSettings = "Všeobecné nastavenia"; LocLanguage = "Jazyk";
                LocHideEmpty = "Prázdne servery"; LocHideEmptyDesc = "Skryť servery bez aktívnych hráčov";
                LocInterval = "Interval aktualizácie (min)"; LocIntervalDesc = "Ako často miniaplikácia získava nové údaje";
                LocServerVisibility = "Viditeľnosť serverov"; LocServerVisibilityDesc = "Vyberte, ktoré servery chcete zobraziť";
                LocSaveBtn = "ULOŽIŤ KONFIGURÁCIU"; LocWidgetApp = "Vzhľad miniaplikácie";
                LocCornerRadius = "Polomer Zaoblenia"; LocThemeColor = "Farba a priehľadnosť"; LocThemeColorDesc = "Vyberte základnú farbu pozadia";
                LocBackgroundImage = "Obrázok na pozadí"; LocBackgroundImageDesc = "Vyberte si obrázok z galérie"; LocSelectImage = "Vybrať Obrázok"; LocImageOpacity = "Nepriehľadnosť obrázka";
                LocLivePreview = "NÁHĽAD (Dlhým stlačením upravíte)";
                LocPreviewDisclaimer = "Toto je len vizuálny náhľad na úpravu štýlu, nie skutočne funkčná miniaplikácia. Na pridanie skutočnej miniaplikácie použite ponuku domovskej obrazovky zariadenia.";
                LocAppVersion = "Verzia aplikácie";
                LocCheckUpdateBtn = "Skontrolovať"; LocDownloadBtn = "Stiahnuť";
                LocAboutDesc = "Prispôsobiteľná miniaplikácia na domovskú obrazovku, ktorá zobrazuje aktuálny stav serverov Killing Floor 2 MOD-EU a ponúka pokročilé vizuálne nastavenia a ďalšie nástroje.";
                LocSpecialThanks = "Tento projekt sa podarilo zrealizovať vďaka obrovskej podpore mnohých skvelých ľudí. Obrovské, úprimné ďakujem všetkým, ktorí sa podieľali na vývoji a rozvoji! Zvláštna a nekonečná vďaka patrí zakladateľom, administrátorom a manažérom projektu — Edvisovi a Wyvernovi. Vaše nekonečné odhodlanie a viera robia túto komunitu skutočne živou a úžasnou!";
                LocWidgetEditorTitle = "Editor Miniaplikácie"; LocBgPosition = "Pozícia Pozadia";
                LocSwapLR = "Vymeniť Ľavú a Pravú"; LocSwapWP = "Vymeniť Vlnu a Hráčov";
                LocShowMap = "Zobraziť Mapu"; LocShowPlayers = "Zobraziť Počet Hráčov";
                LocShowWave = "Zobraziť Aktuálnu Vlnu"; LocSwapTopBarText = "Vymeniť prvky hlavičky"; 
                LocFullSizeImage = "Obrázok na pozadí v plnej veľkosti"; LocFadeBackground = "Postupné splývanie obrázka s pozadím";
                LocDone = "Hotovo"; LocReset = "Obnoviť";
                LocWave = "Vlna:"; LocUpdated = "Aktualizované:"; LocSettingsSaved = "Nastavenia boli úspešne uložené!";
                LocDiscordBtn = "Oficiálny Discord server MOD-EU";
                LocRepoBtn = "Oficiálny repozitár GitHub";
                LocRemoteUpdateTitle = "Aktualizácia z cloudu"; 
                LocRemoteUpdateDesc = "Ak servery zmenili IP alebo port, stiahnite si najnovší zoznam. Vždy sa môžete vrátiť k predvoleným nastaveniam."; 
                LocBtnUpdateServers = "Aktualizovať z cloudu"; 
                LocMsgNoUpdates = "Nenašli sa žiadne aktualizácie.";
                LocMsgUpdated = "Servery boli úspešne aktualizované.";
                LocMsgReset = "Návrat k predvoleným serverom.";
                LocEnableNotifications = "Povoliť upozornenia na sledovanie hráčov";
                LocTrackedPlayersHint = "Zadajte prezývky (oddelené čiarkou)";
                LocSuggestedPlayers = "Online hráči (klepnutím pridáte):";
                LocEnableCountNotifications = "Upozorniť na preplnený server"; LocPlayerCountThreshold = "Hranica hráčov:";
                LocPopWarning = "Upozornenia budú fungovať len pre servery povolené v sekcii 'Viditeľnosť serverov'.";
                LocNotifPlayerTitle = "Hráč je online!"; LocNotifPlayerBody = "{0} sa pripojil na {1}";
                LocNotifPopTitle = "Server sa plní!"; LocNotifPopBody = "Na serveri {0} je teraz {1} hráčov.";
                LocNotifPlayerLeftTitle = "Hráč sa odpojil!"; LocNotifPlayerLeftBody = "{0} sa odpojil z {1}";
                LocNotifUpdateTitle = "Nová aktualizácia!"; LocNotifUpdateBody = "Verzia {0} je pripravená na stiahnutie.";
                LocCreatedBy = "Nápad a vývoj: EXT4N (09.2026)";
                LocCurrentVersionBlock = "História verzií"; LocUpdateVersionBlock = "Zmeny v novej aktualizácii";
                LocUpdateModalTitle = "Aktualizácia aplikácie"; LocInstallBtn = "Inštalovať"; LocCancelBtn = "Zrušiť"; LocReadyToInstall = "Pripravené na inštaláciu";
                _locCurrentSuffix = "(Aktuálna)"; _locPreviousSuffix = "(Predchádzajúca)";
                
                LocStatus = "Stav";
                LocPhrase1 = "Čaká sa na hrdinov...";
                LocPhrase2 = "Ticho v zariadení...";
                LocPhrase3 = "Zatiaľ žiadni mŕtvi zedovia...";
                LocPhrase4 = "Oblasť čistá, zatiaľ...";
                LocPhrase5 = "Pripravené na nasadenie.";
                break;
            case "lt":
                LocUpdateBadge = "NAUJA";
                LocTabGeneral = "Pagrindinis"; LocTabWidget = "Valdiklis"; LocTabFaq = "DUK"; LocTabUpdates = "Atnaujinimai"; LocTabAbout = "Apie";
                LocGeneralSettings = "Pagrindiniai nustatymai"; LocLanguage = "Kalba";
                LocHideEmpty = "Tušti serveriai"; LocHideEmptyDesc = "Slėpti serverius be aktyvių žaidėjų";
                LocInterval = "Atnaujinimo intervalas (min)"; LocIntervalDesc = "Kaip dažnai valdiklis gauna duomenis";
                LocServerVisibility = "Serverių matomumas"; LocServerVisibilityDesc = "Pasirinkite, kuriuos serverius rodyti";
                LocSaveBtn = "IŠSAUGOTI KONFIGŪRACIJĄ"; LocWidgetApp = "Valdiklio išvaizda";
                LocCornerRadius = "Kampo Spindulys"; LocThemeColor = "Tema ir skaidrumas"; LocThemeColorDesc = "Pasirinkite valdiklio fono spalvą";
                LocBackgroundImage = "Fono paveikslėlis"; LocBackgroundImageDesc = "Pasirinkite paveikslėlį iš galerijos"; LocSelectImage = "Pasirinkti"; LocImageOpacity = "Paveikslėlio skaidrumas";
                LocLivePreview = "PERŽIŪRA (Ilgai paspauskite, kad redaguotumėte)";
                LocPreviewDisclaimer = "Tai tik vizualinė peržiūra stiliaus tikslais, o ne veikiantis valdiklis. Norėdami pridėti tikrą valdiklį, naudokite savo įrenginio pagrindinio ekrano meniu.";
                LocAppVersion = "Programos versija";
                LocCheckUpdateBtn = "Tikrinti"; LocDownloadBtn = "Atsisiųsti";
                LocAboutDesc = "Pritaikomas pagrindinio ekrano valdiklis, rodantis „Killing Floor 2 MOD-EU“ serverių būseną realiuoju laiku, su išplėstiniais vaizdo nustatymais ir papildomais įrankiais.";
                LocSpecialThanks = "Šis projektas tapo įmanomas dėl didžiulio daugelio nuostabių žmonių palaikymo. Didžiulis, nuoširdus ačiū visiems, prisidėjusiems prie kūrimo ir tobulinimo! Ypatingas ir beribis dėkingumas projekto įkūrėjams, administratoriams ir vadovams — Edvis ir Wyvern. Jų begalinis atsidavimas ir tikėjimas daro šią bendruomenę tikrai gyvą ir nuostabią!";
                LocWidgetEditorTitle = "Valdiklio Redaktorius"; LocBgPosition = "Fono Pozicija";
                LocSwapLR = "Sukeisti Kairę ir Dešinę"; LocSwapWP = "Sukeisti Bangą ir Žaidėjus";
                LocShowMap = "Rodyti Žemėlapį"; LocShowPlayers = "Rodyti Žaidėjų Skaičių";
                LocShowWave = "Rodyti Dabartinę Bangą"; LocSwapTopBarText = "Sukeisti antraštės elementus"; 
                LocFullSizeImage = "Viso dydžio fono paveikslėlis"; LocFadeBackground = "Išblukinti paveikslėlį į foną";
                LocDone = "Atlikta"; LocReset = "Atstatyti";
                LocWave = "Banga:"; LocUpdated = "Atnaujinta:"; LocSettingsSaved = "Nustatymai sėkmingai išsaugoti!";
                LocDiscordBtn = "Oficialus MOD-EU Discord serveris";
                LocRepoBtn = "Oficiali GitHub saugykla";
                LocRemoteUpdateTitle = "Debesies atnaujinimas"; 
                LocRemoteUpdateDesc = "Jei serveriai pakeitė IP arba prievadą, atsisiųskite naujausią sąrašą. Visada galite grįžti prie numatytųjų nustatymų."; 
                LocBtnUpdateServers = "Atnaujinti iš debesies"; 
                LocMsgNoUpdates = "Naujų atnaujinimų nerasta.";
                LocMsgUpdated = "Serveriai sėkmingai atnaujinti.";
                LocMsgReset = "Grąžinti numatytieji serveriai.";
                LocEnableNotifications = "Įjungti pranešimus apie žaidėjų stebėjimą";
                LocTrackedPlayersHint = "Įveskite pravardes (atskirtas kableliais)";
                LocSuggestedPlayers = "Prisijungę žaidėjai (palieskite, kad pridėtumėte):";
                LocEnableCountNotifications = "Pranešti apie pilną serverį"; LocPlayerCountThreshold = "Žaidėjų riba:";
                LocPopWarning = "Pranešimai veiks tik tiems serveriams, kurie įjungti skiltyje 'Serverių matomumas'.";
                LocNotifPlayerTitle = "Žaidėjas prisijungė!"; LocNotifPlayerBody = "{0} prisijungė prie {1}";
                LocNotifPopTitle = "Serveris pildosi!"; LocNotifPopBody = "Serveryje {0} dabar yra {1} žaidėjų.";
                LocNotifPlayerLeftTitle = "Žaidėjas atsijungė!"; LocNotifPlayerLeftBody = "{0} išėjo iš {1}";
                LocNotifUpdateTitle = "Naujas atnaujinimas!"; LocNotifUpdateBody = "Versija {0} paruošta atsisiųsti.";
                LocCreatedBy = "Idėja ir kūrimas: EXT4N (09.2026)";
                LocCurrentVersionBlock = "Versijų istorija"; LocUpdateVersionBlock = "Naujo atnaujinimo pakeitimai";
                LocUpdateModalTitle = "Programos atnaujinimas"; LocInstallBtn = "Įdiegti"; LocCancelBtn = "Atšaukti"; LocReadyToInstall = "Paruošta įdiegti";
                _locCurrentSuffix = "(Dabartinė)"; _locPreviousSuffix = "(Ankstesnė)";
                
                LocStatus = "Būsena";
                LocPhrase1 = "Laukiama didvyrių...";
                LocPhrase2 = "Tyla objekte...";
                LocPhrase3 = "Dar nenužudytas nė vienas zedas...";
                LocPhrase4 = "Teritorija švari, kol kas...";
                LocPhrase5 = "Pasiruošę dislokavimui.";
                break;
            case "lv":
                LocUpdateBadge = "JAUNS";
                LocTabGeneral = "Galvenais"; LocTabWidget = "Logrīks"; LocTabFaq = "BUJ"; LocTabUpdates = "Atjauninājumi"; LocTabAbout = "Par";
                LocGeneralSettings = "Galvenie iestatījumi"; LocLanguage = "Valoda";
                LocHideEmpty = "Tukšie serveri"; LocHideEmptyDesc = "Paslēpt serverus bez spēlētājiem";
                LocInterval = "Atjaunināšanas intervāls (min)"; LocIntervalDesc = "Cik bieži logrīks iegūst datus";
                LocServerVisibility = "Serveru redzamība"; LocServerVisibilityDesc = "Izvēlieties, kurus serverus rādīt";
                LocSaveBtn = "SAGLABĀT KONFIGURĀCIJU"; LocWidgetApp = "Logrīka izskats";
                LocCornerRadius = "Stūra Rādiuss"; LocThemeColor = "Tēma un caurspīdīgums"; LocThemeColorDesc = "Izvēlieties logrīka fona krāsu";
                LocBackgroundImage = "Fona attēls"; LocBackgroundImageDesc = "Izvēlieties attēlu no galerijas"; LocSelectImage = "Izvēlēties"; LocImageOpacity = "Attēla caurspīdīgums";
                LocLivePreview = "PRIEKŠSKATĪJUMS (Turiet nospiestu, lai rediģētu)";
                LocPreviewDisclaimer = "Šis ir tikai vizuāls priekšskatījums stila nolūkiem, nevis funkcionāls logrīks. Lai pievienotu faktisko logrīku, izmantojiet ierīces sākuma ekrāna izvēlni.";
                LocAppVersion = "Lietotnes versija";
                LocCheckUpdateBtn = "Pārbaudīt"; LocDownloadBtn = "Lejupielādēt";
                LocAboutDesc = "Pielāgojams sākuma ekrāna logrīks, kas reāllaikā parāda Killing Floor 2 MOD-EU serveru statusu, piedāvājot papildu vizuālos iestatījumus un rīkus.";
                LocSpecialThanks = "Šis projekts kļuva iespējams, pateicoties daudzu brīnišķīgu cilvēku milzīgajam atbalstam. Liels un sirsnīgs paldies visiem, kas iesaistījās izstrādē un attīstībā! Un bezgalīga pateicība projekta dibinātājiem, administratoriem un vadītājiem — Edvis un Wyvern. Jūsu nebeidzamie centieni un ticība padara šo kopienu patiesi dzīvu un foršu!";
                LocWidgetEditorTitle = "Logrīka Redaktors"; LocBgPosition = "Fona Pozīcija";
                LocSwapLR = "Apmainīt Kreiso un Labo"; LocSwapWP = "Apmainīt Vilni un Spēlētājus";
                LocShowMap = "Rādīt Karti"; LocShowPlayers = "Rādīt Spēlētāju Skaitu";
                LocShowWave = "Rādīt Pašreizējo Vilni"; LocSwapTopBarText = "Apmainīt galvenes elementus"; 
                LocFullSizeImage = "Pilna izmēra fona attēls"; LocFadeBackground = "Sapludināt attēlu ar fonu";
                LocDone = "Gatavs"; LocReset = "Atiestatīt";
                LocWave = "Vilnis:"; LocUpdated = "Atjaunināts:"; LocSettingsSaved = "Iestatījumi veiksmīgi saglabāti!";
                LocDiscordBtn = "Oficiālais MOD-EU Discord serveris";
                LocRepoBtn = "Oficiālais GitHub repozitorijs";
                LocRemoteUpdateTitle = "Mākoņa atjauninājums"; 
                LocRemoteUpdateDesc = "Ja serveri mainīja IP vai portu, lejupielādējiet jaunāko sarakstu. Jūs vienmēr varat atgriezties pie noklusējuma iestatījumiem."; 
                LocBtnUpdateServers = "Atjaunināt no mākoņa"; 
                LocMsgNoUpdates = "Jauni atjauninājumi nav atrasti.";
                LocMsgUpdated = "Serveri veiksmīgi atjaunināti.";
                LocMsgReset = "Atgriezts pie noklusējuma serveriem.";
                LocEnableNotifications = "Iespējot spēlētāju izsekošanas paziņojumus";
                LocTrackedPlayersHint = "Ievadiet iesaukas (atdalītas ar komatiem)";
                LocSuggestedPlayers = "Tiešsaistes spēlētāji (pieskarieties, lai pievienotu):";
                LocEnableCountNotifications = "Paziņot par pilnu serveri"; LocPlayerCountThreshold = "Spēlētāju slieksnis:";
                LocPopWarning = "Paziņojumi darbosies tikai tiem serveriem, kas ir iespējoti sadaļā 'Serveru redzamība'.";
                LocNotifPlayerTitle = "Spēlētājs tiešsaistē!"; LocNotifPlayerBody = "{0} pievienojās {1}";
                LocNotifPopTitle = "Serveris piepildās!"; LocNotifPopBody = "Serverī {0} tagad ir {1} spēlētāji.";
                LocNotifPlayerLeftTitle = "Spēlētājs atvienojās!"; LocNotifPlayerLeftBody = "{0} pameta {1}";
                LocNotifUpdateTitle = "Jauns atjauninājums!"; LocNotifUpdateBody = "Versija {0} ir gatava lejupielādei.";
                LocCreatedBy = "Ideja un izstrāde: EXT4N (09.2026)";
                LocCurrentVersionBlock = "Versiju vēsture"; LocUpdateVersionBlock = "Jaunā atjauninājuma izmaiņas";
                LocUpdateModalTitle = "Lietotnes atjaunināšana"; LocInstallBtn = "Instalēt"; LocCancelBtn = "Atcelt"; LocReadyToInstall = "Gatavs instalēšanai";
                _locCurrentSuffix = "(Pašreizējā)"; _locPreviousSuffix = "(Iepriekšējā)";
                
                LocStatus = "Statuss";
                LocPhrase1 = "Gaida varoņus...";
                LocPhrase2 = "Klusums objektā...";
                LocPhrase3 = "Vēl nav nogalināts neviens zeds...";
                LocPhrase4 = "Teritorija tīra, pagaidām...";
                LocPhrase5 = "Gatavs izvietošanai.";
                break;
            case "fr":
                LocUpdateBadge = "NOUVEAU";
                LocTabGeneral = "Général"; LocTabWidget = "Widget"; LocTabFaq = "FAQ"; LocTabUpdates = "Mises à jour"; LocTabAbout = "À propos";
                LocGeneralSettings = "Paramètres généraux"; LocLanguage = "Langue";
                LocHideEmpty = "Serveurs vides"; LocHideEmptyDesc = "Masquer les serveurs sans joueurs";
                LocInterval = "Intervalle de mise à jour (min)"; LocIntervalDesc = "Fréquence d'actualisation du widget";
                LocServerVisibility = "Visibilité des serveurs"; LocServerVisibilityDesc = "Choisissez les serveurs à afficher";
                LocSaveBtn = "ENREGISTRER LA CONFIGURATION"; LocWidgetApp = "Apparence du widget";
                LocCornerRadius = "Rayon de Coin"; LocThemeColor = "Couleur et transparence"; LocThemeColorDesc = "Choisissez la couleur de fond";
                LocBackgroundImage = "Image de fond"; LocBackgroundImageDesc = "Choisissez une image de votre galerie"; LocSelectImage = "Sélectionner"; LocImageOpacity = "Opacité de l'image";
                LocLivePreview = "APERÇU EN DIRECT (Appui long pour éditer)";
                LocPreviewDisclaimer = "Ceci est un aperçu visuel pour la personnalisation, pas un widget fonctionnel. Utilisez le menu de l'écran d'accueil de votre appareil pour ajouter le vrai widget.";
                LocAppVersion = "Version de l'application";
                LocCheckUpdateBtn = "Vérifier"; LocDownloadBtn = "Télécharger";
                LocAboutDesc = "Un widget d'écran d'accueil personnalisable qui affiche l'état en temps réel des serveurs Killing Floor 2 MOD-EU, avec des paramètres visuels avancés et des outils supplémentaires.";
                LocSpecialThanks = "Ce projet a été rendu possible grâce à l'immense soutien de nombreuses personnes formidables. Un grand merci du fond du cœur à tous ceux qui ont participé au développement et à l'évolution ! Une gratitude toute particulière et sans bornes aux fondateurs, administrateurs et managers du projet — Edvis et Wyvern. Votre dévouement et votre conviction sans faille rendent cette communauté vraiment vivante et géniale !";
                LocWidgetEditorTitle = "Éditeur de Widget"; LocBgPosition = "Position du Fond";
                LocSwapLR = "Inverser Gauche et Droite"; LocSwapWP = "Inverser Vague et Joueurs";
                LocShowMap = "Afficher la Carte"; LocShowPlayers = "Afficher le Nombre de Joueurs";
                LocShowWave = "Afficher la Vague Actuelle"; LocSwapTopBarText = "Inverser les éléments d'en-tête"; 
                LocFullSizeImage = "Image de fond en taille réelle"; LocFadeBackground = "Fondre l'image dans l'arrière-plan";
                LocDone = "Terminé"; LocReset = "Réinitialiser";
                LocWave = "Vague:"; LocUpdated = "Mis à jour:"; LocSettingsSaved = "Paramètres enregistrés avec succès!";
                LocDiscordBtn = "Serveur Discord officiel MOD-EU";
                LocRepoBtn = "Dépôt GitHub officiel";
                LocRemoteUpdateTitle = "Mise à jour Cloud"; 
                LocRemoteUpdateDesc = "Si les serveurs ont changé d'IP ou de port, téléchargez la dernière liste. Vous pouvez toujours revenir aux paramètres par défaut."; 
                LocBtnUpdateServers = "Mettre à jour (Cloud)"; 
                LocMsgNoUpdates = "Aucune nouvelle mise à jour trouvée.";
                LocMsgUpdated = "Serveurs mis à jour avec succès.";
                LocMsgReset = "Retour aux serveurs par défaut.";
                LocEnableNotifications = "Activer les notifications de suivi des joueurs";
                LocTrackedPlayersHint = "Entrez les pseudos (séparés par des virgules)";
                LocSuggestedPlayers = "Joueurs en ligne (appuyez pour ajouter):";
                LocEnableCountNotifications = "Notifier si le serveur se remplit"; LocPlayerCountThreshold = "Seuil de joueurs:";
                LocPopWarning = "Les notifications ne fonctionneront que pour les serveurs activés dans 'Visibilité des serveurs'.";
                LocNotifPlayerTitle = "Joueur en ligne !"; LocNotifPlayerBody = "{0} a rejoint {1}";
                LocNotifPopTitle = "Le serveur se remplit !"; LocNotifPopBody = "{0} a maintenant {1} joueurs.";
                LocNotifPlayerLeftTitle = "Joueur déconnecté !"; LocNotifPlayerLeftBody = "{0} a quitté {1}";
                LocNotifUpdateTitle = "Nouvelle mise à jour !"; LocNotifUpdateBody = "La version {0} est prête à être téléchargée.";
                LocCreatedBy = "Idée et développement par EXT4N (09.2026)";
                LocCurrentVersionBlock = "Historique des versions"; LocUpdateVersionBlock = "Changements de la nouvelle mise à jour";
                LocUpdateModalTitle = "Mise à jour de l'application"; LocInstallBtn = "Installer"; LocCancelBtn = "Annuler"; LocReadyToInstall = "Prêt à installer";
                _locCurrentSuffix = "(Actuelle)"; _locPreviousSuffix = "(Précédente)";
                
                LocStatus = "Statut";
                LocPhrase1 = "En attente de héros...";
                LocPhrase2 = "Silence dans l'installation...";
                LocPhrase3 = "Aucun zed abattu pour le moment...";
                LocPhrase4 = "Zone dégagée, pour l'instant...";
                LocPhrase5 = "Prêt pour le déploiement.";
                break;
            case "pl":
                LocUpdateBadge = "NOWE";
                LocTabGeneral = "Ogólne"; LocTabWidget = "Widżet"; LocTabFaq = "FAQ"; LocTabUpdates = "Aktualizacje"; LocTabAbout = "O aplikacji";
                LocGeneralSettings = "Ustawienia ogólne"; LocLanguage = "Język";
                LocHideEmpty = "Puste serwery"; LocHideEmptyDesc = "Ukryj serwery bez graczy";
                LocInterval = "Interwał aktualizacji (min)"; LocIntervalDesc = "Jak często widżet pobiera dane";
                LocServerVisibility = "Widoczność serwerów"; LocServerVisibilityDesc = "Wybierz, które serwery mają być wyświetlane";
                LocSaveBtn = "ZAPISZ KONFIGURACJĘ"; LocWidgetApp = "Wygląd widżetu";
                LocCornerRadius = "Promień Rogu"; LocThemeColor = "Kolor i przezroczystość"; LocThemeColorDesc = "Wybierz kolor tła widżetu";
                LocBackgroundImage = "Obraz w tle"; LocBackgroundImageDesc = "Wybierz obraz ze swojej galerii"; LocSelectImage = "Wybierz Obraz"; LocImageOpacity = "Krycie obrazu";
                LocLivePreview = "PODGLĄD NA ŻYWO (Przytrzymaj by edytować)";
                LocPreviewDisclaimer = "To jest tylko podgląd wizualny do stylizacji, a nie funkcjonalny widżet. Użyj menu ekranu głównego urządzenia, aby dodać właściwy widżet.";
                LocAppVersion = "Wersja aplikacji";
                LocCheckUpdateBtn = "Sprawdź"; LocDownloadBtn = "Pobierz";
                LocAboutDesc = "Konfigurowalny widżet ekranu głównego, który wyświetla stan serwerów Killing Floor 2 MOD-EU w czasie rzeczywistym, oferując zaawansowane ustawienia wizualne i dodatkowe narzędzia.";
                LocSpecialThanks = "Ten projekt powstał dzięki ogromnememu wsparciu wielu wspaniałych ludzi. Ogromne, płynące z głębi serca podziękowania dla wszystkich zaangażowanych w rozwój! Szczególna, bezgraniczna wdzięczność dla założycieli, administratorów i menedżerów projektu — Edvisa i Wyverna. Wasze niekończące się zaangażowanie i wiara sprawiają, że ta społeczność jest naprawdę żywa i niesamowita!";
                LocWidgetEditorTitle = "Edytor Widżetu"; LocBgPosition = "Pozycja Tła";
                LocSwapLR = "Zamień Lewo i Prawo"; LocSwapWP = "Zamień Falę i Graczy";
                LocShowMap = "Pokaż Mapę"; LocShowPlayers = "Pokaż Liczbę Graczy";
                LocShowWave = "Pokaż Obecną Falę"; LocSwapTopBarText = "Zamień elementy nagłówka"; 
                LocFullSizeImage = "Pełnowymiarowy obraz tła"; LocFadeBackground = "Zanikanie obrazu w tło";
                LocDone = "Gotowe"; LocReset = "Resetuj";
                LocWave = "Fala:"; LocUpdated = "Zaktualizowano:"; LocSettingsSaved = "Ustawienia zapisane pomyślnie!";
                LocDiscordBtn = "Oficjalny serwer Discord MOD-EU";
                LocRepoBtn = "Oficjalne repozytorium GitHub";
                LocRemoteUpdateTitle = "Aktualizacja z chmury"; 
                LocRemoteUpdateDesc = "Jeśli serwery zmieniły IP lub port, pobierz najnowszą listę. Możesz zawsze wrócić do ustawień domyślnych."; 
                LocBtnUpdateServers = "Aktualizuj z chmury"; 
                LocMsgNoUpdates = "Nie znaleziono nowych aktualizacji.";
                LocMsgUpdated = "Serwery zaktualizowane pomyślnie.";
                LocMsgReset = "Przywrócono domyślne serwery.";
                LocEnableNotifications = "Włącz powiadomienia o śledzeniu graczy";
                LocTrackedPlayersHint = "Wprowadź nick'i (oddzielone przecinkiem)";
                LocSuggestedPlayers = "Gracze online (dotknij, aby dodać):";
                LocEnableCountNotifications = "Powiadom, gdy serwer się zapełni"; LocPlayerCountThreshold = "Próg graczy:";
                LocPopWarning = "Powiadomienia zadziałają tylko dla serwerów włączonych w 'Widoczności serwerów'.";
                LocNotifPlayerTitle = "Gracz online!"; LocNotifPlayerBody = "{0} dołączył do {1}";
                LocNotifPopTitle = "Serwer się zapełnia!"; LocNotifPopBody = "{0} ma teraz {1} graczy.";
                LocNotifPlayerLeftTitle = "Gracz opuścił!"; LocNotifPlayerLeftBody = "{0} opuścił {1}";
                LocNotifUpdateTitle = "Nowa aktualizacja!"; LocNotifUpdateBody = "Wersja {0} jest gotowa do pobrania.";
                LocCreatedBy = "Pomysł i wykonanie: EXT4N (09.2026)";
                LocCurrentVersionBlock = "Historia wersji"; LocUpdateVersionBlock = "Zmiany w nowej aktualizacji";
                LocUpdateModalTitle = "Aktualizacja aplikacji"; LocInstallBtn = "Zainstaluj"; LocCancelBtn = "Anuluj"; LocReadyToInstall = "Gotowe do instalacji";
                _locCurrentSuffix = "(Obecna)"; _locPreviousSuffix = "(Poprzednia)";
                
                LocStatus = "Status";
                LocPhrase1 = "Oczekiwanie na bohaterów...";
                LocPhrase2 = "Cisza w obiekcie...";
                LocPhrase3 = "Żadni zedowie nie zostali jeszcze zabici...";
                LocPhrase4 = "Strefa czysta, na razie...";
                LocPhrase5 = "Gotowy do wdrożenia.";
                break;
            case "de":
                LocUpdateBadge = "NEU";
                LocTabGeneral = "Allgemein"; LocTabWidget = "Widget"; LocTabFaq = "FAQ"; LocTabUpdates = "Updates"; LocTabAbout = "Über";
                LocGeneralSettings = "Allgemeine Einstellungen"; LocLanguage = "Sprache";
                LocHideEmpty = "Leere Server"; LocHideEmptyDesc = "Server ohne Spieler ausblenden";
                LocInterval = "Aktualisierungsintervall (Min)"; LocIntervalDesc = "Wie oft das Widget Daten abruft";
                LocServerVisibility = "Server-Sichtbarkeit"; LocServerVisibilityDesc = "Wählen Sie, welche Server angezeigt werden sollen";
                LocSaveBtn = "KONFIGURATION SPEICHERN"; LocWidgetApp = "Widget-Aussehen";
                LocCornerRadius = "Eckenradius"; LocThemeColor = "Farbe & Transparenz"; LocThemeColorDesc = "Wählen Sie die Hintergrundfarbe";
                LocBackgroundImage = "Hintergrundbild"; LocBackgroundImageDesc = "Wählen Sie ein Bild aus der Galerie"; LocSelectImage = "Bild wählen"; LocImageOpacity = "Bild-Deckkraft";
                LocLivePreview = "LIVE-VORSCHAU (Lang drücken zum Bearbeiten)";
                LocPreviewDisclaimer = "Dies ist nur eine visuelle Vorschau für das Design, kein funktionierendes Widget. Verwenden Sie das Startbildschirm-Menü Ihres Geräts, um das eigentliche Widget hinzuzufügen.";
                LocAppVersion = "App-Version";
                LocCheckUpdateBtn = "Prüfen"; LocDownloadBtn = "Herunterladen";
                LocAboutDesc = "Ein anpassbares Startbildschirm-Widget, das den Echtzeitstatus der Killing Floor 2 MOD-EU-Server anzeigt und erweiterte visuelle Einstellungen sowie zusätzliche Tools bietet.";
                LocSpecialThanks = "Dieses Projekt wurde durch die immense Unterstützung vieler großartiger Menschen möglich gemacht. Ein riesiges, herzliches Dankeschön an alle, die an der Entwicklung und dem Wachstum beteiligt waren! Besonderer, grenzenloser Dank geht an die Gründer, Administratoren und Manager des Projekts — Edvis und Wyvern. Euer endloses Engagement und euer Glaube machen diese Community wirklich lebendig und großartig!";
                LocWidgetEditorTitle = "Widget-Editor"; LocBgPosition = "Hintergrundposition";
                LocSwapLR = "Links und Rechts tauschen"; LocSwapWP = "Welle und Spieler tauschen";
                LocShowMap = "Karte anzeigen"; LocShowPlayers = "Spieleranzahl anzeigen";
                LocShowWave = "Aktuelle Welle anzeigen"; LocSwapTopBarText = "Kopfzeilenelemente tauschen"; 
                LocFullSizeImage = "Hintergrundbild in voller Größe"; LocFadeBackground = "Bild in den Hintergrund überblenden";
                LocDone = "Fertig"; LocReset = "Zurücksetzen";
                LocWave = "Welle:"; LocUpdated = "Aktualisiert:"; LocSettingsSaved = "Einstellungen erfolgreich gespeichert!";
                LocDiscordBtn = "Offizieller MOD-EU Discord Server";
                LocRepoBtn = "Offizielles GitHub-Repository";
                LocRemoteUpdateTitle = "Cloud-Update"; 
                LocRemoteUpdateDesc = "Wenn die Server IP oder Port geändert haben, laden Sie die neueste Liste herunter. Sie können jederzeit zu den Standardeinstellungen zurückkehren."; 
                LocBtnUpdateServers = "Aus der Cloud aktualisieren"; 
                LocMsgNoUpdates = "Keine neuen Updates gefunden.";
                LocMsgUpdated = "Server erfolgreich aktualisiert.";
                LocMsgReset = "Auf Standardserver zurückgesetzt.";
                LocEnableNotifications = "Benachrichtigungen zur Spielerverfolgung aktivieren";
                LocTrackedPlayersHint = "Spitznamen eingeben (durch Komma getrennt)";
                LocSuggestedPlayers = "Online-Spieler (zum Hinzufügen antippen):";
                LocEnableCountNotifications = "Benachrichtigen, wenn Server voll wird"; LocPlayerCountThreshold = "Spieler-Schwellenwert:";
                LocPopWarning = "Benachrichtigungen funktionieren nur für Server, die unter 'Server-Sichtbarkeit' aktiviert sind.";
                LocNotifPlayerTitle = "Spieler online!"; LocNotifPlayerBody = "{0} ist {1} beigetreten";
                LocNotifPopTitle = "Server füllt sich!"; LocNotifPopBody = "{0} hat jetzt {1} Spieler.";
                LocNotifPlayerLeftTitle = "Spieler hat verlassen!"; LocNotifPlayerLeftBody = "{0} hat {1} verlassen";
                LocNotifUpdateTitle = "Neues Update verfügbar!"; LocNotifUpdateBody = "Version {0} ist zum Download bereit.";
                LocCreatedBy = "Idee & Entwicklung von EXT4N (09.2026)";
                LocCurrentVersionBlock = "Versionsverlauf"; LocUpdateVersionBlock = "Änderungen im neuen Update";
                LocUpdateModalTitle = "App-Update"; LocInstallBtn = "Installieren"; LocCancelBtn = "Abbrechen"; LocReadyToInstall = "Bereit zur Installation";
                _locCurrentSuffix = "(Aktuell)"; _locPreviousSuffix = "(Vorherige)";
                
                LocStatus = "Status";
                LocPhrase1 = "Warten auf Helden...";
                LocPhrase2 = "Stille in der Einrichtung...";
                LocPhrase3 = "Noch keine Zeds geschlachtet...";
                LocPhrase4 = "Bereich sicher, vorerst...";
                LocPhrase5 = "Bereit zum Einsatz.";
                break;
            case "ja":
                LocUpdateBadge = "新着";
                LocTabGeneral = "一般"; LocTabWidget = "ウィジェット"; LocTabFaq = "FAQ"; LocTabUpdates = "更新"; LocTabAbout = "情報";
                LocGeneralSettings = "一般設定"; LocLanguage = "言語";
                LocHideEmpty = "空のサーバー"; LocHideEmptyDesc = "アクティブなプレイヤーがいないサーバーを非表示";
                LocInterval = "更新間隔 (分)"; LocIntervalDesc = "ウィジェットが新しいデータを取得する頻度";
                LocServerVisibility = "サーバーの表示"; LocServerVisibilityDesc = "Pingして表示するサーバーを選択";
                LocSaveBtn = "設定を保存"; LocWidgetApp = "ウィジェットの外観";
                LocCornerRadius = "角の丸み"; LocThemeColor = "テーマカラーと透明度"; LocThemeColorDesc = "背景のプライマリカラーを選択";
                LocBackgroundImage = "背景画像"; LocBackgroundImageDesc = "ギャラリーから画像を選択"; LocSelectImage = "画像を選択"; LocImageOpacity = "画像の不透明度";
                LocLivePreview = "ライブプレビュー (長押しで編集)";
                LocPreviewDisclaimer = "これはスタイリングの目的のための視覚的なプレビューであり、機能するウィジェットではありません。実際のウィジェットを追加するには、デバイスのホーム画面メニューを使用してください。";
                LocAppVersion = "アプリのバージョン";
                LocCheckUpdateBtn = "確認する"; LocDownloadBtn = "ダウンロード";
                LocAboutDesc = "Killing Floor 2 MOD-EU サーバーのリアルタイム ステータスを表示するカスタマイズ可能なホーム画面ウィジェット。高度なビジュアル設定と追加ツールを備えています。";
                LocSpecialThanks = "このプロジェクトは、多くの素晴らしい人々の多大なるサポートによって実現しました。開発と成長に関わってくださった皆様に、心からの感謝を申し上げます！ そして、プロジェクトの創設者、管理者、マネージャーであるEdvisとWyvernへ、特別な、限りない感謝を捧げます。あなた方の果てしない献身と信念が、このコミュニティを真に活気ある、素晴らしいものにしています！";
                LocWidgetEditorTitle = "ウィジェットエディタ"; LocBgPosition = "背景の位置";
                LocSwapLR = "左右を入れ替える"; LocSwapWP = "ウェーブとプレイヤーを入れ替える";
                LocShowMap = "マップを表示"; LocShowPlayers = "プレイヤー数を表示";
                LocShowWave = "現在のウェーブを表示"; LocSwapTopBarText = "ヘッダーの要素を入れ替える"; 
                LocFullSizeImage = "フルサイズの背景画像"; LocFadeBackground = "画像を背景にフェードイン";
                LocDone = "完了"; LocReset = "リセット";
                LocWave = "ウェーブ:"; LocUpdated = "更新日時:"; LocSettingsSaved = "設定が正常に保存されました!";
                LocDiscordBtn = "公式MOD-EU Discordサーバー";
                LocRepoBtn = "公式GitHubリポジトリ";
                LocRemoteUpdateTitle = "クラウド更新"; 
                LocRemoteUpdateDesc = "サーバーのIPやポートが変更された場合、最新のリストをダウンロードします。いつでもデフォルトの設定に戻すことができます。"; 
                LocBtnUpdateServers = "クラウドから更新"; 
                LocMsgNoUpdates = "新しい更新は見つかりませんでした。";
                LocMsgUpdated = "サーバーが正常に更新されました。";
                LocMsgReset = "デフォルトのサーバーに戻しました。";
                LocEnableNotifications = "プレイヤートラッキング通知を有効にする";
                LocTrackedPlayersHint = "ニックネームを入力（カンマ区切り）";
                LocSuggestedPlayers = "オンラインのプレイヤー (タップして追加):";
                LocEnableCountNotifications = "サーバーが満員になったら通知する"; LocPlayerCountThreshold = "プレイヤーのしきい値:";
                LocPopWarning = "通知は、「サーバーの表示」で有効になっているサーバーでのみ機能します。";
                LocNotifPlayerTitle = "プレイヤーがオンラインです！"; LocNotifPlayerBody = "{0} が {1} に参加しました";
                LocNotifPopTitle = "サーバーがいっぱいです！"; LocNotifPopBody = "{0} には現在 {1} 人のプレイヤーがいます。";
                LocNotifPlayerLeftTitle = "プレイヤーが退出しました！"; LocNotifPlayerLeftBody = "{0} が {1} から退出しました";
                LocNotifUpdateTitle = "新しいアップデートが利用可能！"; LocNotifUpdateBody = "バージョン {0} をダウンロードする準備ができました。";
                LocCreatedBy = "EXT4Nによるアイデアと開発 (09.2026)";
                LocCurrentVersionBlock = "バージョン履歴"; LocUpdateVersionBlock = "新しいアップデートの変更点";
                LocUpdateModalTitle = "アプリの更新"; LocInstallBtn = "インストール"; LocCancelBtn = "キャンセル"; LocReadyToInstall = "インストールの準備が完了";
                _locCurrentSuffix = "(現在の)"; _locPreviousSuffix = "(以前の)";
                
                LocStatus = "ステータス";
                LocPhrase1 = "ヒーローを待っています...";
                LocPhrase2 = "施設内の静寂...";
                LocPhrase3 = "Zedはまだ倒されていません...";
                LocPhrase4 = "今のところ、エリアはクリアです...";
                LocPhrase5 = "展開の準備完了。";
                break;
            case "en":
            default:
                LocUpdateBadge = "NEW";
                LocTabGeneral = "General"; LocTabWidget = "Widget"; LocTabFaq = "FAQ"; LocTabUpdates = "Updates"; LocTabAbout = "About";
                LocGeneralSettings = "General Settings"; LocLanguage = "Language";
                LocHideEmpty = "Empty Servers"; LocHideEmptyDesc = "Hide servers with no active players";
                LocInterval = "Update Interval (Minutes)"; LocIntervalDesc = "How often the widget fetches new data";
                LocServerVisibility = "Server Visibility"; LocServerVisibilityDesc = "Select which servers to ping and display";
                LocSaveBtn = "SAVE CONFIGURATION"; LocWidgetApp = "Widget Appearance";
                LocCornerRadius = "Corner Radius"; LocThemeColor = "Theme Color & Transparency"; LocThemeColorDesc = "Pick a primary color for the background";
                LocBackgroundImage = "Background Image"; LocBackgroundImageDesc = "Pick an image from your gallery"; LocSelectImage = "Select Image"; LocImageOpacity = "Image Opacity";
                LocLivePreview = "LIVE PREVIEW (Long press to edit)";
                LocPreviewDisclaimer = "This is a visual preview for styling purposes only, not a functional widget. Use your device's home screen menu to add the actual widget.";
                LocAppVersion = "Application Version";
                LocCheckUpdateBtn = "Check"; LocDownloadBtn = "Download";
                LocAboutDesc = "A customizable home screen widget that displays the real-time status of Killing Floor 2 MOD-EU servers, featuring advanced visual settings and extra tools.";
                LocSpecialThanks = "This project was made possible through the immense support of many amazing people. A huge, heartfelt thank you to everyone involved in the development and growth! Special, boundless gratitude to the founders, admins, and managers of the project — Edvis and Wyvern. Your endless dedication and belief make this community truly alive and awesome!";
                LocWidgetEditorTitle = "Widget Editor"; LocBgPosition = "Background Position";
                LocSwapLR = "Swap Left and Right"; LocSwapWP = "Swap Wave & Players";
                LocShowMap = "Show Map"; LocShowPlayers = "Show Player Count";
                LocShowWave = "Show Current Wave"; LocSwapTopBarText = "Swap Header Controls"; 
                LocFullSizeImage = "Full-size background image"; LocFadeBackground = "Fade image into background";
                LocDone = "Done"; LocReset = "Reset Defaults";
                LocWave = "Wave:"; LocUpdated = "Updated:"; LocSettingsSaved = "Settings saved successfully!";
                LocDiscordBtn = "Official MOD-EU Discord Server";
                LocRepoBtn = "Official GitHub Repository";
                LocRemoteUpdateTitle = "Cloud Update"; 
                LocRemoteUpdateDesc = "If servers changed IP or port, download the latest list. You can always revert to stock settings."; 
                LocBtnUpdateServers = "Update from Cloud"; 
                LocMsgNoUpdates = "No new updates found.";
                LocMsgUpdated = "Servers updated successfully.";
                LocMsgReset = "Reverted to stock servers.";
                LocEnableNotifications = "Enable Player Tracking Notifications";
                LocTrackedPlayersHint = "Enter nicknames (comma separated)";
                LocSuggestedPlayers = "Online players (tap to add):";
                LocEnableCountNotifications = "Notify if server gets crowded"; LocPlayerCountThreshold = "Player threshold:";
                LocPopWarning = "Notifications will only trigger for servers enabled in 'Server Visibility'.";
                LocNotifPlayerTitle = "Player Online!"; LocNotifPlayerBody = "{0} joined {1}";
                LocNotifPopTitle = "Server Populating!"; LocNotifPopBody = "{0} now has {1} players.";
                LocNotifPlayerLeftTitle = "Player Left!"; LocNotifPlayerLeftBody = "{0} left {1}";
                LocNotifUpdateTitle = "New Update Available!"; LocNotifUpdateBody = "Version {0} is ready to download.";
                LocCreatedBy = "Idea & Development by EXT4N (09.2026)";
                LocCurrentVersionBlock = "Version History"; LocUpdateVersionBlock = "New Update Changes";
                LocUpdateModalTitle = "Update Application"; LocInstallBtn = "Install"; LocCancelBtn = "Cancel"; LocReadyToInstall = "Ready to install";
                _locCurrentSuffix = "(Current)"; _locPreviousSuffix = "(Previous)";
                
                LocStatus = "Status";
                LocPhrase1 = "Waiting for heroes...";
                LocPhrase2 = "Silence in the facility...";
                LocPhrase3 = "No zeds slaughtered yet...";
                LocPhrase4 = "Area clear, for now...";
                LocPhrase5 = "Ready for deployment.";
                break;
        }

        LocUpdatedJustNow = $"{LocUpdated} Just now";
        OnPropertyChanged(nameof(PreviewTopBgText));
        OnPropertyChanged(nameof(PreviewBottomBgText));
        
        RefreshChangelogUI();
    }
}