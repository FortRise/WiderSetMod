using System;
using System.Reflection;
using System.Xml;
using FortRise;
using Microsoft.Xna.Framework;
using Moments.Encoder;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using TowerFall;

namespace EightPlayerMod 
{
    public static partial class ScreenPatch 
    {
        private static IDetour hook_LevelLoaderXML;
        

        public static void Load() 
        {
            foreach (var methodType in ILTypes) 
            {
                methodType.Load(Screen_patch);
            }
            /* It has some special cases */
            IL.TowerFall.LevelEntity.Render += LevelEntityRender_patch;
            hook_LevelLoaderXML = new ILHook(
                typeof(LevelLoaderXML).GetNestedTypes(BindingFlags.NonPublic)[0]
                    .GetMethod("MoveNext", BindingFlags.NonPublic | BindingFlags.Instance),
                LevelLoaderXML_patch
            );
        }

        private static void LevelLoaderXML_patch(ILContext ctx) 
        {
            var cursor = new ILCursor(ctx);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(32))) 
            {
                cursor.EmitDelegate<Func<float, float>>(width => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 42;
                    return width;
                });
            }
        }

        public static void Unload() 
        {
            foreach (var methodType in ILTypes) 
            {
                methodType.Unload();
            }
            IL.TowerFall.LevelEntity.Render -= LevelEntityRender_patch;
            hook_LevelLoaderXML.Dispose();
        }


        private static void LevelEntityRender_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(160f))) 
            {
                cursor.EmitDelegate<Func<float, float>>(width => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 420 / 2;
                    return width;
                });
            }
        }

        private static void Screen_patch(ILContext ctx) 
        {
            var intCursor = new ILCursor(ctx);
            while (intCursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(320))) 
            {
                intCursor.EmitDelegate<Func<int, int>>(OnApplyPatchInt);
            }
            var floatCursor = new ILCursor(ctx);
            while (floatCursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(320f))) 
            {
                floatCursor.EmitDelegate<Func<float, float>>(OnApplyPatchFloat);
            }
        }

        private static float OnApplyPatchFloat(float width) 
        {
            if (EightPlayerModule.IsEightPlayer)
                return 420f;

            return width;
        }

        private static int OnApplyPatchInt(int width) 
        {
            if (EightPlayerModule.IsEightPlayer)
                return 420;

            return width;
        }
    }
}