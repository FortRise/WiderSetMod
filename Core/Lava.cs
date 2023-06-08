using System;
using MonoMod.Cil;

namespace EightPlayerMod 
{
    public static class OrbLogicPatch 
    {
        public static void Load() 
        {
            IL.TowerFall.OrbLogic.DoSpaceOrb += DoSpaceOrb_patch;
            IL.TowerFall.OrbLogic.DoOffsetWorldVariant += DoOffsetWorldVariant_patch;
            IL.TowerFall.OrbLogic.EndScroll += EndScroll_patch;
        }

        public static void Unload() 
        {
            IL.TowerFall.OrbLogic.DoSpaceOrb -= DoSpaceOrb_patch;
            IL.TowerFall.OrbLogic.DoOffsetWorldVariant -= DoOffsetWorldVariant_patch;
            IL.TowerFall.OrbLogic.EndScroll -= EndScroll_patch;
        }

        private static void EndScroll_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(160))) 
            {
                cursor.EmitDelegate<Func<float, float>>(width => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 210;
                    return width;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(320))) 
            {
                cursor.EmitDelegate<Func<float, float>>(width => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 420;
                    return width;
                });
            }
        }

        private static void DoOffsetWorldVariant_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(160))) 
            {
                cursor.EmitDelegate<Func<float, float>>(width => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 210;
                    return width;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(-160))) 
            {
                cursor.EmitDelegate<Func<float, float>>(width => {
                    if (EightPlayerModule.IsEightPlayer)
                        return -210;
                    return width;
                });
            }
        }

        private static void DoSpaceOrb_patch(ILContext ctx)
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

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(-320))) 
            {
                cursor.EmitDelegate<Func<float, float>>(width => {
                    if (EightPlayerModule.IsEightPlayer)
                        return -420;
                    return width;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(160))) 
            {
                cursor.EmitDelegate<Func<int, int>>(width => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 210;
                    return width;
                });
            }
        }
    }

    public static class LavaPatch 
    {
        public static void Load() 
        {
            IL.TowerFall.Lava.ctor += ctor_patch;
            IL.TowerFall.Lava.DrawLight += DrawLight_patch;
            IL.TowerFall.Lava.Render += Render_patch;
            IL.TowerFall.Lava.Update += Update_patch;
        }

        public static void Unload() 
        {
            IL.TowerFall.Lava.ctor -= ctor_patch;
            IL.TowerFall.Lava.DrawLight -= DrawLight_patch;
            IL.TowerFall.Lava.Render -= Render_patch;
            IL.TowerFall.Lava.Update -= Update_patch;
        }

        private static void Update_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(320))) 
            {
                cursor.EmitDelegate<Func<int, int>>(width => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 420;
                    return width;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(310))) 
            {
                cursor.EmitDelegate<Func<float, float>>(width => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 410;
                    return width;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(305))) 
            {
                cursor.EmitDelegate<Func<float, float>>(width => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 405;
                    return width;
                });
            }
        }

        private static void Render_patch(ILContext ctx)
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

        private static void DrawLight_patch(ILContext ctx)
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

        private static void ctor_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(320))) 
            {
                cursor.EmitDelegate<Func<int, int>>(width => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 420;
                    return width;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(320))) 
            {
                cursor.EmitDelegate<Func<float, float>>(width => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 420;
                    return width;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(370))) 
            {
                cursor.EmitDelegate<Func<float, float>>(width => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 470;
                    return width;
                });
            }
        }
    }
}