using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using CustomCampaign.Controller;

namespace CustomCampaign.UI
{
	internal static class CustomCampaignSelectPanel
	{
        #region CustomStory Selection
        private static OptionsMenu primaryMenu;
		static public void SetMenu(OptionsMenu optionsMenu)
		{
			primaryMenu = optionsMenu;
		}

		static public void GoToPanel()
		{
			primaryMenu.ShowPage(OptionsMenu.Page.Customization);
			CleanUpPage(primaryMenu);
			AddButtons(primaryMenu);
			primaryMenu.screenTitle.text = "Campaign Select";

            if (Main.QueueSongListReload)
            {
				GoBack();
            }
		}

		public static void GoBack()
		{
			Main.State = State.None;
			//MenuState.I.GoToMainPage();
			MenuState.SetState(MenuState.State.MainPage);
		}

		private static void SetupText(GameObject textObject)
        {
			var tmp = textObject.transform.GetChild(0).GetComponent<TextMeshPro>();
			tmp.fontSizeMax = 32;
			tmp.fontSizeMin = 8;
		}

		private static void AddRow(OptionsMenu menu, GameObject obj1, GameObject obj2)
        {
			Il2CppSystem.Collections.Generic.List<GameObject> row = new Il2CppSystem.Collections.Generic.List<GameObject>();
			row.Add(obj1);
			row.Add(obj2);
			menu.scrollable.AddRow(row);
		}

		private static void AddButtons(OptionsMenu menu)
		{
			OptionsMenuButton button = null;
			GameObject text = null;

			text = menu.AddTextBlock(0, "Main Campaign");
			SetupText(text);
			button = menu.AddButton(1, "Select", new Action(() => { CampaignController.GoToMainCampaign(); }), null, "Select Main Campaign", menu.buttonPrefab);
			AddRow(menu, text, button.gameObject);

			foreach(Campaign campaign in Main.Campaigns)
            {
				text = menu.AddTextBlock(0, campaign.Name);
				SetupText(text);
				button = menu.AddButton(1, "Select", new Action(() => { CampaignController.GoToCustomCampaign(campaign); }), null, $"Select {campaign.Name}", menu.buttonPrefab);
				AddRow(menu, text, button.gameObject);
            }
		}

		private static void CleanUpPage(OptionsMenu optionsMenu)
		{
			Transform optionsTransform = optionsMenu.transform;
			for (int i = 0; i < optionsTransform.childCount; i++)
			{
				Transform child = optionsTransform.GetChild(i);
				if (child.gameObject.name.Contains("(Clone)"))
				{
					GameObject.Destroy(child.gameObject);
				}
			}
			optionsMenu.mRows.Clear();
			optionsMenu.scrollable.ClearRows();
			optionsMenu.scrollable.mRows.Clear();
		}
		#endregion
		#region Difficulty Selection
		internal static void EnableDifficultyButtons(CampaignSelectButton button)
        {
			Campaign selected = CampaignController.SelectedCampaign;
            switch (button.difficulty)
            {
				case KataConfig.Difficulty.Expert:
					button.gameObject.active = selected.HasExpert;
					break;
				case KataConfig.Difficulty.Hard:
					button.gameObject.active = selected.HasAdvanced;
					break;
				case KataConfig.Difficulty.Normal:
					button.gameObject.active = selected.HasStandard;
					break;
				case KataConfig.Difficulty.Easy:
					button.gameObject.active = selected.HasBeginner;
					break;
			}
		}
		#endregion
    }
}