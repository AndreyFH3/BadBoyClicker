using System;

namespace OfflineIncome
{
    public interface IOfflineIncomeView
    {
        event Action ClaimRequested;
        event Action ClaimForHardRequested;
        event Action ClaimWithAdRequested;

        void Show(OfflineIncomeViewData data);
        void Hide();
    }
}
