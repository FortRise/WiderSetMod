using System;
using MonoMod.Cil;

namespace EightPlayerMod 
{
    public static class CoOpDataDisplayPatch 
    {
        public static void Load() 
        {
            IL.TowerFall.CoOpDataDisplay.Render += Render_patch;
        }

        public static void Unload() 
        {
            IL.TowerFall.CoOpDataDisplay.Render -= Render_patch;
        }

        private static void Render_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdstr("FOR 1 OR 2 ARCHERS"))) 
            {
                cursor.EmitDelegate<Func<string, string>>(str => {
                    if (EightPlayerModule.LaunchedEightPlayer) 
                    {
                        return "FOR 1-4 ARCHERS";
                    }
                    return str;
                });
            }

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdstr("FOR 1-4 ARCHERS"))) 
            {
                cursor.EmitDelegate<Func<string, string>>(str => {
                    if (EightPlayerModule.LaunchedEightPlayer) 
                    {
                        return "FOR 1-8 ARCHERS";
                    }
                    return str;
                });
            }
        }
    }
}