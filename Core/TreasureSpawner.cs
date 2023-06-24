using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using TowerFall;

namespace EightPlayerMod 
{
    public static class RoundLogicPatch 
    {
        public static int[] TeamStartArrows = new int[7] { 3, 3, 3, 3, 2, 2, 1 };
        public static void Load() 
        {
            IL.TowerFall.RoundLogic.SpawnPlayersFFA += SpawnPlayers_patch;
            IL.TowerFall.RoundLogic.SpawnPlayersTeams += SpawnPlayers_patch;
            IL.TowerFall.RoundLogic.ctor += RoundLogicctor_patch;
            On.TowerFall.Session.ctor += Sessionctor_patch;
            IL.TowerFall.Session.GetSpawnArrows += SessionGetSpawnArrows_patch;
            On.TowerFall.Variant.ctor += Variantctor_patch;
            IL.TowerFall.Player.cctor += Playercctor_patch;
        }

        public static void Unload() 
        {
            IL.TowerFall.RoundLogic.SpawnPlayersFFA -= SpawnPlayers_patch;
            IL.TowerFall.RoundLogic.SpawnPlayersTeams -= SpawnPlayers_patch;
            IL.TowerFall.RoundLogic.ctor -= RoundLogicctor_patch;
            On.TowerFall.Session.ctor -= Sessionctor_patch;
            IL.TowerFall.Session.GetSpawnArrows -= SessionGetSpawnArrows_patch;
            On.TowerFall.Variant.ctor -= Variantctor_patch;
            IL.TowerFall.Player.cctor -= Playercctor_patch;
        }

