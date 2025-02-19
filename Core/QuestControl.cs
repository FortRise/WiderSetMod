using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using FortRise;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using TowerFall;

namespace EightPlayerMod;

public static class QuestControlPatch 
{
    private static ILHook hook_SpawnGroup;
    private static ILHook hook_QuestControlLevelSequence;
    private static Action<QuestControl> base_Added;


    public static void Load() 
    {
        base_Added = CallHelper.CallBaseGen<HUD, QuestControl>("Added");
        hook_SpawnGroup = new ILHook(
            typeof(QuestControl).GetMethod("SpawnGroup", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetStateMachineTarget(),
            ScreenUtils.PatchHalfWidthFloat
        );
        hook_QuestControlLevelSequence = new ILHook(
            typeof(QuestControl).GetMethod("LevelSequence", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetStateMachineTarget(),
            QuestControlLevelSequence_patch
        );

        On.TowerFall.QuestControl.Added += QuestControlAdded_patch;
        On.TowerFall.QuestControl.LoadWaves += QuestControlLoadWaves_patch;
    }

    public static void Unload() 
    {
        hook_SpawnGroup.Dispose();
        hook_QuestControlLevelSequence.Dispose();
        On.TowerFall.QuestControl.Added -= QuestControlAdded_patch;
        On.TowerFall.QuestControl.LoadWaves -= QuestControlLoadWaves_patch;
    }


    private static void QuestControlLevelSequence_patch(ILContext ctx)
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
                if (EightPlayerModule.IsEightPlayer) 
                {
                    return EightPlayerModule.SaveData.QuestStats.TotalRedSkulls;
                }
                return x;
            });
        }
    }

    private static void QuestControlAdded_patch(On.TowerFall.QuestControl.orig_Added orig, QuestControl self)
    {
        if (!EightPlayerModule.IsEightPlayer) 
        {
            orig(self);
            return;
        }
        var selfDynamic = DynamicData.For(self);
        base_Added.Invoke(self);
        selfDynamic.Invoke("LoadSpawns");
        var dataPath = (self.Level.Session.MatchSettings.LevelSystem as QuestLevelSystem).QuestTowerData.DataPath
            .Replace("Levels", "WideLevels")
            .Replace('\\', '/');
        using var fs = EightPlayerModule.Instance.Content.MapResource[dataPath].Stream;
        XmlDocument xmlDocument = Calc.LoadXML(fs);
        self.Gauntlet = xmlDocument["data"].AttrBool("gauntlet", false);
        if (self.Gauntlet)
        {
            selfDynamic.Invoke("LoadGauntlet", xmlDocument);
            return;
        }
        selfDynamic.Invoke("LoadWaves", xmlDocument);
    }

    private static void QuestControlLoadWaves_patch(On.TowerFall.QuestControl.orig_LoadWaves orig, QuestControl self, XmlDocument doc)
    {
        if (!EightPlayerModule.IsEightPlayer && !EightPlayerModule.LaunchedEightPlayer)
        {
            orig(self, doc);
            return;
        }
        var questRoundLogic = self.Level.Session.RoundLogic as QuestRoundLogic;
        var selfDynamic = DynamicData.For(self);
        selfDynamic.Set("waves", new List<IEnumerator>());
        int num = 0;
        XmlNodeList xmlNodeList = (self.Level.Session.MatchSettings.QuestHardcoreMode 
            ? doc["data"]["hardcore"].GetElementsByTagName("wave") 
            : doc["data"]["normal"].GetElementsByTagName("wave"));
        int count = xmlNodeList.Count;
        questRoundLogic.TotalWaves = count;
        foreach (XmlElement wavesXml in xmlNodeList)
        {
            if (num >= self.Level.Session.QuestTestWave)
            {
                var list = new List<IEnumerator>();
                foreach (XmlElement groupXml in wavesXml.GetElementsByTagName("group"))
                {
                    if (groupXml.HasAttr("players")) 
                    {
                        bool condition = false;
                        var amount = groupXml.Attr("players");
                        var parsed = int.Parse(amount.Replace("=", ""));
                        if (amount.Contains("="))
                            condition = TFGame.PlayerAmount == parsed;
                        else 
                            condition = TFGame.PlayerAmount >= parsed;
                        if (condition)
                        {
                            var routine = selfDynamic.Invoke<IEnumerator>("SpawnGroup", 
                                Calc.ReadCSV(groupXml.ChildText("spawns", "")), 
                                groupXml.ChildText("enemies", ""), 
                                Calc.ReadCSV(groupXml.ChildText("treasure", "")), 
                                Calc.ReadCSV(groupXml.ChildText("bigTreasure", "")), 
                                groupXml.ChildInt("reaper", -1), 
                                groupXml.ChildBool("jester", false), 
                                groupXml.AttrInt("delay", 0), 
                                num >= count - 1
                            );
                            list.Add(routine);
                        }
                    }
                    else if ((TFGame.PlayerAmount == 2 || !groupXml.AttrBool("coop", false)) 
                    && (TFGame.PlayerAmount == 1 || !groupXml.AttrBool("solo", false))
                    && !groupXml.HasAttr("players"))
                    {
                        var routine = selfDynamic.Invoke<IEnumerator>("SpawnGroup", 
                            Calc.ReadCSV(groupXml.ChildText("spawns", "")), 
                            groupXml.ChildText("enemies", ""), 
                            Calc.ReadCSV(groupXml.ChildText("treasure", "")), 
                            Calc.ReadCSV(groupXml.ChildText("bigTreasure", "")), 
                            groupXml.ChildInt("reaper", -1), 
                            groupXml.ChildBool("jester", false), 
                            groupXml.AttrInt("delay", 0), 
                            num >= count - 1
                        );
                        list.Add(routine);
                    }
                }
                int[] array = null;
                if (wavesXml.HasChild("floors"))
                {
                    array = Calc.ReadCSVInt(wavesXml.ChildText("floors"));
                }
                var waveRoutine = selfDynamic.Invoke<IEnumerator>("SpawnWave", 
                    num - self.Level.Session.QuestTestWave, list, array, 
                    wavesXml.AttrBool("dark", false), 
                    wavesXml.AttrBool("slow", false), 
                    wavesXml.AttrBool("scroll", false));
                selfDynamic.Get<List<IEnumerator>>("waves").Add(waveRoutine);
            }
            num++;
        }
    }
}