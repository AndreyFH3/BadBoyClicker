using System;

namespace RewardActivation
{
    public interface IRewardActivationView
    {
        event Action ActivateRequested;
        event Action PostponeRequested;

        void Show(RewardActivationViewData data);
        void Hide();
    }
}
