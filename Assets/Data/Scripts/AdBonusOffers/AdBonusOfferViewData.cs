using UnityEngine;

namespace AdBonusOffers
{
    public readonly struct AdBonusOfferViewData
    {
        public AdBonusOfferViewData(
            string id,
            string rewardTitle,
            string rewardValueText,
            string description,
            string resultDescription,
            Sprite icon,
            bool canClaimWithAd,
            bool canClaimForHard,
            long hardPrice)
        {
            Id = id;
            RewardTitle = rewardTitle;
            RewardValueText = rewardValueText;
            Description = description;
            ResultDescription = resultDescription;
            Icon = icon;
            CanClaimWithAd = canClaimWithAd;
            CanClaimForHard = canClaimForHard;
            HardPrice = hardPrice;
        }

        public string Id { get; }
        public string RewardTitle { get; }
        public string RewardValueText { get; }
        public string Description { get; }
        public string ResultDescription { get; }
        public Sprite Icon { get; }
        public bool CanClaimWithAd { get; }
        public bool CanClaimForHard { get; }
        public long HardPrice { get; }

        public AdBonusOfferViewData WithAdClaimAvailability(bool canClaimWithAd)
        {
            return new AdBonusOfferViewData(
                Id,
                RewardTitle,
                RewardValueText,
                Description,
                ResultDescription,
                Icon,
                canClaimWithAd,
                CanClaimForHard,
                HardPrice);
        }
    }
}
