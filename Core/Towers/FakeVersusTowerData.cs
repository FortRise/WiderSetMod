using System.Collections.Generic;
using System.IO;
using Monocle;

namespace TowerFall 
{
    public class FakeVersusTowerData 
    {
        internal static Dictionary<int, FakeVersusTowerData> Chapters = new();
        public List<VersusLevelData> Levels = new();

        public static void Load(int chapter, string directory) 
        {
            var fakeVersusTowerData = new FakeVersusTowerData();
            foreach (var text in Directory.EnumerateFiles(directory, "*.oel", SearchOption.TopDirectoryOnly)) 
            {
                fakeVersusTowerData.Levels.Add(new VersusLevelData(text));
            }
            Chapters.Add(chapter, fakeVersusTowerData);
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
                LevelMap.Add(text, path);
            }
        }
    }
}