using System;
using System.Collections.Generic;
using Installer.Init;
using PlayerProgression;
using Shop;
using Zenject;
using GameLocalization;

namespace QuestSystem
{
    public class QuestService : IQuestService, IInitializable, IDisposable
    {
        private QuestConfig _config;
        private QuestRuntimeSave _save;
        private QuestFactory _factory;
        private IQuestRewardService _rewardService;
        private GameStartRouter _gameStartRouter;
        private Core.Wallet _wallet;
        private IShopModel _shopModel;
        private IPlayerProgressionService _playerProgression;
        private ILocalizationService _localization;
        private readonly List<Quest> _quests = new();
        private readonly Dictionary<string, Quest> _questsById = new();

        public IReadOnlyList<Quest> Quests => _quests;
        public event Action Changed;
        public event Action<Quest> QuestChanged;

        [Inject]
        public void Construct(
            QuestConfig config,
            QuestRuntimeSave save,
            QuestFactory factory,
            IQuestRewardService rewardService,
            GameStartRouter gameStartRouter,
            Core.Wallet wallet,
            IShopModel shopModel,
            IPlayerProgressionService playerProgression,
            ILocalizationService localization)
        {
            _config = config;
            _save = save;
            _factory = factory;
            _rewardService = rewardService;
            _gameStartRouter = gameStartRouter;
            _wallet = wallet;
            _shopModel = shopModel;
            _playerProgression = playerProgression;
            _localization = localization;
        }

        public void Initialize()
        {
            _gameStartRouter.OnClickValueEvent += OnClicked;
            _wallet.SoftAdded += OnSoftEarned;
            _shopModel.ItemBought += OnShopItemBought;
            _playerProgression.LevelCompleted += OnPlayerLevelCompleted;

            RebuildQuests();
        }

        public void Dispose()
        {
            _gameStartRouter.OnClickValueEvent -= OnClicked;
            _wallet.SoftAdded -= OnSoftEarned;
            _shopModel.ItemBought -= OnShopItemBought;
            _playerProgression.LevelCompleted -= OnPlayerLevelCompleted;

            foreach (var quest in _quests)
            {
                quest.Changed -= OnQuestChanged;
            }
        }

        public Quest GetQuest(string id)
        {
            return !string.IsNullOrEmpty(id) && _questsById.TryGetValue(id, out var quest) ? quest : null;
        }

        public IReadOnlyList<QuestViewData> GetAllViewData()
        {
            var result = new List<QuestViewData>(_quests.Count);
            foreach (var quest in _quests)
            {
                result.Add(CreateLocalizedViewData(quest));
            }

            return result;
        }

        private QuestViewData CreateLocalizedViewData(Quest quest)
        {
            QuestViewData data = quest.CreateViewData();
            data.Title = _localization.Localize(quest.Data.TitleLocalizationKey, quest.Data.Title);
            data.Description = _localization.Localize(quest.Data.DescriptionLocalizationKey, quest.Data.Description);
            return data;
        }

        public void Set(QuestSaveData data)
        {
            _save.Set(data);
            RebuildQuests();
        }

        public QuestSaveData Get()
        {
            return _save.Get();
        }

        private void RebuildQuests()
        {
            foreach (var quest in _quests)
            {
                quest.Changed -= OnQuestChanged;
            }

            _quests.Clear();
            _questsById.Clear();

            var questDatas = _config?.Quests;
            if (questDatas == null)
            {
                Changed?.Invoke();
                return;
            }

            foreach (var data in questDatas)
            {
                if (data == null || string.IsNullOrEmpty(data.Id) || _questsById.ContainsKey(data.Id))
                {
                    continue;
                }

                Quest quest = _factory.Create(data, _save.GetState(data.Id));
                if (quest == null)
                {
                    continue;
                }

                quest.Changed += OnQuestChanged;
                _quests.Add(quest);
                _questsById.Add(quest.Id, quest);

                if (TryGiveReward(quest))
                {
                    _save.SetQuestState(quest);
                }
            }

            Changed?.Invoke();
        }

        private void OnQuestChanged(Quest quest)
        {
            TryGiveReward(quest);
            _save.SetQuestState(quest);
            QuestChanged?.Invoke(quest);
            Changed?.Invoke();
        }

        private bool TryGiveReward(Quest quest)
        {
            if (quest == null || !quest.IsCompleted || quest.IsRewardClaimed)
            {
                return false;
            }

            _rewardService.GiveRewards(quest.Data.Rewards);
            quest.MarkRewardClaimed();
            return true;
        }

        private void OnClicked(long clickValue)
        {
            foreach (var quest in _quests)
            {
                if (quest is ClickQuest clickQuest)
                {
                    clickQuest.OnClick();
                }
            }
        }

        private void OnSoftEarned(long amount)
        {
            foreach (var quest in _quests)
            {
                if (quest is EarnSoftQuest earnSoftQuest)
                {
                    earnSoftQuest.OnSoftEarned(amount);
                }
            }
        }

        private void OnShopItemBought(string itemId)
        {
            foreach (var quest in _quests)
            {
                if (quest is BuyShopItemQuest buyShopItemQuest)
                {
                    buyShopItemQuest.OnShopItemBought(itemId);
                }
            }
        }

        private void OnPlayerLevelCompleted(int completedLevel)
        {
            foreach (var quest in _quests)
            {
                if (quest is CompletePlayerLevelQuest completePlayerLevelQuest)
                {
                    completePlayerLevelQuest.OnPlayerLevelCompleted();
                }
            }
        }
    }
}
