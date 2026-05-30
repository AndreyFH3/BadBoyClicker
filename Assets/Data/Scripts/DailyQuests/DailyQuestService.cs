using System;
using System.Collections.Generic;
using Core;
using Core.Time;
using Installer.Init;
using PlayerProgression;
using PlayerFeatures;
using QuestSystem;
using Shop;
using Zenject;
using GameLocalization;

namespace DailyQuests
{
    public class DailyQuestService : IDailyQuestService, IInitializable, IDisposable
    {
        private DailyQuestConfig _config;
        private DailyQuestRuntimeSave _save;
        private IQuestRewardService _rewardService;
        private ITimeService _timeService;
        private IPlayerFeatureUnlockService _featureUnlockService;
        private GameStartRouter _gameStartRouter;
        private Wallet _wallet;
        private IShopModel _shopModel;
        private IPlayerProgressionService _playerProgression;
        private ILocalizationService _localization;
        private bool _isUnlocked;
        private bool _isProgressListening;

        public int Points => _save.Points;
        public event Action Changed;

        [Inject]
        public void Construct(
            DailyQuestConfig config,
            DailyQuestRuntimeSave save,
            IQuestRewardService rewardService,
            ITimeService timeService,
            IPlayerFeatureUnlockService featureUnlockService,
            GameStartRouter gameStartRouter,
            Wallet wallet,
            IShopModel shopModel,
            IPlayerProgressionService playerProgression,
            ILocalizationService localization)
        {
            _config = config;
            _save = save;
            _rewardService = rewardService;
            _timeService = timeService;
            _featureUnlockService = featureUnlockService;
            _gameStartRouter = gameStartRouter;
            _wallet = wallet;
            _shopModel = shopModel;
            _playerProgression = playerProgression;
            _localization = localization;
        }

        public void Initialize()
        {
            _save.Changed += OnSaveChanged;
            _featureUnlockService.FeatureUnlocked += OnFeatureUnlocked;

            _isUnlocked = _featureUnlockService.IsUnlocked(PlayerFeatureType.DailyQuest);
            if (_isUnlocked)
            {
                StartUnlockedFlow();
            }

            Changed?.Invoke();
        }

        public void Dispose()
        {
            _save.Changed -= OnSaveChanged;
            _featureUnlockService.FeatureUnlocked -= OnFeatureUnlocked;
            StopProgressListening();
        }

        public DailyQuestBoardViewData GetViewData()
        {
            if (!IsFeatureUnlocked())
            {
                return CreateEmptyViewData();
            }

            EnsureToday();

            var questDatas = _config?.Quests;
            var quests = new List<DailyQuestViewData>(questDatas?.Count ?? 0);
            if (questDatas != null)
            {
                foreach (var data in questDatas)
                {
                    if (data == null || string.IsNullOrEmpty(data.Id))
                    {
                        continue;
                    }

                    var state = _save.GetQuestState(data.Id);
                    long targetValue = Math.Max(1, data.TargetValue);
                    long currentValue = Math.Min(targetValue, Math.Max(0, state.CurrentValue));
                    quests.Add(new DailyQuestViewData
                    {
                        Id = data.Id,
                        Title = _localization.Localize(data.TitleLocalizationKey, data.Title),
                        Description = _localization.Localize(data.DescriptionLocalizationKey, data.Description),
                        ProgressText = $"{currentValue}/{targetValue}",
                        CurrentValue = currentValue,
                        TargetValue = targetValue,
                        Progress = targetValue <= 0 ? 0f : Math.Min(1f, (float)currentValue / targetValue),
                        Points = data.Points,
                        IsCompleted = state.IsCompleted,
                        IsPointsClaimed = state.IsPointsAdded,
                        CanClaimPoints = state.IsCompleted && !state.IsPointsAdded
                    });
                }
            }

            var milestoneDatas = _config?.Milestones;
            var milestones = new List<DailyQuestMilestoneViewData>(milestoneDatas?.Count ?? 0);
            if (milestoneDatas != null)
            {
                foreach (var data in milestoneDatas)
                {
                    if (data == null || data.RequiredPoints <= 0)
                    {
                        continue;
                    }

                    milestones.Add(new DailyQuestMilestoneViewData
                    {
                        RequiredPoints = data.RequiredPoints,
                        IsUnlocked = _save.Points >= data.RequiredPoints,
                        IsClaimed = _save.IsMilestoneClaimed(data.RequiredPoints),
                        RewardText = CreateRewardText(data.Rewards)
                    });
                }
            }

            return new DailyQuestBoardViewData
            {
                Points = _save.Points,
                MaxPoints = GetMaxMilestonePoints(),
                PointsProgress = GetMaxMilestonePoints() <= 0 ? 0f : Math.Min(1f, (float)_save.Points / GetMaxMilestonePoints()),
                Quests = quests,
                Milestones = milestones
            };
        }

