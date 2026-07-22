using System;
using System.Collections.Generic;
using CardCollections;
using Shop;

namespace CardCollectionMVP
{
    public class CardCollectionsNullView : ICardCollectionsView
    {
        public event Action<string> CollectionSelected
        {
            add { }
            remove { }
        }

        public event Action<string> CollectRequested
        {
            add { }
            remove { }
        }

        public event Action CardChestPurchaseRequested
        {
            add { }
            remove { }
        }

        public event Action CardChestAdRequested
        {
            add { }
            remove { }
        }

        public void SetVisible(bool isVisible)
        {
        }

        public void SetData(IReadOnlyList<CardCollectionViewData> collections)
        {
        }

        public void UpdateCollection(CardCollectionViewData collection)
        {
        }

        public void SetCardChestPurchaseData(ShopElementData data)
        {
        }

        public void SetCardChestAdAvailable(bool isAvailable)
        {
        }
    }
}
