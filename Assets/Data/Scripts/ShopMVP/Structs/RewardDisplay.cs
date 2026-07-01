using UnityEngine;

namespace Rewards
{
    /// <summary>
    /// Lightweight presentation model for a single reward (icon + formatted amount),
    /// so views do not need to know about config types. Shared across the game.
    /// </summary>
    public struct RewardDisplay
    {
        public Sprite Icon;
        public string Amount;
    }
}
