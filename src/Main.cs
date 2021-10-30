using System;
using System.Collections.Generic;
using MelonLoader;
using UnityEngine;
using Harmony;
using CustomCampaign.Controller;
using CustomCampaign.UI;
using TMPro;

namespace CustomCampaign
{
    public class Main : MelonMod
    {
        internal static State State { get; set; } = State.None;
        internal static List<Campaign> Campaigns { get; set; } = new List<Campaign>();
        internal static List<SongList.SongData> InternalSongList { get; set; } = new List<SongList.SongData>();
        internal static bool QueueSongListReload { get; set; } = false;
        public static bool IsCampaignActive => State == State.Main || State == State.Custom;
        public static class BuildInfo
        {
            public const string Name = "CustomCampaign";  // Name of the Mod.  (MUST BE SET)
            public const string Author = "Continuum"; // Author of the Mod.  (Set as null if none)
            public const string Company = null; // Company that made the Mod.  (Set as null if none)
            public const string Version = "1.0.1"; // Version of the Mod.  (MUST BE SET)
            public const string DownloadLink = null; // Download Link for the Mod.  (Set as null if none)
        }
        private bool campaignsLoaded = false;
        private static bool internalSongListInitialized = false;
        private static bool doInitializeInternalSonglist = false;
	    public override void OnApplicationStart()
        {
            Integrations.FindIntegrations();
            LoadCampaigns(false);
            SongList.OnSongListLoaded.On(new Action(() =>
            {
                if (doInitializeInternalSonglist)
                {
                    InitializeInternalSongList();
                }
                else if (!campaignsLoaded)
                {
                    //LoadCampaigns();
                    campaignsLoaded = true;
                    Integrations.ReloadSongList(false);
                    doInitializeInternalSonglist = true;
                }


            }));
        }

        public static void InitializeInternalSongList()
        {
            if (internalSongListInitialized) return;
            internalSongListInitialized = true;
            GetInternalSongList();
        }

        internal static void LoadCampaigns(bool reload)
        {
            //GetInternalSongList();
            Campaigns = IOHandler.LoadCampaigns();
            MelonLogger.Msg("Loaded " + Campaigns.Count + " custom campaigns");
            if(!reload) Integrations.SetCampaignSongs();
        }

        private static void GetInternalSongList()
        {
            InternalSongList = new List<SongList.SongData>();
            for (int i = 0; i < SongList.I.songs.Count; i++)
            {
                InternalSongList.Add(SongList.I.songs[i]);
            }
        }

        internal static List<string> GetAllSongs()
        {
            List<string> songs = new List<string>();
            foreach(Campaign campaign in Campaigns)
            {
                foreach(Campaign.Song song in campaign.Songs)
                {
                    songs.Add(song.FileName);
                }
            }
            return songs;
        }

        internal static void ReloadSongList()
        {
            internalSongListInitialized = false;
            doInitializeInternalSonglist = true;
            QueueSongListReload = false;
            LoadCampaigns(true);
            Integrations.ReloadSongList(false);          
        }

    }

    public enum State
    {
        None,
        Select,
        Custom,
        Main,
        DifficultySelect
    }
}

















































































