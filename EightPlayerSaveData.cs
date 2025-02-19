using FortRise;
using TowerFall;

namespace EightPlayerMod 
{
    public sealed class EightPlayerSaveData : ModuleSaveData
    {
        public WideQuestStats QuestStats { get; set; } = new();
        public WideDarkWorldStats DarkWorldStats { get; set; } = new();


        public override void Verify()
        {
            QuestStats.Verify();
            DarkWorldStats.Verify();
        }
    }
}