using System;
using MonoMod.Cil;

namespace EightPlayerMod 
{
    public static class VersusAwardsPatch
    {
        public static void Load() 
        {
            IL.TowerFall.VersusAwards.GetAwards += GetAwards_patch;
            IL.TowerFall.VersusAwards.AssignAward += AssignAward_patch;
        }

        public static void Unload() 
        {
            IL.TowerFall.VersusAwards.GetAwards -= GetAwards_patch;
            IL.TowerFall.VersusAwards.AssignAward -= AssignAward_patch;
        }

        private static void AssignAward_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(4))) 
            {
                cursor.EmitDelegate<Func<int, int>>(x => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 8;
                    return x;
                });
            }
        }

        private static void GetAwards_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(4))) 
            {
                cursor.EmitDelegate<Func<int, int>>(x => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 8;
                    return x;
                });
            }
        }
    }
}