using System;

namespace Core.Ads
{
    public interface IRewardedAdsService
    {
        event Action AdRewarded;

        bool IsAvailable(string placementId);
        void Show(string placementId, Action onRewarded, Action onFailed = null);
    }
}
