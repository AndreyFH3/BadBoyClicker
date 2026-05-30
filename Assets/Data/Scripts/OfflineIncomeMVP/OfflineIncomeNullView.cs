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

        public void Hide()
        {
        }
    }
#pragma warning restore 0067
}
