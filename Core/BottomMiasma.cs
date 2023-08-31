using System;
using MonoMod.Cil;

namespace EightPlayerMod 
{
    public static class BottomMiasmaPatch 
    {
        public static void Load() 
        {
            IL.TowerFall.BottomMiasma.ctor += ScreenUtils.PatchWidthFloat;
        }

        public static void Unload() 
        {
            IL.TowerFall.BottomMiasma.ctor -= ScreenUtils.PatchWidthFloat;
        }
    }
}