        public bool ClaimQuestPoints(string questId)
        {
            if (!IsFeatureUnlocked())
            {
                return false;
            }

            EnsureToday();

            if (string.IsNullOrEmpty(questId))
            {
                return false;
            }

            DailyQuestConfig.DailyQuestData data = FindQuest(questId);
            if (data == null)
            {
                return false;
            }

            var state = _save.GetQuestState(questId);
            if (!state.IsCompleted || state.IsPointsAdded)
            {
                return false;
            }

            state.Id = questId;
            state.IsPointsAdded = true;
            _save.SetQuestState(state);
            _save.AddPoints(data.Points);
            Changed?.Invoke();
            return true;
        }

        public bool ClaimMilestone(int requiredPoints)
        {
            if (!IsFeatureUnlocked())
            {
                return false;
            }

            EnsureToday();

            if (_save.Points < requiredPoints || _save.IsMilestoneClaimed(requiredPoints))
            {
                return false;
            }

            var milestone = FindMilestone(requiredPoints);
            if (milestone == null)
            {
                return false;
            }

            _rewardService.GiveRewards(milestone.Rewards);
            _save.SetMilestoneClaimed(requiredPoints);
            Changed?.Invoke();
            return true;
        }

        public void Set(DailyQuestSaveData data)
        {
            _save.Set(data);
            if (IsFeatureUnlocked())
            {
                EnsureToday();
            }
        }

        public DailyQuestSaveData Get()
        {
            if (IsFeatureUnlocked())
            {
                EnsureToday();
            }

            return _save.Get();
        }

        private void OnSaveChanged()
        {
            if (IsFeatureUnlocked())
            {
                Changed?.Invoke();
            }
        }

        private bool IsFeatureUnlocked()
        {
            return _isUnlocked || (_featureUnlockService != null && _featureUnlockService.IsUnlocked(PlayerFeatureType.DailyQuest));
        }

        private void StartUnlockedFlow()
        {
            _isUnlocked = true;
            EnsureToday();
            StartProgressListening();
            Changed?.Invoke();
        }

        private void StartProgressListening()
        {
            if (_isProgressListening)
            {
                return;
            }

            _gameStartRouter.OnClickValueEvent += OnClicked;
            _wallet.SoftAdded += OnSoftEarned;
            _shopModel.ItemBought += OnShopItemBought;
            _playerProgression.LevelCompleted += OnPlayerLevelCompleted;
            _isProgressListening = true;
        }

        private void StopProgressListening()
        {
            if (!_isProgressListening)
            {
                return;
            }

            _gameStartRouter.OnClickValueEvent -= OnClicked;
            _wallet.SoftAdded -= OnSoftEarned;
            _shopModel.ItemBought -= OnShopItemBought;
            _playerProgression.LevelCompleted -= OnPlayerLevelCompleted;
            _isProgressListening = false;
        }

        private void OnFeatureUnlocked(PlayerFeatureType feature)
        {
            if (feature == PlayerFeatureType.DailyQuest && !_isUnlocked)
            {
                StartUnlockedFlow();
            }
        }

        private DailyQuestBoardViewData CreateEmptyViewData()
        {
            return new DailyQuestBoardViewData
            {
                Points = 0,
                MaxPoints = GetMaxMilestonePoints(),
                PointsProgress = 0f,
                Quests = Array.Empty<DailyQuestViewData>(),
                Milestones = Array.Empty<DailyQuestMilestoneViewData>()
            };
        }

        private void EnsureToday()
        {
            string todayKey = GetTodayKey();
            if (_save.DayKey != todayKey)
            {
                _save.ResetForDay(todayKey);
            }
        }

