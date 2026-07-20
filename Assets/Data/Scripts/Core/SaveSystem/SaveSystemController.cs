using YG;
using Zenject;
using Analytics;
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
    public class SaveYGController : ISaveSystem, IInitializable, ITickable, IDisposable
    {
        // Wallet/progression/quest changes fire far more often than once a second
        // (every click, every auto-income tick, ...). Writing the full save blob
        // to disk/cloud on each one caused a noticeable hitch, so changes just
        // mark the save dirty and the actual write is coalesced to this cadence.
        private const float SaveInterval = 1f;

        private bool _isDirty;
        private float _elapsedSinceSave;

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
        private IAnalyticsRuntimeSave _analyticsSave;
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
            IAnalyticsRuntimeSave analyticsSave,
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
            _analyticsSave = analyticsSave;
            _config = config;
        }

        public void Initialize()
        {
            YG2.onGetSDKData += Load;
            _wallet.OnChanged += MarkDirty;
            _shopSave.Changed += MarkDirty;
            _offlineIncomeSave.Changed += MarkDirty;
            _dailyLoginSave.Changed += MarkDirty;
            _dailyQuestSave.Changed += MarkDirty;
            _playerProgression.Changed += MarkDirty;
            _questSave.Changed += MarkDirty;
            _cardCollectionSave.Changed += MarkDirty;
            _customizationSave.Changed += MarkDirty;
            _adBonusOfferSave.Changed += MarkDirty;
            _analyticsSave.Changed += MarkDirty;

            if (YG2.isSDKEnabled)
            {
                Load();
            }
        }

        public void Dispose()
        {
            YG2.onGetSDKData -= Load;
            _wallet.OnChanged -= MarkDirty;
            _shopSave.Changed -= MarkDirty;
            _offlineIncomeSave.Changed -= MarkDirty;
            _dailyLoginSave.Changed -= MarkDirty;
            _dailyQuestSave.Changed -= MarkDirty;
            _playerProgression.Changed -= MarkDirty;
            _questSave.Changed -= MarkDirty;
            _cardCollectionSave.Changed -= MarkDirty;
            _customizationSave.Changed -= MarkDirty;
            _adBonusOfferSave.Changed -= MarkDirty;
            _analyticsSave.Changed -= MarkDirty;

            // Flush any pending change so the last (<1s) of progress isn't lost on shutdown.
            if (_isDirty)
            {
                Save();
            }
        }

        public void Tick()
        {
            if (!_isDirty)
            {
                return;
            }

            _elapsedSinceSave += UnityEngine.Time.unscaledDeltaTime;
            if (_elapsedSinceSave >= SaveInterval)
            {
                Save();
            }
        }

        private void MarkDirty()
        {
            // Applying loaded data re-fires these same Changed events; skip marking
            // dirty then so we don't immediately re-save the data we just loaded.
            if (_isApplyingSaveData)
            {
                return;
            }

            _isDirty = true;
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
                _analyticsSave.Set(data.Analytics);
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
                AdBonusOffers = _adBonusOfferSave.Get(),
                Analytics = _analyticsSave.Get()
            };

            YG2.SaveProgress();

            _isDirty = false;
            _elapsedSinceSave = 0f;
        }
    }
}
