using System;
using System.IO;
using Mono.Cecil.Cil;
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

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallOrCallvirt(
                typeof(File), "System.IO.FileStream OpenRead(System.String)"
            ))) 
            {
                cursor.Emit(OpCodes.Ldloc_1);
                cursor.EmitDelegate<Func<Stream, string, Stream>>((stream, path) => 
                {
                    if (EightPlayerModule.LaunchedEightPlayer || EightPlayerModule.IsEightPlayer) 
                    {
                        var correctPath = path.Replace('\\', '/');
#if DEBUG
                        if (FakeDarkWorldTowerData.LevelMap.TryGetValue(correctPath, out var newPath)) 
                        {
                            stream.Dispose();
                            var zipStream = EightPlayerModule.Instance.Content.MapResource[newPath].Stream;
                            return zipStream;
                        }
#else
                        stream.Dispose();
                        var zipStream = EightPlayerModule.Instance.Content.MapResource[FakeDarkWorldTowerData.LevelMap[correctPath]].Stream;
                        return zipStream;
#endif
                    }
                    return stream;
                });
            }
        }
    }
}