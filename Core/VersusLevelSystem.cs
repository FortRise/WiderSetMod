using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace EightPlayerMod;

public static class VersusLevelSystemPatch 
{
    public static bool Customized;
    public static FortRise.RiseCore.ModResource CurrentResource;
    public static void Load() 
    {
        On.TowerFall.VersusLevelSystem.GenLevels += GenLevels_patch;
        On.TowerFall.VersusLevelSystem.GetNextRoundLevel += GetNextRoundLevel_patch;
    }

    public static void Unload() 
    {
        On.TowerFall.VersusLevelSystem.GetNextRoundLevel -= GetNextRoundLevel_patch;
        On.TowerFall.VersusLevelSystem.GenLevels -= GenLevels_patch;
    }


    private static XmlElement GetNextRoundLevel_patch(On.TowerFall.VersusLevelSystem.orig_GetNextRoundLevel orig, VersusLevelSystem self, MatchSettings matchSettings, int roundIndex, out int randomSeed)
    {
        if (!EightPlayerModule.LaunchedEightPlayer)
        {
            return orig(self, matchSettings, roundIndex, out randomSeed);
        }
        var selfDynamic = DynamicData.For(self);
        var levels = selfDynamic.Get<List<string>>("levels");
        if (levels.Count == 0)
        {
            selfDynamic.Invoke("GenLevels", matchSettings);
        }
        levels = selfDynamic.Get<List<string>>("levels");
        selfDynamic.Set("lastLevel", levels[0]);
        var lastLevel = selfDynamic.Get<string>("lastLevel");
        levels.RemoveAt(0);
        randomSeed = 0;
        foreach (char c in lastLevel)
        {
            randomSeed += (int)c;
        }
        Stream fs = null;
        try 
        {
            if (CurrentResource == null)
                fs = EightPlayerModule.Instance.Content.MapResource[lastLevel].Stream;
            else
                fs = CurrentResource.Resources[lastLevel].Stream;
            return Calc.LoadXML(fs)["level"];
        }
        finally 
        {
            fs.Dispose();
        }
    }


    private static void GenLevels_patch(On.TowerFall.VersusLevelSystem.orig_GenLevels orig, VersusLevelSystem self, MatchSettings matchSettings)
    {
        Customized = false;
        if (!EightPlayerModule.LaunchedEightPlayer) 
        {
            orig(self, matchSettings);
            return;
        }
        var levelSystem = DynamicData.For(self);
        var lastLevel = levelSystem.Get<string>("lastlevel");
        FakeVersusTowerData fake;
        if (FakeVersusTowerData.CustomChapters.TryGetValue(matchSettings.CurrentModeName, out var level)) 
        {
            fake = level[self.ID.X];
            Customized = true;
            CurrentResource = fake.ResourceSystem;
        }
        else 
        {
            fake = FakeVersusTowerData.Chapters[self.ID.X];
            CurrentResource = null;
        }
        var levels = fake.GetLevels(matchSettings);
        if (self.VersusTowerData.FixedFirst && lastLevel == null)
        {
            string text = levels[0];
            levels.RemoveAt(0);
            levels.Shuffle(new Random());
            levels.Insert(0, text);
            levelSystem.Set("levels", levels);
            return;
        }
        levels.Shuffle(new Random());
        if (levels[0] == lastLevel)
        {
            levels.RemoveAt(0);
            levels.Add(lastLevel);
        }
        levelSystem.Set("levels", levels);
    }
}