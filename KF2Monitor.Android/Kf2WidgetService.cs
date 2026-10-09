#pragma warning disable CA1416
#pragma warning disable CA1422

using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Widget;
using System;
using System.Collections.Generic;

namespace KF2Monitor.Android
{
    [Service(Permission = "android.permission.BIND_REMOTEVIEWS", Exported = true)]
    public class Kf2WidgetService : RemoteViewsService
    {
        public override IRemoteViewsFactory? OnGetViewFactory(Intent? intent)
        {
            if (intent == null) return null;
            return new Kf2RemoteViewsFactory(this.ApplicationContext ?? this, intent);
        }
    }

    public class Kf2RemoteViewsFactory : Java.Lang.Object, RemoteViewsService.IRemoteViewsFactory
    {
        private Context _context;
        private List<ServerInfo> _servers;

        public Kf2RemoteViewsFactory(Context context, Intent intent)
        {
            _context = context;
            Kf2WidgetProvider.LoadCache(_context);
            _servers = Kf2WidgetProvider.CachedServers ?? new List<ServerInfo>();
        }

        public void OnCreate() { }

        public void OnDataSetChanged()
        {
            Kf2WidgetProvider.LoadCache(_context);
            _servers = Kf2WidgetProvider.CachedServers ?? new List<ServerInfo>();
        }

