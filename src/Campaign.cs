using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomCampaign.Controller;
namespace CustomCampaign
{
    [Serializable]
    public class Campaign
    {
        public string Name { get; set; }
        public string Creator { get; set; }
        public List<Tier> Tiers { get; set; } = new List<Tier>();
        public int ScrollIndex { get; set; }
        [JsonIgnore] public List<Campaign.Song> Songs { get; set; } = new List<Campaign.Song>();
        [JsonIgnore] public int PercentageExpert { get; set; } = 0;
        [JsonIgnore] public int PercentageAdvanced { get; set; } = 0;
        [JsonIgnore] public int PercentageStandard { get; set; } = 0;
        [JsonIgnore] public int PercentageBeginner { get; set; } = 0;
        [JsonIgnore] public bool HasExpert { get; set; } = true;
        [JsonIgnore] public bool HasAdvanced { get; set; } = true;
        [JsonIgnore] public bool HasStandard { get; set; } = true;
        [JsonIgnore] public bool HasBeginner { get; set; } = true;
        [JsonIgnore] public List<CampaignStructure.Campaign> InternalCampaigns { get; set; } = new List<CampaignStructure.Campaign>();
        [JsonIgnore] public string FilePath { get; set; } = "";
        [JsonIgnore] public bool SongDataInitialized { get; set; } = false;
        public Campaign(string name, string creator, List<Tier> tiers, int scrollIndex)
        {
            Name = name;
            Creator = creator;
            Tiers = tiers;
            ScrollIndex = scrollIndex;

            foreach (Tier tier in Tiers) Songs.AddRange(tier.Songs);

            SetupDifficulties();
            //Initialize();
        }

        public void SetupDifficulties()
        {
            HasExpert = !Songs.Any(song => song.Data.Expert == false);
            HasAdvanced = !Songs.Any(song => song.Data.Advanced == false);
            HasStandard = !Songs.Any(song => song.Data.Standard == false);
            HasBeginner = !Songs.Any(song => song.Data.Beginner == false);
        }

        public void SetStorySeen()
        {
            KataConfig.Difficulty difficulty = CampaignStructure.I.GetCampaignDifficulty();
            foreach(Tier tier in Tiers)
            {
                int starCount = 0;
                switch (difficulty)
                {
                    case KataConfig.Difficulty.Expert:
                        starCount = tier.ExpertStarCount;
                        break;
                    case KataConfig.Difficulty.Hard:
                        starCount = tier.AdvancedStarCount;
                        break;
                    case KataConfig.Difficulty.Normal:
                        starCount = tier.StandardStarCount;
                        break;
                    case KataConfig.Difficulty.Easy:
                        starCount = tier.BeginnerStarCount;
                        break;
                }
                if (starCount >= tier.RequiredStars) continue;
                tier.HasDisplayedStory = true;
                //Save();
                break;
            }
        }

        public Tier GetHighestUnlockedTier()
        {
            var _tiers = CampaignStructure.I.GetCampaign(CampaignStructure.I.mCurrentCampaignDifficulty).tiers;
            int index = 0;
            /*for (int i = 0; i < _tiers.Count; i++)
            {
                if (CampaignStructure.I.IsTierUnlocked(_tiers[i])) continue;
                index = i;
                break;
            }*/
            //if (index == 0) return Tiers[0];
            bool lastTier = true;
            for(int i = 0; i < CampaignController.SelectedCampaign.Tiers.Count; i++)
            {
                if (CampaignController.SelectedCampaign.Tiers[i].IsThreshholdMet(CampaignStructure.I.mCurrentCampaignDifficulty)) continue;
                index = i;
                lastTier = false;
                break;
            }
            if (lastTier) index = CampaignController.SelectedCampaign.Tiers.Count - 1;
            return Tiers[index == 0 ? 0 : index];
        }

