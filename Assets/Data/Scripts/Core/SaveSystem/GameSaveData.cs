using System;
using Shop;
using OfflineIncome;
using DailyLogin;
using DailyQuests;
using PlayerProgression;
using QuestSystem;
using CardCollections;
using Customization;

namespace Core
{
    [Serializable]
    public class GameSaveData
    {
        public Wallet.SaveData Wallet;
        public ShopRuntimeSave.SaveData Shop;
        public OfflineIncomeRuntimeSave.SaveData OfflineIncome;
        public DailyLoginSaveData DailyLogin;
        public DailyQuestSaveData DailyQuests;
        public PlayerProgressionRuntimeSave.SaveData PlayerProgression;
        public QuestSaveData Quests;
        public CardCollectionSaveData CardCollections;
        public CustomizationSaveData Customization;
    }
}
