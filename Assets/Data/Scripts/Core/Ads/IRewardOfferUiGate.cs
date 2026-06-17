using System;

namespace Core.Ads
{
    public interface IRewardOfferUiGate
    {
        event Action Changed;

        bool IsBlocked { get; }

        void Block(object owner);
        void Unblock(object owner);
    }
}
