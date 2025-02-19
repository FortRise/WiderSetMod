using System;
using System.Reflection;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using TowerFall;

namespace EightPlayerMod 
{
    public static class AmaranthBossPatch 
    {
        private static ILHook hook_MoveSequence;
        private static ILHook hook_DeadSequence45_2b__3;

        public static void Load() 
        {
            IL.TowerFall.AmaranthBoss.ctor += ScreenUtils.PatchHalfWidthFloat;
            hook_MoveSequence = new ILHook(
                typeof(AmaranthBoss).GetMethod("MoveSequence", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetStateMachineTarget(),
                MoveSequence_patch
            );
            hook_DeadSequence45_2b__3 = new ILHook(
                typeof(AmaranthBoss).GetNestedType("<>c__DisplayClass45_2", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.GetMethod("<DeadCoroutine>b__3", BindingFlags.NonPublic | BindingFlags.Instance) ??
                typeof(AmaranthBoss).GetNestedType("<>c__DisplayClass19", BindingFlags.NonPublic | BindingFlags.Instance)
                    .FindMethod("<DeadCoroutine>b__10"),
                ScreenUtils.PatchHalfWidthFloat
            );
        }

        public static void Unload() 
        {
            IL.TowerFall.AmaranthBoss.ctor -= ScreenUtils.PatchHalfWidthFloat;
            hook_MoveSequence.Dispose();
            hook_DeadSequence45_2b__3.Dispose();
        }

        private static void MoveSequence_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(160f))) 
            {
                cursor.EmitDelegate<Func<float, float>>(x => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 420 / 2;
                    return x;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(130f))) 
            {
                cursor.EmitDelegate<Func<float, float>>(x => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 360 / 2;
                    return x;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(190f))) 
            {
                cursor.EmitDelegate<Func<float, float>>(x => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 480 / 2;
                    return x;
                });
            }
        }
    }
}