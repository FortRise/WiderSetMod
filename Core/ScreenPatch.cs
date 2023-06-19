using System;
using System.Diagnostics;
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
        private static IDetour hook_QuestControlStartSequenceb__2;
        private static IDetour hook_QuestCompleteSequence;
        private static IDetour hook_QuestCompleteSequenceb__1;
        private static IDetour hook_QuestCompleteSequenceb__5;
        private static IDetour hook_QuestGameOverSequence;
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
            IL.TowerFall.PauseMenu.Render += MiddlePos_patch;
            IL.TowerFall.Level.HandlePausing += MiddlePos_patch;
            IL.TowerFall.Level.ctor += Levelctor;
            IL.TowerFall.MapScene.InitButtons += InitButtons_patch;
            IL.TowerFall.QuestPlayerHUD.ctor += QuestPlayerHUD_patch;
            IL.TowerFall.QuestSpawnPortal.FinishSpawn += MiddlePos_patch;
            IL.TowerFall.Level.CoreRender += LevelCoreRender_patch;
            IL.TowerFall.Session.EndlessContinue += SwapLevelLoader_patch;
            IL.TowerFall.Session.GotoNextRound += SwapLevelLoader_patch;
            IL.TowerFall.QuestWavesHUD.GetWaveX += MiddlePos_patch;
            IL.TowerFall.QuestGauntletCounter.ctor += MiddlePos_patch;
            IL.TowerFall.QuestGameOver.Render += MiddlePosAndScreen_patch;
            IL.TowerFall.MenuButtonGuide.ctor_int_ButtonModes_string += MenuButtonGuide_patch;
            IL.TowerFall.Saver.ctor += Saver_patch;

            On.TowerFall.MainMenu.ctor += MainMenuctor_patch;
            On.TowerFall.MapScene.ctor += MapScenector_patch;
            On.TowerFall.VersusLevelSystem.GenLevels += GenLevels_patch;
            On.TowerFall.QuestLevelSystem.GetNextRoundLevel += GetNextRoundLevel_patch;

            hook_QuestGameOverSequence = new ILHook(
                typeof(QuestGameOver).GetMethod("Sequence", BindingFlags.Instance | BindingFlags.NonPublic).GetStateMachineTarget(),
                MiddlePos_patch
            );

            hook_orig_StartGame = new ILHook(
                typeof(Session).GetMethod("orig_StartGame"),
                SwapLevelLoader_patch
            );
            hook_QuestControlStartSequence = new ILHook(
                typeof(QuestControl).GetMethod("StartSequence", BindingFlags.Instance | BindingFlags.NonPublic).GetStateMachineTarget(),
                MiddlePos_patch
            );
            hook_QuestControlStartSequenceb__2 = new ILHook(
                typeof(QuestControl).GetNestedType(
                    "<>c__DisplayClass17_0", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetMethod("<StartSequence>b__2", BindingFlags.Instance | BindingFlags.NonPublic),
                MiddlePos_patch
            );
            hook_QuestCompleteSequenceb__1 = new ILHook(
                typeof(QuestComplete).GetNestedType(
                    "<>c__DisplayClass8_0", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetMethod("<Sequence>b__1", BindingFlags.Instance | BindingFlags.NonPublic),
                MiddlePos_patch
            );
            hook_QuestCompleteSequenceb__5 = new ILHook(
                typeof(QuestComplete).GetNestedType(
                    "<>c__DisplayClass8_2", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetMethod("<Sequence>b__5", BindingFlags.Instance | BindingFlags.NonPublic),
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
            IL.TowerFall.Level.ctor -= Levelctor;
            IL.TowerFall.PauseMenu.Render -= MiddlePos_patch;
            IL.TowerFall.MapScene.InitButtons -= InitButtons_patch;
            IL.TowerFall.Level.HandlePausing -= MiddlePos_patch;
            IL.TowerFall.QuestPlayerHUD.ctor -= QuestPlayerHUD_patch;
            IL.TowerFall.QuestSpawnPortal.FinishSpawn -= MiddlePos_patch;
            IL.TowerFall.Level.CoreRender -= LevelCoreRender_patch;
            IL.TowerFall.Session.EndlessContinue -= SwapLevelLoader_patch;
            IL.TowerFall.Session.GotoNextRound -= SwapLevelLoader_patch;
            IL.TowerFall.QuestWavesHUD.GetWaveX -= MiddlePos_patch;
            IL.TowerFall.QuestGauntletCounter.ctor -= MiddlePos_patch;
            IL.TowerFall.QuestGameOver.Render -= MiddlePosAndScreen_patch;
            IL.TowerFall.MenuButtonGuide.ctor_int_ButtonModes_string -= MenuButtonGuide_patch;
            IL.TowerFall.Saver.ctor -= Saver_patch;

            On.TowerFall.MainMenu.ctor -= MainMenuctor_patch;
            On.TowerFall.MapScene.ctor -= MapScenector_patch;
            On.TowerFall.VersusLevelSystem.GenLevels -= GenLevels_patch;
            On.TowerFall.QuestLevelSystem.GetNextRoundLevel -= GetNextRoundLevel_patch;


            hook_QuestGameOverSequence.Dispose();
            hook_orig_StartGame.Dispose();
            hook_QuestControlStartSequence.Dispose();
            hook_QuestControlStartSequenceb__2.Dispose();
            hook_QuestCompleteSequenceb__1.Dispose();
            hook_QuestCompleteSequenceb__5.Dispose();
            hook_QuestCompleteSequence.Dispose();
        }

        private static void Saver_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(280f))) 
            {
                cursor.EmitDelegate<Func<float, float>>(width => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 380f;
                    return width;
                });
            }
        }

        private static void MenuButtonGuide_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(310f))) 
            {
                cursor.EmitDelegate<Func<float, float>>(width => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 410;
                    return width;
                });
            }

            cursor = new ILCursor(ctx);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(4))) 
            {
                cursor.EmitDelegate<Func<int, int>>(width => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 8;
                    return width;
                });
            }
        }

        private static void MiddlePosAndScreen_patch(ILContext ctx)
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

            var screencursor = new ILCursor(ctx);
            while (screencursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(320))) 
            {
                if (screencursor.Next.MatchLdcR4(1))
                    continue;
                screencursor.EmitDelegate<Func<float, float>>(width => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 420;
                    return width;
                });
            }
        }

        private static void MainMenuctor_patch(On.TowerFall.MainMenu.orig_ctor orig, MainMenu self, MainMenu.MenuState state)
        {
            orig(self, state);
            if (EightPlayerModule.IsEightPlayer) 
            {
                EightPlayerModule.IsEightPlayer = false;
                Engine.Instance.Screen.Resize(320, 240, 3f);
                WrapMath.AddWidth = new Vector2(320, 0f);
            }
        }

        private static void MapScenector_patch(On.TowerFall.MapScene.orig_ctor orig, MapScene self, MainMenu.RollcallModes mode)
        {
            orig(self, mode);
            if (EightPlayerModule.IsEightPlayer) 
            {
                EightPlayerModule.IsEightPlayer = false;
                Engine.Instance.Screen.Resize(320, 240, 3f);
                WrapMath.AddWidth = new Vector2(320, 0f);
            }
        }

        private static void Levelctor(ILContext ctx)
        {
            var intCursor = new ILCursor(ctx);
            while (intCursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(320))) 
            {
                intCursor.EmitDelegate<Func<int, int>>(width => {
                    if (EightPlayerModule.LaunchedEightPlayer) 
                        return 420;
                    return width;
                });
            }
        }

        private static void InitButtons_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallOrCallvirt("TowerFall.MapScene", "GetButtonX"))) 
            {
                cursor.EmitDelegate<Func<float, float>>(width => {
                    if (EightPlayerModule.IsEightPlayer)
                        return width + 50;
                    return width;
                });
            }
        }

        private static XmlElement GetNextRoundLevel_patch(On.TowerFall.QuestLevelSystem.orig_GetNextRoundLevel orig, QuestLevelSystem self, MatchSettings matchSettings, int roundIndex, out int randomSeed)
        {
            if (!EightPlayerModule.LaunchedEightPlayer)
                return orig(self, matchSettings, roundIndex, out randomSeed);
            
            var id = self.QuestTowerData.ID.X;
            randomSeed = id;
            var path = $"{EightPlayerModule.Instance.Content.GetContentPath(QuestTowers.Towers[id])}";
            return Calc.LoadXML(path)["level"];
        }

        private static void GenLevels_patch(On.TowerFall.VersusLevelSystem.orig_GenLevels orig, VersusLevelSystem self, MatchSettings matchSettings)
        {
            if (!EightPlayerModule.LaunchedEightPlayer) 
            {
                orig(self, matchSettings);
                return;
            }
            var levelSystem = DynamicData.For(self);
            var lastLevel = levelSystem.Get<string>("lastlevel");
            var fake = FakeVersusTowerData.Chapters[self.ID.X];
            var levels = fake.GetLevels(matchSettings);
			if (self.VersusTowerData.FixedFirst && lastLevel == null)
			{
				string text = levels[0];
				levels.RemoveAt(0);
				levels.Shuffle(new Random());
				levels.Insert(0, text);
                levelSystem.Set("levels", levels);
				return;
			}
			levels.Shuffle(new Random());
			if (levels[0] == lastLevel)
			{
				levels.RemoveAt(0);
				levels.Add(lastLevel);
			}
            levelSystem.Set("levels", levels);
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
                    if (EightPlayerModule.LaunchedEightPlayer) 
                    {
                        return new BigLevelLoaderXML(xml.Session);
                    }
                    return xml;
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

            var intcursor = new ILCursor(ctx);
            while (intcursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(160))) 
            {
                intcursor.EmitDelegate<Func<int, int>>(width => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 420 / 2;
                    return width;
                });
            }

            var offCursor = new ILCursor(ctx);
            while (offCursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(480))) 
            {
                offCursor.EmitDelegate<Func<float, float>>(width => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 540f;
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