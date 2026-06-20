using System;
using YG;

namespace Core.Ads
{
    public class YGRewardedAdsService : IRewardedAdsService
    {
        private Action _pendingRewarded;
        private Action _pendingFailed;

        public bool IsAvailable(string placementId)
        {
            return YG2.isSDKEnabled && !YG2.nowAdsShow;
        }

        public void Show(string placementId, Action onRewarded, Action onFailed = null)
        {
            if (!IsAvailable(placementId))
            {
                onFailed?.Invoke();
                return;
            }

            CleanupPendingCallbacks();

            _pendingRewarded = () =>
            {
                CleanupPendingCallbacks();
                onRewarded?.Invoke();
            };

            _pendingFailed = () =>
            {
                CleanupPendingCallbacks();
                onFailed?.Invoke();
            };

            YG2.onErrorRewardedAdv += _pendingFailed;
            YG2.RewardedAdvShow(placementId, _pendingRewarded);
        }

        private void CleanupPendingCallbacks()
        {
            if (_pendingFailed != null)
            {
                YG2.onErrorRewardedAdv -= _pendingFailed;
            }

            _pendingRewarded = null;
            _pendingFailed = null;
        }
    }
}
