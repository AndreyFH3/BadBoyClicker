using System.Collections.Generic;
using Rewards;
using UnityEngine;

namespace Shop
{
    public class ShopElementData
    {
        public string Id;
        public ShopItemType Type;
        public Sprite Icon;
        public string Name;
        public string Level;
        public Sprite PriceIcon;
        public string Price;
        public string Bonus;
        public bool CanBuy;

        // Paid offers only: which received-resource slot this offer belongs to
        // (crystals / decor / soft), so offers are grouped by what the player gets.
        public ShopRewardGroup RewardGroup;

        // Paid offers only: whether the offer is bought with real money (badge only).
        public bool IsRealMoney;

        // Paid offers only: what the player actually receives (icons + amounts),
        // rendered f2p-style inside the dedicated paid element.
        public IReadOnlyList<RewardDisplay> Rewards;
    }
}
