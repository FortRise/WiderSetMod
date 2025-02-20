using System;
using System.Collections;
using System.Reflection;
using System.Text.Json.Serialization;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using TowerFall;

namespace EightPlayerMod
{
    public static class QuestSavePatch 
    {
        private static ILHook hook_MainMenuCreateCredits;
        private static ILHook hook_MapButtonUnlockSequence;
        private static ILHook hook_orig_RefreshLevelStats;

        public static void Load() 
        {
            IL.TowerFall.MapButton.InitQuestGraphics += InlineTowers_patch;
            IL.TowerFall.MapButton.UnlockSequence += InlineTowers_patch;
            IL.TowerFall.UnlockData.GetQuestTowerUnlocked += InlineTowers_patch;
            IL.TowerFall.UnlockData.GetArcherToIntro += InlineTowers_patch;
            IL.TowerFall.QuestLevelSelectOverlay.ctor += QuestLevelSelectOverlayctor_patch;
            IL.TowerFall.QuestMapButton.GetLocked += InlineTowers_patch;
            IL.FortRise.RiseCore.Events.InvokeQuestRoundLogic_PlayerDeath += InlineTowers_patch;
            IL.FortRise.RiseCore.Events.InvokeQuestRoundLogic_LevelLoadFinish += InlineTowers_patch;
            IL.FortRise.RiseCore.Events.InvokeQuestComplete_Result += InlineTowers_patch;
            // IL.TowerFall.CoOpDataDisplay.ctor += CoOpDataDisplayctor_patch;
            On.TowerFall.MapScene.QuestIntroSequence += QuestIntroSequence_patch;

            hook_orig_RefreshLevelStats = new ILHook(
                typeof(QuestLevelSelectOverlay).GetMethod("orig_RefreshLevelStats", BindingFlags.NonPublic | BindingFlags.Instance),
                InlineTowers_patch
            );

            hook_MainMenuCreateCredits = new ILHook(
                typeof(MainMenu).GetMethod("<CreateCredits>b__111_7", BindingFlags.Instance | BindingFlags.NonPublic) ??
                typeof(MainMenu).GetMethod("<CreateCredits>b__3e", BindingFlags.Instance | BindingFlags.NonPublic),
                CreateCredits_patch
            );
            hook_MapButtonUnlockSequence = new ILHook(
                typeof(MapButton).GetMethod("UnlockSequence").GetStateMachineTarget(),
                InlineTowers_patch
            );
        }

        public static void Unload() 
        {
            IL.TowerFall.MapButton.InitQuestGraphics -= InlineTowers_patch;
            IL.TowerFall.MapButton.UnlockSequence -= InlineTowers_patch;
            IL.TowerFall.UnlockData.GetQuestTowerUnlocked -= InlineTowers_patch;
            IL.TowerFall.UnlockData.GetArcherToIntro -= InlineTowers_patch;
            IL.TowerFall.QuestLevelSelectOverlay.ctor -= QuestLevelSelectOverlayctor_patch;
            IL.TowerFall.QuestMapButton.GetLocked -= InlineTowers_patch;
            IL.FortRise.RiseCore.Events.InvokeQuestRoundLogic_PlayerDeath -= InlineTowers_patch;
            IL.FortRise.RiseCore.Events.InvokeQuestRoundLogic_LevelLoadFinish -= InlineTowers_patch;
            IL.FortRise.RiseCore.Events.InvokeQuestComplete_Result -= InlineTowers_patch;
            // IL.TowerFall.CoOpDataDisplay.ctor -= CoOpDataDisplayctor_patch;
            On.TowerFall.MapScene.QuestIntroSequence -= QuestIntroSequence_patch;

            hook_orig_RefreshLevelStats.Dispose();
            hook_MainMenuCreateCredits.Dispose();
            hook_MapButtonUnlockSequence.Dispose();
        }

