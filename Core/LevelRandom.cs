using System;
using MonoMod.Cil;

namespace EightPlayerMod 
{
    public static class LevelRandomPatch 
    {
        public static void Load() 
        {
            IL.TowerFall.LevelRandomGeometry.GenerateData += GenerateData_patch;
            IL.TowerFall.LevelRandomBGTiles.GenerateBitData += GenerateBitData_patch;
            IL.TowerFall.LevelRandomBGTiles.Set += Set_patch;
            IL.TowerFall.LevelRandomBGDetails.GenCataclysm += GenCataclysm_patch;
            IL.TowerFall.LevelRandomBGDetails.Check += Check_patch;
            IL.TowerFall.LevelRandomBGDetails.Empty += Empty_patch;
            IL.TowerFall.LevelRandomTreasure.AddRandomTreasure += AddRandomTreasure_patch;
            IL.TowerFall.LevelRandomItems.AddItems += AddItems_patch;
            // IL.TowerFall.LevelRandomItems.Collide_BooleanArray_Rectangle += Collide_BooleanArray_Rectangle_patch;
        }

        public static void Unload() 
        {
            IL.TowerFall.LevelRandomGeometry.GenerateData -= GenerateData_patch;
            IL.TowerFall.LevelRandomBGTiles.GenerateBitData -= GenerateBitData_patch;
            IL.TowerFall.LevelRandomBGTiles.Set -= Set_patch;
            IL.TowerFall.LevelRandomBGDetails.GenCataclysm -= GenCataclysm_patch;
            IL.TowerFall.LevelRandomBGDetails.Check -= Check_patch;
            IL.TowerFall.LevelRandomBGDetails.Empty -= Empty_patch;
            IL.TowerFall.LevelRandomTreasure.AddRandomTreasure -= AddRandomTreasure_patch;
            IL.TowerFall.LevelRandomItems.AddItems -= AddItems_patch;
            // IL.TowerFall.LevelRandomItems.Collide_BooleanArray_Rectangle -= Collide_BooleanArray_Rectangle_patch;
        }

        // private static void Collide_BooleanArray_Rectangle_patch(ILContext ctx)
        // {
        //     var cursor = new ILCursor(ctx);

        //     while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(32))) 
        //     {
        //         cursor.EmitDelegate<Func<int, int>>(x => {
        //             if (EightPlayerModule.LaunchedEightPlayer || EightPlayerModule.IsEightPlayer)
        //                 return 42;
        //             return x;
        //         });
        //     }
        // }

