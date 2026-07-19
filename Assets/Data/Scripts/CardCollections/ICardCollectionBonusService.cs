using System;

namespace CardCollections
{
    public interface ICardCollectionBonusService
    {
        event Action Changed;

        float PassiveIncomeMultiplier { get; }
        float ClickIncomeMultiplier { get; }
        float ShopPriceMultiplier { get; }
        float OfflineIncomeMultiplier { get; }
    }
}
