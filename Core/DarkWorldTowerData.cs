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
                orig_ctor_patch
            );
        }

        public static void Unload() 
        {
            hook_orig_ctor.Dispose();
        }

        private static void orig_ctor_patch(ILContext ctx) 
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(4))) 
            {
                cursor.EmitDelegate<Func<int, int>>(x => {
                    return 8;
                });
            }
        }
    }
}