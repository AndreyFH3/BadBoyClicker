using System;
using System.Collections.Generic;
using Core;

namespace CardCollections
{
    public interface ICardCollectionService : ISavable<CardCollectionSaveData>
    {
        bool AreAllCollectionsCompleted { get; }
        long LastCardChestAdUtcTicks { get; }
        IReadOnlyList<CardCollectionConfig.CardCollectionData> Collections { get; }
        event Action Changed;
        event Action<CardCollectionViewData> CollectionChanged;
        event Action<CardViewData> CardChanged;
        event Action<CardCollectionViewData> CollectionRewardClaimed;

        bool HasCard(string cardId);
        int GetCardAmount(string cardId);
        CardCollectionConfig.CardData GetCard(string cardId);
        CardCollectionConfig.CardCollectionData GetCollection(string collectionId);
        CardCollectionConfig.CardCollectionData GetCollectionByCard(string cardId);
        IReadOnlyList<CardCollectionViewData> GetAllViewData();
        CardCollectionViewData GetViewData(string collectionId);
        bool TryAddCard(string cardId, int amount = 1);
        bool TrySetCardAmount(string cardId, int amount);
        bool TryResetCard(string cardId);
        bool TryResetCollectionProgress(string collectionId);
        bool TryClaimReward(string collectionId);
        bool IsRewardClaimed(string collectionId);
        void SetLastCardChestAdUtcTicks(long utcTicks);
        void ResetAllProgress();
    }
}
