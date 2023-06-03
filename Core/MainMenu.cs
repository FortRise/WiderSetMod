using System;
using System.Collections.Generic;
using System.Reflection;
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
            IL.TowerFall.MainMenu.CreateCoOp += CreateCoop_patch;
            IL.TowerFall.MainMenu.CreateRollcall += CreateRollcall_patch;
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
            IL.TowerFall.MainMenu.CreateCoOp -= CreateCoop_patch;
            IL.TowerFall.MainMenu.CreateRollcall -= CreateRollcall_patch;
            hook_orig_Update.Dispose();
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

        private static void CreateRollcall_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(5))) 
            {
                cursor.EmitDelegate<Func<MainMenu.MenuState, MainMenu.MenuState>>(state => {
                    return MainMenu.MenuState.None;
                });
            }
        }

        private static void CreateMain_patch(On.TowerFall.MainMenu.orig_CreateMain orig, MainMenu self)
        {
            orig(self);
            EightPlayerModule.CanCoopLevelSet = false;
            EightPlayerModule.CanVersusLevelSet = false;
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
                        var menuDynamic = DynamicData.For(menu);
                        var list = new List<MenuItem>();
                        var standardLevelSet = new StandardSetButton(new Vector2(160f - 80, 240/2f), new Vector2(-160f, 240/2f), setType);
                        list.Add(standardLevelSet);
                        var eightPlayerSet = new EightPlayerSet(new Vector2(160 + 80, 240/2f), new Vector2(560f, 240/2f), setType);
                        list.Add(eightPlayerSet);

                        menu.Add(list);
                        standardLevelSet.RightItem = eightPlayerSet;
                        eightPlayerSet.LeftItem = standardLevelSet;
                        menu.BackState = MainMenu.MenuState.Main;
                        menuDynamic.Set("ToStartSelected", standardLevelSet);
                        tweenBGToUICamera(menu, 1);
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
        private Image image;
        private LevelSetType levelSetType;
        public override bool Rotate => false;

        public StandardSetButton(Vector2 position, Vector2 tweenFrom, LevelSetType levelSetType) : base(position, tweenFrom, "STANDARD", "2-4 ARCHERS")
        {
            this.levelSetType = levelSetType;
            image = new Image(EightPlayerModule.Instance.EightPlayerAtlas["levelset/levelset"]);
            image.CenterOrigin();
            Add(image);
        }

        public override float BaseScale => 1f;

        public override float ImageScale { get => image.Scale.X; set => image.Scale = Vector2.One * value; }
        public override float ImageRotation { get => image.Rotation; set => image.Rotation = value; }
        public override float ImageY { get => image.Y; set => image.Y = value; }

        protected override void MenuAction()
        {
            if (levelSetType == LevelSetType.Coop) 
            {
                MainMenu.State = MainMenu.MenuState.CoOp;
                MainMenu.BackState = MainMenu.MenuState.Main;
                EightPlayerModule.LaunchedEightPlayer = false;
                return;
            }
            MainMenu.RollcallMode = MainMenu.RollcallModes.Versus;
            MainMenu.State = MainMenu.MenuState.Rollcall;
            MainMenu.BackState = MainMenu.MenuState.Main;
            EightPlayerModule.LaunchedEightPlayer = false;
        }
    }

    public class EightPlayerSet : MainModeButton
    {
        private Image image;
        private LevelSetType levelSetType;
        public override bool Rotate => false;

        public EightPlayerSet(Vector2 position, Vector2 tweenFrom, LevelSetType levelSetType) : base(position, tweenFrom, "WIDER", "2-8 ARCHERS")
        {
            this.levelSetType = levelSetType;
            image = new Image(EightPlayerModule.Instance.EightPlayerAtlas["levelset/biglevelset"]);
            image.CenterOrigin();
            Add(image);
        }

        public override float BaseScale => 1f;

        public override float ImageScale { get => image.Scale.X; set => image.Scale = Vector2.One * value; }
        public override float ImageRotation { get => image.Rotation; set => image.Rotation = value; }
        public override float ImageY { get => image.Y; set => image.Y = value; }

        protected override void MenuAction()
        {
            if (levelSetType == LevelSetType.Coop) 
            {
                MainMenu.State = MainMenu.MenuState.CoOp;
                MainMenu.BackState = MainMenu.MenuState.Main;
                EightPlayerModule.LaunchedEightPlayer = true;
                return;
            }
            MainMenu.RollcallMode = MainMenu.RollcallModes.Versus;
            MainMenu.State = MainMenu.MenuState.Rollcall;
            MainMenu.BackState = MainMenu.MenuState.Main;
            EightPlayerModule.LaunchedEightPlayer = true;
        }
    }
}