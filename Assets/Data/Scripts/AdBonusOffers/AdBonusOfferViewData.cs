using UnityEngine;

namespace AdBonusOffers
{
    public readonly struct AdBonusOfferViewData
    {
        public AdBonusOfferViewData(
            string id,
            string cardTitle,
            string cardDescription,
            string confirmationTitle,
            string confirmationDescription,
            Sprite icon,
            bool canClaimForHard,
            long hardPrice)
        {
            Id = id;
            CardTitle = cardTitle;
            CardDescription = cardDescription;
            ConfirmationTitle = confirmationTitle;
            ConfirmationDescription = confirmationDescription;
            Icon = icon;
            CanClaimForHard = canClaimForHard;
            HardPrice = hardPrice;
        }

        public string Id { get; }
        public string CardTitle { get; }
        public string CardDescription { get; }
        public string ConfirmationTitle { get; }
        public string ConfirmationDescription { get; }
        public Sprite Icon { get; }
        public bool CanClaimForHard { get; }
        public long HardPrice { get; }
    }
}
