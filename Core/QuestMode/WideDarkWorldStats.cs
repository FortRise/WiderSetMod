using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json.Serialization;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using TowerFall;

namespace EightPlayerMod;

public static class WideDarkWorldSavePatch
{
    private static ILHook hook_MapButtonUnlockSequence;
    private static ILHook hook_DarkWorldLevelSelectOverlayRefreshLevelStats;

    public static void Load() 
    {
        IL.TowerFall.MapButton.InitDarkWorldGraphics += InlineTowers_patch;
        IL.TowerFall.MapButton.UnlockSequence += InlineTowers_patch;
        IL.TowerFall.UnlockData.GetDarkWorldTowerUnlocked += InlineTowers_patch;
        IL.TowerFall.UnlockData.GetArcherToIntro += InlineTowers_patch;
        // IL.TowerFall.QuestLevelSelectOverlay.ctor += QuestLevelSelectOverlayctor_patch;
        IL.TowerFall.DarkWorldMapButton.GetLocked += InlineTowers_patch;
        IL.TowerFall.Session.StartGame += InlineTowers_patch;
        IL.FortRise.RiseCore.Events.InvokeDarkWorldComplete_Result += InlineTowers_patch;

        IL.TowerFall.DarkWorldRoundLogic.OnPlayerDeath += InlineTowers_patch;
        // IL.TowerFall.CoOpDataDisplay.ctor += CoOpDataDisplayctor_patch;
        On.TowerFall.MapScene.DarkWorldIntroSequence += DarkWorldIntroSequence_patch;

        hook_DarkWorldLevelSelectOverlayRefreshLevelStats = new ILHook(
            typeof(DarkWorldLevelSelectOverlay).GetMethod("orig_RefreshLevelStats", BindingFlags.Instance | BindingFlags.NonPublic),
            InlineTowers_patch
        );
        hook_MapButtonUnlockSequence = new ILHook(
            typeof(MapButton).GetMethod("UnlockSequence").GetStateMachineTarget(),
            InlineTowers_patch
        );
    }

    public static void Unload() 
    {
        IL.TowerFall.MapButton.InitDarkWorldGraphics -= InlineTowers_patch;
        IL.TowerFall.MapButton.UnlockSequence -= InlineTowers_patch;
        IL.TowerFall.UnlockData.GetDarkWorldTowerUnlocked -= InlineTowers_patch;
        IL.TowerFall.UnlockData.GetArcherToIntro -= InlineTowers_patch;
        IL.TowerFall.DarkWorldMapButton.GetLocked -= InlineTowers_patch;
        IL.TowerFall.Session.StartGame -= InlineTowers_patch;
        IL.FortRise.RiseCore.Events.InvokeDarkWorldComplete_Result -= InlineTowers_patch;

        IL.TowerFall.DarkWorldRoundLogic.OnPlayerDeath -= InlineTowers_patch;
        // IL.TowerFall.CoOpDataDisplay.ctor -= CoOpDataDisplayctor_patch;
        On.TowerFall.MapScene.DarkWorldIntroSequence -= DarkWorldIntroSequence_patch;

        hook_MapButtonUnlockSequence.Dispose();
        hook_DarkWorldLevelSelectOverlayRefreshLevelStats.Dispose();
    }

    private static void CoOpDataDisplayctor_patch(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);

        while (cursor.TryGotoNext(MoveType.After, 
            instr => instr.MatchCallOrCallvirt<DarkWorldStats>("get_TotalRedSkulls")))
        {
            cursor.EmitDelegate<Func<int, int>>(x => {
                if (EightPlayerModule.LaunchedEightPlayer) 
                {
                    return EightPlayerModule.SaveData.DarkWorldStats.TotalRedSkulls;
                }
                return x;
            });
        }

        cursor = new ILCursor(ctx);

        while (cursor.TryGotoNext(MoveType.After, 
            instr => instr.MatchCallOrCallvirt<DarkWorldStats>("get_TotalWhiteSkulls")))
        {
            cursor.EmitDelegate<Func<int, int>>(x => {
                if (EightPlayerModule.LaunchedEightPlayer) 
                {
                    return EightPlayerModule.SaveData.DarkWorldStats.TotalWhiteSkulls;
                }
                return x;
            });
        }

