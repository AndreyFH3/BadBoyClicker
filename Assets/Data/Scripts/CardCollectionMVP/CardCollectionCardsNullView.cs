using System;
using CardCollections;

namespace CardCollectionMVP
{
    public class CardCollectionCardsNullView : ICardCollectionCardsView
    {
        public event Action CloseRequested
        {
            add { }
            remove { }
        }

        public void SetVisible(bool isVisible)
        {
        }

        public void SetData(CardCollectionViewData collection)
        {
        }

        public void UpdateCard(CardViewData card)
        {
        }
    }
}
