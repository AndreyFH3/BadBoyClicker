using System;
using Customization;
using QuestSystem;

namespace CardCollections
{
    // Handles the "card_collection_*" Custom quest reward ids referenced by
    // CardCollectionConfig collection rewards. Skin unlocks are granted here;
    // the percent-bonus ids (passive/click/shop/offline income) are applied
    // reactively by ICardCollectionBonusService from claimed-collection state,
    // so they're simply acknowledged as handled without further action.
    public class CardCollectionCustomRewardService : ICustomRewardHandler
    {
        private const string BonusRewardPrefix = "card_collection_bonus_";

        private readonly ICustomizationService _customizationService;

        public CardCollectionCustomRewardService(ICustomizationService customizationService)
        {
            _customizationService = customizationService;
        }

        public bool TryGiveCustomReward(string rewardId)
        {
            if (string.IsNullOrEmpty(rewardId))
            {
                return false;
            }

            if (CardCollectionRewardIds.SkinRewardIdToCatId.TryGetValue(rewardId, out string catId))
            {
                _customizationService.Give(CustomizationItemType.Cat, catId);
                return true;
            }

            return rewardId.StartsWith(BonusRewardPrefix, StringComparison.Ordinal);
        }
    }
}
