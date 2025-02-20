using System;
using MonoMod.Cil;

namespace EightPlayerMod;

public static class ScreenUtils 
{
    public static void PatchWidthInt(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);

        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(320))) 
        {
            cursor.EmitDelegate<Func<float, float>>(width => {
                if (EightPlayerModule.IsEightPlayer)
                    return 420;
                return width;
            });
        }
    }

    public static void PatchHalfWidthInt(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);

        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(160))) 
        {
            cursor.EmitDelegate<Func<float, float>>(x => {
                if (EightPlayerModule.IsEightPlayer)
                    return 420 / 2;
                return x;
            });
        }
    }

    public static void PatchWidthFloat(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);

        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(320))) 
        {
            cursor.EmitDelegate<Func<float, float>>(width => {
                if (EightPlayerModule.IsEightPlayer)
                    return 420;
                return width;
            });
        }
    }

    public static void PatchHalfWidthFloat(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);

        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(160f))) 
        {
            if (cursor.Next.MatchLdcR4(1f))
                continue;
            cursor.EmitDelegate<Func<float, float>>(x => {
                if (EightPlayerModule.IsEightPlayer)
                    return 420 / 2;
                return x;
            });
        }
    }

    public static void PatchWidth(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(320))) 
        {
            cursor.EmitDelegate<Func<int, int>>((width) => {
                if (EightPlayerModule.IsEightPlayer)
                {
                    return 420;
                }

                return width;
            });
        }

        cursor.Goto(0);
        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(320f))) 
        {
            cursor.EmitDelegate<Func<float, float>>((width) => {
                if (EightPlayerModule.IsEightPlayer)
                {
                    return 420f;
                }
                return width;
            });
        }
    }

    public static void PatchHalfWidth(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(160f))) 
        {
            if (cursor.Next.MatchLdcR4(1))
                continue;
            cursor.EmitDelegate<Func<float, float>>(width => {
                if (EightPlayerModule.IsEightPlayer)
                    return 420 / 2;
                return width;
            });
        }

        cursor.Goto(0);

        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(160))) 
        {
            cursor.EmitDelegate<Func<int, int>>(width => {
                if (EightPlayerModule.IsEightPlayer)
                    return 420 / 2;
                return width;
            });
        }

        cursor.Goto(0);

        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(480))) 
        {
            cursor.EmitDelegate<Func<float, float>>(width => {
                if (EightPlayerModule.IsEightPlayer)
                    return 540f;
                return width;
            });
        }
    }
}