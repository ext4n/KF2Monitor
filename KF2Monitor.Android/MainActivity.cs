#pragma warning disable CA1416
#pragma warning disable CA1422
#pragma warning disable CS8604
#pragma warning disable CS8765

using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Avalonia;
using Avalonia.Android;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KF2Monitor.Android;

[Activity(
    Label = "KF2MOD-EU Monitor",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTask,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
    private bool _isFirstLoad = true;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
        {
            Window?.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            Window?.ClearFlags(WindowManagerFlags.TranslucentStatus);
            Window?.SetStatusBarColor(global::Android.Graphics.Color.Transparent);
        }
        
        if (Build.VERSION.SdkInt >= BuildVersionCodes.M && Window?.DecorView != null)
        {
            Window.DecorView.SystemUiFlags = SystemUiFlags.LayoutFullscreen | SystemUiFlags.LayoutStable;
        }

        KF2Monitor.ViewModels.MainViewModel.RequestPermissionsAction = () =>
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            {
                if (CheckSelfPermission(global::Android.Manifest.Permission.PostNotifications) != Permission.Granted)
                {
                    RequestPermissions(new[] { global::Android.Manifest.Permission.PostNotifications }, 1);
                }
            }

            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                var pm = (PowerManager?)GetSystemService(PowerService);
                if (pm != null && !pm.IsIgnoringBatteryOptimizations(PackageName))
                {
                    var intent = new Intent(global::Android.Provider.Settings.ActionRequestIgnoreBatteryOptimizations);
                    intent.SetData(global::Android.Net.Uri.Parse("package:" + PackageName));
                    StartActivity(intent);
                }
            }
        };

        KF2Monitor.ViewModels.MainViewModel.InstallApkAction = (filePath) =>
        {
            try
            {
                var file = new Java.IO.File(filePath);
                var uri = AndroidX.Core.Content.FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file);

                var installIntent = new Intent(Intent.ActionView);
                installIntent.SetDataAndType(uri, "application/vnd.android.package-archive");
                installIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.GrantReadUriPermission);

                StartActivity(installIntent);
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, "Install Error: " + ex.Message, ToastLength.Long)?.Show();
            }
        };

        KF2Monitor.ViewModels.MainViewModel.RequestLoadSettings = (callback) =>
        {
            var prefs = GetSharedPreferences("KF2WidgetPrefs", FileCreationMode.Private);

            bool hide = prefs?.GetBoolean("hide_empty", false) ?? false;
            string color = prefs?.GetString("theme_color", "#059669") ?? "#059669";
            int alpha = prefs?.GetInt("alpha", 255) ?? 255;
            int interval = prefs?.GetInt("interval", 15) ?? 15;
            string langCode = prefs?.GetString("language", "") ?? "";
            bool hasBgImage = prefs?.GetBoolean("has_bg_image", false) ?? false;
            int radius = prefs?.GetInt("corner_radius", 12) ?? 12;
            int opacity = prefs?.GetInt("image_opacity", 100) ?? 100;
            float offsetY = prefs?.GetFloat("image_offset_y", 0f) ?? 0f;
            
            bool swapLR = prefs?.GetBoolean("swap_lr", false) ?? false;
            bool swapWP = prefs?.GetBoolean("swap_wp", false) ?? false;
            bool showMap = prefs?.GetBoolean("show_map", true) ?? true;
            bool showWave = prefs?.GetBoolean("show_wave", true) ?? true;
            bool showPlayers = prefs?.GetBoolean("show_players", true) ?? true;
            bool swapTopBar = prefs?.GetBoolean("swap_top_bar", false) ?? false;
            
            bool enableNotifs = prefs?.GetBoolean("enable_notifications", false) ?? false;
            string trackedPlayers = prefs?.GetString("tracked_players", "") ?? "";
            
            bool enableCountNotifs = prefs?.GetBoolean("enable_count_notifications", false) ?? false;
            int countThreshold = prefs?.GetInt("count_threshold", 6) ?? 6;

            bool fullSize = prefs?.GetBoolean("full_size_image", true) ?? true;
            bool fadeBg = prefs?.GetBoolean("fade_background", false) ?? false;

            bool[] srvs = new bool[] {
                prefs?.GetBoolean("srv1", true) ?? true,
                prefs?.GetBoolean("srv2", true) ?? true,
                prefs?.GetBoolean("srv3", true) ?? true,
                prefs?.GetBoolean("srv4", true) ?? true,
                prefs?.GetBoolean("srv5", true) ?? true
            };
            
            bool[] countSrvs = new bool[] {
                prefs?.GetBoolean("count_srv1", true) ?? true,
                prefs?.GetBoolean("count_srv2", true) ?? true,
                prefs?.GetBoolean("count_srv3", true) ?? true,
                prefs?.GetBoolean("count_srv4", true) ?? true,
                prefs?.GetBoolean("count_srv5", true) ?? true
            };

            List<string> onlinePlayers = new List<string>();
            Kf2WidgetProvider.LoadCache(this);
            if (Kf2WidgetProvider.CachedServers != null)
            {
                foreach (var srv in Kf2WidgetProvider.CachedServers)
                {
                    if (srv.RawPlayers != null)
                    {
                        foreach(var p in srv.RawPlayers)
                        {
                            if(!string.IsNullOrEmpty(p.Name)) onlinePlayers.Add(p.Name);
                        }
                    }
                }
            }
            onlinePlayers = onlinePlayers.Distinct().ToList();

            callback(hide, color, alpha, interval, langCode, srvs, hasBgImage, radius, opacity, offsetY, swapLR, swapWP, showMap, showWave, showPlayers, swapTopBar, enableNotifs, trackedPlayers, enableCountNotifs, countThreshold, countSrvs, onlinePlayers, fullSize, fadeBg);
        };

        KF2Monitor.ViewModels.MainViewModel.OpenUrlAction = (url) =>
        {
            var intent = new Intent(Intent.ActionView, global::Android.Net.Uri.Parse(url));
            StartActivity(intent);
        };

        KF2Monitor.ViewModels.MainViewModel.VibrateAction = () =>
        {
            var vibrator = (Vibrator?)GetSystemService(VibratorService);
            if (vibrator != null && vibrator.HasVibrator)
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    vibrator.Vibrate(VibrationEffect.CreateOneShot(50, VibrationEffect.DefaultAmplitude));
                }
                else
                {
                    vibrator.Vibrate(50);
                }
            }
        };

        KF2Monitor.ViewModels.MainViewModel.ShowToastAction = (msg) =>
        {
            Toast.MakeText(this, msg, ToastLength.Short)?.Show();
        };

        base.OnCreate(savedInstanceState);

        KF2Monitor.ViewModels.MainViewModel.OnSaveSettings = (hideEmpty, color, alpha, interval, langCode, srvs, hasBgImage, radius, opacity, offsetY, swapLR, swapWP, showMap, showWave, showPlayers, swapTopBar, locWave, locUpdated, locSaved, enableNotifs, trackedPlayers, enableCountNotifs, countThreshold, countSrvs, fullSize, fadeBg, locNotifPlayerTitle, locNotifPlayerBody, locNotifPopTitle, locNotifPopBody, locNotifPlayerLeftTitle, locNotifPlayerLeftBody, locNotifUpdateTitle, locNotifUpdateBody, appVer, locStatus, locPhrase1, locPhrase2, locPhrase3, locPhrase4, locPhrase5) =>
        {
            var prefs = GetSharedPreferences("KF2WidgetPrefs", FileCreationMode.Private);
            var editor = prefs?.Edit();

            if (editor != null)
            {
                editor.PutString("app_version", appVer);
                editor.PutBoolean("hide_empty", hideEmpty);
                editor.PutString("theme_color", color);
                editor.PutInt("alpha", alpha);
                editor.PutInt("interval", interval);
                editor.PutString("language", langCode);
                editor.PutBoolean("has_bg_image", hasBgImage);
                editor.PutInt("corner_radius", radius);
                editor.PutInt("image_opacity", opacity);
                editor.PutFloat("image_offset_y", offsetY);
                
                editor.PutBoolean("swap_lr", swapLR);
                editor.PutBoolean("swap_wp", swapWP);
                editor.PutBoolean("show_map", showMap);
                editor.PutBoolean("show_wave", showWave);
                editor.PutBoolean("show_players", showPlayers);
                editor.PutBoolean("swap_top_bar", swapTopBar);
                
                editor.PutString("loc_wave", locWave);
                editor.PutString("loc_updated", locUpdated);

                editor.PutString("loc_status", locStatus);
                editor.PutString("loc_phrase1", locPhrase1);
                editor.PutString("loc_phrase2", locPhrase2);
                editor.PutString("loc_phrase3", locPhrase3);
                editor.PutString("loc_phrase4", locPhrase4);
                editor.PutString("loc_phrase5", locPhrase5);
                
                editor.PutBoolean("enable_notifications", enableNotifs);
                editor.PutString("tracked_players", trackedPlayers);
                
                editor.PutBoolean("enable_count_notifications", enableCountNotifs);
                editor.PutInt("count_threshold", countThreshold);

                editor.PutBoolean("full_size_image", fullSize);
                editor.PutBoolean("fade_background", fadeBg);

                if (srvs != null && srvs.Length >= 5)
                {
                    editor.PutBoolean("srv1", srvs[0]);
                    editor.PutBoolean("srv2", srvs[1]);
                    editor.PutBoolean("srv3", srvs[2]);
                    editor.PutBoolean("srv4", srvs[3]);
                    editor.PutBoolean("srv5", srvs[4]);
                }
                
                if (countSrvs != null && countSrvs.Length >= 5)
                {
                    editor.PutBoolean("count_srv1", countSrvs[0]);
                    editor.PutBoolean("count_srv2", countSrvs[1]);
                    editor.PutBoolean("count_srv3", countSrvs[2]);
                    editor.PutBoolean("count_srv4", countSrvs[3]);
                    editor.PutBoolean("count_srv5", countSrvs[4]);
                }

                editor.PutString("loc_notif_player_title", locNotifPlayerTitle);
                editor.PutString("loc_notif_player_body", locNotifPlayerBody);
                editor.PutString("loc_notif_pop_title", locNotifPopTitle);
                editor.PutString("loc_notif_pop_body", locNotifPopBody);
                editor.PutString("loc_notif_player_left_title", locNotifPlayerLeftTitle);
                editor.PutString("loc_notif_player_left_body", locNotifPlayerLeftBody);
                editor.PutString("loc_notif_update_title", locNotifUpdateTitle);
                editor.PutString("loc_notif_update_body", locNotifUpdateBody);

                editor.Commit();
            }

            Kf2WidgetProvider.ScheduleNextAlarm(this, interval);

            var widgetManager = AppWidgetManager.GetInstance(this);
            var component = new ComponentName(this, Java.Lang.Class.FromType(typeof(Kf2WidgetProvider)));
            var ids = widgetManager?.GetAppWidgetIds(component);

            if (ids != null && ids.Length > 0)
            {
                var intent = new Intent(this, typeof(Kf2WidgetProvider));
                intent.SetAction("com.kf2monitor.widget.REFRESH");
                SendBroadcast(intent);
            }

            Toast.MakeText(this, locSaved, ToastLength.Short)?.Show();
        };

        CheckIntent(Intent);
    }

    public override void OnWindowFocusChanged(bool hasFocus)
    {
        base.OnWindowFocusChanged(hasFocus);
        if (hasFocus)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                Window?.SetStatusBarColor(global::Android.Graphics.Color.Transparent);
                Window?.SetNavigationBarColor(global::Android.Graphics.Color.ParseColor("#0B0C10"));
            }
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Q && Window != null)
            {
                Window.NavigationBarContrastEnforced = false;
            }
        }
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        CheckIntent(intent);
    }

    private void CheckIntent(Intent? intent)
    {
        if (intent?.GetBooleanExtra("open_updates_tab", false) == true)
        {
            new Handler(Looper.MainLooper!).PostDelayed(() =>
            {
                if (KF2Monitor.ViewModels.MainViewModel.Instance != null)
                {
                    KF2Monitor.ViewModels.MainViewModel.Instance.SelectedMenuIndex = 3;
                }
            }, 500);
        }
    }

    protected override void OnResume()
    {
        base.OnResume();
        if (_isFirstLoad)
        {
            _isFirstLoad = false;
            var prefs = GetSharedPreferences("KF2WidgetPrefs", FileCreationMode.Private);

            bool hide = prefs?.GetBoolean("hide_empty", false) ?? false;
            string color = prefs?.GetString("theme_color", "#059669") ?? "#059669";
            int alpha = prefs?.GetInt("alpha", 255) ?? 255;
            int interval = prefs?.GetInt("interval", 15) ?? 15;
            string langCode = prefs?.GetString("language", "") ?? "";
            bool hasBgImage = prefs?.GetBoolean("has_bg_image", false) ?? false;
            int radius = prefs?.GetInt("corner_radius", 12) ?? 12;
            int opacity = prefs?.GetInt("image_opacity", 100) ?? 100;
            float offsetY = prefs?.GetFloat("image_offset_y", 0f) ?? 0f;
            
            bool swapLR = prefs?.GetBoolean("swap_lr", false) ?? false;
            bool swapWP = prefs?.GetBoolean("swap_wp", false) ?? false;
            bool showMap = prefs?.GetBoolean("show_map", true) ?? true;
            bool showWave = prefs?.GetBoolean("show_wave", true) ?? true;
            bool showPlayers = prefs?.GetBoolean("show_players", true) ?? true;
            bool swapTopBar = prefs?.GetBoolean("swap_top_bar", false) ?? false;
            
            bool enableNotifs = prefs?.GetBoolean("enable_notifications", false) ?? false;
            string trackedPlayers = prefs?.GetString("tracked_players", "") ?? "";

            bool enableCountNotifs = prefs?.GetBoolean("enable_count_notifications", false) ?? false;
            int countThreshold = prefs?.GetInt("count_threshold", 6) ?? 6;

            bool fullSize = prefs?.GetBoolean("full_size_image", true) ?? true;
            bool fadeBg = prefs?.GetBoolean("fade_background", false) ?? false;

            bool[] srvs = new bool[] {
                prefs?.GetBoolean("srv1", true) ?? true,
                prefs?.GetBoolean("srv2", true) ?? true,
                prefs?.GetBoolean("srv3", true) ?? true,
                prefs?.GetBoolean("srv4", true) ?? true,
                prefs?.GetBoolean("srv5", true) ?? true
            };
            
            bool[] countSrvs = new bool[] {
                prefs?.GetBoolean("count_srv1", true) ?? true,
                prefs?.GetBoolean("count_srv2", true) ?? true,
                prefs?.GetBoolean("count_srv3", true) ?? true,
                prefs?.GetBoolean("count_srv4", true) ?? true,
                prefs?.GetBoolean("count_srv5", true) ?? true
            };

            List<string> onlinePlayers = new List<string>();
            Kf2WidgetProvider.LoadCache(this);
            if (Kf2WidgetProvider.CachedServers != null)
            {
                foreach (var srv in Kf2WidgetProvider.CachedServers)
                {
                    if (srv.RawPlayers != null)
                    {
                        foreach(var p in srv.RawPlayers)
                        {
                            if(!string.IsNullOrEmpty(p.Name)) onlinePlayers.Add(p.Name);
                        }
                    }
                }
            }
            onlinePlayers = onlinePlayers.Distinct().ToList();

            KF2Monitor.ViewModels.MainViewModel.Instance?.LoadData(hide, color, alpha, interval, langCode, srvs, hasBgImage, radius, opacity, offsetY, swapLR, swapWP, showMap, showWave, showPlayers, swapTopBar, enableNotifs, trackedPlayers, enableCountNotifs, countThreshold, countSrvs, onlinePlayers, fullSize, fadeBg);
        }
    }
}