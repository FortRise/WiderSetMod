using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using TowerFall;

namespace EightPlayerMod 
{
    public class KingReaperPatch 
    {
        private static IDetour hook_AppearCoroutineb__0;
        public static void Load() 
        {
            IL.TowerFall.KingReaper.AttackUpdate += PatchMiddle;
            IL.TowerFall.KingReaper.DeadUpdate += PatchMiddle;
            On.TowerFall.KingReaper.ctor += ctor_patch;
            // we do a little bit of hacking to patch that delegate method.
            hook_AppearCoroutineb__0 = new ILHook(
                typeof(KingReaper)
                .GetNestedType("<>c__DisplayClass35_0", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetMethod("<AppearCoroutine>b__0", BindingFlags.Instance | BindingFlags.NonPublic),
                AppearCoroutine_patch
            );
        }

        public static void Unload() 
        {
            IL.TowerFall.KingReaper.AttackUpdate -= PatchMiddle;
            IL.TowerFall.KingReaper.DeadUpdate -= PatchMiddle;
            On.TowerFall.KingReaper.ctor -= ctor_patch;
            hook_AppearCoroutineb__0.Dispose();
        }

        private static void ctor_patch(On.TowerFall.KingReaper.orig_ctor orig, KingReaper self, QuestControl control, Vector2 position, bool fromIntro, bool finalWave, int difficulty)
        {
            var pos = new Vector2(420/2f, position.Y);
            orig(self, control, pos, fromIntro, finalWave, difficulty);
        }

        private static void AppearCoroutine_patch(ILContext ctx) 
        {
            var cursor = new ILCursor(ctx);
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(160f))) 
            {
                cursor.EmitDelegate<Func<float, float>>(width => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 420/2f;
                    return width;
                });
            }
        }


        private static void PatchMiddle(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(160f))) 
            {
                if (cursor.Next.MatchLdcR4(1f))
                    continue;
                cursor.EmitDelegate<Func<float, float>>(width => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 420/2f;
                    return width;
                });
            }
        }
    }
}