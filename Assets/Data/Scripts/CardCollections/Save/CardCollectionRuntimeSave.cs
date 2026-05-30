using System;
using System.Collections.Generic;
using Core;

namespace CardCollections
{
    public class CardCollectionRuntimeSave : ISavable<CardCollectionSaveData>
    {
        private readonly Dictionary<string, int> _cardAmounts = new();
        private readonly HashSet<string> _rewardClaimedCollections = new();

        public event Action Changed;

        public int GetCardAmount(string cardId)
        {
            return !string.IsNullOrEmpty(cardId) && _cardAmounts.TryGetValue(cardId, out int amount)
                ? amount
                : 0;
        }

        public bool IsRewardClaimed(string collectionId)
        {
            return !string.IsNullOrEmpty(collectionId) && _rewardClaimedCollections.Contains(collectionId);
        }

        public void SetCardAmount(string cardId, int amount)
        {
            if (string.IsNullOrEmpty(cardId))
            {
                return;
            }

            if (amount <= 0)
            {
                _cardAmounts.Remove(cardId);
            }
            else
            {
                _cardAmounts[cardId] = amount;
            }

            Changed?.Invoke();
        }

        public void MarkRewardClaimed(string collectionId)
        {
            if (string.IsNullOrEmpty(collectionId))
            {
                return;
            }

            if (_rewardClaimedCollections.Add(collectionId))
            {
                Changed?.Invoke();
            }
        }

        public void ResetCollection(IEnumerable<string> cardIds, string collectionId)
        {
            if (cardIds != null)
            {
                foreach (string cardId in cardIds)
                {
                    if (!string.IsNullOrEmpty(cardId))
                    {
                        _cardAmounts.Remove(cardId);
                    }
                }
            }

            if (!string.IsNullOrEmpty(collectionId))
            {
                _rewardClaimedCollections.Remove(collectionId);
            }

            Changed?.Invoke();
        }

        public void ResetAll()
        {
            _cardAmounts.Clear();
            _rewardClaimedCollections.Clear();
            Changed?.Invoke();
        }

        public void Set(CardCollectionSaveData data)
        {
            _cardAmounts.Clear();
            _rewardClaimedCollections.Clear();

            if (data?.Cards != null)
            {
                foreach (var state in data.Cards)
                {
                    if (!string.IsNullOrEmpty(state.Id) && state.Amount > 0)
                    {
                        _cardAmounts[state.Id] = state.Amount;
                    }
                }
            }

            if (data?.Collections != null)
            {
                foreach (var state in data.Collections)
                {
                    if (!string.IsNullOrEmpty(state.Id) && state.IsRewardClaimed)
                    {
                        _rewardClaimedCollections.Add(state.Id);
                    }
                }
            }

            Changed?.Invoke();
        }

        public CardCollectionSaveData Get()
        {
            var cards = new CardCollectionSaveData.CardState[_cardAmounts.Count];
            int index = 0;

            foreach (var state in _cardAmounts)
            {
                cards[index] = new CardCollectionSaveData.CardState
                {
                    Id = state.Key,
                    Amount = state.Value
                };
                index++;
            }

            var collections = new CardCollectionSaveData.CollectionState[_rewardClaimedCollections.Count];
            index = 0;

            foreach (string collectionId in _rewardClaimedCollections)
            {
                collections[index] = new CardCollectionSaveData.CollectionState
                {
                    Id = collectionId,
                    IsRewardClaimed = true
                };
                index++;
            }

            return new CardCollectionSaveData
            {
                Cards = cards,
                Collections = collections
            };
        }
    }
}
