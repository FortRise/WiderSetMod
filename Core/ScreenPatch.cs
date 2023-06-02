using System;
using System.Reflection;
using System.Xml;
using FortRise;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
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
        private static IDetour hook_QuestControlStartSequence;
        private static IDetour hook_QuestCompleteSequence;
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
            IL.TowerFall.LevelEntity.Render += MiddlePos_patch;
            IL.TowerFall.QuestPlayerHUD.ctor += QuestPlayerHUD_patch;
            IL.TowerFall.Level.CoreRender += LevelCoreRender_patch;
            IL.TowerFall.Session.EndlessContinue += SwapLevelLoader_patch;
            IL.TowerFall.Session.GotoNextRound += SwapLevelLoader_patch;
            IL.TowerFall.QuestWavesHUD.GetWaveX += MiddlePos_patch;
            hook_orig_StartGame = new ILHook(
                typeof(Session).GetMethod("orig_StartGame"),
                SwapLevelLoader_patch
            );
            hook_QuestControlStartSequence = new ILHook(
                typeof(QuestControl).GetMethod("StartSequence", BindingFlags.Instance | BindingFlags.NonPublic).GetStateMachineTarget(),
                MiddlePos_patch
            );
            hook_QuestCompleteSequence = new ILHook(
                typeof(QuestComplete).GetMethod("Sequence", BindingFlags.Instance | BindingFlags.NonPublic).GetStateMachineTarget(),
                MiddlePos_patch
            );
        }

        public static void Unload() 
        {
            foreach (var methodType in ILTypes) 
            {
                methodType.Unload();
            }
            IL.TowerFall.LevelEntity.Render -= MiddlePos_patch;
            IL.TowerFall.QuestPlayerHUD.ctor -= QuestPlayerHUD_patch;
            IL.TowerFall.Level.CoreRender -= LevelCoreRender_patch;
            IL.TowerFall.Session.EndlessContinue -= SwapLevelLoader_patch;
            IL.TowerFall.Session.GotoNextRound -= SwapLevelLoader_patch;
            IL.TowerFall.QuestWavesHUD.GetWaveX -= MiddlePos_patch;
            hook_orig_StartGame.Dispose();
            hook_QuestControlStartSequence.Dispose();
            hook_QuestCompleteSequence.Dispose();
        }

        private static void LevelCoreRender_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(318f))) 
            {
                cursor.EmitDelegate<Func<int, int>>(width => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 418;
                    return width;
                });
            }
        }

        private static void QuestPlayerHUD_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(312))) 
            {
                cursor.EmitDelegate<Func<int, int>>(width => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 412;
                    return width;
                });
            }

            var sideCursor = new ILCursor(ctx);
            while (sideCursor.TryGotoNext(MoveType.After, instr => instr.MatchLdarg(2))) 
            {
                sideCursor.Emit(OpCodes.Ldarg_3);
                sideCursor.EmitDelegate<Func<Facing, int, Facing>>((facing, index)=> {
                    if (!EightPlayerModule.IsEightPlayer)
                        return facing;

                    if (index == 2)
                        return Facing.Left;
                    return facing;
                });
            }

            var yCursor = new ILCursor(ctx);
            while (yCursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(13f))) 
            {
                yCursor.Emit(OpCodes.Ldarg_3);
                yCursor.EmitDelegate<Func<float, int, float>>((pos, index) => 
                {
                    if (!EightPlayerModule.IsEightPlayer)
                        return pos;
                    if (index is 2 or 3) 
                        return 240 - 10;
                    
                    return pos;
                });
            }
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


        private static void MiddlePos_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(160f))) 
            {
                if (cursor.Next.MatchLdcR4(1))
                    continue;
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