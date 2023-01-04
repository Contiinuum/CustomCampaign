using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using CustomCampaign.Controller;
using System.IO.Compression;
using MelonLoader;

namespace CustomCampaign
{
    internal static class IOHandler
    {
        private static string customCampaignFolder = Path.Combine(Application.dataPath.Replace("Audica_Data", "Mods"), "CustomCampaigns");

        internal static List<Campaign> LoadCampaigns()
        {
            if (!Directory.Exists(customCampaignFolder))
            {
                Directory.CreateDirectory(customCampaignFolder);
                return null;
            }
            List<Campaign> loadedCampaigns = new List<Campaign>();

            string[] directories = Directory.GetDirectories(customCampaignFolder);
            bool needArenaReload = false;
            for(int i = 0; i < directories.Length; i++)
            {
                string[] files = Directory.GetFiles(directories[i]);
                for(int j = 0; j < files.Length; j++)
                {
                    string ext = Path.GetExtension(files[j]);
                    if (ext == ".campaign")
                    {
                        Campaign campaign = LoadCampaign(files[j], directories[i], out bool needReload);
                        if (campaign is null) continue;
                        needArenaReload |= needReload;
                        loadedCampaigns.Add(campaign);
                    }                  
                }
            }
            
            if(needArenaReload)
                Integrations.ReloadArenas();
            
            return loadedCampaigns;
        }


        internal static void SaveCampaign(Campaign campaign)
        {
            string json = JsonConvert.SerializeObject(campaign, Formatting.Indented);
            string filename = $"{campaign.Name}-{campaign.Creator}.campaign";
            filename = filename.Replace(" ", "");
            filename = filename.Replace(":", "");
            File.WriteAllText(Path.Combine(campaign.FilePath, filename), json);
        }

        private static Campaign LoadCampaign(string campaignFile, string filepath, out bool needArenaReload)
        {
            needArenaReload = false;
            
            try
            {
                Campaign campaign = null;
                using (StreamReader reader = new StreamReader(campaignFile))
                {
                    string json = reader.ReadToEnd();
                    campaign = JsonConvert.DeserializeObject<Campaign>(json);
                    campaign.FilePath = filepath;
                }
                foreach (Campaign.Tier tier in campaign.Tiers)
                {
                    foreach (Campaign.Unlock unlock in tier.Unlocks)
                    {
                        if (unlock.Unlocked || unlock.Type == Campaign.UnlockType.None || unlock.Type == Campaign.UnlockType.Song) continue;
                        if (unlock.Type == Campaign.UnlockType.Arena) needArenaReload = true;
                        MoveUnlock(unlock, campaign.FilePath);
                    }
                }
                return campaign;
            }
            catch(Exception e)
            {
                MelonLogger.Warning($"Encountered an error while loading {Path.GetFileName(campaignFile)} - please check if the file has valid json. Error: {e.Message}");
                return null;
            }

        }

        internal static void MoveSong(string campaignPath, string songFileName)
        {
            MoveFile(campaignPath, Application.dataPath.Replace("Audica_Data", "Downloads"), songFileName, songFileName);
        }

        internal static void MoveUnlock(Campaign.Unlock unlock, string basePath)
        {
            switch (unlock.Type)
            {
                case Campaign.UnlockType.Song:
                    MoveFile(basePath, Path.Combine(Application.dataPath, "StreamingAssets", "HmxAudioAssets", "songs"), unlock.FileName1, unlock.Name + ".audica");
                    break;
                case Campaign.UnlockType.Arena:
                    MoveFile(basePath, Path.Combine(Application.dataPath.Replace("Audica_Data", "Mods"), "Arenas"), unlock.FileName1, unlock.Name + ".arena");
                    break;
                case Campaign.UnlockType.Gun:
                    MoveFile(basePath, Path.Combine(Application.dataPath.Replace("Audica_Data", "customization"), "gun"), unlock.FileName1, Encryption.Decipher(unlock.FileName1));
                    MoveFile(basePath, Path.Combine(Application.dataPath.Replace("Audica_Data", "customization"), "gun"), unlock.FileName2, Encryption.Decipher(unlock.FileName2));
                    MoveFile(basePath, Path.Combine(Application.dataPath.Replace("Audica_Data", "customization"), "gun"), unlock.FileName3, Encryption.Decipher(unlock.FileName3));
                    break;
            }
        }

        internal static void RenameItem(Campaign.Unlock unlock)
        {
            string basePath;
            switch (unlock.Type)
            {
                case Campaign.UnlockType.Arena:
                    basePath = Path.Combine(Application.dataPath.Replace("Audica_Data", "Mods"), "Arenas");
                    MoveFile(basePath, unlock.CrypticName, unlock.Name);
                    break;
                case Campaign.UnlockType.Gun:
                    basePath = Path.Combine(Application.dataPath.Replace("Audica_Data", "customization"), "gun");
                    RenameGuns(basePath, unlock.CrypticName, unlock.Name);
                    break;
                case Campaign.UnlockType.Song:
                    basePath = customCampaignFolder;
                    string target = Path.Combine(Application.dataPath.Replace("Audica_Data", "Downloads"));
                    MoveFile(basePath, target, unlock.CrypticName, unlock.Name);
                    break;
                default:
                    return;
            }
        }

        private static void MoveFile(string basePath, string crypticName, string realName)
        {
            MoveFile(basePath, basePath, crypticName, realName);
        }

        private static void MoveFile(string basePath, string targetPath, string crypticName, string realName)
        {
            try
            {

                string origin = Path.Combine(basePath, crypticName);
                if (File.Exists(origin))
                {
                    string target = Path.Combine(targetPath, realName);
                    if (!File.Exists(target))
                    {
                        File.Move(origin, target);
                    }
                }
            }
            catch(Exception ex)
            {
                MelonLogger.Warning("Couldn't move file " + realName + ": " + ex.Message);
            }
            
        }

        private static void RenameGuns(string basePath, string crypticName, string realName)
        {
            string mtlCrypt = Path.Combine(basePath, crypticName + ".mtl");
            string mtlReal = Path.Combine(basePath, realName + ".mtl");
            string objCrypt = Path.Combine(basePath, crypticName + ".obj");
            string objReal = Path.Combine(basePath, realName + ".obj");
            string txtCrypt = Path.Combine(basePath, crypticName + ".txt");
            string txtReal = Path.Combine(basePath, realName + ".txt");
            MoveFile(basePath, mtlCrypt, mtlReal);
            MoveFile(basePath, objCrypt, objReal);
            MoveFile(basePath, txtCrypt, txtReal);
        }
    }
}
