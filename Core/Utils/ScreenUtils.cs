using System;
using MonoMod.Cil;

namespace EightPlayerMod;

public static class ScreenUtils 
{
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
}