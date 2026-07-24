using System;
using YG;

namespace Core.Ads
{
    public class YGRewardedAdsService : IRewardedAdsService
    {
        private Action _pendingRewarded;
        private Action _pendingClosed;
        private Action _pendingError;

        public event Action AdRewarded;

        public bool IsAvailable(string placementId)
        {
            return YG2.isSDKEnabled && !YG2.nowAdsShow;
        }

        public void Show(string placementId, Action onRewarded, Action<RewardedAdFailureReason> onFailed = null)
        {
            if (!IsAvailable(placementId))
            {
                onFailed?.Invoke(RewardedAdFailureReason.NoFill);
                return;
            }

            CleanupPendingCallbacks();

            _pendingRewarded = () =>
            {
                CleanupPendingCallbacks();
                AdRewarded?.Invoke();
                onRewarded?.Invoke();
            };

            _pendingClosed = () =>
            {
                CleanupPendingCallbacks();
                onFailed?.Invoke(RewardedAdFailureReason.Closed);
            };

            _pendingError = () =>
            {
                CleanupPendingCallbacks();
                onFailed?.Invoke(RewardedAdFailureReason.Error);
            };

            YG2.onCloseRewardedAdv += _pendingClosed;
            YG2.onErrorRewardedAdv += _pendingError;
            YG2.RewardedAdvShow(placementId, _pendingRewarded);
        }

        private void CleanupPendingCallbacks()
        {
            if (_pendingClosed != null)
            {
                YG2.onCloseRewardedAdv -= _pendingClosed;
            }

            if (_pendingError != null)
            {
                YG2.onErrorRewardedAdv -= _pendingError;
            }

            _pendingRewarded = null;
            _pendingClosed = null;
            _pendingError = null;
        }
    }
}
