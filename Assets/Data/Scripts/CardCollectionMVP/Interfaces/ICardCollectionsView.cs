using System;
using System.Collections.Generic;
using CardCollections;

namespace CardCollectionMVP
{
    public interface ICardCollectionsView
    {
        event Action<string> CollectionSelected;
        event Action<string> CollectRequested;

        void SetVisible(bool isVisible);
        void SetData(IReadOnlyList<CardCollectionViewData> collections);
        void UpdateCollection(CardCollectionViewData collection);
    }
}
