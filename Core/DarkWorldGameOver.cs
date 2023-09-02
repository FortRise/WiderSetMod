using System;
using System.Reflection;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using TowerFall;

namespace EightPlayerMod;

public static class DarkWorldGameOverPatch 
{
    private static IDetour hook_Sequence;
    private static IDetour hook_Sequenceb__2;

    public static void Load() 
    {
        hook_Sequence = new ILHook(
            typeof(DarkWorldGameOver).GetMethod("Sequence", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetStateMachineTarget(),
            Sequence_patch
        );
        hook_Sequenceb__2 = new ILHook(
            typeof(DarkWorldGameOver).GetNestedType("<>c__DisplayClass6_0", BindingFlags.NonPublic)
                .GetMethod("<Sequence>b__2", BindingFlags.Instance | BindingFlags.NonPublic),
            Sequence_patch
        );
        IL.TowerFall.DarkWorldGameOver.Render += Render_patch;
    }

    public static void Unload() 
    {
        hook_Sequence.Dispose();
        hook_Sequenceb__2.Dispose();
        IL.TowerFall.DarkWorldGameOver.Render -= Render_patch;
    }

    private static void Render_patch(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);

        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(160f))) 
        {
            cursor.EmitDelegate<Func<float, float>>(x => {
                if (EightPlayerModule.IsEightPlayer) 
                {
                    return 420 /2;
                }
                return x;
            });
        }

        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(320f))) 
        {
            cursor.EmitDelegate<Func<float, float>>(x => {
                if (EightPlayerModule.IsEightPlayer) 
                {
                    return 420;
                }
                return x;
            });
        }
    }

    private static void Sequence_patch(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);

        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(160f))) 
        {
            cursor.EmitDelegate<Func<float, float>>(x => {
                if (EightPlayerModule.IsEightPlayer) 
                {
                    return 420 /2;
                }
                return x;
            });
        }

        cursor = new ILCursor(ctx);

        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(480))) 
        {
            cursor.EmitDelegate<Func<float, float>>(x => {
                if (EightPlayerModule.IsEightPlayer) 
                {
                    return 520;
                }
                return x;
            });
        }
    }
}