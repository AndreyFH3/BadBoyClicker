using System;
using YG;

namespace Core.Ads
{
    public class YGRewardedAdsService : IRewardedAdsService
    {
        public void Show(string placementId, Action onRewarded, Action onFailed = null)
        {
            if (!YG2.isSDKEnabled || YG2.nowAdsShow)
            {
                onFailed?.Invoke();
                return;
            }

            YG2.RewardedAdvShow(placementId, onRewarded);
        }
    }
}
