using System;
using System.Collections.Generic;
using Core;
using QuestSystem;
using UnityEngine;

namespace Customization
{
    public interface ICustomizationService : ISavable<CustomizationSaveData>, IQuestBackgroundRewardService
    {
        event Action Changed;
        event Action<CustomizationItemType, string> ActiveItemChanged;
        event Action<CustomizationItemType, string, long> ItemBought;

        string ActiveBackgroundId { get; }
        string ActiveCatId { get; }
        Sprite ActiveBackgroundSprite { get; }
        Sprite ActiveCatSprite { get; }

        IReadOnlyList<CustomizationConfig.CustomizationItemData> GetItems(CustomizationItemType type);
        CustomizationConfig.CustomizationItemData GetItem(CustomizationItemType type, string id);
        bool IsPurchased(CustomizationItemType type, string id);
        bool HasUnseen(CustomizationItemType type);
        void MarkSeen(CustomizationItemType type);
        bool CanBuy(CustomizationItemType type, string id);
        bool Buy(CustomizationItemType type, string id);
        bool Select(CustomizationItemType type, string id);
        void Give(CustomizationItemType type, string id);
    }
}