        private static void SessionGetSpawnArrows_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdsfld<TowerFall.Session>("TeamStartArrows")))
            {
                cursor.EmitDelegate<Func<int[], int[]>>(teamStartArrows => {
                    if (EightPlayerModule.IsEightPlayer || EightPlayerModule.LaunchedEightPlayer)
                        return TeamStartArrows;
                    return teamStartArrows;
                });
            }
        }

        private static void RoundLogicctor_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(4)))
            {
                cursor.EmitDelegate<Func<int, int>>(amount => {
                    return 8;
                });
            }
        }

        private static void Playercctor_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            cursor.GotoNext(MoveType.After, instr => instr.MatchLdcI4(4));
            {
                cursor.EmitDelegate<Func<int, int>>(amount => {
                    return 8;
                });
            }
        }

        private static void Sessionctor_patch(On.TowerFall.Session.orig_ctor orig, Session self, MatchSettings settings)
        {
            orig(self, settings);
            self.MatchStats = new MatchStats[8];
            self.Scores = new int[self.MatchSettings.TeamMode ? 2 : 8];
            self.OldScores = new int[self.MatchSettings.TeamMode ? 2 : 8];
        }

        private static void Variantctor_patch(On.TowerFall.Variant.orig_ctor orig, Variant self, Subtexture icon, string title, string description, Pickups[] itemExclusions, bool perPlayer, string header, UnlockData.Unlocks? unlocker, bool scrollEffect, bool hidden, bool canRandom, bool tournamentRule1v1, bool tournamentRule2v2, bool unlisted, bool darkWorldDLC, int coOpValue)
        {
            orig(self, icon, title, description, itemExclusions, perPlayer, header, unlocker, scrollEffect, hidden, canRandom, tournamentRule1v1, tournamentRule2v2, unlisted, darkWorldDLC, coOpValue);
            if (perPlayer)
                DynamicData.For(self).Set("playerValues", new bool[8]);
        }

        private static void SpawnPlayers_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            // Instead of writing my own instructions.
            // why not intercept them and add what we need inside of the object creation.
            if (cursor.TryGotoNext(MoveType.After, 
                instr => instr.MatchLdstr("TeamSpawnA"),
                instr => instr.MatchCallOrCallvirt<Level>("GetXMLPositions"))) 
            {
                // need access to the class
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<List<Vector2>, RoundLogic, List<Vector2>>>((x, logic) => 
                {
                    if (EightPlayerModule.IsEightPlayer) 
                    {
                        var playerSpawn = logic.Session.CurrentLevel.GetXMLPositions("PlayerSpawn");
                        foreach (var spawn in playerSpawn) 
                        {
                            if (spawn.X <= 210)
                                x.Add(spawn);
                        }
                    }
                    return x;
                });
            }

            if (cursor.TryGotoNext(MoveType.After, 
                instr => instr.MatchLdstr("TeamSpawnB"),
                instr => instr.MatchCallOrCallvirt<Level>("GetXMLPositions"))) 
            {
                // need access to the class
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<List<Vector2>, RoundLogic, List<Vector2>>>((x, logic) => 
                {
                    if (EightPlayerModule.IsEightPlayer) 
                    {
                        var playerSpawn = logic.Session.CurrentLevel.GetXMLPositions("PlayerSpawn");
                        foreach (var spawn in playerSpawn) 
                        {
                            if (spawn.X > 210)
                                x.Add(spawn);
                        }
                    }
                    return x;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(4))) 
            {
                cursor.EmitDelegate<Func<int, int>>(amount => {
                    if (EightPlayerModule.IsEightPlayer) 
                    {
                        return 8;
                    }
                    return amount;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(160))) 
            {
                cursor.EmitDelegate<Func<float, float>>(amount => {
                    if (EightPlayerModule.IsEightPlayer) 
                    {
                        return 420 / 2;
                    }
                    return amount;
                });
            }
        }
    }

    public static class VersusMatchResultsPatch 
    {
        public static IDetour hook_Sequence;
        public static IDetour hook_VersusPlayerMatchResultsSequence;
        public static IDetour hook_VersusPlayerMatchResultsSequenceb__5;
        private static ConstructorInfo base_VersusMatchResult;

        public static void Load() 
        {
            base_VersusMatchResult = typeof(HUD).GetConstructor(Array.Empty<Type>());
            On.TowerFall.VersusMatchResults.ctor += VersusMatchResultsctor_patch;
            hook_Sequence = new ILHook(
                typeof(VersusMatchResults).GetMethod("Sequence", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetStateMachineTarget(),
                Sequence_patch
            );
            hook_VersusPlayerMatchResultsSequence = new ILHook(
                typeof(VersusPlayerMatchResults).GetMethod("Sequence", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetStateMachineTarget(),
                VersusPlayerMatchResultsSequence_patch
            );
            hook_VersusPlayerMatchResultsSequenceb__5 = new ILHook(
                typeof(VersusPlayerMatchResults).GetNestedType("<>c__DisplayClass21_4", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetMethod("<Sequence>b__6", BindingFlags.Instance | BindingFlags.NonPublic),
                VersusPlayerMatchResultsSequenceb__6_patch
            );
        }

        public static void Unload() 
        {
            On.TowerFall.VersusMatchResults.ctor -= VersusMatchResultsctor_patch;
            hook_Sequence.Dispose();
            hook_VersusPlayerMatchResultsSequence.Dispose();
            hook_VersusPlayerMatchResultsSequenceb__5.Dispose();
        }

        private static void VersusPlayerMatchResultsSequenceb__6_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(1f))) 
            {
                cursor.EmitDelegate<Func<float, float>>(scale => {
                    if (EightPlayerModule.IsEightPlayer && TFGame.PlayerAmount > 5) 
                    {
                        return 0.7f;
                    }
                    return scale;
                });
            }

            cursor = new ILCursor(ctx);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(1.5f))) 
            {
                cursor.EmitDelegate<Func<float, float>>(scale => {
                    if (EightPlayerModule.IsEightPlayer && TFGame.PlayerAmount > 5) 
                    {
                        return 1f;
                    }
                    return scale;
                });
            }
        }
        private static void VersusPlayerMatchResultsSequence_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(1.5f))) 
            {
                cursor.EmitDelegate<Func<float, float>>(scale => {
                    if (EightPlayerModule.IsEightPlayer && TFGame.PlayerAmount > 5) 
                    {
                        return 1f;
                    }
                    return scale;
                });
            }
        }

        private static void Sequence_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(160))) 
            {
                if (cursor.Next.MatchLdcR4(1f))
                    continue;
                cursor.EmitDelegate<Func<float, float>>(width => {
                    if (EightPlayerModule.IsEightPlayer)
                        return 210;
                    return width;
                });
            }
        }


        private static void VersusMatchResultsctor_patch(On.TowerFall.VersusMatchResults.orig_ctor orig, VersusMatchResults self, Session session, VersusRoundResults roundResults)
        {
            if (EightPlayerModule.IsEightPlayer) 
            {
                base_VersusMatchResult.Invoke(self, Array.Empty<object>());
                DynamicData.For(self).Set("session", session);
                DynamicData.For(self).Set("roundResults", roundResults);
                self.Position = new Vector2(0f, 240f);
                Vector2[] array;
                Vector2[] array2;
                if (TFGame.PlayerAmount == 2)
                {
                    array = new Vector2[2]
                    {
                        new Vector2(-160f, 120f),
                        new Vector2(580f, 120f)
                    };
                    array2 = new Vector2[2]
                    {
                        new Vector2(160f, 120f),
                        new Vector2(260f, 120f)
                    };
                }
                else if (TFGame.PlayerAmount == 3)
                {
                    array = new Vector2[3]
                    {
                        new Vector2(-160f, 120f),
                        new Vector2(210f, 360f),
                        new Vector2(580f, 120f)
                    };
                    array2 = new Vector2[3]
                    {
                        new Vector2(130f, 120f),
                        new Vector2(210f, 120f),
                        new Vector2(290f, 120f)
                    };
                }
                else if (TFGame.PlayerAmount == 4)
                {
                    array = new Vector2[4]
                    {
                        new Vector2(-160f, 120f),
                        new Vector2(-80f, 120f),
                        new Vector2(500f, 120f),
                        new Vector2(580f, 120f)
                    };
                    array2 = new Vector2[4]
                    {
                        new Vector2(90f, 120f),
                        new Vector2(170f, 120f),
                        new Vector2(250f, 120f),
                        new Vector2(330f, 120f)
                    };
                }
                else if (TFGame.PlayerAmount == 5)
                {
                    array = new Vector2[5]
                    {
                        new Vector2(-240f, 120f),
                        new Vector2(-160f, 120f),
                        new Vector2(210f, 360f),
                        new Vector2(580f, 120f),
                        new Vector2(660f, 120f)
                    };
                    array2 = new Vector2[5]
                    {
                        new Vector2(50f, 120f),
                        new Vector2(130f, 120f),
                        new Vector2(210f, 120f),
                        new Vector2(290f, 120f),
                        new Vector2(370f, 120f)
                    };
                }
                else if (TFGame.PlayerAmount == 6)
                {
                    array2 = new Vector2[6]
                    {
                        new Vector2(100f, 105f),
                        new Vector2(180f, 105f),
                        new Vector2(260f, 105f),
                        new Vector2(160f, 210f),
                        new Vector2(240f, 210f),
                        new Vector2(320f, 210f)
                    };
                    array = new Vector2[6];
                    for (int i = 0; i < 3; i++)
                    {
                        array[i] = array2[i] + new Vector2(-420f, 0f);
                    }
                    for (int j = 3; j < 6; j++)
                    {
                        array[j] = array2[j] + new Vector2(420f, 0f);
                    }
                }
                else if (TFGame.PlayerAmount == 7)
                {
                    array2 = new Vector2[7]
                    {
                        new Vector2(90f, 105f),
                        new Vector2(170f, 105f),
                        new Vector2(250f, 105f),
                        new Vector2(330f, 105f),
                        new Vector2(130f, 210f),
                        new Vector2(210f, 210f),
                        new Vector2(290f, 210f)
                    };
                    array = new Vector2[7];
                    for (int k = 0; k < 4; k++)
                    {
                        array[k] = array2[k] + new Vector2(-420f, 0f);
                    }
                    for (int l = 4; l < 7; l++)
                    {
                        array[l] = array2[l] + new Vector2(420f, 0f);
                    }
                }
                else
                {
                    if (TFGame.PlayerAmount != 8)
                    {
                        throw new Exception("Invalid player amount for match results!");
                    }
                    array2 = new Vector2[8]
                    {
                        new Vector2(70f, 105f),
                        new Vector2(150f, 105f),
                        new Vector2(230f, 105f),
                        new Vector2(310f, 105f),
                        new Vector2(110f, 210f),
                        new Vector2(190f, 210f),
                        new Vector2(270f, 210f),
                        new Vector2(350f, 210f)
                    };
                    array = new Vector2[8];
                    for (int m = 0; m < 4; m++)
                    {
                        array[m] = array2[m] + new Vector2(-420f, 0f);
                    }
                    for (int n = 4; n < 8; n++)
                    {
                        array[n] = array2[n] + new Vector2(420f, 0f);
                    }
                }
                int winner = session.GetWinner();
                for (int num = 0; num < 8; num++)
                {
                    if (TFGame.Players[num] && winner == session.GetScoreIndex(num))
                    {
                        session.MatchStats[num].Won = true;
                        SaveData.Instance.Stats.Wins[TFGame.Characters[num]]++;
                        SessionStats.RegisterArcherWin(num);
                    }
                    else
                    {
                        session.MatchStats[num].Won = false;
                    }
                }
                List<AwardInfo>[] awards = VersusAwards.GetAwards(session.MatchSettings, session.MatchStats);
                SessionStats.RegisterArcherPlays();
                var playerResults = new List<VersusPlayerMatchResults>();
                for (int num2 = 0; num2 < 8; num2++)
                {
                    if (TFGame.Players[num2])
                    {
                        if (TFGame.PlayerAmount >= 6) 
                        {
                            var versusPlayerMatchResults = new SmallVersusPlayerMatchResults(session, self, num2, array[playerResults.Count], array2[playerResults.Count], awards[num2]);
                            var won = session.MatchStats[num2].Won;
                            session.CurrentLevel.Add(versusPlayerMatchResults);
                            versusPlayerMatchResults.AsSmall(won);
                            playerResults.Add(versusPlayerMatchResults);
                        }
                        else 
                        {
                            VersusPlayerMatchResults versusPlayerMatchResults = new VersusPlayerMatchResults(session, self, num2, array[playerResults.Count], array2[playerResults.Count], awards[num2]);
                            session.CurrentLevel.Add(versusPlayerMatchResults);
                            playerResults.Add(versusPlayerMatchResults);
                        }

                    }
                }
                DynamicData.For(self).Set("playerResults", playerResults);
                if (session.MatchSettings.LevelSystem.Procedural)
                {
                    session.CurrentLevel.Add(new VersusSeedDisplay(session.MatchSettings.RandomSeedIcons));
                }
                return;
            }
            orig(self, session, roundResults);
        }
    }

    public static class TreasureSpawnerPatch 
    {
        public static void Load() 
        {
            typeof(TreasureSpawner).GetField("ChestChances").SetValue(null, new float[7][] 
            { 
                new float[4] { 0.9f, 0.9f, 0.2f, 0.1f },
                new float[5] { 0.9f, 0.9f, 0.8f, 0.2f, 0.1f },
                new float[6] { 0.9f, 0.9f, 0.6f, 0.8f, 0.2f, 0.1f },
                new float[6] { 0.9f, 0.9f, 0.8f, 0.8f, 0.2f, 0.1f },
                new float[6] { 0.9f, 0.9f, 0.9f, 0.8f, 0.4f, 0.1f },
                new float[6] { 0.9f, 0.9f, 0.9f, 0.8f, 0.4f, 0.2f },
                new float[7] { 0.9f, 0.9f, 0.9f, 0.9f, 0.4f, 0.2f, 0.1f }
            });
            IL.TowerFall.VersusAwards.GetAwards += GetAwards_patch;
        }

        public static void Unload() 
        {
            IL.TowerFall.VersusAwards.GetAwards -= GetAwards_patch;
        }

        private static void GetAwards_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(4))) 
            {
                cursor.EmitDelegate<Func<int, int>>(x => {
                    if (EightPlayerModule.IsEightPlayer) 
                        return 8;
                    return x;
                });
            }
        }

    }
}