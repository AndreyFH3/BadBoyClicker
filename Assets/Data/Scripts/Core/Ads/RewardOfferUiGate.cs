using System;
using System.Collections.Generic;

namespace Core.Ads
{
    public class RewardOfferUiGate : IRewardOfferUiGate
    {
        private readonly HashSet<object> _blockers = new();

        public event Action Changed;

        public bool IsBlocked => _blockers.Count > 0;

        public void Block(object owner)
        {
            if (owner == null || !_blockers.Add(owner))
            {
                return;
            }

            Changed?.Invoke();
        }

        public void Unblock(object owner)
        {
            if (owner == null || !_blockers.Remove(owner))
            {
                return;
            }

            Changed?.Invoke();
        }
    }
}
