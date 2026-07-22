using System;
using System.Collections.Generic;
using CardCollections;
using Shop;

namespace CardCollectionMVP
{
    public interface ICardCollectionsView
    {
        event Action<string> CollectionSelected;
        event Action<string> CollectRequested;
        event Action CardChestPurchaseRequested;
        event Action CardChestAdRequested;

        void SetVisible(bool isVisible);
        void SetData(IReadOnlyList<CardCollectionViewData> collections);
        void UpdateCollection(CardCollectionViewData collection);
        void SetCardChestPurchaseData(ShopElementData data);
        void SetCardChestAdAvailable(bool isAvailable);
    }
}
