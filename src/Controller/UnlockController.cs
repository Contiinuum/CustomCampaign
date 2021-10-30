using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;

namespace CustomCampaign.Controller
{
    internal static class UnlockController
    {
        #region General
        private static int unlockIndex = 0;
        private static Campaign.Unlock awaitUnlock = null;
        internal static string PrepareUnlock(CampaignUnlockPopup popup)
        {
            int tier = 0;
            CampaignStructure.CampaignTier[] tiers = CampaignStructure.I.GetCampaign(CampaignStructure.I.mCurrentCampaignDifficulty).tiers;
            tier = CampaignController.SelectedCampaign.GetHighestUnlockedTier().Index - 1;
            if (tier < 0) tier = 0;
            if (tier + 2 == CampaignController.SelectedCampaign.Tiers.Count) tier += 1;
            List<CampaignUnlockPopup.UnlockInfo> unlocks = new List<CampaignUnlockPopup.UnlockInfo>();
            CampaignUnlockPopup.UnlockInfo unlock = popup.GetUnlockInfo(tiers[tier].unlocks[unlockIndex].unlockName);
            var _u = CampaignController.SelectedCampaign.Tiers[tier].Unlocks[unlockIndex];
            unlock.unlockID = _u.Name;
            unlock.token = _u.Token;
            unlocks.Add(unlock);
            popup.unlockInfo = new UnhollowerBaseLib.Il2CppReferenceArray<CampaignUnlockPopup.UnlockInfo>(unlocks.ToArray());
            unlockIndex++;
            awaitUnlock = _u;
            string unlockText = "New ";
            unlockText += _u.Type == Campaign.UnlockType.Arena ? "Arena:" : _u.Type == Campaign.UnlockType.Gun ? "Gun:" : "Song:";
            unlockText += "\n" + _u.Name;
            return unlockText;
        }
        internal static void UnlockItem()
        {
            if (awaitUnlock != null)
            {
                awaitUnlock.Unlocked = true;
                LoadUnlock(awaitUnlock);
                awaitUnlock = null;
            }
        }

        private static void LoadUnlock(Campaign.Unlock unlock)
        {
            switch (unlock.Type)
            {
                case Campaign.UnlockType.Song:
                    IOHandler.MoveUnlock(unlock, CampaignController.SelectedCampaign.FilePath);
                    CampaignController.SelectedCampaign.SongDataInitialized = false;
                    CampaignController.SelectedCampaign.Save();
                    Main.QueueSongListReload = true;
                    MenuState.SetState(MenuState.State.CampaignSongSelectPage);
                    break;
                default:
                    break;
            }
        }

        private static IEnumerator IGoBackToMainMenu()
        {
            yield return new WaitForSeconds(1.5f);
            MenuState.I.GoToMainPage();
            yield return new WaitForSeconds(1.5f);
            MenuState.I.GoToMainPage();
        }

        internal static void ResetIndex()
        {
            if (unlockIndex == 1) unlockIndex = 0;
        }
        #endregion
        #region Arena
        internal static bool IsArenaLocked(string arena)
        {
            bool isLocked = false;
            foreach (Campaign campaign in Main.Campaigns)
            {
                foreach (Campaign.Tier tier in campaign.Tiers)
                {
                    foreach (Campaign.Unlock unlock in tier.Unlocks)
                    {
                        if (unlock.Type == Campaign.UnlockType.Arena)
                        {
                            if (!unlock.Unlocked && unlock.Name.ToLower() == arena.ToLower()) isLocked = true;
                            break;
                        }
                    }
                }
            }
            return isLocked;
        }
        #endregion
        #region Gun
        private static bool checkGunNameLeft = false;
        private static bool checkGunNameRight = false;
        private static int direction = 1;
        private static string identifier = "right gun";

        internal static bool IsGunUnlocked(string gunName)
        {
            if ((!checkGunNameLeft && !checkGunNameRight) || gunName is null) return true;
            if (OptionsMenu.I.mPage == OptionsMenu.Page.Customization)
            {
                foreach (Campaign campaign in Main.Campaigns)
                {
                    foreach (Campaign.Tier tier in campaign.Tiers)
                    {
                        foreach (Campaign.Unlock unlock in tier.Unlocks)
                        {
                            if (unlock.Type != Campaign.UnlockType.Gun) continue;                            
                            if (gunName.ToLower() == unlock.Name.ToLower())
                            {
                                if (!unlock.Unlocked)
                                {
                                    CustomModelLoader.CustomModelType type = identifier.Contains("left") ? CustomModelLoader.CustomModelType.GunLeft : CustomModelLoader.CustomModelType.GunRight;
                                    if(checkGunNameLeft && type == CustomModelLoader.CustomModelType.GunLeft || checkGunNameRight && type == CustomModelLoader.CustomModelType.GunRight)
                                        CustomModelLoader.I.AdjustCustomModelSelection(type, direction);

                                    if (identifier.Contains("left")) checkGunNameLeft = false;
                                    else checkGunNameRight = false;
                                    return false;
                                }
                            }
                        }
                    }
                }
                //if (identifier.Contains("left")) checkGunNameLeft = false;
                //else checkGunNameRight = false;
            }
            return true;
        }

        internal static void NextGun(OptionsMenuSlider slider)
        {
            if (slider is null || slider.identifier is null) return;
            if (!slider.identifier.Contains("gun")) return;
            identifier = slider.identifier;
            checkGunNameLeft = true;
            checkGunNameRight = true;
            direction = 1;
        }

        internal static void PreviousGun(OptionsMenuSlider slider)
        {
            if (slider is null || slider.identifier is null) return;
            if (!slider.identifier.Contains("gun")) return;
            identifier = slider.identifier;
            checkGunNameLeft = true;
            checkGunNameRight = true;
            direction = -1;
        }
        #endregion
    }
}
