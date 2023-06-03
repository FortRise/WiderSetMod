using System;
using MonoMod.Cil;

namespace EightPlayerMod
{
    public static class MiasmaPatch 
    {
        public static void Load() 
        {
            IL.TowerFall.Miasma.ctor += Miasmactor_patch;
            IL.TowerFall.Miasma.Update += MiasmaUpdate_patch;
            IL.TowerFall.Miasma.OnPlayerCollide += MiasmaOnPlayerCollide_patch;
            IL.TowerFall.Miasma.Render += MiasmaRender_patch;
        }

        public static void Unload() 
        {
            IL.TowerFall.Miasma.ctor -= Miasmactor_patch;
            IL.TowerFall.Miasma.Update -= MiasmaUpdate_patch;
            IL.TowerFall.Miasma.OnPlayerCollide -= MiasmaOnPlayerCollide_patch;
            IL.TowerFall.Miasma.Render -= MiasmaRender_patch;
        }

        private static void MiasmaRender_patch(ILContext ctx)
        {
            var halfcursor = new ILCursor(ctx);
            if (halfcursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(229))) 
            {
                halfcursor.EmitDelegate<Func<float, float>>(width => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 329;
                    return width;
                });
            }
        }

        private static void MiasmaOnPlayerCollide_patch(ILContext ctx)
        {
            var halfcursor = new ILCursor(ctx);
            if (halfcursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(160f))) 
            {
                halfcursor.EmitDelegate<Func<float, float>>(width => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 420f/2;
                    return width;
                });
            }
        }

        private static void MiasmaUpdate_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(320f))) 
            {
                cursor.EmitDelegate<Func<float, float>>(width => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 420f;
                    return width;
                });
            }
            var scursor = new ILCursor(ctx);
            if (scursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(332f))) 
            {
                scursor.EmitDelegate<Func<float, float>>(width => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 432f;
                    return width;
                });
            }

            var halfcursor = new ILCursor(ctx);
            if (halfcursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(160f))) 
            {
                halfcursor.EmitDelegate<Func<float, float>>(width => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 420f/2;
                    return width;
                });
            }
        }

        private static void Miasmactor_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(320f))) 
            {
                cursor.EmitDelegate<Func<float, float>>(width => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 420f;
                    return width;
                });
            }

            var scursor = new ILCursor(ctx);
            if (scursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(330f))) 
            {
                scursor.EmitDelegate<Func<float, float>>(width => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 430f;
                    return width;
                });
            }
        }
    }
}