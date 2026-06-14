using System;

namespace AdBonusOffers
{
    public interface IAdBonusOfferView
    {
        event Action CardClicked;
        event Action ConfirmRequested;
        event Action HardClaimRequested;
        event Action ClosedRequested;
        event Action ResultClosedRequested;

        void ShowCard(AdBonusOfferViewData data);
        void SetCardRemainingSeconds(float seconds);
        void HideCard();
        void ShowConfirmation(AdBonusOfferViewData data);
        void HideConfirmation();
        void ShowRewardResult(AdBonusOfferViewData data);
        void HideRewardResult();
    }
}
