using System;

namespace ChestsMVP
{
    public interface IChestOpenView
    {
        event Action CloseRequested;
        event Action RewardRevealed;

        void Show(ChestOpenViewData data);
        void Hide();
    }
}
