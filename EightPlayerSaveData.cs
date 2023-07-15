using FortRise;
using TeuJson;
using TowerFall;

namespace EightPlayerMod 
{
    public sealed class EightPlayerSaveData : ModuleSaveData
    {
        public WideQuestStats QuestStats;
        public WideDarkWorldStats DarkWorldStats;
        public EightPlayerSaveData() : base(new JsonSaveDataFormat())
        {
            QuestStats = new();
            DarkWorldStats = new();
        }

        public override void Verify()
        {
            QuestStats.Verify();
            DarkWorldStats.Verify();
        }

        public override void Load(SaveDataFormat formatter)
        {
            var jsonFormatter = formatter.CastTo<JsonSaveDataFormat>();
            var obj = jsonFormatter.GetJsonObject();
            QuestStats.Towers = obj["Towers"].ConvertToArray<WideQuestTowerStats>();
            DarkWorldStats.Towers = obj["DarkWorldTowers"].ConvertToArray<WideDarkWorldTowerStats>();
        }

        public override ClosedFormat Save(FortModule fortModule)
        {
            var obj = new JsonObject();
            var questJson = new JsonArray();
            var darkWorldJson = new JsonArray();
            foreach (var item in QuestStats.Towers) 
            {
                questJson.Add(JsonConvert.Serialize(item));
            }
            foreach (var item in DarkWorldStats.Towers) 
            {
                darkWorldJson.Add(JsonConvert.Serialize(item));
            }
            obj["Towers"] = questJson;
            obj["DarkWorldTowers"] = darkWorldJson;
            return Formatter.Close(obj);
        }
    }
}