        cursor = new ILCursor(ctx);

        while (cursor.TryGotoNext(MoveType.After, 
            instr => instr.MatchCallOrCallvirt<DarkWorldStats>("get_TotalGoldSkulls")))
        {
            cursor.EmitDelegate<Func<int, int>>(x => {
                if (EightPlayerModule.LaunchedEightPlayer) 
                {
                    return EightPlayerModule.SaveData.DarkWorldStats.TotalGoldSkulls;
                }
                return x;
            });
        }
    }

    private static IEnumerator DarkWorldIntroSequence_patch(On.TowerFall.MapScene.orig_DarkWorldIntroSequence orig, MapScene self)
    {
        if (EightPlayerModule.LaunchedEightPlayer) 
        {
			int num = 0;
			for (int i = 0; i < self.Buttons.Count; i = num + 1)
			{
				if (self.Buttons[i] is DarkWorldMapButton)
				{
					if (EightPlayerModule.SaveData.DarkWorldStats.ShouldRevealTower(self.Buttons[i].Data.ID.X))
					{
						Music.Stop();
						yield return self.Buttons[i].UnlockSequence(false);
					}
					num = i;
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
            instr => instr.MatchLdfld<SaveData>("DarkWorld"),
            instr => instr.MatchLdfld<DarkWorldStats>("Towers"))) 
        {
            cursor.EmitDelegate<Func<DarkWorldTowerStats[], DarkWorldTowerStats[]>>(x => {
                if (EightPlayerModule.LaunchedEightPlayer)
                    return EightPlayerModule.SaveData.DarkWorldStats.Towers;
                return x;
            });
        }
    }
}

public class WideDarkWorldStats
{
    [JsonInclude]
    public WideDarkWorldTowerStats[] Towers;


    public void Verify()
    {
        this.Towers = this.Towers.VerifyLength(GameData.DarkWorldTowers.Count);
        for (int i = 0; i < this.Towers.Length; i++)
        {
            if (this.Towers[i] == null)
            {
                this.Towers[i] = new WideDarkWorldTowerStats();
            }
        }
        this.Towers[0].Revealed = true;
        this.Towers[1].Revealed = true;
        this.Towers[2].Revealed = true;
    }

    public void RevealAll()
    {
        WideDarkWorldTowerStats[] towers = this.Towers;
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
        switch (towerID)
        {
        default:
            return true;
        case 3:
        {
            for (int i = 0; i < 3; i++)
            {
                if (!this.Towers[i].CompletedNormal)
                {
                    return false;
                }
            }
            return true;
        }
        case 4:
            return this.Towers[3].CompletedNormal;
        }
    }

    public int TotalWhiteSkulls
    {
        get
        {
            int num = 0;
            WideDarkWorldTowerStats[] towers = this.Towers;
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
            WideDarkWorldTowerStats[] towers = this.Towers;
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
            WideDarkWorldTowerStats[] towers = this.Towers;
            for (int i = 0; i < towers.Length; i++)
            {
                if (towers[i].CompletedLegendary)
                {
                    num++;
                }
            }
            return num;
        }
    }

    public int TotalEyes
    {
        get
        {
            int num = 0;
            WideDarkWorldTowerStats[] towers = this.Towers;
            for (int i = 0; i < towers.Length; i++)
            {
                if (towers[i].EarnedEye)
                {
                    num++;
                }
            }
            return num;
        }
    }

    public long Total1PTime
    {
        get
        {
            long num = 0L;
            foreach (WideDarkWorldTowerStats darkWorldTowerStats in this.Towers)
            {
                if (darkWorldTowerStats.Best1PTime <= 0L)
                {
                    return 0L;
                }
                num += darkWorldTowerStats.Best1PTime;
            }
            return num;
        }
    }

    public long Total2PTime
    {
        get
        {
            long num = 0L;
            foreach (WideDarkWorldTowerStats darkWorldTowerStats in this.Towers)
            {
                if (darkWorldTowerStats.Best2PTime <= 0L)
                {
                    return 0L;
                }
                num += darkWorldTowerStats.Best2PTime;
            }
            return num;
        }
    }

