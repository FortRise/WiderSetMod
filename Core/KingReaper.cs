using System;
using System.Reflection;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using TowerFall;

namespace EightPlayerMod 
{
    public class KingReaperPatch 
    {
        private static IDetour hook_AppearCoroutine;
        public static void Load() 
        {
            IL.TowerFall.KingReaper.AttackUpdate += PatchMiddle;
            IL.TowerFall.KingReaper.DeadUpdate += PatchMiddle;
            IL.TowerFall.KingReaper.ThrowBombB += PatchMiddle;
            IL.TowerFall.QuestControl.SpawnGroup += PatchMiddle;
            hook_AppearCoroutine = new ILHook(
                typeof(KingReaper).GetMethod("AppearCoroutine", BindingFlags.Instance | BindingFlags.NonPublic).GetStateMachineTarget(),
                PatchMiddle
            );
        }

        public static void Unload() 
        {
            IL.TowerFall.KingReaper.AttackUpdate -= PatchMiddle;
            IL.TowerFall.KingReaper.DeadUpdate -= PatchMiddle;
            IL.TowerFall.KingReaper.ThrowBombB -= PatchMiddle;
            IL.TowerFall.QuestControl.SpawnGroup -= PatchMiddle;
            hook_AppearCoroutine.Dispose();
        }

        private static void PatchMiddle(ILContext ctx)
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
    }
}