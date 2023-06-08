using System;
using System.Reflection;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using TowerFall;

namespace EightPlayerMod 
{
    public static class PlayerInputPatch 
    {
        public static void Load() 
        {
            IL.TowerFall.PlayerInput.AssignInputs += AssignInputs_patch;
        }

        public static void Unload() 
        {
            IL.TowerFall.PlayerInput.AssignInputs -= AssignInputs_patch;
        }

        private static void AssignInputs_patch(ILContext ctx)
        {
            var playerCursor = new ILCursor(ctx);
            while (playerCursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(4))) 
            {
                playerCursor.EmitDelegate<Func<int, int>>(_ => {
                    return 8;
                });
            }
            var minCursor = new ILCursor(ctx);
            while (minCursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(7))) 
            {
                minCursor.EmitDelegate<Func<int, int>>(_ => {
                    return 7;
                });
            }
        }
    }

    public static class TFGamePatch 
    {
        private static IDetour hook_orig_Initialize;
        private static IDetour hook_FirstPlayer;

        public static void Load() 
        {
            IL.TowerFall.PlayerInput.AssignInputs += AssignInputs_patch;
            IL.TowerFall.TFGame.CharacterTaken += FourToEight_patch;
            hook_FirstPlayer = new ILHook(
                typeof(TFGame).GetProperty("FirstPlayer").GetGetMethod(),
                FourToEight_patch
            );

            hook_orig_Initialize = new ILHook(
                typeof(TFGame).GetMethod("orig_Initialize", BindingFlags.NonPublic | BindingFlags.Instance),
                orig_Initialize_patch
            );
        }
        

        public static void Unload() 
        {
            IL.TowerFall.PlayerInput.AssignInputs -= AssignInputs_patch;
            IL.TowerFall.TFGame.CharacterTaken -= FourToEight_patch;
            hook_FirstPlayer.Dispose();
            hook_orig_Initialize.Dispose();
        }

        private static void FourToEight_patch(ILContext ctx)
        {
            var playerCursor = new ILCursor(ctx);
            while (playerCursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(4))) 
            {
                playerCursor.EmitDelegate<Func<int, int>>(x => {
                    if (EightPlayerModule.LaunchedEightPlayer || EightPlayerModule.IsEightPlayer)
                        return 8;
                    return x;
                });
            }
        }

        private static void AssignInputs_patch(ILContext ctx)
        {
            var playerCursor = new ILCursor(ctx);
            while (playerCursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(4))) 
            {
                playerCursor.EmitDelegate<Func<int, int>>(_ => {
                    return 8;
                });
            }
            var minCursor = new ILCursor(ctx);
            while (minCursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(3))) 
            {
                minCursor.EmitDelegate<Func<int, int>>(_ => {
                    return 7;
                });
            }
        }

        private static void orig_Initialize_patch(ILContext ctx) 
        {
            var cursor = new ILCursor(ctx);
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(4))) 
            {
                cursor.EmitDelegate<Func<int, int>>(_ => {
                    return 8;
                });
            }
        }
    }
}