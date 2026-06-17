using System;

namespace AdBonusOffers
{
    public interface IAdBonusOfferService
    {
        event Action<AdBonusOfferViewData> OfferShown;
        event Action OfferHidden;
        event Action<AdBonusOfferViewData> RewardGranted;
        event Action<AdBonusOfferViewData> RewardFailed;

        bool HasActiveOffer { get; }
        AdBonusOfferViewData CurrentOffer { get; }
        float CurrentOfferRemainingSeconds { get; }

        bool TryShowNextOffer();
        void HideCurrentOffer();
        void SetCurrentOfferTimerPaused(bool isPaused);
        void ClaimCurrentOffer();
        void ClaimCurrentOfferForHard();
        void CompleteRewardPresentation();
    }
}
