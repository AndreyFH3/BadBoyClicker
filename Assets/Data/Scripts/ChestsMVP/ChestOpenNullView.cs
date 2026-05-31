using System;

namespace ChestsMVP
{
    public class ChestOpenNullView : IChestOpenView
    {
        public event Action CloseRequested;

        public void Show(ChestOpenViewData data)
        {
        }

        public void Hide()
        {
        }
    }
}
