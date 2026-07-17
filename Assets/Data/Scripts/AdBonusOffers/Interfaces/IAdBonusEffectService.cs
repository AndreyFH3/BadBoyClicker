using System;
using System.Collections.Generic;

namespace AdBonusOffers
{
    public interface IAdBonusEffectService
    {
        event Action Changed;

        float ClickIncomeMultiplier { get; }
        float PassiveIncomeMultiplier { get; }
        float AllIncomeMultiplier { get; }
        float ExperienceMultiplier { get; }
        float ShopPriceMultiplier { get; }
        IReadOnlyList<AdBonusActiveEffectViewData> ActiveEffects { get; }

        void Apply(AdBonusOfferConfig.AdBonusEffectData effect);
    }
}
