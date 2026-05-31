using System;

namespace ChestsMVP
{
    public interface IChestOpenView
    {
        event Action CloseRequested;

        void Show(ChestOpenViewData data);
        void Hide();
    }
}
