using System;

namespace OfflineIncome
{
    public interface IOfflineIncomeView
    {
        event Action ClaimRequested;
        event Action ClaimForHardRequested;
        event Action ClaimWithAdRequested;

        void Show(OfflineIncomeViewData data);
        void ShowClaimedReward(long finalReward, Action onHidden = null);
        void Hide(Action onHidden = null);

        // The popup is only ever needed once per session (or not at all). Letting the
        // presenter destroy it after use frees the whole UI subtree instead of leaving
        // an inactive-but-alive popup around for the rest of the session.
        void DestroyView();
    }
}
