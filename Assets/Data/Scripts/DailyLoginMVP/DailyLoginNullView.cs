using System;

namespace DailyLoginMVP
{
    public class DailyLoginNullView : IDailyLoginView
    {
        public event Action ClaimRequested;

        public void Show(DailyLoginViewData data)
        {
            ClaimRequested?.Invoke();
        }

        public void Hide()
        {
        }
    }
}
