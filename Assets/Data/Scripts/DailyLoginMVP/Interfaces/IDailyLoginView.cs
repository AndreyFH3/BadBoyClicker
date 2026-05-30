using System;

namespace DailyLoginMVP
{
    public interface IDailyLoginView
    {
        event Action ClaimRequested;

        void Show(DailyLoginViewData data);
        void Hide();
    }
}
