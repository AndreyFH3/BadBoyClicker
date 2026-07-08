using System.Collections.Generic;
using Rewards;

namespace CardCollections
{
    public class CardCollectionViewData
    {
        public string Id;
        public string Title;
        public string Description;
        public int CollectedCards;
        public int TotalCards;
        public int CollectedStars;
        public int TotalStars;
        public bool IsCompleted;
        public bool IsRewardClaimed;
        public IReadOnlyList<CardViewData> Cards;
        public IReadOnlyList<RewardDisplay> Rewards;
    }
}
