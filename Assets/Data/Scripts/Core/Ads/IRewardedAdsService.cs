using System;

namespace Core.Ads
{
    public interface IRewardedAdsService
    {
        void Show(string placementId, Action onRewarded, Action onFailed = null);
    }
}
