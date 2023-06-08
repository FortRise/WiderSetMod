using FortRise;
using TeuJson;
using TowerFall;

namespace EightPlayerMod 
{
    public sealed class EightPlayerSaveData : ModuleSaveData
    {
        public WideQuestStats QuestStats;
        public EightPlayerSaveData() : base(new JsonSaveDataFormat())
        {
            QuestStats = new();
        }

        public override void Verify()
        {
            QuestStats.Verify();
        }

        public override void Load(SaveDataFormat formatter)
        {
            var jsonFormatter = formatter.CastTo<JsonSaveDataFormat>();
            var obj = jsonFormatter.GetJsonObject();
            QuestStats.Towers = obj["Towers"].ConvertToArray<WideQuestTowerStats>();
        }

        public override ClosedFormat Save(FortModule fortModule)
        {
            var obj = new JsonObject();
            var jsonArray = new JsonArray();
            foreach (var item in QuestStats.Towers) 
            {
                jsonArray.Add(JsonConvert.Serialize(item));
            }
            obj["Towers"] = jsonArray;
            return Formatter.Close(obj);
        }
    }
}