        private static void CoOpDataDisplayctor_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, 
                instr => instr.MatchLdfld<SaveData>("Quest"),
                instr => instr.MatchLdfld<QuestStats>("Towers"))) 
            {
                cursor.EmitDelegate<Func<QuestTowerStats[], QuestTowerStats[]>>(x => {
                    if (EightPlayerModule.LaunchedEightPlayer)
                        return EightPlayerModule.SaveData.QuestStats.Towers;
                    return x;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, 
                instr => instr.MatchCallOrCallvirt<QuestStats>("get_TotalRedSkulls")))
            {
                cursor.EmitDelegate<Func<int, int>>(x => {
                    if (EightPlayerModule.LaunchedEightPlayer) 
                    {
                        return EightPlayerModule.SaveData.QuestStats.TotalRedSkulls;
                    }
                    return x;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, 
                instr => instr.MatchCallOrCallvirt<QuestStats>("get_TotalWhiteSkulls")))
            {
                cursor.EmitDelegate<Func<int, int>>(x => {
                    if (EightPlayerModule.LaunchedEightPlayer) 
                    {
                        return EightPlayerModule.SaveData.QuestStats.TotalWhiteSkulls;
                    }
                    return x;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, 
                instr => instr.MatchCallOrCallvirt<QuestStats>("get_TotalGoldSkulls")))
            {
                cursor.EmitDelegate<Func<int, int>>(x => {
                    if (EightPlayerModule.LaunchedEightPlayer) 
                    {
                        return EightPlayerModule.SaveData.QuestStats.TotalGoldSkulls;
                    }
                    return x;
                });
            }
        }

        private static void QuestLevelSelectOverlayctor_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, 
                instr => instr.MatchCallOrCallvirt<QuestStats>("get_TotalSoloTime")))
            {
                cursor.EmitDelegate<Func<long, long>>(x => {
                    if (EightPlayerModule.LaunchedEightPlayer) 
                    {
                        return EightPlayerModule.SaveData.QuestStats.TotalSoloTime;
                    }
                    return x;
                });
            }

            cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, 
                instr => instr.MatchCallOrCallvirt<QuestStats>("get_TotalCoopTime")))
            {
                cursor.EmitDelegate<Func<long, long>>(x => {
                    if (EightPlayerModule.LaunchedEightPlayer) 
                    {
                        return EightPlayerModule.SaveData.QuestStats.TotalCoopTime;
                    }
                    return x;
                });
            }
        }

        private static IEnumerator QuestIntroSequence_patch(On.TowerFall.MapScene.orig_QuestIntroSequence orig, MapScene self)
        {
            if (EightPlayerModule.LaunchedEightPlayer) 
            {
                for (int i = 0; i < self.Buttons.Count; i++)
                {
                    if (self.Buttons[i] is not QuestMapButton)
                        continue;
                    if (EightPlayerModule.SaveData.QuestStats.ShouldRevealTower(self.Buttons[i].Data.ID.X))
                    {
                        Music.Stop();
                        yield return self.Buttons[i].UnlockSequence(true);
                    }
                }
                yield break;
            } 
            yield return orig(self);
        }

        private static void InlineTowers_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, 
                instr => instr.MatchLdfld<SaveData>("Quest"),
                instr => instr.MatchLdfld<QuestStats>("Towers"))) 
            {
                cursor.EmitDelegate<Func<QuestTowerStats[], QuestTowerStats[]>>(x => {
                    if (EightPlayerModule.LaunchedEightPlayer)
                        return EightPlayerModule.SaveData.QuestStats.Towers;
                    return x;
                });
            }
        }

        private static void CreateCredits_patch(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallOrCallvirt<QuestStats>("RevealAll"))) 
            {
                cursor.EmitDelegate<Action>(() => {
                    EightPlayerModule.SaveData.QuestStats.RevealAll();
                });
            }
        }
    }

    public class WideQuestStats 
    {
        [JsonInclude]
        public WideQuestTowerStats[] Towers;

        public void Verify()
		{
			this.Towers = this.Towers.VerifyLength(GameData.QuestLevels.Length);
			for (int i = 0; i < this.Towers.Length; i++)
			{
				if (this.Towers[i] == null)
				{
					this.Towers[i] = new WideQuestTowerStats();
				}
			}
			this.Towers[0].Revealed = true;
		}

		public void RevealAll()
		{
			WideQuestTowerStats[] towers = this.Towers;
			for (int i = 0; i < towers.Length; i++)
			{
				towers[i].Revealed = true;
			}
		}

		public bool ShouldRevealTower(int towerID)
		{
			if (this.Towers[towerID].Revealed)
			{
				return false;
			}
			int num;
			switch (towerID)
			{
			case 1:
			case 2:
				num = 1;
				break;
			case 3:
			case 4:
			case 5:
			case 6:
				num = 3;
				break;
			case 7:
				num = 7;
				break;
			case 8:
			case 9:
			case 10:
				num = 8;
				break;
			case 11:
				num = 11;
				break;
			case 12:
				num = 12;
				break;
			case 13:
				num = 13;
				break;
			default:
				return true;
			}
			for (int i = 0; i < num; i++)
			{
				if (!this.Towers[i].CompletedNormal && !this.Towers[i].CompletedHardcore)
				{
					return false;
				}
			}
			return true;
		}

		public int TotalWhiteSkulls
		{
			get
			{
				int num = 0;
				WideQuestTowerStats[] towers = this.Towers;
				for (int i = 0; i < towers.Length; i++)
				{
					if (towers[i].CompletedNormal)
					{
						num++;
					}
				}
				return num;
			}
		}

		public int TotalRedSkulls
		{
			get
			{
				int num = 0;
				WideQuestTowerStats[] towers = this.Towers;
				for (int i = 0; i < towers.Length; i++)
				{
					if (towers[i].CompletedHardcore)
					{
						num++;
					}
				}
				return num;
			}
		}

		public int TotalGoldSkulls
		{
			get
			{
				int num = 0;
				WideQuestTowerStats[] towers = this.Towers;
				for (int i = 0; i < towers.Length; i++)
				{
					if (towers[i].CompletedNoDeaths)
					{
						num++;
					}
				}
				return num;
			}
		}

		public long TotalSoloTime
		{
			get
			{
				long num = 0L;
				foreach (WideQuestTowerStats questTowerStats in this.Towers)
				{
					if (questTowerStats.Best1PTime == 0L)
					{
						return 0L;
					}
					num += questTowerStats.Best1PTime;
				}
				return num;
			}
		}

		public long TotalCoopTime
		{
			get
			{
				long num = 0L;
				foreach (WideQuestTowerStats questTowerStats in this.Towers)
				{
					if (questTowerStats.Best2PTime != 0L)
					{
                        num += questTowerStats.Best2PTime;
					}
                    if (questTowerStats.Best3PTime != 0L)
                    {
                        num += questTowerStats.Best2PTime;
                    }
                    if (questTowerStats.Best4PTime != 0L)
                    {
                        num += questTowerStats.Best2PTime;
                    }
				}
				return num;
			}
		}

		public ulong TotalDeaths
		{
			get
			{
				ulong num = 0UL;
				foreach (WideQuestTowerStats questTowerStats in this.Towers)
				{
					num += questTowerStats.TotalDeaths;
				}
				return num;
			}
		}

		public ulong TotalAttempts
		{
			get
			{
				ulong num = 0UL;
				foreach (WideQuestTowerStats questTowerStats in this.Towers)
				{
					num += questTowerStats.TotalAttempts;
				}
				return num;
			}
		}
    }

    public class WideQuestTowerStats : QuestTowerStats
    {
        public new ulong TotalDeaths
        {
            get => base.TotalDeaths;
            set => base.TotalDeaths = value;
        }
        public new ulong TotalAttempts
        {
            get => base.TotalAttempts;
            set => base.TotalAttempts = value;
        }
        public new bool Revealed 
        {
            get => base.Revealed;
            set => base.Revealed = value;
        }
        public new bool CompletedNormal 
        {
            get => base.CompletedNormal;
            set => base.CompletedNormal = value;
        }
        public new bool CompletedHardcore 
        {
            get => base.CompletedHardcore;
            set => base.CompletedHardcore = value;
        }
        public new bool CompletedNoDeaths
        {
            get => base.CompletedNoDeaths;
            set => base.CompletedNoDeaths = value;
        }

        public new long Best1PTime 
        {
            get => base.Best1PTime;
            set => base.Best1PTime = value;
        }
        public new long Best2PTime
        {
            get => base.Best2PTime;
            set => base.Best2PTime = value;
        }
        [JsonInclude]
        public long Best3PTime;
        [JsonInclude]
        public long Best4PTime;

        public static void Load() 
        {
            On.TowerFall.QuestTowerStats.BeatHardcore += BeatHardcore_patch;
        }

        public static void Unload() 
        {
            On.TowerFall.QuestTowerStats.BeatHardcore -= BeatHardcore_patch;
        }

        private static void BeatHardcore_patch(On.TowerFall.QuestTowerStats.orig_BeatHardcore orig, QuestTowerStats self, int players, long time, bool noDeaths)
        {
            if (self is WideQuestTowerStats wideSelf) 
            {
                wideSelf.CompletedNormal = (wideSelf.CompletedHardcore = true);
                if (noDeaths)
                {
                    wideSelf.CompletedNoDeaths = true;
                }
                if (players == 1)
                {
                    if (wideSelf.Best1PTime == 0L)
                    {
                        wideSelf.Best1PTime = time;
                        return;
                    }
                    wideSelf.Best1PTime = Math.Min(wideSelf.Best1PTime, time);
                    return;
                }
                if (players == 2)
                {
                    if (wideSelf.Best2PTime == 0L)
                    {
                        wideSelf.Best2PTime = time;
                        return;
                    }
                    wideSelf.Best2PTime = Math.Min(wideSelf.Best2PTime, time);
                    return;
                }
                if (players == 3)
                {
                    if (wideSelf.Best3PTime == 0L)
                    {
                        wideSelf.Best3PTime = time;
                        return;
                    }
                    wideSelf.Best3PTime = Math.Min(wideSelf.Best3PTime, time);
                    return;
                }
                if (players >= 4)
                {
                    if (wideSelf.Best4PTime == 0L)
                    {
                        wideSelf.Best4PTime = time;
                        return;
                    }
                    wideSelf.Best4PTime = Math.Min(wideSelf.Best4PTime, time);
                    return;
                }
            }
            orig(self, players, time, noDeaths);
        }
    }
}
    