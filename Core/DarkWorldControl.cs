using System;
using System.Reflection;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using TowerFall;

namespace EightPlayerMod 
{
    public static class DarkWorldControlPatch 
    {
        private static ILHook hook_BossIntroSequence;
        private static ILHook hook_LevelIntroSequence;
        public static void Load() 
        {
            hook_BossIntroSequence = new ILHook(
                typeof(DarkWorldControl).GetMethod("BossIntroSequence", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetStateMachineTarget(),
                IntroSequence_patch                
            );
            hook_LevelIntroSequence = new ILHook(
                typeof(DarkWorldControl).GetMethod("LevelIntroSequence", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetStateMachineTarget(),
                IntroSequence_patch                
            );

            IL.TowerFall.DarkWorldControl.ctor += ctor_patch;
            IL.TowerFall.DarkWorldControl.Render += Render_patch;
        }

        public static void Unload() 
        {
            hook_BossIntroSequence.Dispose();
            hook_LevelIntroSequence.Dispose();

            IL.TowerFall.DarkWorldControl.ctor -= ctor_patch;
            IL.TowerFall.DarkWorldControl.Render -= Render_patch;
        }

        private static void ctor_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(160))) 
            {
                cursor.EmitDelegate<Func<float, float>>(width => {
                    if (EightPlayerModule.IsEightPlayer) 
                    {
                        return 420 / 2;
                    }
                    return width;
                });
            }
        }

        private static void Render_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(320))) 
            {
                cursor.EmitDelegate<Func<float, float>>(width => {
                    if (EightPlayerModule.IsEightPlayer) 
                    {
                        return 420;
                    }
                    return width;
                });
            }
        }

        private static void IntroSequence_patch(ILContext ctx) 
        {
            var cursor = new ILCursor(ctx);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(-240))) 
            {
                cursor.EmitDelegate<Func<float, float>>(width => {
                    if (EightPlayerModule.IsEightPlayer) 
                    {
                        return -210 - 80;
                    }
                    return width;
                });
            }

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(240))) 
            {
                cursor.EmitDelegate<Func<float, float>>(width => {
                    if (EightPlayerModule.IsEightPlayer) 
                    {
                        return 210 + 80;
                    }
                    return width;
                });
            }
        }
    }
}