using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomCampaign.UI;
using MelonLoader;

namespace CustomCampaign.Controller
{
    internal static class CampaignController
    {
        public static Campaign SelectedCampaign { get; set; } = null;
        private static UnhollowerBaseLib.Il2CppReferenceArray<CampaignStructure.Campaign> mainCampaign;
        private static string previousArena = "environment1";

        internal static void GoToMainCampaign()
        {
            Main.State = State.Main;
            SelectedCampaign = null;
            CampaignStructure.I.LoadCampaignProgress();
            MenuState.I.GoToCampaignSelectPage();
        }

        internal static void GoToCustomCampaign(Campaign campaign)
        {
            previousArena = PlayerPreferences.I.Environment.Get();
            Main.State = State.Custom;
            SelectedCampaign = campaign;
            InjectCampaign();
            MenuState.I.GoToCampaignSelectPage();
        }

        internal static void ResetCampaign()
        {
            if (mainCampaign is null) return;
            CampaignStructure.I.campaigns = mainCampaign.ToArray();
            mainCampaign = null;
        }

        internal static void InjectCampaign()
        {
            Main.InitializeInternalSongList();
            if (!SelectedCampaign.SongDataInitialized)
            {
                SelectedCampaign.SongDataInitialized = true;
                foreach(Campaign.Song song in SelectedCampaign.Songs)
                {
                    if (Main.InternalSongList.Any(s => s.zipPath.Contains(song.FileName)))
                    {
                        song.Data.ID = Main.InternalSongList.First(s => s.zipPath.Contains(song.FileName)).songID;
                    }
                }
            }
            mainCampaign = CampaignStructure.I.campaigns.ToArray();
            CampaignStructure.I.campaigns = null;
            List<CampaignStructure.Campaign> campaigns = new List<CampaignStructure.Campaign>();
            if (SelectedCampaign.HasExpert) campaigns.Add(CreateCampaignStructure(KataConfig.Difficulty.Expert));
            if (SelectedCampaign.HasAdvanced) campaigns.Add(CreateCampaignStructure(KataConfig.Difficulty.Hard));
            if (SelectedCampaign.HasStandard) campaigns.Add(CreateCampaignStructure(KataConfig.Difficulty.Normal));
            if (SelectedCampaign.HasBeginner) campaigns.Add(CreateCampaignStructure(KataConfig.Difficulty.Easy));
            SelectedCampaign.InternalCampaigns = campaigns;
            CampaignStructure.I.campaigns = new UnhollowerBaseLib.Il2CppReferenceArray<CampaignStructure.Campaign>(campaigns.ToArray());
            MelonLogger.Msg("Finished campaign injection");
        }

        private static CampaignStructure.Campaign CreateCampaignStructure(KataConfig.Difficulty difficulty)
        {
            CampaignStructure.Campaign campaign = new CampaignStructure.Campaign();
            List<CampaignStructure.CampaignTier> tiers = new List<CampaignStructure.CampaignTier>();
            int tierIndex = 0;
            int tokenIndex = 66;
            int assetIndex = 199;
            int songIndex = 80;
            foreach (Campaign.Tier t in SelectedCampaign.Tiers)
            {
                CampaignStructure.CampaignTier campaignTier = new CampaignStructure.CampaignTier();
                campaignTier.campaign = campaign;
                campaignTier.hasSeenNarrativeMoment = t.HasDisplayedStory;
                campaignTier.tierName = (Loc.Token)tokenIndex;
                tokenIndex++;
                campaignTier.narrativeDescription = (Loc.Token)tokenIndex;
                tokenIndex++;
                campaignTier.starThreshold = t.RequiredStars;
                campaignTier.tierIndex = tierIndex;
                List<CampaignStructure.CampaignUnlock> unlocks = new List<CampaignStructure.CampaignUnlock>();
                foreach(Campaign.Unlock u in t.Unlocks)
                {
                    CampaignStructure.CampaignUnlock unlock = new CampaignStructure.CampaignUnlock();
                    switch (u.Type)
                    {
                        case Campaign.UnlockType.Song:
                            unlock.unlockType = CampaignStructure.UnlockType.Song;
                            break;
                        case Campaign.UnlockType.Arena:
                            unlock.unlockType = CampaignStructure.UnlockType.Venue;
                            break;
                        case Campaign.UnlockType.Gun:
                            unlock.unlockType = CampaignStructure.UnlockType.Gun;
                            break;
                        default:
                            continue;
                    }
                    switch (unlock.unlockType)
                    {
                        case CampaignStructure.UnlockType.Gun:
                            unlock.unlockName = "gun" + tierIndex;
                            u.Token = assetIndex == 203 ? Loc.Token.Asset_Gun5 : (Loc.Token)assetIndex;
                            break;
                        case CampaignStructure.UnlockType.Venue:
                            unlock.unlockName = "env" + tierIndex;
                            u.Token = (Loc.Token)assetIndex + 4;
                            break;
                        case CampaignStructure.UnlockType.Song:
                            unlock.unlockName = "song" + tierIndex;
                            u.Token = (Loc.Token)songIndex;
                            break;
                    }
                    unlocks.Add(unlock);
                }
                campaignTier.unlocks = new UnhollowerBaseLib.Il2CppReferenceArray<CampaignStructure.CampaignUnlock>(unlocks.ToArray());
                List<CampaignStructure.CampaignSong> songs = new List<CampaignStructure.CampaignSong>();
                foreach (Campaign.Song s in t.Songs)
                {
                    CampaignStructure.CampaignSong campaignSong = new CampaignStructure.CampaignSong();
                    campaignSong.challenges = s.InternalModifiers.ToArray();
                    campaignSong.challengesPSVR = null;
                    campaignSong.challengesQuest = null;
                    //campaignSong.isFinaleSong = s.IsFinaleSong;
                    campaignSong.isFinaleSong = false;
                    campaignSong.songID = s.Data.ID;
                    switch (difficulty)
                    {
                        case KataConfig.Difficulty.Expert:
                            campaignSong.starCount = s.ExpertStarCount;
                            break;
                        case KataConfig.Difficulty.Hard:
                            campaignSong.starCount = s.AdvancedStarCount;
                            break;
                        case KataConfig.Difficulty.Normal:
                            campaignSong.starCount = s.StandardStarCount;
                            break;
                        case KataConfig.Difficulty.Easy:
                            campaignSong.starCount = s.BeginnerStarCount;
                            break;
                    }
                    campaignSong.useChallengesPSVR = false;
                    campaignSong.useChallengesQuest = false;
                    campaignSong.tier = campaignTier;
                    campaignSong.songIndex = s.Index;
                    songs.Add(campaignSong);
                }
                campaignTier.songs = new UnhollowerBaseLib.Il2CppReferenceArray<CampaignStructure.CampaignSong>(songs.ToArray());
                tiers.Add(campaignTier);
                tierIndex++;
                assetIndex++;
                songIndex++;
            }
            campaign.tiers = new UnhollowerBaseLib.Il2CppReferenceArray<CampaignStructure.CampaignTier>(tiers.ToArray());
            campaign.difficulty = difficulty;
            return campaign;
        }

        internal static bool SwitchArena(string newArena, bool reset)
        {
            if (PlayerPreferences.I.Environment.Get() == newArena)
            {
                if (reset)
                {
                    DoSwitch(previousArena);
                    return true;
                }
                return false;
            }
            DoSwitch(newArena);
            return true;
        }

        private static void DoSwitch(string arena)
        {
            PlayerPreferences.I.Environment.Set(arena);
            EnvironmentLoader.I.SwitchEnvironment();
        }
    }
}
