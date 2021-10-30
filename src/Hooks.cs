using HarmonyLib;
using System;
using System.Reflection;
using MelonLoader;
using CustomCampaign.UI;
using CustomCampaign.Controller;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace CustomCampaign
{
    internal static class Hooks
    {
        #region UI
        private static int buttonCount = 0;
        [HarmonyPatch(typeof(MenuState), "SetState", new Type[] { typeof(MenuState.State) })]
        private static class PatchMenuState
        {
            private static bool Prefix(MenuState __instance, ref MenuState.State state)
            {
                if (state == MenuState.State.CampaignSongSelectPage)
                {
                    if (Main.QueueSongListReload)
                    {
                        state = MenuState.State.MainPage;
                    }
                }
                if (state == MenuState.State.CampaignSelectPage)
                {
                    if(Main.State == State.Custom)
                    {
                        Campaign.Tier tier = CampaignController.SelectedCampaign.GetHighestUnlockedTier(out bool _);
                        if(tier != null)
                        {
                            if(tier.Unlocks.Any(it => it.Type == Campaign.UnlockType.Arena))
                            {
                                Campaign.Unlock unlock = tier.Unlocks.First(it => it.Type == Campaign.UnlockType.Arena);
                                if (!unlock.Unlocked)
                                {
                                    CampaignController.SwitchArena(unlock.Name, true);
                                }
                            }
                            /*else if(tier.NonUnlockArena.Length > 0 && tier.Unlocks.Count == 0)
                            {
                                CampaignController.SwitchArena(tier.NonUnlockArena, false);
                            }*/
                        }
                    }
                }
                if (state == MenuState.State.MainPage)
                {
                    if (Main.State == State.Main || Main.State == State.Custom)
                    {
                        CampaignController.ResetCampaign();
                        Main.State = State.Select;
                        MenuState.SetState(MenuState.State.SettingsPage);
                        return false;
                    }
                    else
                    {
                        CampaignController.SelectedCampaign = null;
                        Main.State = State.None;
                        if (Main.QueueSongListReload)
                        {
                            Main.ReloadSongList();
                        }
                    }
                }
                else if (state == MenuState.State.CampaignSelectPage)
                {
                    if (Main.State == State.None)
                    {
                        Main.State = State.Select;
                        MenuState.SetState(MenuState.State.SettingsPage);
                        return false;
                    }
                    return true;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(OptionsMenu), "ShowPage", new Type[] { typeof(OptionsMenu.Page) })]
        private static class PatchShowOptionsPage
        {
            private static void Prefix(OptionsMenu __instance, OptionsMenu.Page page)
            {
                buttonCount = 0;
            }

            private static void Postfix(OptionsMenu __instance, OptionsMenu.Page page)
            {
                if (page == OptionsMenu.Page.Main)
                {
                    if (Main.State == State.Select)
                    {
                        CustomCampaignSelectPanel.GoToPanel();
                    }
                }

            }
        }

        [HarmonyPatch(typeof(OptionsMenu), "AddButton", new Type[] { typeof(int), typeof(string), typeof(OptionsMenuButton.SelectedActionDelegate), typeof(OptionsMenuButton.IsCheckedDelegate), typeof(string), typeof(OptionsMenuButton), })]
        private static class AddButtonButton
        {
            private static void Postfix(OptionsMenu __instance, int col, string label, OptionsMenuButton.SelectedActionDelegate onSelected, OptionsMenuButton.IsCheckedDelegate isChecked)
            {
                if (__instance.mPage == OptionsMenu.Page.Main)
                {
                    buttonCount++;
                    if (buttonCount == 9)
                    {
                        CustomCampaignSelectPanel.SetMenu(__instance);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(OptionsMenu), "BackOut", new Type[0])]
        private static class Backout
        {
            private static bool Prefix(OptionsMenu __instance)
            {
                switch (Main.State)
                {
                    case State.Select:
                        CustomCampaignSelectPanel.GoBack();
                        return false;
                    default:
                        return true;
                }
            }
        }

        [HarmonyPatch(typeof(CampaignSelectButton), "OnEnable")]
        private static class PatchCampaignSelectPanel
        {
            private static void Postfix(CampaignSelectButton __instance)
            {
                if (Main.State == State.Custom)
                {
                    CustomCampaignSelectPanel.EnableDifficultyButtons(__instance);
                }
            }
        }
#endregion
        #region Progress
        [HarmonyPatch(typeof(CampaignNarrativePanel), "OnEnable")]
        private static class PatchNarrativePanel
        {
            private static void Postfix(CampaignNarrativePanel __instance)
            {
                if(Main.State == State.Custom)
                {
                    CampaignController.SelectedCampaign.SetStorySeen();
                }
            }
        }

        [HarmonyPatch(typeof(CampaignStructure), "UpdateCampaignStars", new Type[] { typeof(int), typeof(KataConfig.Difficulty), typeof(int), typeof(int) })]
        private static class PatchUpdateCampaignStars
        {
            private static void Postfix(CampaignStructure __instance, int stars, KataConfig.Difficulty difficulty, int tierIndex, int songIndex)
            {
                if (Main.State == State.Custom)
                {
                    /*MelonLogger.Msg("Updating campaign stars for" + difficulty);
                    KataConfig.Difficulty diff = CampaignStructure.I.GetCampaignDifficulty();
                    
                    if (diff == KataConfig.Difficulty.Expert)
                    {
                        CampaignStructure.I.GetCampaign(KataConfig.Difficulty.Expert).tiers[tierIndex].songs[songIndex].starCount = stars;
                        if(CampaignController.SelectedCampaign.HasAdvanced) CampaignStructure.I.GetCampaign(KataConfig.Difficulty.Hard).tiers[tierIndex].songs[songIndex].starCount = stars;
                        if (CampaignController.SelectedCampaign.HasStandard) CampaignStructure.I.GetCampaign(KataConfig.Difficulty.Normal).tiers[tierIndex].songs[songIndex].starCount = stars;
                        if (CampaignController.SelectedCampaign.HasBeginner) CampaignStructure.I.GetCampaign(KataConfig.Difficulty.Easy).tiers[tierIndex].songs[songIndex].starCount = stars;
                    }
                    else if(diff == KataConfig.Difficulty.Hard)
                    {
                        CampaignStructure.I.GetCampaign(KataConfig.Difficulty.Hard).tiers[tierIndex].songs[songIndex].starCount = stars;
                        if (CampaignController.SelectedCampaign.HasStandard) CampaignStructure.I.GetCampaign(KataConfig.Difficulty.Normal).tiers[tierIndex].songs[songIndex].starCount = stars;
                        if (CampaignController.SelectedCampaign.HasBeginner) CampaignStructure.I.GetCampaign(KataConfig.Difficulty.Easy).tiers[tierIndex].songs[songIndex].starCount = stars;
                    }
                    else if(diff == KataConfig.Difficulty.Normal)
                    {
                        CampaignStructure.I.GetCampaign(KataConfig.Difficulty.Normal).tiers[tierIndex].songs[songIndex].starCount = stars;
                        if (CampaignController.SelectedCampaign.HasBeginner) CampaignStructure.I.GetCampaign(KataConfig.Difficulty.Easy).tiers[tierIndex].songs[songIndex].starCount = stars;
                    }
                    else
                    {
                        CampaignStructure.I.GetCampaign(KataConfig.Difficulty.Easy).tiers[tierIndex].songs[songIndex].starCount = stars;
                    }
                    //MelonLogger.Msg("Difficulty: " + difficulty);*/
                    //MelonLogger.Msg("Updating campaing stars for " + difficulty);
                    __instance.GetCampaign(difficulty).tiers[tierIndex].songs[songIndex].starCount = stars;
                    //MelonLogger.Msg("Current unlock amount: " + __instance.GetCampaign(difficulty).tiers[tierIndex].unlocks.Length);
                   // MelonLogger.Msg("Current unlock type: " + __instance.GetCampaign(difficulty).tiers[tierIndex].unlocks[0].unlockType);
                    //MelonLogger.Msg("Current unlock name: " + __instance.GetCampaign(difficulty).tiers[tierIndex].unlocks[0].unlockName);
                    //MelonLogger.Msg("Cached: " + __instance.mNewUnlocks[0].unlockName);
                    //return false;
                }
                //return true;
            }
        }
        #endregion
        #region Campaign Saving
        [HarmonyPatch(typeof(CampaignStructure), "SaveCampaignProgress", new Type[] { typeof(bool) })]
        private static class PatchSaveCampaign
        {
            private static bool Prefix()
            {
                if (Main.State == State.Custom)
                {
                    CampaignController.SelectedCampaign.Save();
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(InGameUI), "SetState")]
        private static class PatchIngameSetState
        {
            private static void Postfix(InGameUI.State state)
            {
                if(state == InGameUI.State.ResultsPage)
                {
                    if (Main.State == State.Custom)
                    {
                        CampaignController.SelectedCampaign.Save();
                    }
                }
               
            }
        }
        #endregion
        #region Localization
        
        [HarmonyPatch(typeof(Loc), "Localize", new Type[] {typeof(Loc.Token), typeof(Localizer.KataLanguage), typeof(bool)})]
        private static class PatchLocalize
        {
            private static void Postfix(Loc.Token token, ref string __result)
            {
                if(Main.State == State.Custom)
                {
                    Campaign campaign = CampaignController.SelectedCampaign;
                    switch (token)
                    {
                        case Loc.Token.Campaign_Tier1:
                            __result = campaign.Tiers[0].Name;
                            break;
                        case Loc.Token.Campaign_Tier2:
                            __result = campaign.Tiers[1].Name;
                            break;
                        case Loc.Token.Campaign_Tier3:
                            __result = campaign.Tiers[2].Name;
                            break;
                        case Loc.Token.Campaign_Tier4:
                            __result = campaign.Tiers[3].Name;
                            break;
                        case Loc.Token.Campaign_Tier5:
                            __result = campaign.Tiers[4].Name;
                            break;
                        case Loc.Token.Campaign_Tier1_Description:
                            __result = campaign.Tiers[0].StoryText;
                            break;
                        case Loc.Token.Campaign_Tier2_Description:
                            __result = campaign.Tiers[1].StoryText;
                            break;
                        case Loc.Token.Campaign_Tier3_Description:
                            __result = campaign.Tiers[2].StoryText;
                            break;
                        case Loc.Token.Campaign_Tier4_Description:
                            __result = campaign.Tiers[3].StoryText;
                            break;
                        case Loc.Token.Campaign_Tier5_Description:
                            __result = campaign.Tiers[4].StoryText;
                            break;
                            /*
                        case Loc.Token.Asset_Gun1:
                            __result = campaign.Tiers[0].Unlocks.First(gun => gun.Token == Loc.Token.Asset_Gun1).Name;
                            break;
                        case Loc.Token.Asset_Gun2:
                            __result = campaign.Tiers[1].Unlocks.First(gun => gun.Token == Loc.Token.Asset_Gun2).Name;
                            break;
                        case Loc.Token.Asset_Gun3:
                            __result = campaign.Tiers[2].Unlocks.First(gun => gun.Token == Loc.Token.Asset_Gun3).Name;
                            break;
                        case Loc.Token.Asset_Gun4:
                            __result = campaign.Tiers[3].Unlocks.First(gun => gun.Token == Loc.Token.Asset_Gun4).Name;
                            break;
                        case Loc.Token.Asset_Gun5:
                            __result = campaign.Tiers[4].Unlocks.First(gun => gun.Token == Loc.Token.Asset_Gun5).Name;
                            break;
                        case Loc.Token.Asset_Environment1:
                            __result = campaign.Tiers[0].Unlocks.First(arena => arena.Token == Loc.Token.Asset_Environment1).Name;
                            break;
                        case Loc.Token.Asset_Environment2:
                            __result = campaign.Tiers[1].Unlocks.First(arena => arena.Token == Loc.Token.Asset_Environment2).Name;
                            break;
                        case Loc.Token.Asset_Environment3:
                            __result = campaign.Tiers[2].Unlocks.First(arena => arena.Token == Loc.Token.Asset_Environment3).Name;
                            break;
                        case Loc.Token.Asset_Environment4:
                            __result = campaign.Tiers[3].Unlocks.First(arena => arena.Token == Loc.Token.Asset_Environment4).Name;
                            break;
                        case Loc.Token.Asset_Environment5:
                            __result = campaign.Tiers[4].Unlocks.First(arena => arena.Token == Loc.Token.Asset_Environment5).Name;
                            break;
                        case Loc.Token.CommunityMaps_Title:
                            __result = campaign.Tiers[0].Unlocks.First(song => song.Token == Loc.Token.CommunityMaps_Title).Name;
                            break;
                        case Loc.Token.CompareScores_AddFriendButton:
                            __result = campaign.Tiers[1].Unlocks.First(song => song.Token == Loc.Token.CommunityMaps_Title).Name;
                            break;
                        case Loc.Token.CompareScores_AddFriendSuccess:
                            __result = campaign.Tiers[2].Unlocks.First(song => song.Token == Loc.Token.CommunityMaps_Title).Name;
                            break;
                        case Loc.Token.CompareScores_Delta:
                            __result = campaign.Tiers[3].Unlocks.First(song => song.Token == Loc.Token.CommunityMaps_Title).Name;
                            break;
                        case Loc.Token.CompareScores_Song:
                            __result = campaign.Tiers[4].Unlocks.First(song => song.Token == Loc.Token.CommunityMaps_Title).Name;
                            break;
                            */
                        default:
                            break;

                    }
                }
            }
        }
        #endregion
        #region Songlist Snapping
        [HarmonyPatch(typeof(SongSelect), "OnEnable")]
        private static class PatchGetSnap
        {
            private static void Postfix(SongSelect __instance)
            {
                if(MenuState.sState == MenuState.State.CampaignSongSelectPage)
                {
                    if(Main.State == State.Custom)
                    {
                        //__instance.scroller.SnapTo(CampaignController.SelectedCampaign.ScrollIndex, true);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(SongSelect), "OnSongSelected", new Type[] { typeof(MenuState.State)})]
        private static class PatchSongSelectEnable
        {
            private static void Prefix(SongSelect __instance, MenuState.State destination)
            {
                if(destination == MenuState.State.CampaignLaunchPage)
                {
                    if (Main.State == State.Custom)
                    {
                        CampaignController.SelectedCampaign.ScrollIndex = (int)__instance.scroller.mIndex;
                    }
                }
            }
        }
        #endregion
        #region Unlocks
        #region General
        [HarmonyPatch(typeof(CampaignUnlockPopup), "OnEnable")]
        private static class PatchGetUnlockInfo
        {
            private static bool Prefix(CampaignUnlockPopup __instance)
            {
                if (Main.State == State.Custom)
                {
                    __instance.text.text = UnlockController.PrepareUnlock(__instance);          
                    return false;
                }
                return true;
            }                                              
        }

        [HarmonyPatch(typeof(CampaignUnlockPopup), "OnContinue")]
        private static class PatchOnContinue
        {
            private static void Prefix()
            {
                UnlockController.UnlockItem();
            }
        }

        [HarmonyPatch(typeof(InGameUI), "GoToEndGameContinuePage")]
        private static class PatchGoToEndGameContinuePage
        {
            private static void Postfix()
            {
                if (Main.State == State.Custom)
                {
                    UnlockController.ResetIndex();
                }
            }
        }
        #endregion
        #region Arena
        [HarmonyPatch(typeof(CampaignStructure), "SwitchToTierEnvironment", new Type[] { typeof(CampaignStructure.CampaignTier), typeof(bool)})]
        private static class PatchSwitchEnvironment
        {
            private static bool Prefix(CampaignStructure.CampaignTier tier, bool isTierUnlock)
            {
                if(Main.State == State.Custom)
                {
                    Campaign.Tier _tier = CampaignController.SelectedCampaign.Tiers[tier.tierIndex];
                    for(int i = 0; i < _tier.Unlocks.Count; i++)
                    {
                        if(_tier.Unlocks[i].Type == Campaign.UnlockType.Arena)
                        {
                            Campaign.Unlock unlock = _tier.Unlocks[i];
                            if (!CampaignController.SwitchArena(unlock.Name, false)) return false;
                        }
                    }
                    if (_tier.NonUnlockArena.Length > 0)
                    {
                        CampaignController.SwitchArena(_tier.NonUnlockArena, false);
                    }
                    return false;
                }
                    return true;
            }
        }

        [HarmonyPatch(typeof(OptionsMenu), "AddEnvButton", new Type[] { typeof(int), typeof(string), typeof(string)})]
        private static class PatchAddEnvButton
        {
            private static bool Prefix(OptionsMenu __instance, int col, string envString)
            {
                //bool isLocked = false;
                if(Main.State == State.None)
                {
                    /*foreach(Campaign campaign in Main.campaigns)
                    {
                        foreach(Campaign.Tier tier in campaign.Tiers)
                        {
                            foreach(Campaign.Unlock unlock in tier.Unlocks)
                            {
                                if(unlock.Type == Campaign.UnlockType.Arena)
                                {
                                    if (!unlock.Unlocked && unlock.Name.ToLower() == envString.ToLower()) isLocked = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (isLocked)
                    {
                        __instance.AddLockedEnvButton(col);
                    }*/
                    if (UnlockController.IsArenaLocked(envString))
                    {
                        __instance.AddLockedEnvButton(col);
                        return false;
                    }
                }
                //return !isLocked;
                return true;
            }
        }
        #endregion
        #region Guns
        private static bool checkGunName = false;
        private static int direction = 1;
        private static string identifier = "right gun";
        [HarmonyPatch(typeof(OptionsMenu), "GetGunDisplayName", new Type[] { typeof(string)})]
        private static class PatchGetGunName
        {
            private static bool Prefix(string val)
            {
                bool isUnlocked = UnlockController.IsGunUnlocked(val);
                return isUnlocked;
            }
        }

        [HarmonyPatch(typeof(OptionsMenuSlider), "Increment")]
        private static class PatchIncrement
        {
            private static void Postfix(OptionsMenuSlider __instance)
            {
                UnlockController.NextGun(__instance);
            }
        }

        [HarmonyPatch(typeof(OptionsMenuSlider), "Decrement")]
        private static class PatchDecrement
        {
            private static void Postfix(OptionsMenuSlider __instance)
            {
                UnlockController.PreviousGun(__instance);
            }
        }
        #endregion
        #endregion

    }
}