    public long Total3PTime
    {
        get
        {
            long num = 0L;
            foreach (WideDarkWorldTowerStats darkWorldTowerStats in this.Towers)
            {
                if (darkWorldTowerStats.Best3PTime <= 0L)
                {
                    return 0L;
                }
                num += darkWorldTowerStats.Best3PTime;
            }
            return num;
        }
    }

    public long Total4PTime
    {
        get
        {
            long num = 0L;
            foreach (WideDarkWorldTowerStats darkWorldTowerStats in this.Towers)
            {
                if (darkWorldTowerStats.Best4PTime <= 0L)
                {
                    return 0L;
                }
                num += darkWorldTowerStats.Best4PTime;
            }
            return num;
        }
    }

    public long BestTotalTime
    {
        get
        {
            long num = long.MaxValue;
            if (this.Total1PTime > 0L)
            {
                num = Math.Min(num, this.Total1PTime);
            }
            if (this.Total2PTime > 0L)
            {
                num = Math.Min(num, this.Total2PTime);
            }
            if (this.Total3PTime > 0L)
            {
                num = Math.Min(num, this.Total3PTime);
            }
            if (this.Total4PTime > 0L)
            {
                num = Math.Min(num, this.Total4PTime);
            }
            if (num < 9223372036854775807L)
            {
                return num;
            }
            return 0L;
        }
    }

    public int Total1PCurses
    {
        get
        {
            int num = 0;
            foreach (WideDarkWorldTowerStats darkWorldTowerStats in this.Towers)
            {
                num += darkWorldTowerStats.Most1PCurses;
            }
            return num;
        }
    }

    public int Total2PCurses
    {
        get
        {
            int num = 0;
            foreach (WideDarkWorldTowerStats darkWorldTowerStats in this.Towers)
            {
                num += darkWorldTowerStats.Most2PCurses;
            }
            return num;
        }
    }

    public int Total3PCurses
    {
        get
        {
            int num = 0;
            foreach (WideDarkWorldTowerStats darkWorldTowerStats in this.Towers)
            {
                num += darkWorldTowerStats.Most3PCurses;
            }
            return num;
        }
    }

    public int Total4PCurses
    {
        get
        {
            int num = 0;
            foreach (WideDarkWorldTowerStats darkWorldTowerStats in this.Towers)
            {
                num += darkWorldTowerStats.Most4PCurses;
            }
            return num;
        }
    }

    public ulong TotalDeaths
    {
        get
        {
            ulong num = 0UL;
            foreach (WideDarkWorldTowerStats darkWorldTowerStats in this.Towers)
            {
                num += darkWorldTowerStats.Deaths;
            }
            return num;
        }
    }

    public ulong TotalAttempts
    {
        get
        {
            ulong num = 0UL;
            foreach (WideDarkWorldTowerStats darkWorldTowerStats in this.Towers)
            {
                num += darkWorldTowerStats.Attempts;
            }
            return num;
        }
    }
}

public class WideDarkWorldTowerStats : DarkWorldTowerStats
{
    public new bool Revealed
    {
        get => Revealed;
        set => Revealed = value;
    }

    public new bool CompletedNormal
    {
        get => CompletedNormal;
        set => CompletedNormal = value;
    }

    public new bool CompletedHardcore
    {
        get => CompletedHardcore;
        set => CompletedHardcore = value;
    }

    public new bool CompletedLegendary
    {
        get => CompletedLegendary;
        set => CompletedLegendary = value;
    }

    public new bool EarnedEye
    {
        get => EarnedEye;
        set => EarnedEye = value;
    }

    public new bool EarnedGoldEye
    {
        get => EarnedGoldEye;
        set => EarnedGoldEye = value;
    }

    public new long Best1PTime
    {
        get => Best1PTime;
        set => Best1PTime = value;
    }

    public new long Best2PTime
    {
        get => Best2PTime;
        set => Best2PTime = value;
    }

    public new long Best3PTime
    {
        get => Best3PTime;
        set => Best3PTime = value;
    }

    public new long Best4PTime
    {
        get => Best4PTime;
        set => Best4PTime = value;
    }

    public new int Most1PCurses
    {
        get => Most1PCurses;
        set => Most1PCurses = value;
    }

    public new int Most2PCurses
    {
        get => Most2PCurses;
        set => Most2PCurses = value;
    }

