using System;
using System.Collections.Generic;

namespace CardCollections
{
    // Shared "card_collection_*" Custom quest reward ids referenced by collection
    // reward configs. CardCollectionBonusService applies the percent bonuses,
    // CardCollectionCustomRewardService grants the skin unlocks, and
    // CardCollectionService formats them for display — all read from here so the
    // ids stay in one place.
    public static class CardCollectionRewardIds
    {
        public const string PassiveIncomeBonusPrefix = "card_collection_bonus_passive_income_";
        public const string ClickIncomeBonusPrefix = "card_collection_bonus_click_power_";
        public const string ShopDiscountBonusPrefix = "card_collection_bonus_shop_discount_";
        public const string OfflineIncomeBonusPrefix = "card_collection_bonus_offline_income_";

        public static readonly IReadOnlyDictionary<string, string> SkinRewardIdToCatId = new Dictionary<string, string>
        {
            { "card_collection_unlock_skin_mythic", "cat_mythic" },
            { "card_collection_unlock_skin_legendary", "cat_legend" },
            { "card_collection_unlock_skin_emperor", "cat_emperor" },
        };

        public static bool IsPercentBonus(string rewardId)
        {
            return !string.IsNullOrEmpty(rewardId) && (
                rewardId.StartsWith(PassiveIncomeBonusPrefix, StringComparison.Ordinal) ||
                rewardId.StartsWith(ClickIncomeBonusPrefix, StringComparison.Ordinal) ||
                rewardId.StartsWith(ShopDiscountBonusPrefix, StringComparison.Ordinal) ||
                rewardId.StartsWith(OfflineIncomeBonusPrefix, StringComparison.Ordinal));
        }
    }
}
