using System;

namespace OfflineIncome
{
#pragma warning disable 0067
    public class OfflineIncomeNullView : IOfflineIncomeView
    {
        public event Action ClaimRequested;
        public event Action ClaimForHardRequested;
        public event Action ClaimWithAdRequested;

        public void Show(OfflineIncomeViewData data)
        {
            ClaimRequested?.Invoke();
        }

        public void ShowClaimedReward(long finalReward, Action onHidden = null)
        {
            onHidden?.Invoke();
        }

        public void Hide(Action onHidden = null)
        {
            onHidden?.Invoke();
        }

        public void DestroyView()
        {
        }
    }
#pragma warning restore 0067
}
