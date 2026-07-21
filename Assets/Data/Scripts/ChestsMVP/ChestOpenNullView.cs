using System;

namespace ChestsMVP
{
    public class ChestOpenNullView : IChestOpenView
    {
        public event Action CloseRequested;
        public event Action RewardRevealed;

        public void Show(ChestOpenViewData data)
        {
            RewardRevealed?.Invoke();
        }

        public void Hide()
        {
        }
    }
}
