using System;

namespace CardCollections
{
    [Serializable]
    public class CardCollectionSaveData
    {
        public CardState[] Cards;
        public CollectionState[] Collections;

        [Serializable]
        public struct CardState
        {
            public string Id;
            public int Amount;
        }

        [Serializable]
        public struct CollectionState
        {
            public string Id;
            public bool IsRewardClaimed;
        }
    }
}
