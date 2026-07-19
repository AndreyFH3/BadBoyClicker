using YG;
using Zenject;
using Shop;
using System;
using OfflineIncome;
using DailyLogin;
using DailyQuests;
using PlayerProgression;
using QuestSystem;
using CardCollections;
using Customization;
using AdBonusOffers;

namespace Core
{
    public class SaveYGController : ISaveSystem, IInitializable, IDisposable
    {
        private Wallet _wallet;
        private IShopRuntimeSave _shopSave;
        private IOfflineIncomeRuntimeSave _offlineIncomeSave;
        private OfflineIncomeModel _offlineIncomeModel;
        private IDailyLoginRuntimeSave _dailyLoginSave;
        private IDailyQuestService _dailyQuestService;
        private DailyQuestRuntimeSave _dailyQuestSave;
        private IPlayerProgressionService _playerProgression;
        private IQuestService _questService;
        private QuestRuntimeSave _questSave;
        private ICardCollectionService _cardCollectionService;
        private CardCollectionRuntimeSave _cardCollectionSave;
        private ICustomizationService _customizationService;
        private CustomizationRuntimeSave _customizationSave;
        private AdBonusOfferRuntimeSave _adBonusOfferSave;
        private GameConfig _config;
        private bool _isApplyingSaveData;

        [Inject]
        public void Construct(
            Wallet wallet,
            IShopRuntimeSave shopSave,
            IOfflineIncomeRuntimeSave offlineIncomeSave,
            OfflineIncomeModel offlineIncomeModel,
            IDailyLoginRuntimeSave dailyLoginSave,
            IDailyQuestService dailyQuestService,
            DailyQuestRuntimeSave dailyQuestSave,
            IPlayerProgressionService playerProgression,
            IQuestService questService,
            QuestRuntimeSave questSave,
            ICardCollectionService cardCollectionService,
            CardCollectionRuntimeSave cardCollectionSave,
            ICustomizationService customizationService,
            CustomizationRuntimeSave customizationSave,
            AdBonusOfferRuntimeSave adBonusOfferSave,
            GameConfig config)
        {
            _wallet = wallet;
            _shopSave = shopSave;
            _offlineIncomeSave = offlineIncomeSave;
            _offlineIncomeModel = offlineIncomeModel;
            _dailyLoginSave = dailyLoginSave;
            _dailyQuestService = dailyQuestService;
            _dailyQuestSave = dailyQuestSave;
            _playerProgression = playerProgression;
            _questService = questService;
            _questSave = questSave;
            _cardCollectionService = cardCollectionService;
            _cardCollectionSave = cardCollectionSave;
            _customizationService = customizationService;
            _customizationSave = customizationSave;
            _adBonusOfferSave = adBonusOfferSave;
            _config = config;
        }

        public void Initialize()
        {
            YG2.onGetSDKData += Load;
            _wallet.OnChanged += Save;
            _shopSave.Changed += Save;
            _offlineIncomeSave.Changed += Save;
            _dailyLoginSave.Changed += Save;
            _dailyQuestSave.Changed += Save;
            _playerProgression.Changed += Save;
            _questSave.Changed += Save;
            _cardCollectionSave.Changed += Save;
            _customizationSave.Changed += Save;
            _adBonusOfferSave.Changed += Save;

            if (YG2.isSDKEnabled)
            {
                Load();
            }
        }

        public void Dispose()
        {
            YG2.onGetSDKData -= Load;
            _wallet.OnChanged -= Save;
            _shopSave.Changed -= Save;
            _offlineIncomeSave.Changed -= Save;
            _dailyLoginSave.Changed -= Save;
            _dailyQuestSave.Changed -= Save;
            _playerProgression.Changed -= Save;
            _questSave.Changed -= Save;
            _cardCollectionSave.Changed -= Save;
            _customizationSave.Changed -= Save;
            _adBonusOfferSave.Changed -= Save;
        }

        public void Load()
        {
            GameSaveData data = YG2.saves.GameSaveData;
            if (data == null)
            {
                return;
            }

            try
            {
                _isApplyingSaveData = true;
                _wallet.Set(data.Wallet);
                _shopSave.Set(data.Shop);
                _offlineIncomeSave.Set(data.OfflineIncome);
                _dailyLoginSave.Set(data.DailyLogin);
                _dailyQuestService.Set(data.DailyQuests);
                _playerProgression.Set(data.PlayerProgression);
                _questService.Set(data.Quests);
                _cardCollectionService.Set(data.CardCollections);
                _customizationService.Set(data.Customization);
                _adBonusOfferSave.Set(data.AdBonusOffers);
                _shopSave.Recalculate(_config);
                // Snapshot the offline reward now that both the shop (AutoIncomePerSecond)
                // and the last-online timestamp are applied. Runs once per session.
                _offlineIncomeModel.RecalculateFromSave();
            }
            finally
            {
                _isApplyingSaveData = false;
            }
        }

        public void Save()
        {
            if (_isApplyingSaveData || !YG2.isSDKEnabled)
            {
                return;
            }

            YG2.saves.GameSaveData = new GameSaveData
            {
                Wallet = _wallet.Get(),
                Shop = _shopSave.Get(),
                OfflineIncome = _offlineIncomeSave.Get(),
                DailyLogin = _dailyLoginSave.Get(),
                DailyQuests = _dailyQuestService.Get(),
                PlayerProgression = _playerProgression.Get(),
                Quests = _questService.Get(),
                CardCollections = _cardCollectionService.Get(),
                Customization = _customizationService.Get(),
                AdBonusOffers = _adBonusOfferSave.Get()
            };

            YG2.SaveProgress();
        }
    }
}
