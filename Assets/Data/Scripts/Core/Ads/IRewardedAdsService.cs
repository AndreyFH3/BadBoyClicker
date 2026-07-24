using System;

namespace Core.Ads
{
    public enum RewardedAdFailureReason
    {
        NoFill,
        Closed,
        Error
    }

    public interface IRewardedAdsService
    {
        event Action AdRewarded;

        bool IsAvailable(string placementId);
        void Show(string placementId, Action onRewarded, Action<RewardedAdFailureReason> onFailed = null);
    }
}
