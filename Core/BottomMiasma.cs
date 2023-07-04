using System;
using MonoMod.Cil;

namespace EightPlayerMod 
{
    public static class BottomMiasmaPatch 
    {
        public static void Load() 
        {
            IL.TowerFall.BottomMiasma.ctor += ctor_patch;
        }

        public static void Unload() 
        {
            IL.TowerFall.BottomMiasma.ctor -= ctor_patch;
        }

        private static void ctor_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(320))) 
            {
                cursor.EmitDelegate<Func<float, float>>(width => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 420;
                    return width;
                });
            }
        }
    }
}