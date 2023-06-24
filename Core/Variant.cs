using System;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using TowerFall;

namespace EightPlayerMod 
{
    public static class VariantPatch 
    {
        private static IDetour hook_get_AllTrue, hook_get_Players, hook_get_Value, hook_set_Value;
        public static void Load() 
        {
            IL.TowerFall.Variant.Clean += PlayersPatch;
            hook_get_AllTrue = new ILHook(
                typeof(Variant).GetProperty("AllTrue").GetGetMethod(),
                PlayersPatch
            );
            hook_get_Value = new ILHook(
                typeof(Variant).GetProperty("Value").GetGetMethod(),
                PlayersPatch
            );
            hook_get_Players = new ILHook(
                typeof(Variant).GetProperty("Players").GetGetMethod(),
                PlayersPatch
            );
            hook_set_Value = new ILHook(
                typeof(Variant).GetProperty("Value").GetSetMethod(),
                PlayersPatch
            );
        }

        public static void Unload() 
        {
            IL.TowerFall.Variant.Clean -= PlayersPatch;
            hook_get_AllTrue.Dispose();
            hook_get_Value.Dispose();
            hook_get_Players.Dispose();
            hook_set_Value.Dispose();
        }

        private static void PlayersPatch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(4))) 
            {
                cursor.EmitDelegate<Func<int, int>>(x => {
                    if (EightPlayerModule.LaunchedEightPlayer)
                        return 8;
                    return x;
                });
            }
        }
    }
}