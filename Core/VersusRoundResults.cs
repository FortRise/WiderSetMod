using System;
using System.Reflection;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using TowerFall;

namespace EightPlayerMod 
{
    public static class VersusRoundResultsPatch 
    {
        private static ILHook hook_Sequence;
        public static void Load() 
        {
            hook_Sequence = new ILHook(
                typeof(VersusRoundResults).GetMethod("Sequence", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetStateMachineTarget(),
                Sequence_patch
            );
        }

        public static void Unload() 
        {
            hook_Sequence.Dispose();
        }

        private static void Sequence_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(160))) 
            {
                cursor.EmitDelegate<Func<int, int>>(width => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 210;
                    return width;
                });
            }

            cursor = new ILCursor(ctx);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(3))) 
            {
                cursor.EmitDelegate<Func<int, int>>(players => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 7;
                    return players;
                });
            }
        }
    }
}