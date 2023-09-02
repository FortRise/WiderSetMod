using System;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using TowerFall;

namespace EightPlayerMod;

public static class DarkWorldTowerDataPatch 
{
    public static class LevelDataPatch 
    {
        private static IDetour hook_orig_ctor;
        public static void Load() 
        {
            hook_orig_ctor = new ILHook(
                typeof(DarkWorldTowerData.LevelData).GetMethod("orig_ctor"),
                PlayerAmountUtils.Player4ToPlayer8
            );
        }

        public static void Unload() 
        {
            hook_orig_ctor.Dispose();
        }
    }
}