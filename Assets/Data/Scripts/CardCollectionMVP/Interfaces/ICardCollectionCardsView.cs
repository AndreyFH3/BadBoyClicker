using System;
using CardCollections;

namespace CardCollectionMVP
{
    public interface ICardCollectionCardsView
    {
        event Action CloseRequested;

        void SetVisible(bool isVisible);
        void SetData(CardCollectionViewData collection);
        void UpdateCard(CardViewData card);
    }
}
