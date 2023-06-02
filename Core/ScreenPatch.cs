using System;
using System.Reflection;
using FortRise;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using TowerFall;

namespace EightPlayerMod
{
    public static partial class ScreenPatch 
    {
        private static IDetour hook_orig_StartGame;
        public static void Load() 
        {
            foreach (var methodType in ILTypes) 
            {
#if DEBUG
                if (methodType is MethodILType method) 
                {
                    Logger.Log($"Loading in: {method.Type.Name}.{method.MethodName}");
                }
                else
                    Logger.Log("Loading in: " + methodType.Type.Name);
#endif

                methodType.Load(Screen_patch);
            }
            /* It has some special cases */
            IL.TowerFall.LevelEntity.Render += LevelEntityRender_patch;
            IL.TowerFall.Session.EndlessContinue += SwapLevelLoader_patch;
            IL.TowerFall.Session.GotoNextRound += SwapLevelLoader_patch;
            hook_orig_StartGame = new ILHook(
                typeof(Session).GetMethod("orig_StartGame"),
                SwapLevelLoader_patch
            );
        }


        public static void Unload() 
        {
            foreach (var methodType in ILTypes) 
            {
                methodType.Unload();
            }
            IL.TowerFall.LevelEntity.Render -= LevelEntityRender_patch;
            IL.TowerFall.Session.EndlessContinue -= SwapLevelLoader_patch;
            IL.TowerFall.Session.GotoNextRound -= SwapLevelLoader_patch;
            hook_orig_StartGame.Dispose();
        }

        private static void SwapLevelLoader_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchNewobj("TowerFall.LevelLoaderXML"))) 
            {
                cursor.EmitDelegate<Func<LevelLoaderXML, Scene>>((LevelLoaderXML xml) => {
                    if (EightPlayerModule.IsEightPlayer) 
                    {
                        return new BigLevelLoaderXML(xml.Session);
                    }
                    return xml;
                });
            }
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

        public static MethodInfo GetStateMachineTarget(MethodInfo method, int index) 
        {
            var nested = method.DeclaringType.GetNestedTypes()[index];

            return nested.GetMethod("MoveNext", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}