        public void Save(bool debug = false)
        {
            if (!debug)
            {
                List<CampaignStructure.Campaign> campaigns = CampaignStructure.I.campaigns.ToList();
                foreach (CampaignStructure.Campaign c in campaigns)
                {
                    foreach (CampaignStructure.CampaignTier campaignTier in c.tiers)
                    {
                        foreach (CampaignStructure.CampaignSong campaignSong in campaignTier.songs)
                        {
                            Song s = null;
                            if(Songs.Any(selected => selected.Data.ID == campaignSong.songID))
                            {
                                s = Songs.First(selected => selected.Data.ID == campaignSong.songID);
                            }
                            if(s is null)
                            {
                                MelonLoader.MelonLogger.Warning("Didn't find " + campaignSong.songID + " in " + c.difficulty.ToString() + " campaign");
                                continue;
                            }
                            int songStarCount = campaignSong.starCount;
                            switch (c.difficulty)
                            {
                                case KataConfig.Difficulty.Expert:
                                    s.ExpertStarCount = songStarCount;
                                    break;
                                case KataConfig.Difficulty.Hard:
                                    s.AdvancedStarCount = songStarCount;
                                    break;
                                case KataConfig.Difficulty.Normal:
                                    s.StandardStarCount = songStarCount;
                                    break;
                                case KataConfig.Difficulty.Easy:
                                    s.BeginnerStarCount = songStarCount;
                                    break;
                            }
                        }
                        Tier t = Tiers.First(selected => selected.Index == campaignTier.tierIndex);
                        //t.HasDisplayedStory = campaignTier.hasSeenNarrativeMoment;
                        int tierStarCount = campaignTier.GetStarCount();
                        switch (c.difficulty)
                        {
                            case KataConfig.Difficulty.Expert:
                                t.ExpertStarCount = tierStarCount;
                                break;
                            case KataConfig.Difficulty.Hard:
                                t.AdvancedStarCount = tierStarCount;
                                break;
                            case KataConfig.Difficulty.Normal:
                                t.StandardStarCount = tierStarCount;
                                break;
                            case KataConfig.Difficulty.Easy:
                                t.BeginnerStarCount = tierStarCount;
                                break;
                        }
                    }

                }
            }

            IOHandler.SaveCampaign(this);
        }

        [Serializable]
        public class Tier
        {
            public string Name { get; set; }
            public string StoryText { get; set; }
            public List<Unlock> Unlocks { get; set; } = new List<Unlock>();
            public int RequiredStars { get; set; }
            public int ExpertStarCount { get; set; }
            public int AdvancedStarCount { get; set; }
            public int StandardStarCount { get; set; }
            public int BeginnerStarCount { get; set; }
            public bool HasDisplayedStory { get; set; }
            public int Index { get; set; }
            public List<Song> Songs { get; set; } = new List<Song>();
            public string NonUnlockArena { get; set; } = "";

            public Tier(string name, string storyText, List<Unlock> unlocks, int requiredStars, int expertStarCount, int advancedStarCount, int standardStarCount, int beginnerStarCount, bool hasDisplayedStory, List<Song> songs, int index, string nonUnlockArena = "")
            {
                Name = name;
                StoryText = storyText;
                foreach(Unlock unlock in unlocks)
                {
                    if (unlock.Type != UnlockType.None) Unlocks.Add(unlock);
                }
                Index = index;
                RequiredStars = requiredStars;
                ExpertStarCount = expertStarCount;
                AdvancedStarCount = advancedStarCount;
                StandardStarCount = standardStarCount;
                BeginnerStarCount = beginnerStarCount;
                HasDisplayedStory = hasDisplayedStory;
                Songs = songs;
                NonUnlockArena = nonUnlockArena;
            }

            public bool IsThreshholdMet(KataConfig.Difficulty difficulty)
            {
                switch (difficulty)
                {
                    case KataConfig.Difficulty.Expert:
                        return ExpertStarCount >= RequiredStars;
                    case KataConfig.Difficulty.Hard:
                        return AdvancedStarCount >= RequiredStars;
                    case KataConfig.Difficulty.Normal:
                        return StandardStarCount >= RequiredStars;
                    case KataConfig.Difficulty.Easy:
                        return BeginnerStarCount >= RequiredStars;
                    default:
                        return false;
                }
            }

