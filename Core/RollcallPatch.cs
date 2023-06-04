using System;
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
    public static class RollcallPatch 
    {
        private static IDetour hook_RollcallElementMaxPlayers;
        private static FastReflectionDelegate ChangeSelectionLeft;
        private static FastReflectionDelegate ChangeSelectionRight;
        private static FastReflectionDelegate ForceStart;
        public static void Load() 
        {
            ChangeSelectionLeft = FastReflectionHelper.CreateFastDelegate(typeof(RollcallElement)
                .GetMethod("ChangeSelectionLeft", BindingFlags.NonPublic | BindingFlags.Instance));
            ChangeSelectionRight = FastReflectionHelper.CreateFastDelegate(typeof(RollcallElement)
                .GetMethod("ChangeSelectionRight", BindingFlags.NonPublic | BindingFlags.Instance));
            ForceStart = FastReflectionHelper.CreateFastDelegate(typeof(RollcallElement)
                .GetMethod("ForceStart", BindingFlags.NonPublic | BindingFlags.Instance));
            On.TowerFall.MainMenu.CreateRollcall += CreateRollcall_patch;
            On.TowerFall.MainMenu.DestroyRollcall += DestroyRollcall_patch;
            hook_RollcallElementMaxPlayers = new ILHook(
                typeof(RollcallElement).GetProperty("MaxPlayers", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true),
                RollcallElementMaxPlayers_patch
            );
            IL.TowerFall.RollcallElement.ctor += RollcallElementctor_patch;
            IL.TowerFall.RollcallElement.HandleControlIcons += OffsetControlIcons;
            IL.TowerFall.RollcallElement.Update += OffsetControlIcons;
            IL.TowerFall.RollcallElement.Render += Render_patch;
            // Move it to other class I guess?
            IL.TowerFall.QuestRoundLogic.OnLevelLoadFinish += OnLevelLoadFinish_patch;
        }

        public static void Unload() 
        {
            On.TowerFall.MainMenu.CreateRollcall -= CreateRollcall_patch;
            On.TowerFall.MainMenu.DestroyRollcall -= DestroyRollcall_patch;
            hook_RollcallElementMaxPlayers.Dispose();

            IL.TowerFall.RollcallElement.ctor -= RollcallElementctor_patch;
            IL.TowerFall.RollcallElement.HandleControlIcons -= OffsetControlIcons;
            IL.TowerFall.RollcallElement.Update -= OffsetControlIcons;
            IL.TowerFall.RollcallElement.Render -= Render_patch;
            IL.TowerFall.QuestRoundLogic.OnLevelLoadFinish -= OnLevelLoadFinish_patch;
        }

        private static void Render_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Action<RollcallElement>>(self => {
                if (EightPlayerModule.LaunchedEightPlayer) 
                {
                    var playerIndex = DynamicData.For(self).Get<int>("playerIndex");
                    Draw.OutlineTextCentered(
                        TFGame.Font, 
                        $"P{playerIndex + 1}",
                        self.Position + new Vector2(15, -40), ArcherData.GetColorA(playerIndex), 1.2f);
                }
            });
        }

        private static void OffsetControlIcons(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdsfld<RollcallElement>("ControlIconPos"))) 
            {
                cursor.EmitDelegate<Func<Vector2, Vector2>>(pos => {
                    if (EightPlayerModule.LaunchedEightPlayer)
                        return new Vector2(-15, -30f);
                    return pos;
                });
            }
        }

        private static void RollcallElementctor_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(60))) 
            {
                cursor.EmitDelegate<Func<float, float>>(y => {
                    if (EightPlayerModule.LaunchedEightPlayer)
                        return 30;
                    return y;
                });
            }

            cursor = new ILCursor(ctx);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchNewobj<Func<int>>())) 
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<Func<int>, RollcallElement, Func<int>>>((func, self) => {
                    return () => NotJoinedUpdate(self);
                });
            }
        }

        private static int NotJoinedUpdate(RollcallElement self) 
        {
            var selfDynamic = DynamicData.For(self);
            var input = selfDynamic.Get<PlayerInput>("input");
            if (input == null)
            {
                return 0;
            }
            var CanChangeSelection = selfDynamic.Get<bool>("CanChangeSelection");
            var playerIndex = selfDynamic.Get<int>("playerIndex");
            var archerType = selfDynamic.Get<ArcherData.ArcherTypes>("archerType");
            var arrowWiggle = selfDynamic.Get<Wiggler>("arrowWiggle");
            var altWiggle = selfDynamic.Get<Wiggler>("altWiggle");
            var darkWorldLockEase = selfDynamic.Get<float>("darkWorldLockEase");
            var shakeTimer = selfDynamic.Get<float>("shakeTimer");
            var portrait = selfDynamic.Get<ArcherPortrait>("portrait");
            var MaxPlayers = selfDynamic.Get<int>("MaxPlayers");

            if (input.MenuBack && !self.MainMenu.Transitioning)
            {
                for (int i = 0; i < 8; i++)
                {
                    TFGame.Players[i] = false;
                }
                Sounds.ui_clickBack.Play(160f, 1f);
                if (MainMenu.RollcallMode == MainMenu.RollcallModes.Versus)
                {
                    self.MainMenu.State = MainMenu.MenuState.None;
                }
                else if (MainMenu.RollcallMode == MainMenu.RollcallModes.Trials) 
                {
                    self.MainMenu.State = MainMenu.MenuState.Main;
                }
                else
                {
                    self.MainMenu.State = MainMenu.MenuState.CoOp;
                }
            }
            else if (input.MenuLeft && CanChangeSelection)
            {
                selfDynamic.Set("drawDarkWorldLock", false);
                ChangeSelectionLeft(self);
                Sounds.ui_move2.Play(160f, 1f);
                arrowWiggle.Start();
                selfDynamic.Set("rightArrowWiggle", false);
            }
            else if (input.MenuRight && CanChangeSelection)
            {
                selfDynamic.Set("drawDarkWorldLock", false);
                ChangeSelectionRight(self);
                Sounds.ui_move2.Play(160f, 1f);
                arrowWiggle.Start();
                selfDynamic.Set("rightArrowWiggle", true);
            }
            else if (input.MenuAlt && GameData.DarkWorldDLC)
            {
                selfDynamic.Set("drawDarkWorldLock", false);
                altWiggle.Start();
                Sounds.ui_altCostumeShift.Play(self.X, 1f);
                if (archerType == ArcherData.ArcherTypes.Normal)
                {
                    selfDynamic.Set("archerType", ArcherData.ArcherTypes.Alt);
                }
                else
                {
                    selfDynamic.Set("archerType", ArcherData.ArcherTypes.Normal);
                }
                portrait.SetCharacter(self.CharacterIndex, archerType, 1);
            }
            else if (input.MenuConfirmOrStart && !TFGame.CharacterTaken(self.CharacterIndex) && TFGame.PlayerAmount < MaxPlayers)
            {
                if (ArcherData.Get(self.CharacterIndex, archerType).RequiresDarkWorldDLC && !GameData.DarkWorldDLC)
                {
                    selfDynamic.Set("drawDarkWorldLock", true);
                    if (darkWorldLockEase < 1f || !TFGame.OpenStoreDarkWorldDLC())
                    {
                        portrait.Shake();
                        shakeTimer = 30f;
                        Sounds.ui_invalid.Play(self.X, 1f);
                        if (TFGame.PlayerInputs[playerIndex] != null)
                        {
                            TFGame.PlayerInputs[playerIndex].Rumble(1f, 20);
                        }
                    }
                    return 0;
                }
                if (input.MenuAlt2Check && archerType == ArcherData.ArcherTypes.Normal && 
                    ArcherData.SecretArchers[self.CharacterIndex] != null)
                {
                    selfDynamic.Set("archerType", ArcherData.ArcherTypes.Secret);
                    portrait.SetCharacter(self.CharacterIndex, archerType, 1);
                }
                portrait.Join(false);
                TFGame.Players[playerIndex] = true;
                TFGame.AltSelect[playerIndex] = archerType;
                if (TFGame.PlayerInputs[playerIndex] != null)
                {
                    TFGame.PlayerInputs[playerIndex].Rumble(1f, 20);
                }
                shakeTimer = 20f;
                if (TFGame.PlayerAmount == MaxPlayers)
                {
                    ForceStart(self);
                }
                return 1;
            }
            return 0;
        }

        private static void OnLevelLoadFinish_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(2))) 
            {
                cursor.EmitDelegate<Func<int, int>>(maxPlayer => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 4;
                    return 2;
                });
            }
        }

        public static void RollcallElementMaxPlayers_patch(ILContext ctx) 
        {
            var cursor = new ILCursor(ctx);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(2))) 
            {
                cursor.EmitDelegate<Func<int, int>>(maxPlayer => {
                    if (EightPlayerModule.LaunchedEightPlayer)
                        return 4;
                    return maxPlayer;
                });
            }

            cursor = new ILCursor(ctx);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(4))) 
            {
                cursor.EmitDelegate<Func<int, int>>(maxPlayer => {
                    if (EightPlayerModule.LaunchedEightPlayer)
                        return 8;
                    return maxPlayer;
                });
            }
        }

        private static void DestroyRollcall_patch(On.TowerFall.MainMenu.orig_DestroyRollcall orig, MainMenu self)
        {
            orig(self);
        }

        private static void CreateRollcall_patch(On.TowerFall.MainMenu.orig_CreateRollcall orig, MainMenu self)
        {
            orig(self);
        }
    }
}