    public new int Most3PCurses
    {
        get => Most3PCurses;
        set => Most3PCurses = value;
    }

    public new int Most4PCurses
    {
        get => Most4PCurses;
        set => Most4PCurses = value;
    }

    public new ulong Deaths
    {
        get => Deaths;
        set => Deaths = value;
    }

    public new ulong Attempts
    {
        get => Attempts;
        set => Attempts = value;
    }

    [JsonInclude]
    public long Best5PTime;
    [JsonInclude]
    public long Best6PTime;
    [JsonInclude]
    public long Best7PTime;
    [JsonInclude]
    public long Best8PTime;

    [JsonInclude]
    public int Most5PCurses;
    [JsonInclude]
    public int Most6PCurses;
    [JsonInclude]
    public int Most7PCurses;
    [JsonInclude]
    public int Most8PCurses;

    public static void Load() 
    {
        On.TowerFall.DarkWorldTowerStats.Complete += Complete_patch;
    }

    public static void Unload() 
    {
        On.TowerFall.DarkWorldTowerStats.Complete -= Complete_patch;
    }


    private static void Complete_patch(On.TowerFall.DarkWorldTowerStats.orig_Complete orig, DarkWorldTowerStats self, DarkWorldDifficulties difficulty, int players, long time, int continues, int deaths, int curses)
    {
        orig(self, difficulty, players, time, continues, deaths, curses);
        if (self is not WideDarkWorldTowerStats wideSelf)
        {
            return;
        }
        if (difficulty != DarkWorldDifficulties.Legendary)
        {
            return;
        }
        
        wideSelf.internal_Complete(difficulty, players, time, continues, deaths, curses);
    }

    private void internal_Complete(DarkWorldDifficulties difficulty, int players, long time, int continues, int deaths, int curses)
    {
        switch (players)
        {
            default:
                if (this.Best1PTime > 0L)
                {
                    this.Best1PTime = Math.Min(this.Best1PTime, time);
                }
                else
                {
                    this.Best1PTime = time;
                }
                this.Most1PCurses = Math.Max(this.Most1PCurses, curses);
                return;
            case 2:
                if (this.Best2PTime > 0L)
                {
                    this.Best2PTime = Math.Min(this.Best2PTime, time);
                }
                else
                {
                    this.Best2PTime = time;
                }
                this.Most2PCurses = Math.Max(this.Most2PCurses, curses);
                return;
            case 3:
                if (this.Best3PTime > 0L)
                {
                    this.Best3PTime = Math.Min(this.Best3PTime, time);
                }
                else
                {
                    this.Best3PTime = time;
                }
                this.Most3PCurses = Math.Max(this.Most3PCurses, curses);
                return;
            case 4:
                if (this.Best4PTime > 0L)
                {
                    this.Best4PTime = Math.Min(this.Best4PTime, time);
                }
                else
                {
                    this.Best4PTime = time;
                }
                this.Most4PCurses = Math.Max(this.Most4PCurses, curses);
                break;
            case 5:
                if (this.Best5PTime > 0L)
                {
                    this.Best5PTime = Math.Min(this.Best5PTime, time);
                }
                else
                {
                    this.Best5PTime = time;
                }
                this.Most5PCurses = Math.Max(this.Most5PCurses, curses);
                break;
            case 6:
                if (this.Best6PTime > 0L)
                {
                    this.Best6PTime = Math.Min(this.Best6PTime, time);
                }
                else
                {
                    this.Best6PTime = time;
                }
                this.Most6PCurses = Math.Max(this.Most6PCurses, curses);
                break;
            case 7:
                if (this.Best7PTime > 0L)
                {
                    this.Best7PTime = Math.Min(this.Best7PTime, time);
                }
                else
                {
                    this.Best7PTime = time;
                }
                this.Most7PCurses = Math.Max(this.Most7PCurses, curses);
                break;
            case 8:
                if (this.Best8PTime > 0L)
                {
                    this.Best8PTime = Math.Min(this.Best8PTime, time);
                }
                else
                {
                    this.Best8PTime = time;
                }
                this.Most8PCurses = Math.Max(this.Most8PCurses, curses);
                break;
        }
    }
}
