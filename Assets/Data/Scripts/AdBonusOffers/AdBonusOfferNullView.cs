using System;

namespace AdBonusOffers
{
    public class AdBonusOfferNullView : IAdBonusOfferView
    {
        public event Action CardClicked
        {
            add { }
            remove { }
        }

        public event Action ConfirmRequested
        {
            add { }
            remove { }
        }

        public event Action HardClaimRequested
        {
            add { }
            remove { }
        }

        public event Action ClosedRequested
        {
            add { }
            remove { }
        }

        public event Action ResultClosedRequested
        {
            add { }
            remove { }
        }

        public void ShowCard(AdBonusOfferViewData data) { }
        public void SetCardRemainingSeconds(float seconds) { }
        public void HideCard() { }
        public void ShowConfirmation(AdBonusOfferViewData data) { }
        public void HideConfirmation() { }
        public void ShowRewardResult(AdBonusOfferViewData data) { }
        public void HideRewardResult() { }
    }
}
