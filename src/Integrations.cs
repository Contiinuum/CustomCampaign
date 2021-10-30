using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudicaModding;
using ArenaLoader;
using System.Collections;
using UnityEngine;

namespace CustomCampaign
{
    internal static class Integrations
    {
        internal static bool SongBrowserInstalled { get; set; } = false;
        internal static bool ArenaLoaderInstalled { get; set; } = false;
        internal static void FindIntegrations()
        {
            if (MelonHandler.Mods.Any(it => it.Info.SystemType.Name == nameof(SongBrowser)))
            {
                var scoreVersion = new Version(MelonHandler.Mods.First(it => it.Assembly.GetName().Name == "SongBrowser").Info.Version);
                var lastUnsupportedVersion = new Version("3.0.4");
                var result = scoreVersion.CompareTo(lastUnsupportedVersion);
                if (result > 0)
                {
                    SongBrowserInstalled = true;
                }
                MelonLogger.Msg("Song Browser is installed. Enabling integration");
            }
            if(MelonHandler.Mods.Any(it => it.Info.SystemType.Name == nameof(ArenaLoaderMod)))
            {
                MelonLogger.Msg("Arena Loader is installed. Enabling integration");
                ArenaLoaderInstalled = true;
            }
        }

        internal static void ReloadSongList(bool fullReload)
        {
            if (SongBrowserInstalled) DoReloadSongList(fullReload);
        }

        internal static void RegisterSongListPostProcess(Action action)
        {
            if (SongBrowserInstalled) RegisterPostProcess(action);
        }

        private static void RegisterPostProcess(Action action)
        {
            SongBrowser.RegisterSongListPostProcessing(action);
        }

        private static void DoReloadSongList(bool fullReload)
        {
            SongBrowser.ReloadSongList(fullReload);
        }

        internal static void SetCampaignSongs()
        {
            if (SongBrowserInstalled)
            {
                DoSetCampaignSongs();
            }
        }

        private static void DoSetCampaignSongs()
        {
            PlaylistManager.SetCampaignSongs(Main.GetAllSongs());
        }

        internal static void ReloadArenas()
        {
            if (ArenaLoaderInstalled)
            {
                DoReloadArenas();
            }
        }

        private static void DoReloadArenas()
        {
            ArenaLoaderMod.LoadAllFoundArenas();
        }
    }
}
