using QuestSystem;
using CardCollections;

namespace Chests
{
    public class ChestOpenResult
    {
        public ChestConfig.ChestData Chest { get; set; }
        public QuestReward Reward { get; set; }
        public CardCollectionConfig.CardData Card { get; set; }
    }
}
