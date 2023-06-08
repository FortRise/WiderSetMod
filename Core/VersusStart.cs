using System;
using System.Reflection;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using TowerFall;

namespace EightPlayerMod
{
    public static class VersusStartPatch 
    {
        private static IDetour hook_IntroSequence;
        public static void Load() 
        {
            IL.TowerFall.VersusStart.ctor += VersusStartctor_patch;
            hook_IntroSequence = new ILHook(
                typeof(VersusStart).GetMethod("IntroSequence", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(),
                IntroSequence_patch
            );
        }

        public static void Unload() 
        {
            IL.TowerFall.VersusStart.ctor -= VersusStartctor_patch;
            hook_IntroSequence.Dispose();
        }

        private static void VersusStartctor_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(160f))) 
            {
                cursor.EmitDelegate<Func<float, float>>(val => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 210;
                    return val;
                });
            }
        }

        private static void IntroSequence_patch(ILContext ctx) 
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(-160f))) 
            {
                cursor.EmitDelegate<Func<float, float>>(val => {
                    if (EightPlayerModule.IsEightPlayer)
                        return -210;
                    return val;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(160f))) 
            {
                if (cursor.Next.MatchLdcR4(1f))
                    continue;
                cursor.EmitDelegate<Func<float, float>>(val => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 210;
                    return val;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(-200f))) 
            {
                cursor.EmitDelegate<Func<float, float>>(val => {
                    if (EightPlayerModule.IsEightPlayer)
                        return -300;
                    return val;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(200f))) 
            {
                cursor.EmitDelegate<Func<float, float>>(val => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 300;
                    return val;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(-60f))) 
            {
                cursor.EmitDelegate<Func<float, float>>(val => {
                    if (EightPlayerModule.IsEightPlayer)
                        return -120;
                    return val;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(60f))) 
            {
                cursor.EmitDelegate<Func<float, float>>(val => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 120;
                    return val;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(-140f))) 
            {
                cursor.EmitDelegate<Func<float, float>>(val => {
                    if (EightPlayerModule.IsEightPlayer)
                        return -240;
                    return val;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(140f))) 
            {
                cursor.EmitDelegate<Func<float, float>>(val => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 240;
                    return val;
                });
            }
        }
    }
}