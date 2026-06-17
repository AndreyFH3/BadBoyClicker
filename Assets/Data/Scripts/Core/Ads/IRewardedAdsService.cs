using System;

namespace Core.Ads
{
    public interface IRewardedAdsService
    {
        bool IsAvailable(string placementId);
        void Show(string placementId, Action onRewarded, Action onFailed = null);
    }
}