        public RemoteViews? GetViewAt(int position)
        {
            if (position < 0 || position >= _servers.Count) return null;

            var server = _servers[position];
            var rv = new RemoteViews(_context.PackageName, Resource.Layout.kf2_widget_item);

            var prefs = _context.GetSharedPreferences("KF2WidgetPrefs", FileCreationMode.Private);
            string hexColor = prefs?.GetString("theme_color", "#059669") ?? "#059669";
            if (!hexColor.StartsWith("#")) hexColor = "#" + hexColor;
            var baseC = global::Android.Graphics.Color.ParseColor(hexColor);
            
            bool swapLR = prefs?.GetBoolean("swap_lr", false) ?? false;
            bool swapWP = prefs?.GetBoolean("swap_wp", false) ?? false;
            bool showMap = prefs?.GetBoolean("show_map", true) ?? true;
            bool showWave = prefs?.GetBoolean("show_wave", true) ?? true;
            bool showPlayers = prefs?.GetBoolean("show_players", true) ?? true;
            string locWave = prefs?.GetString("loc_wave", "Wave:") ?? "Wave:";

            var accentColor1 = global::Android.Graphics.Color.Argb(255, 
                Math.Min(255, baseC.R + 80), Math.Min(255, baseC.G + 80), Math.Min(255, baseC.B + 80));
            var accentColor2 = global::Android.Graphics.Color.Argb(255, 
                Math.Min(255, baseC.R + 130), Math.Min(255, baseC.G + 130), Math.Min(255, baseC.B + 130));

            string topPlain = server.Name;
            string topBg = swapWP ? (string.IsNullOrEmpty(server.Wave) ? "" : $"{locWave} {server.Wave}") : $"{server.PlayerCount}";
            bool showTopBg = swapWP ? (showWave && !string.IsNullOrEmpty(server.Wave)) : showPlayers;

            string bottomPlain = server.Map;
            string bottomBg = swapWP ? $"{server.PlayerCount}" : (string.IsNullOrEmpty(server.Wave) ? "" : $"{locWave} {server.Wave}");
            bool showBottomBg = swapWP ? showPlayers : (showWave && !string.IsNullOrEmpty(server.Wave));

            // Top Row
            if (swapLR) {
                rv.SetViewVisibility(Resource.Id.text_bg_top_left, showTopBg ? global::Android.Views.ViewStates.Visible : global::Android.Views.ViewStates.Gone);
                rv.SetTextViewText(Resource.Id.text_bg_top_left, topBg);
                rv.SetTextColor(Resource.Id.text_bg_top_left, swapWP ? accentColor2 : accentColor1);
                
                rv.SetViewVisibility(Resource.Id.text_plain_top_left, global::Android.Views.ViewStates.Gone);
                rv.SetViewVisibility(Resource.Id.text_plain_top_right, global::Android.Views.ViewStates.Visible);
                rv.SetTextViewText(Resource.Id.text_plain_top_right, topPlain);
                rv.SetViewVisibility(Resource.Id.text_bg_top_right, global::Android.Views.ViewStates.Gone);
            } else {
                rv.SetViewVisibility(Resource.Id.text_bg_top_left, global::Android.Views.ViewStates.Gone);
                rv.SetViewVisibility(Resource.Id.text_plain_top_left, global::Android.Views.ViewStates.Visible);
                rv.SetTextViewText(Resource.Id.text_plain_top_left, topPlain);
                rv.SetViewVisibility(Resource.Id.text_plain_top_right, global::Android.Views.ViewStates.Gone);
                
                rv.SetViewVisibility(Resource.Id.text_bg_top_right, showTopBg ? global::Android.Views.ViewStates.Visible : global::Android.Views.ViewStates.Gone);
                rv.SetTextViewText(Resource.Id.text_bg_top_right, topBg);
                rv.SetTextColor(Resource.Id.text_bg_top_right, swapWP ? accentColor2 : accentColor1);
            }

            // Bottom Row
            if (swapLR) {
                rv.SetViewVisibility(Resource.Id.text_bg_bottom_left, showBottomBg ? global::Android.Views.ViewStates.Visible : global::Android.Views.ViewStates.Gone);
                rv.SetTextViewText(Resource.Id.text_bg_bottom_left, bottomBg);
                rv.SetTextColor(Resource.Id.text_bg_bottom_left, swapWP ? accentColor1 : accentColor2);

                rv.SetViewVisibility(Resource.Id.text_plain_bottom_left, global::Android.Views.ViewStates.Gone);
                rv.SetViewVisibility(Resource.Id.text_plain_bottom_right, showMap ? global::Android.Views.ViewStates.Visible : global::Android.Views.ViewStates.Gone);
                rv.SetTextViewText(Resource.Id.text_plain_bottom_right, bottomPlain);
                rv.SetViewVisibility(Resource.Id.text_bg_bottom_right, global::Android.Views.ViewStates.Gone);
            } else {
                rv.SetViewVisibility(Resource.Id.text_bg_bottom_left, global::Android.Views.ViewStates.Gone);
                rv.SetViewVisibility(Resource.Id.text_plain_bottom_left, showMap ? global::Android.Views.ViewStates.Visible : global::Android.Views.ViewStates.Gone);
                rv.SetTextViewText(Resource.Id.text_plain_bottom_left, bottomPlain);
                rv.SetViewVisibility(Resource.Id.text_plain_bottom_right, global::Android.Views.ViewStates.Gone);

                rv.SetViewVisibility(Resource.Id.text_bg_bottom_right, showBottomBg ? global::Android.Views.ViewStates.Visible : global::Android.Views.ViewStates.Gone);
                rv.SetTextViewText(Resource.Id.text_bg_bottom_right, bottomBg);
                rv.SetTextColor(Resource.Id.text_bg_bottom_right, swapWP ? accentColor1 : accentColor2);
            }

            // Players List Check
            if (server.RawPlayers != null && server.RawPlayers.Count > 0)
            {
                rv.SetViewVisibility(Resource.Id.divider_players, global::Android.Views.ViewStates.Visible);
                rv.SetViewVisibility(Resource.Id.players_container, global::Android.Views.ViewStates.Visible);
                rv.SetViewVisibility(Resource.Id.text_players_list, global::Android.Views.ViewStates.Gone);
                
                rv.RemoveAllViews(Resource.Id.players_container);

                int rLight = Math.Min(255, baseC.R + 150);
                int gLight = Math.Min(255, baseC.G + 150);
                int bLight = Math.Min(255, baseC.B + 150);

                foreach (var p in server.RawPlayers)
                {
                    var playerRv = new RemoteViews(_context.PackageName, Resource.Layout.kf2_player_item);
                    
                    TimeSpan t = TimeSpan.FromSeconds(p.Duration);
                    string time = $"{(int)t.TotalHours:D2}:{t.Minutes:D2}:{t.Seconds:D2}";
                    
                    playerRv.SetTextViewText(Resource.Id.text_player_name, p.Name);
                    playerRv.SetTextViewText(Resource.Id.text_player_time, time);
                    playerRv.SetTextColor(Resource.Id.text_player_time, accentColor1);

                    if (Kf2WidgetProvider.AdminSteamNames != null && Kf2WidgetProvider.AdminSteamNames.Contains(p.Name))
                    {
                        var highlightColor = global::Android.Graphics.Color.Argb(255, rLight, gLight, bLight);
                        playerRv.SetTextColor(Resource.Id.text_player_name, highlightColor);
                        playerRv.SetTextColor(Resource.Id.text_player_time, highlightColor);
                    }

                    rv.AddView(Resource.Id.players_container, playerRv);
                }
            }
            else if (!string.IsNullOrEmpty(server.PlayersText))
            {
                rv.SetViewVisibility(Resource.Id.divider_players, global::Android.Views.ViewStates.Visible);
                rv.SetViewVisibility(Resource.Id.players_container, global::Android.Views.ViewStates.Gone);
                rv.SetViewVisibility(Resource.Id.text_players_list, global::Android.Views.ViewStates.Visible);
                rv.SetTextViewText(Resource.Id.text_players_list, global::Android.Text.Html.FromHtml(server.PlayersText, global::Android.Text.FromHtmlOptions.ModeLegacy));
            }
            else
            {
                rv.SetViewVisibility(Resource.Id.divider_players, global::Android.Views.ViewStates.Gone);
                rv.SetViewVisibility(Resource.Id.players_container, global::Android.Views.ViewStates.Gone);
                rv.SetViewVisibility(Resource.Id.text_players_list, global::Android.Views.ViewStates.Gone);
            }

            return rv;
        }

        public int Count => _servers.Count;
        public long GetItemId(int position) => position;
        public RemoteViews? LoadingView => null;
        public int ViewTypeCount => 1;
        public bool HasStableIds => true;
        public void OnDestroy() => _servers.Clear();
    }

    public class ServerPlayer
    {
        public string Name { get; set; } = string.Empty;
        public float Duration { get; set; }
    }

    public class ServerInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Map { get; set; } = string.Empty;
        public int PlayerCount { get; set; }
        public int MaxPlayers { get; set; }
        public string Wave { get; set; } = string.Empty;
        public string PlayersText { get; set; } = string.Empty;
        public List<ServerPlayer> RawPlayers { get; set; } = new List<ServerPlayer>();
    }
}