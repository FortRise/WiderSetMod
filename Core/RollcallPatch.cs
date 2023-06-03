using System;
using System.Reflection;
using FortRise;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using TowerFall;

namespace EightPlayerMod
{
    public static class RollcallPatch 
    {
        private static IDetour hook_RollcallElementMaxPlayers;
        public static void Load() 
        {
            On.TowerFall.MainMenu.CreateRollcall += CreateRollcall_patch;
            On.TowerFall.MainMenu.DestroyRollcall += DestroyRollcall_patch;
            hook_RollcallElementMaxPlayers = new ILHook(
                typeof(RollcallElement).GetProperty("MaxPlayers", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true),
                RollcallElementMaxPlayers_patch
            );

            // Move it to other class I guess?
            IL.TowerFall.QuestRoundLogic.OnLevelLoadFinish += OnLevelLoadFinish_patch;
        }

        public static void Unload() 
        {
            On.TowerFall.MainMenu.CreateRollcall -= CreateRollcall_patch;
            On.TowerFall.MainMenu.DestroyRollcall -= DestroyRollcall_patch;
            hook_RollcallElementMaxPlayers.Dispose();

            IL.TowerFall.QuestRoundLogic.OnLevelLoadFinish -= OnLevelLoadFinish_patch;
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

    public static class Commands 
    {
        [Command("widescreen")]
        public static void TurnOnWidescreen(string[] args) 
        {
            EightPlayerModule.IsEightPlayer = !EightPlayerModule.IsEightPlayer;
            if (EightPlayerModule.IsEightPlayer) 
            {
                Engine.Instance.Screen.Resize(420, 240, 3f);
                WrapMath.AddWidth = new Vector2(420, 0f);
            }
            else 
            {
                Engine.Instance.Screen.Resize(320, 240, 3f);
                WrapMath.AddWidth = new Vector2(320, 0f);
            }
        }
    }
}