        private string GetTodayKey()
        {
            long ticks = _timeService?.CurrentUtcTicks ?? DateTime.UtcNow.Ticks;
            return new DateTime(ticks, DateTimeKind.Utc).ToString("yyyyMMdd");
        }

        private void OnClicked(long clickValue)
        {
            AddProgress(QuestObjectiveType.Click, null, 1);
        }

        private void OnSoftEarned(long amount)
        {
            AddProgress(QuestObjectiveType.EarnSoft, null, amount);
        }

        private void OnShopItemBought(string itemId)
        {
            AddProgress(QuestObjectiveType.BuyShopItem, itemId, 1);
        }

        private void OnPlayerLevelCompleted(int completedLevel)
        {
            AddProgress(QuestObjectiveType.CompletePlayerLevel, null, 1);
        }

        private void AddProgress(QuestObjectiveType objectiveType, string targetId, long amount)
        {
            if (!IsFeatureUnlocked())
            {
                return;
            }

            EnsureToday();

            if (amount <= 0)
            {
                return;
            }

            var questDatas = _config?.Quests;
            if (questDatas == null)
            {
                return;
            }

            bool changed = false;
            foreach (var data in questDatas)
            {
                if (!CanProgress(data, objectiveType, targetId))
                {
                    continue;
                }

                var state = _save.GetQuestState(data.Id);
                if (state.IsCompleted)
                {
                    continue;
                }

                long targetValue = Math.Max(1, data.TargetValue);
                state.Id = data.Id;
                state.CurrentValue = Math.Min(targetValue, Math.Max(0, state.CurrentValue) + amount);
                state.IsCompleted = state.CurrentValue >= targetValue;
                _save.SetQuestState(state);
                changed = true;
            }

            if (changed)
            {
                Changed?.Invoke();
            }
        }

        private bool CanProgress(DailyQuestConfig.DailyQuestData data, QuestObjectiveType objectiveType, string targetId)
        {
            if (data == null || string.IsNullOrEmpty(data.Id) || data.ObjectiveType != objectiveType)
            {
                return false;
            }

            return string.IsNullOrEmpty(data.TargetId) || data.TargetId == targetId;
        }

        private DailyQuestConfig.DailyQuestMilestoneData FindMilestone(int requiredPoints)
        {
            var milestones = _config?.Milestones;
            if (milestones == null)
            {
                return null;
            }

            foreach (var milestone in milestones)
            {
                if (milestone != null && milestone.RequiredPoints == requiredPoints)
                {
                    return milestone;
                }
            }

            return null;
        }

        private DailyQuestConfig.DailyQuestData FindQuest(string questId)
        {
            var quests = _config?.Quests;
            if (quests == null)
            {
                return null;
            }

            foreach (var quest in quests)
            {
                if (quest != null && quest.Id == questId)
                {
                    return quest;
                }
            }

            return null;
        }

        private int GetMaxMilestonePoints()
        {
            int max = 0;
            var milestones = _config?.Milestones;
            if (milestones == null)
            {
                return max;
            }

            foreach (var milestone in milestones)
            {
                if (milestone != null && milestone.RequiredPoints > max)
                {
                    max = milestone.RequiredPoints;
                }
            }

            return max;
        }

        private string CreateRewardText(IReadOnlyList<QuestReward> rewards)
        {
            if (rewards == null || rewards.Count == 0)
            {
                return string.Empty;
            }

            var parts = new List<string>(rewards.Count);
            foreach (var reward in rewards)
            {
                if (reward == null)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(reward.DisplayTextLocalizationKey) || !string.IsNullOrEmpty(reward.DisplayText))
                {
                    parts.Add(_localization.Localize(reward.DisplayTextLocalizationKey, reward.DisplayText));
                }
                else if (reward.RewardType == QuestRewardType.Currency)
                {
                    string currency = _localization.Localize(
                        $"currency.{reward.CurrencyType.ToString().ToLowerInvariant()}",
                        reward.CurrencyType.ToString());
                    parts.Add(_localization.Format("reward.currency_amount", "+{0} {1}", reward.Amount, currency));
                }
                else
                {
                    parts.Add(string.IsNullOrEmpty(reward.RewardId) ? reward.RewardType.ToString() : reward.RewardId);
                }
            }

            return string.Join(", ", parts);
        }
    }
}
