using System;
using System.Collections;
using System.Collections.Generic;
using Monocle;
using MonoMod.Utils;
using TowerFall;

namespace CursedMode;

public class CursedModule 
{
    public static void Load() 
    {
        IllusionerSkeleton.LoadEntity();
        FakeSkeleton.LoadEntity();
        LiteralLaserSkeleton.LoadEntity();
        QuestSpawnPortalPatch.Load();
    }

    public static void Unload() 
    {
        IllusionerSkeleton.UnloadEntity();
        FakeSkeleton.UnloadEntity();
        LiteralLaserSkeleton.UnloadEntity();
        QuestSpawnPortalPatch.Unload();
    }
}

public static class QuestSpawnPortalPatch 
{
    public static void Load() 
    {
        On.TowerFall.QuestSpawnPortal.Appear += Appear_patch;
        On.TowerFall.QuestSpawnPortal.Update += Update_patch;
    }

    public static void Unload() 
    {
        On.TowerFall.QuestSpawnPortal.Appear -= Appear_patch;
        On.TowerFall.QuestSpawnPortal.Update -= Update_patch;
    }

    private static void Update_patch(On.TowerFall.QuestSpawnPortal.orig_Update orig, TowerFall.QuestSpawnPortal self)
    {
        var selfDynamic = DynamicData.For(self);
        if (selfDynamic.TryGet<Counter>("skullCounter", out var value)) 
        {
            var isSummonedCursed = selfDynamic.Get<bool>("isSummonedCursed");
            if (!value && !isSummonedCursed) 
            {
                var deathSkull = new DeathSkull(self.Position, DeathSkull.Types.Cursed);
                self.Level.Add(deathSkull);
                selfDynamic.Set("isSummonedCursed", true);
            }
        }
        orig(self);
    }

    private static bool Appear_patch(On.TowerFall.QuestSpawnPortal.orig_Appear orig, TowerFall.QuestSpawnPortal self)
    {
        var counter = new Counter(40);
        self.Add(counter);
        var toSpawn = DynamicData.For(self).Get<Queue<string>>("toSpawn");
        if (toSpawn.Count != 0) 
        {
            var peek = toSpawn.Peek();
            if (peek.StartsWith("CursedMode/")) 
            {
                DynamicData.For(self).Set("skullCounter", counter);
                DynamicData.For(self).Set("isSummonedCursed", false);
            }
        }

        return orig(self);
    }
}