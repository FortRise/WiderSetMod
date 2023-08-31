using System;
using MonoMod.Cil;

namespace EightPlayerMod;

public static class AmaranthShotPatch 
{
    public static void Load() 
    {
        IL.TowerFall.AmaranthShot.Update += Update_patch;
    }

    public static void Unload() 
    {
        IL.TowerFall.AmaranthShot.Update -= Update_patch;
    }

    private static void Update_patch(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);

        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(360f))) 
        {
            cursor.EmitDelegate<Func<float, float>>(x => {
                if (EightPlayerModule.IsEightPlayer) 
                {
                    return 460;
                }
                return x;
            });
        }
    }
}