        private static void Set_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(31))) 
            {
                cursor.EmitDelegate<Func<int, int>>(x => {
                    if (EightPlayerModule.LaunchedEightPlayer || EightPlayerModule.IsEightPlayer)
                        return 41;
                    return x;
                });
            }
        }

        private static void AddItems_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdstr("160"))) 
            {
                cursor.EmitDelegate<Func<string, string>>(x => {
                    if (EightPlayerModule.LaunchedEightPlayer || EightPlayerModule.IsEightPlayer)
                        return "210";
                    return x;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdstr("310"))) 
            {
                cursor.EmitDelegate<Func<string, string>>(x => {
                    if (EightPlayerModule.LaunchedEightPlayer || EightPlayerModule.IsEightPlayer)
                        return "410";
                    return x;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(32))) 
            {
                cursor.EmitDelegate<Func<int, int>>(x => {
                    if (EightPlayerModule.LaunchedEightPlayer || EightPlayerModule.IsEightPlayer)
                        return 42;
                    return x;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(31))) 
            {
                cursor.EmitDelegate<Func<int, int>>(x => {
                    if (EightPlayerModule.LaunchedEightPlayer || EightPlayerModule.IsEightPlayer)
                        return 41;
                    return x;
                });
            }

            cursor = new ILCursor(ctx);

            int i = 0;
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(16))) 
            {
                if (i is 11)
                {
                    i++;
                    continue;
                }
                cursor.EmitDelegate<Func<int, int>>(x => {
                    if (EightPlayerModule.LaunchedEightPlayer || EightPlayerModule.IsEightPlayer)
                        return 21;
                    return x;
                });

                i++;
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(11))) 
            {
                cursor.EmitDelegate<Func<int, int>>(x => {
                    if (EightPlayerModule.LaunchedEightPlayer || EightPlayerModule.IsEightPlayer)
                        return 16;
                    return x;
                });
            }
        }

        private static void AddRandomTreasure_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(160))) 
            {
                cursor.EmitDelegate<Func<float, float>>(x => {
                    if (EightPlayerModule.LaunchedEightPlayer || EightPlayerModule.IsEightPlayer)
                        return 210;
                    return x;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(320))) 
            {
                cursor.EmitDelegate<Func<float, float>>(x => {
                    if (EightPlayerModule.LaunchedEightPlayer || EightPlayerModule.IsEightPlayer)
                        return 420;
                    return x;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(16))) 
            {
                cursor.EmitDelegate<Func<int, int>>(x => {
                    if (EightPlayerModule.LaunchedEightPlayer || EightPlayerModule.IsEightPlayer)
                        return 21;
                    return x;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(14))) 
            {
                cursor.EmitDelegate<Func<int, int>>(x => {
                    if (EightPlayerModule.LaunchedEightPlayer || EightPlayerModule.IsEightPlayer)
                        return 19;
                    return x;
                });
            }
        }

        private static void Empty_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(31))) 
            {
                cursor.EmitDelegate<Func<int, int>>(x => {
                    if (EightPlayerModule.LaunchedEightPlayer || EightPlayerModule.IsEightPlayer)
                        return 41;
                    return x;
                });
            }
        }

        private static void Check_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(32))) 
            {
                cursor.EmitDelegate<Func<int, int>>(x => {
                    if (EightPlayerModule.LaunchedEightPlayer || EightPlayerModule.IsEightPlayer)
                        return 42;
                    return x;
                });
            }
        }

        private static void GenCataclysm_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);
            int i = 0;

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(32))) 
            {
                // Skip some parts that might break
                if (i is 5 or 6) 
                {
                    i++;
                    continue;
                }

                cursor.EmitDelegate<Func<int, int>>(x => {
                    if (EightPlayerModule.LaunchedEightPlayer || EightPlayerModule.IsEightPlayer)
                        return 42;
                    return x;
                });
                i++;
            }

            cursor = new ILCursor(ctx);
            i = 0;

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(16))) 
            {
                // Skip some parts that might break
                if (i is 13 or 14 or 15) 
                {
                    i++;
                    continue;
                }
                cursor.EmitDelegate<Func<int, int>>(x => {
                    if (EightPlayerModule.LaunchedEightPlayer || EightPlayerModule.IsEightPlayer)
                        return 21;
                    return x;
                });
                i++;
            }

            cursor = new ILCursor(ctx);
            i = 0;

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(31))) 
            {
                // Skip some parts that might break
                if (i is 0 or 1) 
                {
                    i++;
                    continue;
                }
                cursor.EmitDelegate<Func<int, int>>(x => {
                    if (EightPlayerModule.LaunchedEightPlayer || EightPlayerModule.IsEightPlayer)
                        return 41;
                    return x;
                });
                i++;
            }
        }

        private static void GenerateData_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(32))) 
            {
                cursor.EmitDelegate<Func<int, int>>(x => {
                    if (EightPlayerModule.LaunchedEightPlayer || EightPlayerModule.IsEightPlayer)
                        return 42;
                    return x;
                });
            }

            cursor = new ILCursor(ctx);

            int i = 0;
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(16))) 
            {
                if (i == 0)
                {
                    i++;
                    continue;
                }
                cursor.EmitDelegate<Func<int, int>>(x => {
                    if (EightPlayerModule.LaunchedEightPlayer || EightPlayerModule.IsEightPlayer)
                        return 21;
                    return x;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(31))) 
            {
                cursor.EmitDelegate<Func<int, int>>(x => {
                    if (EightPlayerModule.LaunchedEightPlayer || EightPlayerModule.IsEightPlayer)
                        return 41;
                    return x;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(30))) 
            {
                cursor.EmitDelegate<Func<int, int>>(x => {
                    if (EightPlayerModule.LaunchedEightPlayer || EightPlayerModule.IsEightPlayer)
                        return 40;
                    return x;
                });
            }
        }

        private static void GenerateBitData_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(32))) 
            {
                cursor.EmitDelegate<Func<int, int>>(x => {
                    if (EightPlayerModule.LaunchedEightPlayer || EightPlayerModule.IsEightPlayer)
                        return 42;
                    return x;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(31))) 
            {
                cursor.EmitDelegate<Func<int, int>>(x => {
                    if (EightPlayerModule.LaunchedEightPlayer || EightPlayerModule.IsEightPlayer)
                        return 41;
                    return x;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(16))) 
            {
                cursor.EmitDelegate<Func<int, int>>(x => {
                    if (EightPlayerModule.LaunchedEightPlayer || EightPlayerModule.IsEightPlayer)
                        return 21;
                    return x;
                });
            }
        }
    }
}