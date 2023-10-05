using System;
using MonoMod.Cil;

namespace EightPlayerMod;

public static class MInputPatch 
{
    public static void Load() 
    {
        IL.Monocle.MInput.UpdateJoysticks += PlayerAmountUtils.Player4ToPlayer8;
    }


    public static void Unload() 
    {
        IL.Monocle.MInput.UpdateJoysticks -= PlayerAmountUtils.Player4ToPlayer8;
    }
}