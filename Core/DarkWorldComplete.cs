using System;
using System.Reflection;
using FortRise;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using TowerFall;

namespace EightPlayerMod;

public static class DarkWorldCompletePatch 
{
    private static ILHook hook_Sequence;
    private static MethodInfo SequenceEnumeratorInfo;

    public static void Load() 
    {
        SequenceEnumeratorInfo = typeof(DarkWorldComplete).GetMethod("Sequence", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetStateMachineTarget();
        hook_Sequence = new ILHook(
            SequenceEnumeratorInfo,
            Sequence_patch
        );
        IL.TowerFall.DarkWorldComplete.Update += ScreenUtils.PatchHalfWidthFloat;
    }

    public static void Unload() 
    {
        hook_Sequence.Dispose();
        IL.TowerFall.DarkWorldComplete.Update -= ScreenUtils.PatchHalfWidthFloat;
    }

    private static void Sequence_patch(ILContext ctx) 
    {
        var from = SequenceEnumeratorInfo.DeclaringType.GetField("<from>5__6", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? SequenceEnumeratorInfo.DeclaringType.GetField("<from>5__1e");

        var to = SequenceEnumeratorInfo.DeclaringType.GetField("<to>5__7", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? SequenceEnumeratorInfo.DeclaringType.GetField("<to>5__1f");

        var Vector2 = typeof(Vector2);
        var Vector2ctor = Vector2.GetConstructor(new Type[2] { typeof(float), typeof(float) });
        var throwHolder = typeof(DarkWorldCompletePatch).GetMethod("ThrowHolder");
        var cursor = new ILCursor(ctx);


        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(160))) 
        {
            if (cursor.Next.MatchLdcR4(1f))
                continue;
            cursor.EmitDelegate<Func<float, float>>(x => {
                if (EightPlayerModule.IsEightPlayer) 
                {
                    return 420 / 2;
                }
                return x;
            });
        }

        cursor = new ILCursor(ctx);

        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(4))) 
        {
            cursor.EmitDelegate<Func<int, int>>(x => {
                if (EightPlayerModule.IsEightPlayer) 
                {
                    return 8;
                }
                return x;
            });
        }


        var cctx = new CursorCtx(Vector2ctor, Vector2, cursor);

        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(4))) 
        {
            cursor.GotoNext(MoveType.Before, instr => instr.MatchThrow());
            cursor.Next.OpCode = OpCodes.Call;
            cursor.Next.Operand = throwHolder;
            cursor.GotoNext();
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldc_I4_8);
            cursor.Emit(OpCodes.Newarr, Vector2);
            EmitVector2(cctx, 110f, 360f, 0);
            EmitVector2(cctx, 210f, 360f, 1);
            EmitVector2(cctx, 310f, 360f, 2);
            EmitVector2(cctx, 410f, 360f, 3);
            EmitVector2(cctx, 110f, -60f, 4);
            EmitVector2(cctx, 210f, -60f, 5);
            EmitVector2(cctx, 310f, -60f, 6);
            EmitVector2(cctx, 410f, -60f, 7);
            cursor.Emit(OpCodes.Stfld, from);

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldc_I4_8);
            cursor.Emit(OpCodes.Newarr, Vector2);
            EmitVector2(cctx, 90f, 220f, 0);
            EmitVector2(cctx, 170f, 220f, 1);
            EmitVector2(cctx, 250f, 220f, 2);
            EmitVector2(cctx, 330f, 220f, 3);
            EmitVector2(cctx, 90f, 60f, 4);
            EmitVector2(cctx, 170f, 60f, 5);
            EmitVector2(cctx, 250f, 60f, 6);
            EmitVector2(cctx, 330f, 60f, 7);
            cursor.Emit(OpCodes.Stfld, to);
        }
    }

    //Can you hold the exception for me? so it won't throw
    public static void ThrowHolder(Exception ex) {}


    private static void EmitVector2(in CursorCtx ctx, float x, float y, int num) 
    {
        ctx.Cursor.Emit(OpCodes.Dup);
        ctx.Cursor.Emit(OpCodes.Ldc_I4, num);
        ctx.Cursor.Emit(OpCodes.Ldc_R4, x);
        ctx.Cursor.Emit(OpCodes.Ldc_R4, y);
        ctx.Cursor.Emit(OpCodes.Newobj, ctx.Info);
        ctx.Cursor.Emit(OpCodes.Stelem_Any, ctx.Type);
    }


    readonly struct CursorCtx 
    {
        public readonly ConstructorInfo Info;
        public readonly Type Type;
        public readonly ILCursor Cursor;

        public CursorCtx(ConstructorInfo info, Type type, ILCursor cursor) 
        {
            Info = info;
            Cursor = cursor;
            Type = type;
        }
    }
}