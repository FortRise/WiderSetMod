using System;
using System.IO;
using System.Xml;
using FortRise;
using FortRise.Adventure;
using Monocle;
using MonoMod.Utils;

namespace EightPlayerMod
{
    public static class DarkWorldLevelSystemPatch 
    {
        public static void Load() 
        {
            // IL.TowerFall.DarkWorldLevelSystem.GetNextRoundLevel += GetNextRoundLevel_patch;
            On.TowerFall.DarkWorldLevelSystem.GetNextRoundLevel += GetNextRoundLevel_patch;
        }

        public static void Unload() 
        {
            // IL.TowerFall.DarkWorldLevelSystem.GetNextRoundLevel -= GetNextRoundLevel_patch;
            On.TowerFall.DarkWorldLevelSystem.GetNextRoundLevel -= GetNextRoundLevel_patch;
        }

        private static XmlElement GetNextRoundLevel_patch(On.TowerFall.DarkWorldLevelSystem.orig_GetNextRoundLevel orig, TowerFall.DarkWorldLevelSystem self, TowerFall.MatchSettings matchSettings, int roundIndex, out int randomSeed)
        {
            if (!EightPlayerModule.LaunchedEightPlayer && !EightPlayerModule.IsEightPlayer) 
            {
                return orig(self, matchSettings, roundIndex, out randomSeed);
            }

            XmlElement xmlElement;
            try
            {
                if (self.Procedural)
                {
                    matchSettings.RandomLevelSeed = new Random().Next(1000000000);
                }
                int file = self.DarkWorldTowerData[matchSettings.DarkWorldDifficulty][roundIndex + DynamicData.For(self).Get<int>("startLevel")].File;
                randomSeed = file;
                string text = self.DarkWorldTowerData.Levels[file];

                if (self.DarkWorldTowerData is AdventureWorldTowerData)
                {
                    text = text.Replace("Content/Levels/", "Content/WideLevels/");
                    using Stream stream = RiseCore.ResourceTree.TreeMap[text].Stream;
                    if (text.EndsWith("json"))
                    {
                        return Ogmo3ToOel.OgmoToOel(Ogmo3ToOel.LoadOgmo(stream))["level"];
                    }
                    return Calc.LoadXML(stream)["level"];
                }
                text = text.Replace("DarkWorldContent", "Content")
                    .Replace('\\', '/')
                    .Replace("Content/Levels", "Content/WideLevels");
                Logger.Log(text);
                using Stream levelStream = EightPlayerModule.Instance.Content[text].Stream;
                xmlElement = Calc.LoadXML(levelStream)["level"];
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                ErrorHelper.StoreException("Missing Level", ex);
                randomSeed = 0;
                xmlElement = null;
            }
            return xmlElement;
        }
    }
}