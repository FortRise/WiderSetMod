using System;
using MonoMod.Cil;

namespace EightPlayerMod;

public static class DarkWorldPlayerResults
{
    public static void Load() 
    {
        IL.TowerFall.DarkWorldPlayerResults.ctor += PlayerAmountUtils.Player4ToPlayer8;
    }

    public static void Unload() 
    {
        IL.TowerFall.DarkWorldPlayerResults.ctor -= PlayerAmountUtils.Player4ToPlayer8;
    }
}