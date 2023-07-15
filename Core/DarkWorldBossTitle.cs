using System;
using MonoMod.Cil;

namespace EightPlayerMod 
{
    public static class DarkWorldBossTitlePatch 
    {
        public static void Load() 
        {
            IL.TowerFall.DarkWorldBossTitle.ctor += ctor_patch;
        }

        public static void Unload() 
        {
            IL.TowerFall.DarkWorldBossTitle.ctor -= ctor_patch;
        }

        private static void ctor_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(160f))) 
            {
                cursor.EmitDelegate<Func<float, float>>(x => {
                    if (EightPlayerModule.IsEightPlayer) 
                    {
                        return 420 / 2f;
                    }
                    return x;
                });
            }
        }
    }
}