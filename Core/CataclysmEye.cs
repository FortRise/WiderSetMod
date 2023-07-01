using System;
using System.Reflection;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using TowerFall;

namespace EightPlayerMod 
{
    public static class CataclysmEyePatch 
    {
        private static IDetour hook_IdleSafeCoroutine;
        private static IDetour hook_ExplodingCoroutine;
        private static IDetour hook_DeadCoroutine;
        private static IDetour hook_DeadCoroutine91_0b__1;
        public static void Load() 
        {
            IL.TowerFall.CataclysmEye.ctor += MiddlePatch;
            IL.TowerFall.CataclysmEye.IdleFreakoutEnter += IdleFreakoutEnter_patch;
            IL.TowerFall.CataclysmEye.DoSlam += MiddlePatch;
            IL.TowerFall.CataclysmBlade.Update += CataclysmBladeUpdate_patch;
            IL.TowerFall.CataclysmBullet.Update += CataclysmBulletUpdate_patch;
            hook_IdleSafeCoroutine = new ILHook(
                typeof(CataclysmEye).GetMethod("IdleSafeCoroutine", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetStateMachineTarget(),
                MiddlePatchSafe
            );
            hook_ExplodingCoroutine = new ILHook(
                typeof(CataclysmEye).GetMethod("ExplodingCoroutine", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetStateMachineTarget(),
                MiddlePatchSafe
            );
            hook_DeadCoroutine = new ILHook(
                typeof(CataclysmEye).GetMethod("DeadCoroutine", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetStateMachineTarget(),
                MiddlePatchSafe
            );
            hook_DeadCoroutine91_0b__1 = new ILHook(
                typeof(CataclysmEye).GetNestedType("<>c__DisplayClass91_0", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetMethod("<DeadCoroutine>b__1", BindingFlags.Instance | BindingFlags.NonPublic),
                MiddlePatch
            );
        }

        public static void Unload() 
        {
            IL.TowerFall.CataclysmEye.ctor -= MiddlePatch;
            IL.TowerFall.CataclysmEye.IdleFreakoutEnter -= IdleFreakoutEnter_patch;
            IL.TowerFall.CataclysmEye.DoSlam -= MiddlePatch;
            IL.TowerFall.CataclysmBlade.Update -= CataclysmBladeUpdate_patch;
            IL.TowerFall.CataclysmBullet.Update -= CataclysmBulletUpdate_patch;
            hook_IdleSafeCoroutine.Dispose();
            hook_ExplodingCoroutine.Dispose();
            hook_DeadCoroutine.Dispose();
            hook_DeadCoroutine91_0b__1.Dispose();
        }

        private static void CataclysmBulletUpdate_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(330f))) 
            {
                cursor.EmitDelegate<Func<float, float>>(x => {
                    if (EightPlayerModule.IsEightPlayer) 
                    {
                        return 420 + 10;
                    }
                    return x;
                });
            }
        }

        private static void CataclysmBladeUpdate_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(295f))) 
            {
                cursor.EmitDelegate<Func<float, float>>(x => {
                    if (EightPlayerModule.IsEightPlayer) 
                    {
                        return 420 - 25;
                    }
                    return x;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(25f))) 
            {
                cursor.EmitDelegate<Func<float, float>>(x => {
                    if (EightPlayerModule.IsEightPlayer) 
                    {
                        return 50;
                    }
                    return x;
                });
            }
        }

        private static void IdleFreakoutEnter_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(160f))) 
            {
                cursor.EmitDelegate<Func<float, float>>(x => {
                    if (EightPlayerModule.IsEightPlayer) 
                    {
                        return 420 / 2;
                    }
                    return x;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(190f))) 
            {
                cursor.EmitDelegate<Func<float, float>>(x => {
                    if (EightPlayerModule.IsEightPlayer) 
                    {
                        return 240;
                    }
                    return x;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(130f))) 
            {
                cursor.EmitDelegate<Func<float, float>>(x => {
                    if (EightPlayerModule.IsEightPlayer) 
                    {
                        return 180;
                    }
                    return x;
                });
            }
        }

        private static void MiddlePatchSafe(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(160f))) 
            {
                if (cursor.Next.MatchLdcR4(1))  
                    continue;
                cursor.EmitDelegate<Func<float, float>>(x => {
                    if (EightPlayerModule.IsEightPlayer) 
                    {
                        return 420 / 2;
                    }
                    return x;
                });
            }
        }

        private static void MiddlePatch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(160f))) 
            {
                cursor.EmitDelegate<Func<float, float>>(x => {
                    if (EightPlayerModule.IsEightPlayer) 
                    {
                        return 420 / 2;
                    }
                    return x;
                });
            }
        }
    }
}