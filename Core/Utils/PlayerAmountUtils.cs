using System;
using MonoMod.Cil;

namespace EightPlayerMod;

public static class PlayerAmountUtils 
{
    public static void Player4ToPlayer8(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);

        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(4))) 
        {
            cursor.EmitDelegate<Func<int, int>>(x => {
                if (EightPlayerModule.IsEightPlayer || EightPlayerModule.LaunchedEightPlayer) 
                {
                    return 8;
                }
                return x;
            });
        }
    }
}