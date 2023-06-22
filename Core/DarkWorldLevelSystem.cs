using System;
using FortRise;
using MonoMod.Cil;
using TowerFall;

namespace EightPlayerMod 
{
    public static class DarkWorldLevelSystemPatch 
    {
        public static void Load() 
        {
            IL.TowerFall.DarkWorldLevelSystem.GetNextRoundLevel += GetNextRoundLevel_patch;
        }

        public static void Unload() 
        {
            IL.TowerFall.DarkWorldLevelSystem.GetNextRoundLevel -= GetNextRoundLevel_patch;
        }

        private static void GetNextRoundLevel_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            if (cursor.TryGotoNext(MoveType.Before, instr => instr.MatchCallOrCallvirt(
                "Monocle.Calc", "System.Xml.XmlDocument LoadXML(System.String)"))) 
            {
                cursor.EmitDelegate<Func<string, string>>(text => 
                {
                    if (EightPlayerModule.LaunchedEightPlayer || EightPlayerModule.IsEightPlayer) 
                    {
#if DEBUG
                        if (FakeDarkWorldTowerData.LevelMap.TryGetValue(text, out var newPath)) 
                        {
                            return newPath;
                        }
#else
                        return FakeDarkWorldTowerData.LevelMap[text];
#endif
                    }
                    return text;
                });
            }
        }
    }
}