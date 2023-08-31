using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using EightPlayerMod;
using FortRise;
using Monocle;

namespace TowerFall
{
    public class FakeVersusTowerData 
    {
        internal static Dictionary<int, FakeVersusTowerData> Chapters = new();
        internal static Dictionary<string, Dictionary<int, FakeVersusTowerData>> CustomChapters = new();
        public List<VersusLevelData> Levels = new();
        public RiseCore.ModResource ResourceSystem;

        public static void Load(int chapter, FortContent content, RiseCore.Resource resource, string gamemodeName) 
        {
            var fakeVersusTowerData = new FakeVersusTowerData();
            foreach (var text in resource.Childrens)
            {
                if (text.ResourceType != typeof(RiseCore.ResourceTypeOel))
                    continue;
                using var fs = text.Stream;
                fakeVersusTowerData.ResourceSystem = content.ResourceSystem;
                fakeVersusTowerData.Levels.Add(CreateVersusLevelData(text.Path, fs));
            }
            if (CustomChapters.TryGetValue(gamemodeName, out var val)) 
            {
                val.Add(chapter, fakeVersusTowerData);
                return;
            }
            CustomChapters.Add(gamemodeName, new Dictionary<int, FakeVersusTowerData>() {{chapter, fakeVersusTowerData }});
        }

        public static void Load(int chapter, string directory) 
        {
            var fakeVersusTowerData = new FakeVersusTowerData();
            foreach (var text in EightPlayerModule.Instance.Content["Content/" + directory].Childrens)
            {
                if (!text.Path.EndsWith("oel"))
                    continue;
                using var fs = text.Stream;
                fakeVersusTowerData.Levels.Add(CreateVersusLevelData(text.Path, fs));
            }
            Chapters.Add(chapter, fakeVersusTowerData);
        }

        public static VersusLevelData CreateVersusLevelData(string textPath, Stream path) 
        {
            VersusLevelData versusLevelData = (VersusLevelData)FormatterServices.GetUninitializedObject(typeof(VersusLevelData));
            versusLevelData.Path = textPath;
            var xmlElement = Calc.LoadXML(path)["level"]["Entities"];
            versusLevelData.PlayerSpawns = xmlElement.GetElementsByTagName("PlayerSpawn").Count;
            versusLevelData.TeamSpawns = Math.Min(xmlElement.GetElementsByTagName("TeamSpawnA").Count, xmlElement.GetElementsByTagName("TeamSpawnB").Count);
            return versusLevelData;
        }

        public List<string> GetLevels(MatchSettings matchSettings)
        {
            List<string> list = new List<string>();
            if (matchSettings.Mode == Modes.LevelTest)
            {
                foreach (VersusLevelData level in Levels)
                {
                    list.Add(level.Path);
                }
                return list;
            }
            if (matchSettings.Mode == Modes.TeamDeathmatch)
            {
                int maxTeamSize = matchSettings.GetMaxTeamSize();
                {
                    foreach (VersusLevelData level2 in Levels)
                    {
                        if (level2.TeamSpawns >= maxTeamSize)
                        {
                            list.Add(level2.Path);
                        }
                    }
                    return list;
                }
            }
            foreach (VersusLevelData level3 in Levels)
            {
                if (level3.PlayerSpawns >= TFGame.PlayerAmount)
                {
                    list.Add(level3.Path);
                }
            }
            return list;
        }
    }

    public static class FakeDarkWorldTowerData
    {
        public static Dictionary<string, string> LevelMap = new();

        public static void Load(string tower, string modDirectory) 
        {
            var directory = Path.Combine("DarkWorldContent", "Levels", "DarkWorld", tower);
            foreach (var text in Directory.EnumerateFiles(directory, "*.oel", SearchOption.TopDirectoryOnly)) 
            {
                var file = Path.GetFileName(text);
                var path = Path.Combine(modDirectory, tower, file);
                LevelMap.Add(text.Replace('\\', '/'), path.Replace('\\', '/'));
            }
        }
    }
}