using System.Collections.Generic;
using Rewards;

namespace CardCollectionMVP
{
    public class CardCollectionRewardNullWindow : ICardCollectionRewardWindow
    {
        public void Show(IReadOnlyList<RewardDisplay> rewards)
        {
        }
    }
}
