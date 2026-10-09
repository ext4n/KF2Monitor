#pragma warning disable CS8602
#pragma warning disable CS8604
#pragma warning disable CA1416
#pragma warning disable CA1422

using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.OS;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KF2Monitor.Android
{
    public class RemoteServerConfig
    {
        public string Name { get; set; } = string.Empty;
        public string IP { get; set; } = string.Empty;
        public int Port { get; set; }
    }

    [BroadcastReceiver(Label = "KF2 MOD-EU CHECKER", Exported = true)]
    [IntentFilter(new string[] { "android.appwidget.action.APPWIDGET_UPDATE", "com.kf2monitor.widget.REFRESH", "com.kf2monitor.widget.ALARM_UPDATE" })]
    [MetaData("android.appwidget.provider", Resource = "@xml/kf2_widget_info")]
    public class Kf2WidgetProvider : AppWidgetProvider
    {
        public static List<ServerInfo> CachedServers = new List<ServerInfo>();
        public static string LastGlobalError = "";
        
        public static List<string> AdminSteamNames = new List<string>();
        private static DateTime _lastDevNameFetch = DateTime.MinValue;
        
        private const int UPDATE_NOTIF_ID = 9999;

        public static void ScheduleNextAlarm(Context context, int intervalMinutes)
        {
            var alarmManager = (AlarmManager?)context.GetSystemService(Context.AlarmService);
            var alarmIntent = new Intent(context, typeof(Kf2WidgetProvider));
            alarmIntent.SetAction("com.kf2monitor.widget.ALARM_UPDATE");

            var pendingAlarm = PendingIntent.GetBroadcast(context, 0, alarmIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
            if (pendingAlarm != null) alarmManager?.Cancel(pendingAlarm);

            long triggerTime = Java.Lang.JavaSystem.CurrentTimeMillis() + (intervalMinutes * 60 * 1000L);
            try
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                {
                    alarmManager?.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, triggerTime, pendingAlarm!);
                }
                else
                {
                    alarmManager?.SetExact(AlarmType.RtcWakeup, triggerTime, pendingAlarm!);
                }
            }
            catch (Java.Lang.SecurityException)
            {
                alarmManager?.SetWindow(AlarmType.RtcWakeup, triggerTime, 60000, pendingAlarm!);
            }
        }

        private void UpdateAdminNames()
        {
            if ((DateTime.Now - _lastDevNameFetch).TotalHours < 24 && AdminSteamNames.Count > 0)
                return;

            try
            {
                using var client = new System.Net.Http.HttpClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                
                var adminUrls = new[] {
                    "https://steamcommunity.com/profiles/76561198091587775?xml=1",
                    "https://steamcommunity.com/profiles/76561198130824886?xml=1"
                };

                var newNames = new List<string>();
                foreach (var url in adminUrls)
                {
                    try
                    {
                        var xml = client.GetStringAsync(url).Result;
                        var match = System.Text.RegularExpressions.Regex.Match(xml, @"<steamID><!\[CDATA\[(.*?)\]\]></steamID>");
                        if (match.Success)
                        {
                            newNames.Add(match.Groups[1].Value);
                        }
                    }
                    catch { }
                }

                if (newNames.Count > 0)
                {
                    AdminSteamNames = newNames;
                    _lastDevNameFetch = DateTime.Now;
                }
            }
            catch { }
        }

        public override void OnUpdate(Context? context, AppWidgetManager? appWidgetManager, int[]? appWidgetIds)
        {
        }

        public override void OnAppWidgetOptionsChanged(Context? context, AppWidgetManager? appWidgetManager, int appWidgetId, Bundle? newOptions)
        {
            base.OnAppWidgetOptionsChanged(context, appWidgetManager, appWidgetId, newOptions);
            if (context != null && appWidgetManager != null)
            {
                UpdateWidgetState(context, appWidgetManager, appWidgetId, false);
                appWidgetManager.NotifyAppWidgetViewDataChanged(appWidgetId, Resource.Id.list_servers);
            }
        }

        public override void OnReceive(Context? context, Intent? intent)
        {
            base.OnReceive(context, intent);

            if (context != null)
            {
                var appWidgetManager = AppWidgetManager.GetInstance(context);
                var componentName = new ComponentName(context, Java.Lang.Class.FromType(typeof(Kf2WidgetProvider)));
                var appWidgetIds = appWidgetManager?.GetAppWidgetIds(componentName);
                string? act = intent?.Action;

                if (act == "com.kf2monitor.widget.REFRESH" || act == AppWidgetManager.ActionAppwidgetUpdate || act == "com.kf2monitor.widget.ALARM_UPDATE")
                {
                    if (appWidgetManager != null && appWidgetIds != null && appWidgetIds.Length > 0)
                    {
                        if (act == "com.kf2monitor.widget.ALARM_UPDATE")
                        {
                            var prefs = context.GetSharedPreferences("KF2WidgetPrefs", FileCreationMode.Private);
                            int interval = prefs?.GetInt("interval", 15) ?? 15;
                            ScheduleNextAlarm(context, interval);
                            
                            CheckForUpdatesBackground(context, prefs);
                        }

                        var pendingResult = GoAsync();
                        RunUpdateTask(context, appWidgetManager, appWidgetIds, pendingResult);
                    }
                }
            }
        }

        private void RunUpdateTask(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds, PendingResult? pendingResult = null)
        {
            var powerManager = (PowerManager?)context.GetSystemService(Context.PowerService);
            var wakeLock = powerManager?.NewWakeLock(WakeLockFlags.Partial, "KF2Monitor::WidgetUpdate");
            wakeLock?.Acquire(15000); 

            foreach (var widgetId in appWidgetIds) UpdateWidgetState(context, appWidgetManager, widgetId, true);

            Task.Run(() =>
            {
                try
                {
                    LastGlobalError = "";
                    RefreshServersDataSync(context);
                }
                catch (Exception ex)
                {
                    LastGlobalError = ex.Message;
                }
                finally
                {
                    new global::Android.OS.Handler(global::Android.OS.Looper.MainLooper!).Post(() =>
                    {
                        foreach (var widgetId in appWidgetIds) UpdateWidgetState(context, appWidgetManager, widgetId, false);
                        appWidgetManager.NotifyAppWidgetViewDataChanged(appWidgetIds, Resource.Id.list_servers);
                        
                        pendingResult?.Finish();
                        
                        if (wakeLock != null && wakeLock.IsHeld)
                        {
                            wakeLock.Release();
                        }
                    });
                }
            });
        }

        private void CheckForUpdatesBackground(Context context, ISharedPreferences? prefs)
        {
            if (prefs == null) return;
            
            bool enableNotifs = prefs.GetBoolean("enable_notifications", false);
            if (!enableNotifs) return;

            long lastCheck = prefs.GetLong("last_update_check_time", 0);
            long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            
            if (nowMs - lastCheck > 24 * 60 * 60 * 1000L)
            {
                prefs.Edit()?.PutLong("last_update_check_time", nowMs)?.Commit();

                Task.Run(async () => {
                    try {
                        using var client = new System.Net.Http.HttpClient();
                        client.Timeout = TimeSpan.FromSeconds(10);
                        string latest = (await client.GetStringAsync("https://raw.githubusercontent.com/ext4n/KF2Monitor/main/version/android/version")).Trim();
                        string appVer = prefs.GetString("app_version", "1.0.0") ?? "1.0.0";
                        
                        if (latest != appVer && !string.IsNullOrEmpty(latest)) 
                        {
                            string title = prefs.GetString("loc_notif_update_title", "New Update Available!") ?? "New Update Available!";
                            string bodyTpl = prefs.GetString("loc_notif_update_body", "Version {0} is ready to download.") ?? "Version {0} is ready to download.";
                            string body = string.Format(bodyTpl, latest);

                            var notificationManager = (NotificationManager?)context.GetSystemService(Context.NotificationService);
                            if (notificationManager == null) return;

                            string channelId = "kf2_monitor_channel";
                            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                            {
                                var channel = new NotificationChannel(channelId, "Server Notifications", NotificationImportance.Default);
                                notificationManager.CreateNotificationChannel(channel);
                            }

                            var intent = new Intent(context, typeof(MainActivity));
                            intent.PutExtra("open_updates_tab", true);
                            intent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                            var pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

                            var builder = new Notification.Builder(context, channelId)
                                .SetContentTitle(title)
                                .SetContentText(body)
                                .SetSmallIcon(Resource.Drawable.Icon)
                                .SetContentIntent(pendingIntent)
                                .SetAutoCancel(true)
                                .SetShowWhen(true)
                                .SetWhen(Java.Lang.JavaSystem.CurrentTimeMillis());

                            notificationManager.Notify(UPDATE_NOTIF_ID, builder.Build());
                        }
                    } catch { }
                });
            }
        }

        private void RefreshServersDataSync(Context context)
        {
            UpdateAdminNames();

            var prefs = context.GetSharedPreferences("KF2WidgetPrefs", FileCreationMode.Private);
            bool hideEmpty = prefs?.GetBoolean("hide_empty", false) ?? false;
            
            bool[] srvEnabled = new bool[] {
                prefs?.GetBoolean("srv1", true) ?? true,
                prefs?.GetBoolean("srv2", true) ?? true,
                prefs?.GetBoolean("srv3", true) ?? true,
                prefs?.GetBoolean("srv4", true) ?? true,
                prefs?.GetBoolean("srv5", true) ?? true
            };

            var baseEndpoints = new List<RemoteServerConfig>
            {
                new RemoteServerConfig { Name = "Mod-EU | Normal #1", IP = "78.58.223.116", Port = 27030 },
                new RemoteServerConfig { Name = "Mod-EU | Hard #2", IP = "78.58.223.116", Port = 27031 },
                new RemoteServerConfig { Name = "Mod-EU | Suicidal #3", IP = "78.58.223.116", Port = 27032 },
                new RemoteServerConfig { Name = "Mod-EU | HOE #4", IP = "78.58.223.116", Port = 27033 },
                new RemoteServerConfig { Name = "Mod-EU | Extreme #5", IP = "78.58.223.116", Port = 27034 }
            };

            string customServersPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "servers.json");
            if (System.IO.File.Exists(customServersPath))
            {
                try
                {
                    string json = System.IO.File.ReadAllText(customServersPath);
                    var parsed = JsonSerializer.Deserialize<List<RemoteServerConfig>>(json);
                    if (parsed != null && parsed.Count == 5)
                    {
                        baseEndpoints = parsed;
                    }
                }
                catch { }
            }

            string hexColor = prefs?.GetString("theme_color", "#059669") ?? "#059669";
            if (!hexColor.StartsWith("#")) hexColor = "#" + hexColor;
            var baseC = global::Android.Graphics.Color.ParseColor(hexColor);
            
            int rLight = Math.Min(255, baseC.R + 150);
            int gLight = Math.Min(255, baseC.G + 150);
            int bLight = Math.Min(255, baseC.B + 150);
            var lightHex = $"#{rLight:X2}{gLight:X2}{bLight:X2}";

            var activeEndpoints = baseEndpoints
                .Select((srv, index) => new { srv.Name, srv.IP, srv.Port, Enabled = srvEnabled[index] })
                .Where(x => x.Enabled)
                .ToList();

            var tasks = activeEndpoints.Select(srv => Task.Run(() => FetchServerDataSync(srv.Name, srv.IP, srv.Port, lightHex))).ToList();
            Task.WaitAll(tasks.ToArray());

            bool allTimeouts = activeEndpoints.Count > 0 && tasks.All(t => t.Result.Map.StartsWith("Offline"));
            if (allTimeouts)
            {
                LastGlobalError = "Timeout (Background restricted)";
                return;
            }

            var list = new List<ServerInfo>();
            foreach (var task in tasks)
            {
                var info = task.Result;
                if (hideEmpty && info.PlayerCount == 0) continue;
                list.Add(info);
            }

            if (hideEmpty && list.Count == 0 && activeEndpoints.Count > 0)
            {
                string statusTxt = prefs?.GetString("loc_status", "Status") ?? "Status";
                string[] emptyPhrases = {
                    prefs?.GetString("loc_phrase1", "Waiting for heroes...") ?? "Waiting for heroes...",
                    prefs?.GetString("loc_phrase2", "Silence in the facility...") ?? "Silence in the facility...",
                    prefs?.GetString("loc_phrase3", "No zeds slaughtered yet...") ?? "No zeds slaughtered yet...",
                    prefs?.GetString("loc_phrase4", "Area clear, for now...") ?? "Area clear, for now...",
                    prefs?.GetString("loc_phrase5", "Ready for deployment.") ?? "Ready for deployment."
                };
                
                list.Add(new ServerInfo {
                    Name = "KF2MOD-EU",
                    Map = statusTxt,
                    PlayerCount = 0,
                    PlayersText = $"  {emptyPhrases[new Random().Next(emptyPhrases.Length)]}"
                });
            }

            CachedServers = list;

            try
            {
                CheckAndSendNotifications(context, list);

                var json = JsonSerializer.Serialize(list);
                prefs?.Edit()?.PutString("cached_servers", json)?.Commit();
            }
            catch { }
        }
        
        private bool IsCountEnabledForServer(string name, bool[] countSrvs)
        {
            if (name.Contains("Normal #1")) return countSrvs[0];
            if (name.Contains("Hard #2")) return countSrvs[1];
            if (name.Contains("Suicidal #3")) return countSrvs[2];
            if (name.Contains("HOE #4")) return countSrvs[3];
            if (name.Contains("Extreme #5")) return countSrvs[4];
            return false;
        }

        private void CheckAndSendNotifications(Context context, List<ServerInfo> servers)
        {
            var prefs = context.GetSharedPreferences("KF2WidgetPrefs", FileCreationMode.Private);
            if (prefs == null) return;
            
            bool enableNotifs = prefs.GetBoolean("enable_notifications", false);
            string tracked = prefs.GetString("tracked_players", "") ?? "";

            bool enableCountNotifs = prefs.GetBoolean("enable_count_notifications", false);
            int countThreshold = prefs.GetInt("count_threshold", 6);
            
            bool[] countSrvEnabled = new bool[] {
                prefs.GetBoolean("count_srv1", true),
                prefs.GetBoolean("count_srv2", true),
                prefs.GetBoolean("count_srv3", true),
                prefs.GetBoolean("count_srv4", true),
                prefs.GetBoolean("count_srv5", true)
            };
            
            string popTitle = prefs.GetString("loc_notif_pop_title", "Server Populating!") ?? "Server Populating!";
            string popBodyTpl = prefs.GetString("loc_notif_pop_body", "{0} now has {1} players.") ?? "{0} now has {1} players.";
            
            string playerTitle = prefs.GetString("loc_notif_player_title", "Player Online!") ?? "Player Online!";
            string playerBodyTpl = prefs.GetString("loc_notif_player_body", "{0} joined {1}") ?? "{0} joined {1}";

            string playerLeftTitle = prefs.GetString("loc_notif_player_left_title", "Player Left!") ?? "Player Left!";
            string playerLeftBodyTpl = prefs.GetString("loc_notif_player_left_body", "{0} left {1}") ?? "{0} left {1}";

            var currentOnlineDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var highPopServersStr = prefs.GetString("high_pop_servers", "") ?? "";
            var highPopServers = new HashSet<string>(highPopServersStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase);

            var currentHighPop = new HashSet<string>();

            foreach (var srv in servers)
            {
                if (enableCountNotifs && srv.PlayerCount >= countThreshold && IsCountEnabledForServer(srv.Name, countSrvEnabled))
                {
                    currentHighPop.Add(srv.Name);

                    if (!highPopServers.Contains(srv.Name))
                    {
                        string body = string.Format(popBodyTpl, srv.Name, srv.PlayerCount);
                        SendCountNotification(context, srv.Name, popTitle, body);
                    }
                }

                if (srv.RawPlayers != null)
                {
                    foreach (var p in srv.RawPlayers)
                    {
                        if (!string.IsNullOrEmpty(p.Name))
                        {
                            if (!currentOnlineDict.ContainsKey(p.Name)) currentOnlineDict[p.Name] = srv.Name;
                        }
                    }
                }
            }
            
            prefs.Edit()?.PutString("high_pop_servers", string.Join(",", currentHighPop))?.Commit();

            var lastSeenDictJson = prefs.GetString("last_seen_dict", "{}") ?? "{}";
            var lastSeenDict = JsonSerializer.Deserialize<Dictionary<string, string>>(lastSeenDictJson) ?? new Dictionary<string, string>();

            prefs.Edit()?.PutString("last_seen_dict", JsonSerializer.Serialize(currentOnlineDict))?.Commit();

            if (!enableNotifs || string.IsNullOrWhiteSpace(tracked)) return;

            var trackedList = tracked.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                     .Select(p => p.Trim())
                                     .Where(p => !string.IsNullOrEmpty(p))
                                     .ToList();

            if (trackedList.Count == 0) return;

            var toNotifyJoined = new List<string>();
            var toNotifyLeft = new List<string>();

            foreach (var kvp in currentOnlineDict)
            {
                if (!lastSeenDict.ContainsKey(kvp.Key))
                {
                    bool isTracked = trackedList.Any(t => kvp.Key.IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0);
                    if (isTracked) toNotifyJoined.Add(kvp.Key);
                }
            }

            foreach (var kvp in lastSeenDict)
            {
                if (!currentOnlineDict.ContainsKey(kvp.Key))
                {
                    bool isTracked = trackedList.Any(t => kvp.Key.IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0);
                    if (isTracked) toNotifyLeft.Add(kvp.Key);
                }
            }

            if (toNotifyJoined.Count > 0)
            {
                SendNotification(context, toNotifyJoined, currentOnlineDict, playerTitle, playerBodyTpl, 0);
            }
            
            if (toNotifyLeft.Count > 0)
            {
                SendNotification(context, toNotifyLeft, lastSeenDict, playerLeftTitle, playerLeftBodyTpl, 5000);
            }
        }

        private void SendCountNotification(Context context, string serverName, string title, string text)
        {
            try
            {
                var notificationManager = (NotificationManager?)context.GetSystemService(Context.NotificationService);
                if (notificationManager == null) return;

                string channelId = "kf2_monitor_channel";
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    var channel = new NotificationChannel(channelId, "Server Notifications", NotificationImportance.Default);
                    notificationManager.CreateNotificationChannel(channel);
                }

                var intent = new Intent(context, typeof(MainActivity));
                intent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                var pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

                var builder = new Notification.Builder(context, channelId)
                    .SetContentTitle(title)
                    .SetContentText(text)
                    .SetSmallIcon(Resource.Drawable.Icon)
                    .SetContentIntent(pendingIntent)
                    .SetAutoCancel(true)
                    .SetShowWhen(true)
                    .SetWhen(Java.Lang.JavaSystem.CurrentTimeMillis());

                notificationManager.Notify(serverName.GetHashCode() + 1000, builder.Build());
            }
            catch { }
        }

        private void SendNotification(Context context, List<string> players, Dictionary<string, string> serverMap, string title, string bodyTpl, int idOffset)
        {
            try
            {
                var notificationManager = (NotificationManager?)context.GetSystemService(Context.NotificationService);
                if (notificationManager == null) return;

                string channelId = "kf2_monitor_channel";
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    var channel = new NotificationChannel(channelId, "Player Tracking", NotificationImportance.Default);
                    notificationManager.CreateNotificationChannel(channel);
                }

                var intent = new Intent(context, typeof(MainActivity));
                intent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                var pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

                foreach (var player in players)
                {
                    string serverName = serverMap.ContainsKey(player) ? serverMap[player] : "a server";
                    string body = string.Format(bodyTpl, player, serverName);
                    
                    var builder = new Notification.Builder(context, channelId)
                        .SetContentTitle(title)
                        .SetContentText(body)
                        .SetSmallIcon(Resource.Drawable.Icon)
                        .SetContentIntent(pendingIntent)
                        .SetAutoCancel(true)
                        .SetShowWhen(true)
                        .SetWhen(Java.Lang.JavaSystem.CurrentTimeMillis());

                    notificationManager.Notify(player.GetHashCode() + idOffset, builder.Build());
                }
            }
            catch { }
        }

        public static void LoadCache(Context context)
        {
            if (CachedServers != null && CachedServers.Count > 0) return;
            try
            {
                var prefs = context.GetSharedPreferences("KF2WidgetPrefs", FileCreationMode.Private);
                var json = prefs?.GetString("cached_servers", "");
                if (!string.IsNullOrEmpty(json))
                {
                    CachedServers = JsonSerializer.Deserialize<List<ServerInfo>>(json) ?? new List<ServerInfo>();
                }
            }
            catch { }
        }

        private byte[] SendQuerySync(UdpClient udp, IPEndPoint ep, byte[] baseRequest, byte expectedHeader)
        {
            byte[] currentRequest = baseRequest;

            for (int attempt = 0; attempt < 3; attempt++)
            {
                try
                {
                    udp.Send(currentRequest, currentRequest.Length, ep);

                    var remoteEp = new IPEndPoint(IPAddress.Any, 0);
                    long endTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 1500;

                    while (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() < endTime)
                    {
                        if (udp.Available > 0)
                        {
                            byte[] data = udp.Receive(ref remoteEp);
                            int offset = 0;

                            if (data.Length > 4 && data[0] == 0xFE && data[1] == 0xFF && data[2] == 0xFF && data[3] == 0xFF) offset = 12;

                            if (data.Length >= offset + 5 && data[offset] == 0xFF && data[offset + 1] == 0xFF)
                            {
                                byte type = data[offset + 4];

                                if (type == 0x41)
                                {
                                    if (baseRequest[4] == 0x54)
                                    {
                                        currentRequest = new byte[baseRequest.Length + 4];
                                        Array.Copy(baseRequest, currentRequest, baseRequest.Length);
                                        Array.Copy(data, offset + 5, currentRequest, baseRequest.Length, 4);
                                    }
                                    else if (baseRequest[4] == 0x55 || baseRequest[4] == 0x56)
                                    {
                                        currentRequest = new byte[9];
                                        Array.Copy(baseRequest, currentRequest, 5);
                                        Array.Copy(data, offset + 5, currentRequest, 5, 4);
                                    }

                                    udp.Send(currentRequest, currentRequest.Length, ep);
                                    endTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 1500;
                                    continue;
                                }
                                else if (type == expectedHeader)
                                {
                                    if (offset > 0)
                                    {
                                        byte[] cleanData = new byte[data.Length - offset];
                                        Array.Copy(data, offset, cleanData, 0, cleanData.Length);
                                        return cleanData;
                                    }
                                    return data;
                                }
                            }
                        }
                        else
                        {
                            System.Threading.Thread.Sleep(15);
                        }
                    }
                }
                catch (SocketException) { }
            }

            throw new Exception("Timeout");
        }

        private ServerInfo FetchServerDataSync(string name, string ip, int port, string lightHex)
        {
            var serverData = new ServerInfo { Name = name, Map = "Offline", PlayersText = "" };
            try
            {
                using var udp = new UdpClient();
                udp.Client.SendTimeout = 2000;
                udp.Client.ReceiveTimeout = 2000;

                var ep = new IPEndPoint(IPAddress.Parse(ip), port);

                byte[] reqInfo = { 0xFF, 0xFF, 0xFF, 0xFF, 0x54, 0x53, 0x6F, 0x75, 0x72, 0x63, 0x65, 0x20, 0x45, 0x6E, 0x67, 0x69, 0x6E, 0x65, 0x20, 0x51, 0x75, 0x65, 0x72, 0x79, 0x00 };
                byte[] dataInfo = SendQuerySync(udp, ep, reqInfo, 0x49);
                
                int index = 6;
                ReadString(dataInfo, ref index); 
                serverData.Map = ReadString(dataInfo, ref index);
                ReadString(dataInfo, ref index); 
                ReadString(dataInfo, ref index); 
                index += 2; 

                serverData.PlayerCount = dataInfo[index++];
                serverData.MaxPlayers = dataInfo[index++];

                try
                {
                    byte[] reqRules = { 0xFF, 0xFF, 0xFF, 0xFF, 0x56, 0xFF, 0xFF, 0xFF, 0xFF };
                    byte[] dataRules = SendQuerySync(udp, ep, reqRules, 0x45);

                    if (dataRules.Length >= 7)
                    {
                        int rCount = BitConverter.ToInt16(dataRules, 5);
                        int rIndex = 7;
                        var rules = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                        for (int i = 0; i < rCount && rIndex < dataRules.Length; i++)
                        {
                            string key = ReadString(dataRules, ref rIndex).Trim();
                            string val = ReadString(dataRules, ref rIndex).Trim();
                            if (!string.IsNullOrEmpty(key)) rules[key] = val;
                        }
                        
                        string cw = rules.TryGetValue("CurrentWave", out var w) ? w : "0";
                        string mw = rules.TryGetValue("NumWaves", out var m) ? m : "?";
                        serverData.Wave = $"{cw} / {mw}";
                    }
                }
                catch { }

                if (serverData.PlayerCount > 0)
                {
                    byte[] reqPlayers = { 0xFF, 0xFF, 0xFF, 0xFF, 0x55, 0xFF, 0xFF, 0xFF, 0xFF };
                    byte[] dataPlayers = SendQuerySync(udp, ep, reqPlayers, 0x44);

                    if (dataPlayers.Length >= 6)
                    {
                        int count = dataPlayers[5];
                        int pIndex = 6;
                        var pList = new List<(string Name, float Duration)>();
                        
                        for (int i = 0; i < count && pIndex < dataPlayers.Length; i++)
                        {
                            pIndex++; 
                            string pName = ReadString(dataPlayers, ref pIndex);
                            if (pIndex + 8 > dataPlayers.Length) break;

                            pIndex += 4; 
                            float duration = BitConverter.ToSingle(dataPlayers, pIndex);
                            pIndex += 4; 

                            if (!string.IsNullOrEmpty(pName))
                            {
                                pList.Add((pName, duration));
                            }
                        }

                        serverData.RawPlayers = pList.OrderByDescending(x => x.Duration)
                                                     .Select(p => new ServerPlayer { Name = p.Name, Duration = p.Duration })
                                                     .ToList();

                        var formatted = serverData.RawPlayers.Select(p =>
                        {
                            TimeSpan t = TimeSpan.FromSeconds(p.Duration);
                            string n = p.Name;
                            if (n.Length > 18) n = n.Substring(0, 16) + "..";
                            string time = $"{(int)t.TotalHours:D2}:{t.Minutes:D2}:{t.Seconds:D2}";
                            
                            int totalChars = 32;
                            int dotsCount = totalChars - n.Length - time.Length;
                            if (dotsCount < 1) dotsCount = 1;
                            
                            string dots = new string('.', dotsCount);
                            string safeName = n.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace(" ", "&nbsp;");

                            string line = $"{safeName}{dots}{time}";
                            return line;
                        });

                        serverData.PlayersText = string.Join("<br>", formatted);
                    }
                }
            }
            catch (Exception ex)
            {
                serverData.Map = $"Offline ({ex.Message})";
            }

            return serverData;
        }

        private string ReadString(byte[] data, ref int index)
        {
            if (index >= data.Length) return string.Empty;
            int start = index;
            while (index < data.Length && data[index] != 0) index++;
            string result = Encoding.UTF8.GetString(data, start, index - start);
            if (index < data.Length) index++;
            return result;
        }

        private void UpdateWidgetState(Context context, AppWidgetManager appWidgetManager, int widgetId, bool isLoading)
        {
            LoadCache(context);

            var views = new RemoteViews(context.PackageName, Resource.Layout.kf2_widget);
            
            try
            {
                var intent = new Intent(context, typeof(Kf2WidgetService));
                intent.PutExtra(AppWidgetManager.ExtraAppwidgetId, widgetId);
                intent.SetData(global::Android.Net.Uri.Parse("kf2://widget/" + widgetId));

                views.SetRemoteAdapter(Resource.Id.list_servers, intent);

                var prefs = context.GetSharedPreferences("KF2WidgetPrefs", FileCreationMode.Private);
                string hexColor = prefs?.GetString("theme_color", "#059669") ?? "#059669";
                int alpha = prefs?.GetInt("alpha", 255) ?? 255;
                bool hasBgImage = prefs?.GetBoolean("has_bg_image", false) ?? false;
                int cornerRadius = prefs?.GetInt("corner_radius", 12) ?? 12;
                int imgOpacity = prefs?.GetInt("image_opacity", 100) ?? 100;
                float offsetY = prefs?.GetFloat("image_offset_y", 0f) ?? 0f;
                bool swapTopBar = prefs?.GetBoolean("swap_top_bar", false) ?? false;

                string locUpdated = prefs?.GetString("loc_updated", "Updated:") ?? "Updated:";
                
                bool fullSizeImage = prefs?.GetBoolean("full_size_image", true) ?? true;
                bool fadeBackground = prefs?.GetBoolean("fade_background", false) ?? false;

                if (!hexColor.StartsWith("#")) hexColor = "#" + hexColor;
                if (hexColor.Length == 4) hexColor = $"#{hexColor[1]}{hexColor[1]}{hexColor[2]}{hexColor[2]}{hexColor[3]}{hexColor[3]}";
                
                var baseColor = global::Android.Graphics.Color.ParseColor(hexColor);
                var finalColor = global::Android.Graphics.Color.Argb(alpha, baseColor.R, baseColor.G, baseColor.B);

                float density = context.Resources.DisplayMetrics.Density;
                var options = appWidgetManager.GetAppWidgetOptions(widgetId);
                int minWidth = options.GetInt(AppWidgetManager.OptionAppwidgetMinWidth);
                
                int w = (int)((minWidth > 0 ? minWidth : 300) * density);

                float hDp = 85f; 
                foreach (var srv in CachedServers)
                {
                    hDp += 60f; 
                    if (!string.IsNullOrEmpty(srv.PlayersText))
                    {
                        hDp += 15f; 
                        int lines = srv.PlayersText.Split(new[] { "<br>", "<br/>", "\n" }, StringSplitOptions.None).Length;
                        hDp += lines * 14f; 
                    }
                }
                if (CachedServers.Count > 1) hDp += (CachedServers.Count - 1) * 8f; 

                int h = (int)(hDp * density);
                if (h <= 0) h = 300;
                if (w <= 0) w = 300;

                global::Android.Graphics.Bitmap? imgBmp = null;
                float scale = 1f;

                if (hasBgImage)
                {
                    string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "widget_bg.png");
                    var file = new Java.IO.File(path);
                    if (file.Exists())
                    {
                        var imgOptions = new global::Android.Graphics.BitmapFactory.Options { InJustDecodeBounds = true };
                        global::Android.Graphics.BitmapFactory.DecodeFile(file.AbsolutePath, imgOptions);

                        int inSampleSize = 1;
                        if (imgOptions.OutHeight > h || imgOptions.OutWidth > w)
                        {
                            int halfHeight = imgOptions.OutHeight / 2;
                            int halfWidth = imgOptions.OutWidth / 2;
                            while ((halfHeight / inSampleSize) >= h && (halfWidth / inSampleSize) >= w)
                            {
                                inSampleSize *= 2;
                            }
                        }

                        if (fullSizeImage)
                        {
                            float tempScale = (float)w / imgOptions.OutWidth;
                            int expandedH = (int)(imgOptions.OutHeight * tempScale);
                            if (expandedH > h) h = expandedH;
                        }

                        imgOptions.InJustDecodeBounds = false;
                        imgOptions.InSampleSize = inSampleSize;

                        imgBmp = global::Android.Graphics.BitmapFactory.DecodeFile(file.AbsolutePath, imgOptions);
                        if (imgBmp != null)
                        {
                            scale = (float)w / imgBmp.Width;
                        }
                    }
                }

                var bmp = global::Android.Graphics.Bitmap.CreateBitmap(w, h, global::Android.Graphics.Bitmap.Config.Argb8888!);
                using (var canvas = new global::Android.Graphics.Canvas(bmp))
                using (var paint = new global::Android.Graphics.Paint { AntiAlias = true, FilterBitmap = true })
                {
                    var rect = new global::Android.Graphics.RectF(0, 0, w, h);
                    float rPx = cornerRadius * density;

                    paint.Color = finalColor;
                    canvas.DrawRoundRect(rect, rPx, rPx, paint);

                    if (imgBmp != null)
                    {
                        var matrix = new global::Android.Graphics.Matrix();
                        matrix.PostScale(scale, scale);
                        matrix.PostTranslate(0, offsetY * density);
                        
                        canvas.Save();
                        var pathObj = new global::Android.Graphics.Path();
                        pathObj.AddRoundRect(rect, rPx, rPx, global::Android.Graphics.Path.Direction.Cw!);
                        canvas.ClipPath(pathObj);

                        paint.Alpha = (int)(imgOpacity / 100f * 255f);
                        canvas.DrawBitmap(imgBmp, matrix, paint);

                        if (fadeBackground)
                        {
                            var fadeShader = new global::Android.Graphics.LinearGradient(
                                0, h * 0.4f, 0, h,
                                global::Android.Graphics.Color.Transparent.ToArgb(),
                                finalColor.ToArgb(),
                                global::Android.Graphics.Shader.TileMode.Clamp!
                            );
                            paint.SetShader(fadeShader);
                            paint.Alpha = 255;
                            canvas.DrawRect(rect, paint);
                            paint.SetShader(null);
                        }
                        else
                        {
                            paint.Alpha = 255;
                            paint.Color = finalColor;
                            canvas.DrawRect(rect, paint);
                        }

                        canvas.Restore();
                        imgBmp.Recycle();
                    }
                }

                views.SetImageViewBitmap(Resource.Id.widget_bg_image, bmp);
                views.SetViewVisibility(Resource.Id.widget_bg_image, global::Android.Views.ViewStates.Visible);
                views.SetInt(Resource.Id.widget_background, "setBackgroundColor", global::Android.Graphics.Color.Transparent.ToArgb());

                if (swapTopBar)
                {
                    views.SetViewVisibility(Resource.Id.header_normal, global::Android.Views.ViewStates.Gone);
                    views.SetViewVisibility(Resource.Id.header_swapped, global::Android.Views.ViewStates.Visible);
                }
                else
                {
                    views.SetViewVisibility(Resource.Id.header_normal, global::Android.Views.ViewStates.Visible);
                    views.SetViewVisibility(Resource.Id.header_swapped, global::Android.Views.ViewStates.Gone);
                }

                if (isLoading)
                {
                    views.SetViewVisibility(Resource.Id.btn_refresh, global::Android.Views.ViewStates.Invisible);
                    views.SetViewVisibility(Resource.Id.btn_refresh_swapped, global::Android.Views.ViewStates.Invisible);
                    views.SetViewVisibility(Resource.Id.progress_refresh, global::Android.Views.ViewStates.Visible);
                    views.SetViewVisibility(Resource.Id.progress_refresh_swapped, global::Android.Views.ViewStates.Visible);

                    views.SetTextViewText(Resource.Id.text_last_update, "Loading...");
                    views.SetTextViewText(Resource.Id.text_last_update_swapped, "Loading...");
                }
                else
                {
                    views.SetViewVisibility(Resource.Id.btn_refresh, global::Android.Views.ViewStates.Visible);
                    views.SetViewVisibility(Resource.Id.btn_refresh_swapped, global::Android.Views.ViewStates.Visible);
                    views.SetViewVisibility(Resource.Id.progress_refresh, global::Android.Views.ViewStates.Gone);
                    views.SetViewVisibility(Resource.Id.progress_refresh_swapped, global::Android.Views.ViewStates.Gone);
                    
                    string updateText = string.IsNullOrEmpty(LastGlobalError) 
                        ? $"{locUpdated} {DateTime.Now:HH:mm:ss}" 
                        : $"Err: {LastGlobalError}";

                    views.SetTextViewText(Resource.Id.text_last_update, updateText);
                    views.SetTextViewText(Resource.Id.text_last_update_swapped, updateText);
                }

                var appIntent = new Intent(context, typeof(MainActivity));
                appIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                var pendingApp = PendingIntent.GetActivity(context, 0, appIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
                if (pendingApp != null)
                {
                    views.SetOnClickPendingIntent(Resource.Id.widget_background, pendingApp);
                    views.SetOnClickPendingIntent(Resource.Id.btn_settings, pendingApp);
                    views.SetOnClickPendingIntent(Resource.Id.btn_settings_swapped, pendingApp);
                }

                var refreshIntent = new Intent(context, typeof(Kf2WidgetProvider));
                refreshIntent.SetAction("com.kf2monitor.widget.REFRESH");
                var pendingRefresh = PendingIntent.GetBroadcast(context, widgetId, refreshIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
                if (pendingRefresh != null)
                {
                    views.SetOnClickPendingIntent(Resource.Id.btn_refresh, pendingRefresh);
                    views.SetOnClickPendingIntent(Resource.Id.btn_refresh_swapped, pendingRefresh);
                }
            }
            catch (Exception ex)
            {
                views.SetTextViewText(Resource.Id.text_last_update, "Err UI: " + ex.Message);
            }

            appWidgetManager.UpdateAppWidget(widgetId, views);
        }
    }
}