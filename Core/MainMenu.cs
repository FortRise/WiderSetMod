using System;
using System.Collections.Generic;
using System.Reflection;
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
    public static class MainMenuPatch 
    {
        private static IDetour hook_orig_Update;
        private static FastReflectionDelegate tweenBGToUICamera;

        public static void Load() 
        {
            tweenBGToUICamera = FastReflectionHelper.CreateFastDelegate(typeof(MainMenu)
                .GetMethod("TweenBGCameraToY", BindingFlags.Instance | BindingFlags.NonPublic));
            On.TowerFall.FightButton.MenuAction += FightButtonMenuAction_patch;
            On.TowerFall.CoOpButton.MenuAction += CoopButtonMenuAction_patch;
            On.TowerFall.MainMenu.CreateMain += CreateMain_patch;
            On.TowerFall.MainMenu.CreateRollcall += CreateRollcall_patch;
            IL.TowerFall.MainMenu.CreateCoOp += CreateCoop_patch;
            IL.TowerFall.ReadyBanner.ctor += ReadyBanner_patch;
            IL.TowerFall.ReadyBanner.Update += ReadyBanner_patch;
            On.TowerFall.RollcallElement.GetPosition += RollcallElementGetPosition_patch;
            On.TowerFall.RollcallElement.GetTweenSource += RollcallElementGetTweenSource_patch;
            On.TowerFall.MainMenu.CreateTeamSelect += CreateTeamSelect_patch;
            hook_orig_Update = new ILHook(
                typeof(MainMenu).GetMethod("orig_Update"),
                MainMenuUpdate_patch
            );
        }

        public static void Unload() 
        {
            On.TowerFall.FightButton.MenuAction -= FightButtonMenuAction_patch;
            On.TowerFall.CoOpButton.MenuAction -= CoopButtonMenuAction_patch;
            On.TowerFall.MainMenu.CreateMain -= CreateMain_patch;
            On.TowerFall.MainMenu.CreateRollcall -= CreateRollcall_patch;
            IL.TowerFall.MainMenu.CreateCoOp -= CreateCoop_patch;
            IL.TowerFall.ReadyBanner.ctor -= ReadyBanner_patch;
            IL.TowerFall.ReadyBanner.Update -= ReadyBanner_patch;
            On.TowerFall.RollcallElement.GetPosition -= RollcallElementGetPosition_patch;
            On.TowerFall.RollcallElement.GetTweenSource -= RollcallElementGetTweenSource_patch;
            On.TowerFall.MainMenu.CreateTeamSelect -= CreateTeamSelect_patch;
            hook_orig_Update.Dispose();
        }

        private static void ReadyBanner_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(4))) 
            {
                cursor.EmitDelegate<Func<int, int>>(x => {
                    if (EightPlayerModule.LaunchedEightPlayer)
                        return 8;
                    return x;
                });
            }
        }

        private static void CreateTeamSelect_patch(On.TowerFall.MainMenu.orig_CreateTeamSelect orig, MainMenu self)
        {
            if (EightPlayerModule.LaunchedEightPlayer) 
            {
                var selfDynamic = DynamicData.For(self);
                var teamBanner = new TeamBanner(new Vector2(64f, 80f), new Vector2(-40f, 50f), "teamA2x");
                var teamBanner2 = new TeamBanner(new Vector2(256f, 80f), new Vector2(360f, 50f), "teamB2x");
                self.Add<TeamBanner>(new TeamBanner[] { teamBanner, teamBanner2 });
                var readyBanner = new ReadyBanner();
                self.Add<ReadyBanner>(readyBanner);
                for (int i = 0; i < 8; i++)
                {
                    if (TFGame.Players[i])
                    {
                        Vector2 vector = new Vector2(160f, (float)(125 + 15 * i));
                        Vector2 vector2 = new Vector2(160f, (float)(115 + 15 * i));
                        if ((~i & 1) != 0) 
                        {
                            vector2.X = -40f;
                        }
                        else 
                        {
                            vector2.X = 360f;
                        }
                        // if (i % 2 == 0)
                        // {
                        //     vector2.X = -40f;
                        // }
                        // else
                        // {
                        //     vector2.X = 360f;
                        // }
                        TeamSelector teamSelector = new TeamSelector(vector, vector2, i);
                        self.Add<TeamSelector>(teamSelector);
                    }
                }
                selfDynamic.Set("ToStartSelected", null);
                self.BackState = MainMenu.MenuState.VersusOptions;
                selfDynamic.Invoke("TweenBGCameraToY", 3);
                return;
            }
            orig(self);
        }

        private static Vector2 RollcallElementGetTweenSource_patch(On.TowerFall.RollcallElement.orig_GetTweenSource orig, int playerIndex)
        {
            if (EightPlayerModule.LaunchedEightPlayer) 
            {
                Vector2 position = RollcallElement.GetPosition(playerIndex);
                if (playerIndex < 4)
                {
                    return position + Vector2.UnitX * -420f;
                }
                return position + Vector2.UnitX * 420f;
            }
            return orig(playerIndex);
        }

        private static Vector2 RollcallElementGetPosition_patch(On.TowerFall.RollcallElement.orig_GetPosition orig, int playerIndex)
        {
            if (EightPlayerModule.LaunchedEightPlayer)
            {
                float num = 10 + playerIndex % 4 * 70;
                float numY = 75;
                if (playerIndex >= 4)
                {
                    numY = 165f;
                }
                return new Vector2(45 + num, numY);
            }
            return orig(playerIndex);
        }

        private static void CreateRollcall_patch(On.TowerFall.MainMenu.orig_CreateRollcall orig, MainMenu self)
        {
            if (EightPlayerModule.LaunchedEightPlayer) 
            {
                if (MainMenu.RollcallMode is MainMenu.RollcallModes.Trials or MainMenu.RollcallModes.Quest)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        TFGame.Players[i] = false;
                    }
                }
                if (MainMenu.RollcallMode != MainMenu.RollcallModes.Trials)
                {
                    ReadyBanner readyBanner = new ReadyBanner();
                    self.Add<ReadyBanner>(readyBanner);
                }
                for (int j = 0; j < 8; j++)
                {
                    RollcallElement rollcallElement = new RollcallElement(j);
                    self.Add<RollcallElement>(rollcallElement);
                }
                self.BackState = MainMenu.MenuState.Rollcall;
                DynamicData.For(self).Set("ToStartSelected", null);
                if (MainMenu.RollcallMode == MainMenu.RollcallModes.Versus || MainMenu.RollcallMode == MainMenu.RollcallModes.Trials)
                {
                    tweenBGToUICamera(self, 1);
                }
                else
                {
                    tweenBGToUICamera(self, 2);
                }
                for (int k = 0; k < 8; k++)
                {
                    TFGame.CoOpCrowns[k] = false;
                }
                return;
            }
            orig(self);
        }

        private static void CreateCoop_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(3))) 
            {
                cursor.EmitDelegate<Func<MainMenu.MenuState, MainMenu.MenuState>>(state => 
                {
                    return MainMenu.MenuState.None;
                });
            }
        }

        private static void CreateMain_patch(On.TowerFall.MainMenu.orig_CreateMain orig, MainMenu self)
        {
            orig(self);
            EightPlayerModule.CanCoopLevelSet = false;
            EightPlayerModule.CanVersusLevelSet = false;
            EightPlayerModule.LaunchedEightPlayer = false;
            if (EightPlayerModule.StandardTeams == null)
                EightPlayerModule.StandardTeams = MainMenu.VersusMatchSettings.Teams;
            MainMenu.VersusMatchSettings.Teams = EightPlayerModule.StandardTeams;
        }

        private static void CoopButtonMenuAction_patch(On.TowerFall.CoOpButton.orig_MenuAction orig, CoOpButton self)
        {
            orig(self);
            self.MainMenu.State = MainMenu.MenuState.None;
            EightPlayerModule.CanCoopLevelSet = true;
        }

        private static void MainMenuUpdate_patch(ILContext ctx) 
        {
            var cursor = new ILCursor(ctx);
            // Destroy
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallOrCallvirt("TowerFall.MainMenu", "CallStateFunc"))) 
            {
                // cursor.Emit(OpCodes.Ldarg_0);
                // cursor.EmitDelegate<Action<MainMenu>>(menu => {
                //     if (menu.OldState == MainMenu.MenuState.Rollcall) 
                //     {
                //         menu.State = MainMenu.MenuState.None;
                //     }
                // });
            }
            // Create
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallOrCallvirt("TowerFall.MainMenu", "CallStateFunc"))) 
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Action<MainMenu>>(menu => {
                    if (menu.State == MainMenu.MenuState.None && (EightPlayerModule.CanVersusLevelSet || EightPlayerModule.CanCoopLevelSet)) 
                    {
                        LevelSetType setType = LevelSetType.Versus;
                        if (EightPlayerModule.CanCoopLevelSet)
                        {
                            setType = LevelSetType.Coop;
                        }
                        var list = new List<MenuItem>();
                        var standardLevelSet = new StandardSetButton(new Vector2(160f - 60, 90f), new Vector2(-160f, 120f), setType);
                        list.Add(standardLevelSet);
                        var eightPlayerSet = new EightPlayerSetButton(new Vector2(160f + 60, 90f), new Vector2(560f, 120f), setType);
                        list.Add(eightPlayerSet);

                        menu.Add(list);
                        standardLevelSet.RightItem = eightPlayerSet;
                        eightPlayerSet.LeftItem = standardLevelSet;
                        menu.BackState = MainMenu.MenuState.Main;
                        DynamicData.For(menu).Set("ToStartSelected", standardLevelSet);
                        tweenBGToUICamera(menu, 1);
                        menu.Add(new LevelSetDisplay(standardLevelSet, eightPlayerSet));
                    }
                });
            }
        }

        private static void FightButtonMenuAction_patch(On.TowerFall.FightButton.orig_MenuAction orig, TowerFall.FightButton self)
        {
            orig(self);
            MainMenu.CurrentMatchSettings = MainMenu.VersusMatchSettings;
            MainMenu.RollcallMode = MainMenu.RollcallModes.Versus;
            self.MainMenu.State = MainMenu.MenuState.None;
            EightPlayerModule.CanVersusLevelSet = true;
        }
    }

    public enum LevelSetType { Versus, Coop }

    public class StandardSetButton : MainModeButton
    {
        private Sprite<int> tower;
        private LevelSetType levelSetType;
        public override bool Rotate => false;

        public StandardSetButton(Vector2 position, Vector2 tweenFrom, LevelSetType levelSetType) : base(
            position, tweenFrom, "STANDARD", EightPlayerModule.CanCoopLevelSet ? "1-4 ARCHERS" : "2-4 ARCHERS")
        {
            this.levelSetType = levelSetType;
            tower = EightPlayerModule.Instance.EightPlayerSpriteData.GetSpriteInt("StandardGameMode");
            tower.Play(0);
            tower.CenterOrigin();
            Add(tower);
        }

        public override float BaseScale => 1f;

        public override float ImageScale { get => tower.Scale.X; set => tower.Scale = Vector2.One * value; }
        public override float ImageRotation { get => tower.Rotation; set => tower.Rotation = value; }
        public override float ImageY { get => tower.Y; set => tower.Y = value; }

        protected override void OnSelect()
        {
            tower.Play(1);
            base.OnSelect();
        }

        protected override void OnDeselect()
        {
            tower.Play(0);
            base.OnDeselect();
        }

        protected override void OnConfirm()
        {
            base.OnConfirm();
            tower.Play(1);
            EightPlayerModule.LaunchedEightPlayer = false;
            MainMenu.VersusMatchSettings.Teams = EightPlayerModule.StandardTeams;
        }

        protected override void MenuAction()
        {
            tower.Play(1);
            if (levelSetType == LevelSetType.Coop) 
            {
                MainMenu.State = MainMenu.MenuState.CoOp;
                MainMenu.BackState = MainMenu.MenuState.Main;
                return;
            }
            MainMenu.RollcallMode = MainMenu.RollcallModes.Versus;
            MainMenu.State = MainMenu.MenuState.Rollcall;
            MainMenu.BackState = MainMenu.MenuState.Main;
        }
    }

    public class EightPlayerSetButton : MainModeButton
    {
        private Sprite<int> tower;
        private LevelSetType levelSetType;
        public override bool Rotate => false;

        public EightPlayerSetButton(Vector2 position, Vector2 tweenFrom, LevelSetType levelSetType) : base(
            position, tweenFrom, "WIDER", EightPlayerModule.CanCoopLevelSet ? "1-8 ARCHERS" : "2-8 ARCHERS")
        {
            this.levelSetType = levelSetType;
            tower = EightPlayerModule.Instance.EightPlayerSpriteData.GetSpriteInt("WideGameMode");
            tower.Play(0);
            tower.CenterOrigin();
            Add(tower);
        }

        public override float BaseScale => 1f;

        public override float ImageScale { get => tower.Scale.X; set => tower.Scale = Vector2.One * value; }
        public override float ImageRotation { get => tower.Rotation; set => tower.Rotation = value; }
        public override float ImageY { get => tower.Y; set => tower.Y = value; }

        protected override void OnSelect()
        {
            tower.Play(1);
            base.OnSelect();
        }

        protected override void OnDeselect()
        {
            tower.Play(0);
            base.OnDeselect();
        }

        protected override void OnConfirm()
        {
            base.OnConfirm();
            tower.Play(1);
            EightPlayerModule.LaunchedEightPlayer = true;
            MainMenu.VersusMatchSettings.Teams = EightPlayerModule.EightPlayerTeams;
        }

        protected override void MenuAction()
        {
            if (levelSetType == LevelSetType.Coop) 
            {
                MainMenu.State = MainMenu.MenuState.CoOp;
                MainMenu.BackState = MainMenu.MenuState.Main;
                return;
            }
            MainMenu.RollcallMode = MainMenu.RollcallModes.Versus;
            MainMenu.State = MainMenu.MenuState.Rollcall;
            MainMenu.BackState = MainMenu.MenuState.Main;
        }
    }
}