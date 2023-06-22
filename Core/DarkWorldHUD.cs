using System;
using MonoMod.Cil;

namespace EightPlayerMod 
{
    public static class DarkWorldHUDPatch 
    {
        public static void Load() 
        {
            IL.TowerFall.DarkWorldHUD.Render += Render_patch;
        }

        public static void Unload() 
        {
            IL.TowerFall.DarkWorldHUD.Render -= Render_patch;
        }

        private static void Render_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(60))) 
            {
                cursor.EmitDelegate<Func<float, float>>(x => {
                    if (EightPlayerModule.IsEightPlayer) 
                    {
                        return 110;
                    }
                    return x;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(62))) 
            {
                cursor.EmitDelegate<Func<float, float>>(x => {
                    if (EightPlayerModule.IsEightPlayer) 
                    {
                        return 112;
                    }
                    return x;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(59))) 
            {
                cursor.EmitDelegate<Func<float, float>>(x => {
                    if (EightPlayerModule.IsEightPlayer) 
                    {
                        return 109;
                    }
                    return x;
                });
            }
        }
    }
}