            public int GetStarCount(KataConfig.Difficulty difficulty)
            {
                switch (difficulty)
                {
                    case KataConfig.Difficulty.Expert:
                        return ExpertStarCount;
                    case KataConfig.Difficulty.Hard:
                        return AdvancedStarCount;
                    case KataConfig.Difficulty.Normal:
                        return StandardStarCount;
                    case KataConfig.Difficulty.Easy:
                        return BeginnerStarCount;
                    default:
                        return 0;
                }
            }
        }

        [Serializable]
        public class Unlock
        {
            public UnlockType Type { get; set; }
            public string Name { get; set; }
            public string CrypticName { get; set; }
            public bool IsUnlockedByDefault { get; set; }
            public bool Unlocked { get; set; }
            public string FileName1 { get; set; } = "";
            public string FileName2 { get; set; } = "";
            public string FileName3 { get; set; } = "";
            [JsonIgnore] public Loc.Token Token { get; set; }

            public Unlock(UnlockType type, string name, string crypticName, bool isUnlockedByDefault, bool unlocked, string fileName1, string fileName2, string fileName3)
            {
                Type = type;
                Name = Encryption.Decipher(crypticName);
                CrypticName = crypticName;
                IsUnlockedByDefault = isUnlockedByDefault;
                Unlocked = IsUnlockedByDefault ? true : unlocked;
                FileName1 = fileName1;
                FileName2 = fileName2;
                FileName3 = fileName3;
            }
        }

        [Serializable]
        public class Song
        {
            public string FileName { get; set; } = "";
            public List<string> Modifiers { get; set; } = new List<string>();
            public int ExpertStarCount { get; set; }
            public int AdvancedStarCount { get; set; }
            public int StandardStarCount { get; set; }
            public int BeginnerStarCount { get; set; }
            public int Index { get; set; }
            public bool AllowAuthorableModifiers { get; set; } = false;
            public bool IsFinaleSong { get; set; } = false;

            [JsonIgnore] public Tier Tier { get; set; }
            [JsonIgnore] public List<GameplayModifiers.Modifier> InternalModifiers { get; set; } = new List<GameplayModifiers.Modifier>();
            public SongData Data { get; set; }

            public Song(string fileName, List<string> modifiers, int expertStarCount, int advancedStarCount, int standardStarCount, int beginnerStarCount, int index, bool allowAuthorableModifiers, SongData data, bool isFinaleSong)
            {
                FileName = fileName;
                Modifiers = modifiers;
                ExpertStarCount = expertStarCount;
                AdvancedStarCount = advancedStarCount;
                StandardStarCount = standardStarCount;
                BeginnerStarCount = beginnerStarCount;
                Index = index;
                AllowAuthorableModifiers = allowAuthorableModifiers;
                IsFinaleSong = isFinaleSong;
                Data = data;
                InitializeGameplayModifiers();
            }

            private void InitializeGameplayModifiers()
            {
                foreach (string s in Modifiers)
                {
                    GameplayModifiers.Modifier modifier;
                    if (Enum.TryParse(s, true, out modifier))
                    {
                        InternalModifiers.Add(modifier);
                    }
                    else
                    {
                        MelonLoader.MelonLogger.Warning($"Wrong value for Modifiers in custom Campaign: didn't recognize {s}");
                    }
                }
            }

            public class SongData
            {
                public string ID { get; set; } = "";
                public bool Expert { get; set; } = false;
                public bool Advanced { get; set; } = false;
                public bool Standard { get; set; } = false;
                public bool Beginner { get; set; } = false;
            }
        }

        public enum UnlockType
        {
            Song,
            Gun,
            Arena,
            None
        }
    }

    
    

}
