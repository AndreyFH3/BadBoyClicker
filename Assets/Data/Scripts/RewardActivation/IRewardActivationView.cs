using System;

namespace RewardActivation
{
    public interface IRewardActivationView
    {
        event Action ActivateRequested;

        void Show(RewardActivationViewData data);
        void Hide();
    }
}
