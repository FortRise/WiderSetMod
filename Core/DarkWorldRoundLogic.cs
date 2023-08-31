using System;
using MonoMod.Cil;

namespace EightPlayerMod;

public static class DarkWorldRoundLogicPatch 
{
    public static void Load() 
    {
        IL.TowerFall.DarkWorldRoundLogic.OnUpdate += PlayerAmountUtils.Player4ToPlayer8;
        IL.TowerFall.DarkWorldRoundLogic.OnLevelLoadFinish += PlayerAmountUtils.Player4ToPlayer8;
        IL.TowerFall.DarkWorldRoundLogic.ctor += PlayerAmountUtils.Player4ToPlayer8;
        IL.TowerFall.DarkWorldRoundLogic.CompareSpawns += CompareSpawns_patch;
    }

    public static void Unload() 
    {
        IL.TowerFall.DarkWorldRoundLogic.OnUpdate -= PlayerAmountUtils.Player4ToPlayer8;
        IL.TowerFall.DarkWorldRoundLogic.OnLevelLoadFinish -= PlayerAmountUtils.Player4ToPlayer8;
        IL.TowerFall.DarkWorldRoundLogic.ctor -= PlayerAmountUtils.Player4ToPlayer8;
        IL.TowerFall.DarkWorldRoundLogic.CompareSpawns -= CompareSpawns_patch;
    }

    private static void CompareSpawns_patch(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);

        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(3))) 
        {
            cursor.EmitDelegate<Func<int, int>>(x => {
                if (EightPlayerModule.IsEightPlayer) 
                {
                    return 7;
                }
                return x;
            });
        }
    }
}