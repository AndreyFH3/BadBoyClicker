using System.Collections.Generic;
using Rewards;

namespace CardCollectionMVP
{
    public interface ICardCollectionRewardWindow
    {
        void Show(IReadOnlyList<RewardDisplay> rewards);
    }
}
