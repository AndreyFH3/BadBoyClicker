using System;
using System.Collections.Generic;
using Core;
using Core.Ads;
using Core.Time;
using Installer.Init;
using PlayerProgression;
using QuestSystem;
using Rewards;
using Shop;
using Utils;
using UnityEngine;
using Zenject;
using GameLocalization;
using RewardActivation;

namespace DailyQuests
{
    public class DailyQuestService : IDailyQuestService, IInitializable, IDisposable, ITickable
    {
        private const float BaseIncomeFallback = 50f;

        private DailyQuestConfig _config;
        private DailyQuestRuntimeSave _save;
        private IQuestRewardService _rewardService;
        private ITimeService _timeService;
        private GameStartRouter _gameStartRouter;
        private Wallet _wallet;
        private IShopModel _shopModel;
        private IPlayerProgressionService _playerProgression;
        private IRewardedAdsService _rewardedAds;
        private ILocalizationService _localization;
        private IRewardActivationService _rewardActivationService;
        private readonly HashSet<int> _pendingMilestoneClaims = new();
        private bool _isProgressListening;
        private float _sessionSeconds;
        private long _sessionSoftEarned;

        public int Points => _save.Points;
        public event Action Changed;
        public event Action<string, int> QuestPointsClaimed;
        public event Action<int> MilestoneClaimed;

        [Inject]
        public void Construct(
            DailyQuestConfig config,
            DailyQuestRuntimeSave save,
            IQuestRewardService rewardService,
            ITimeService timeService,
            GameStartRouter gameStartRouter,
            Wallet wallet,
            IShopModel shopModel,
            IPlayerProgressionService playerProgression,
            IRewardedAdsService rewardedAds,
            ILocalizationService localization,
            IRewardActivationService rewardActivationService)
        {
            _config = config;
            _save = save;
            _rewardService = rewardService;
            _timeService = timeService;
            _gameStartRouter = gameStartRouter;
            _wallet = wallet;
            _shopModel = shopModel;
            _playerProgression = playerProgression;
            _rewardedAds = rewardedAds;
            _localization = localization;
            _rewardActivationService = rewardActivationService;
        }

        public void Initialize()
        {
            _save.Changed += OnSaveChanged;
            Application.quitting += OnApplicationQuitting;

            ResetSessionQuests();
            EnsureToday();
            StartProgressListening();

            Changed?.Invoke();
        }

        public void Tick()
        {
            _sessionSeconds += UnityEngine.Time.deltaTime;
            UpdateMaxProgress(QuestObjectiveType.PlayTime, null, (long)_sessionSeconds);
        }

        public void Dispose()
        {
            _save.Changed -= OnSaveChanged;
            Application.quitting -= OnApplicationQuitting;
            StopProgressListening();
        }

        private void OnApplicationQuitting()
        {
            CheckpointAvgIncome();
        }

        public DailyQuestBoardViewData GetViewData()
        {
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
                    long targetValue = GetTargetValue(data, state);
                    long currentValue = Math.Min(targetValue, Math.Max(0, state.CurrentValue));
                    quests.Add(new DailyQuestViewData
                    {
                        Id = data.Id,
                        Title = _localization.Localize(data.TitleLocalizationKey),
                        Description = _localization.Format(data.DescriptionLocalizationKey, targetValue.ConvertFromLongToString()),
                        Icon = data.Icon,
                        ProgressText = $"{currentValue}/{targetValue}",
                        CurrentValue = currentValue,
                        TargetValue = targetValue,
                        Progress = targetValue <= 0 ? 0f : Math.Min(1f, (float)currentValue / targetValue),
                        Points = data.Points,
                        IsCompleted = state.IsCompleted,
                        IsPointsClaimed = state.IsPointsAdded,
                        CanClaimPoints = state.IsCompleted && !state.IsPointsAdded,
                        Rewards = BuildRewardDisplays(data.Rewards)
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

                    bool isClaimed = _save.IsMilestoneClaimed(data.RequiredPoints);
                    milestones.Add(new DailyQuestMilestoneViewData
                    {
                        RequiredPoints = data.RequiredPoints,
                        IsClaimed = isClaimed,
                        CanClaim = _save.Points >= data.RequiredPoints && !isClaimed && !_pendingMilestoneClaims.Contains(data.RequiredPoints),
                        Rewards = BuildRewardDisplays(data.Rewards)
                    });
                }
            }

            int maxMilestonePoints = GetMaxMilestonePoints();
            return new DailyQuestBoardViewData
            {
                Points = _save.Points,
                MaxPoints = maxMilestonePoints,
                PointsProgress = maxMilestonePoints <= 0 ? 0f : Math.Min(1f, (float)_save.Points / maxMilestonePoints),
                Quests = quests,
                Milestones = milestones
            };
        }

        public bool ClaimQuestPoints(string questId)
        {
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
            _rewardService.GiveRewards(data.Rewards);
            QuestPointsClaimed?.Invoke(questId, data.Points);
            Changed?.Invoke();
            return true;
        }

        public bool ClaimMilestone(int requiredPoints)
        {
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

            if (!_pendingMilestoneClaims.Add(requiredPoints))
            {
                return false;
            }

            QuestReward previewReward = milestone.Rewards != null && milestone.Rewards.Count > 0
                ? milestone.Rewards[0]
                : null;

            if (previewReward == null || _rewardActivationService == null)
            {
                CompleteMilestoneClaim(requiredPoints, milestone);
                return true;
            }

            _rewardActivationService.Enqueue(
                previewReward,
                () => CompleteMilestoneClaim(requiredPoints, milestone),
                () => PostponeMilestoneClaim(requiredPoints));
            return true;
        }

        private void PostponeMilestoneClaim(int requiredPoints)
        {
            if (_pendingMilestoneClaims.Remove(requiredPoints))
            {
                Changed?.Invoke();
            }
        }

        private void CompleteMilestoneClaim(int requiredPoints, DailyQuestConfig.DailyQuestMilestoneData milestone)
        {
            _pendingMilestoneClaims.Remove(requiredPoints);
            EnsureToday();

            if (_save.Points < requiredPoints || _save.IsMilestoneClaimed(requiredPoints))
            {
                return;
            }

            _rewardService.GiveRewardsImmediately(milestone.Rewards);
            _save.SetMilestoneClaimed(requiredPoints);
            MilestoneClaimed?.Invoke(requiredPoints);
            Changed?.Invoke();
        }

        public void Set(DailyQuestSaveData data)
        {
            _save.Set(WithSessionProgressStripped(data));
            EnsureToday();
        }

        public DailyQuestSaveData Get()
        {
            EnsureToday();

            return WithSessionProgressStripped(_save.Get());
        }

        // PerSession quests (e.g. play time) only track live progress via an in-memory
        // counter that restarts at 0 every launch. Persisting the raw CurrentValue let a
        // stale number from a previous session survive the reload and block the UI from
        // showing any progress until this session's counter caught back up to it - so only
        // completion state is kept, not the elapsed-time value itself.
        private DailyQuestSaveData WithSessionProgressStripped(DailyQuestSaveData data)
        {
            if (data?.Quests == null || data.Quests.Length == 0)
            {
                return data;
            }

            for (int i = 0; i < data.Quests.Length; i++)
            {
                var quest = data.Quests[i];
                if (!quest.IsCompleted && IsPerSessionResetQuest(quest.Id))
                {
                    quest.CurrentValue = 0;
                    data.Quests[i] = quest;
                }
            }

            return data;
        }

        private bool IsPerSessionResetQuest(string questId)
        {
            var data = FindQuest(questId);
            return data != null && data.ResetPolicy == DailyQuestResetPolicy.PerSession;
        }

        private void OnSaveChanged()
        {
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
            _wallet.SoftSpent += OnSoftSpent;
            _wallet.OnChanged += OnWalletChanged;
            _shopModel.ItemBought += OnShopItemBought;
            _playerProgression.LevelCompleted += OnPlayerLevelCompleted;
            _rewardedAds.AdRewarded += OnAdRewarded;
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
            _wallet.SoftSpent -= OnSoftSpent;
            _wallet.OnChanged -= OnWalletChanged;
            _shopModel.ItemBought -= OnShopItemBought;
            _playerProgression.LevelCompleted -= OnPlayerLevelCompleted;
            _rewardedAds.AdRewarded -= OnAdRewarded;
            _isProgressListening = false;
        }

        private void EnsureToday()
        {
            string todayKey = GetTodayKey();
            if (_save.DayKey != todayKey)
            {
                _save.ResetForDay(todayKey, GetUntilCompletedQuestIds());
            }

            EnsureBaselines();
            EnsureEffectiveTargets();
        }

        private void EnsureBaselines()
        {
            var questDatas = _config?.Quests;
            if (questDatas == null)
            {
                return;
            }

            foreach (var data in questDatas)
            {
                if (data == null || string.IsNullOrEmpty(data.Id) || data.ObjectiveType != QuestObjectiveType.PlayerLevel)
                {
                    continue;
                }

                var state = _save.GetQuestState(data.Id);
                if (state.IsCompleted || state.HasBaseline)
                {
                    continue;
                }

                state.Id = data.Id;
                state.HasBaseline = true;
                state.BaselineValue = _playerProgression.CurrentLevel;
                _save.SetQuestState(state);
            }
        }

        private void EnsureEffectiveTargets()
        {
            var questDatas = _config?.Quests;
            if (questDatas == null)
            {
                return;
            }

            foreach (var data in questDatas)
            {
                if (data == null || string.IsNullOrEmpty(data.Id) || !UsesIncomeScaledTarget(data.ObjectiveType))
                {
                    continue;
                }

                var state = _save.GetQuestState(data.Id);
                if (state.IsCompleted || state.HasEffectiveTarget)
                {
                    continue;
                }

                state.Id = data.Id;
                state.HasEffectiveTarget = true;
                state.EffectiveTargetValue = GetScaledTarget(data, GetAvgIncomePerSecond());
                _save.SetQuestState(state);
            }
        }

        private static bool UsesIncomeScaledTarget(QuestObjectiveType type)
        {
            return type == QuestObjectiveType.Balance
                || type == QuestObjectiveType.TotalEarned
                || type == QuestObjectiveType.ShopSpent;
        }

        private long GetTargetValue(DailyQuestConfig.DailyQuestData data, DailyQuestSaveData.DailyQuestState state)
        {
            if (UsesIncomeScaledTarget(data.ObjectiveType) && state.HasEffectiveTarget)
            {
                return Math.Max(1, state.EffectiveTargetValue);
            }

            return Math.Max(1, data.TargetValue);
        }

        private float GetAvgIncomePerSecond()
        {
            float avgIncome = _save.AvgIncomePerSecond;
            return avgIncome > 0f ? avgIncome : BaseIncomeFallback;
        }

        // Checkpoints the running session's average soft income (earned ÷ elapsed
        // seconds) so the next day's quest targets scale to how the player has
        // actually been playing, not a live snapshot that could swing wildly.
        private void CheckpointAvgIncome()
        {
            if (_sessionSeconds <= 0f)
            {
                return;
            }

            float avgIncome = _sessionSoftEarned / _sessionSeconds;
            _save.SetAvgIncomePerSecond(avgIncome);
        }

        // Target = how much of this currency the player's recent average income
        // would produce over targetTime, floored at minTarget so early/low-income
        // players still get a meaningful goal.
        private static long GetScaledTarget(DailyQuestConfig.DailyQuestData data, float avgIncomePerSecond)
        {
            long minTarget = Math.Max(1, data?.TargetValue ?? 1);
            double targetByIncome = Math.Max(0f, avgIncomePerSecond) *
                                    Math.Max(0f, data?.IncomeMinutes ?? 0f) * 60d;
            if (targetByIncome >= long.MaxValue)
            {
                return long.MaxValue;
            }

            return Math.Max((long)Math.Ceiling(targetByIncome), minTarget);
        }

        private List<string> GetUntilCompletedQuestIds()
        {
            var ids = new List<string>();
            var questDatas = _config?.Quests;
            if (questDatas == null)
            {
                return ids;
            }

            foreach (var data in questDatas)
            {
                if (data != null && !string.IsNullOrEmpty(data.Id) && data.ResetPolicy == DailyQuestResetPolicy.UntilCompleted)
                {
                    ids.Add(data.Id);
                }
            }

            return ids;
        }

        private void ResetSessionQuests()
        {
            _sessionSeconds = 0f;
            _sessionSoftEarned = 0;

            var questDatas = _config?.Quests;
            if (questDatas == null)
            {
                return;
            }

            foreach (var data in questDatas)
            {
                if (data == null || string.IsNullOrEmpty(data.Id) || data.ResetPolicy != DailyQuestResetPolicy.PerSession)
                {
                    continue;
                }

                _save.SetQuestState(new DailyQuestSaveData.DailyQuestState { Id = data.Id });
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
            _sessionSoftEarned += amount;
            AddProgress(QuestObjectiveType.TotalEarned, null, amount);
        }

        private void OnSoftSpent(long amount)
        {
            AddProgress(QuestObjectiveType.ShopSpent, null, amount);
        }

        private void OnWalletChanged()
        {
            UpdateMaxProgress(QuestObjectiveType.Balance, null, _wallet.Soft);
        }

        private void OnShopItemBought(string itemId)
        {
            AddProgress(QuestObjectiveType.ShopBuy, itemId, 1);
        }

        private void OnPlayerLevelCompleted(int completedLevel)
        {
            CheckpointAvgIncome();
            UpdateDeltaProgress(QuestObjectiveType.PlayerLevel, null, _playerProgression.CurrentLevel);
        }

        private void OnAdRewarded()
        {
            AddProgress(QuestObjectiveType.WatchAd, null, 1);
        }

        private void AddProgress(QuestObjectiveType objectiveType, string targetId, long amount)
        {
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

                long targetValue = GetTargetValue(data, state);
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

        private void UpdateMaxProgress(QuestObjectiveType objectiveType, string targetId, long absoluteValue)
        {
            EnsureToday();

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
                if (state.IsCompleted || absoluteValue <= state.CurrentValue)
                {
                    continue;
                }

                long targetValue = GetTargetValue(data, state);
                state.Id = data.Id;
                state.CurrentValue = Math.Min(targetValue, absoluteValue);
                state.IsCompleted = state.CurrentValue >= targetValue;
                _save.SetQuestState(state);
                changed = true;
            }

            if (changed)
            {
                Changed?.Invoke();
            }
        }

        private void UpdateDeltaProgress(QuestObjectiveType objectiveType, string targetId, long currentAbsoluteValue)
        {
            EnsureToday();

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
                if (state.IsCompleted || !state.HasBaseline)
                {
                    continue;
                }

                long targetValue = GetTargetValue(data, state);
                long delta = Math.Max(0, currentAbsoluteValue - state.BaselineValue);
                if (delta <= state.CurrentValue)
                {
                    continue;
                }

                state.Id = data.Id;
                state.CurrentValue = Math.Min(targetValue, delta);
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

        private static List<RewardDisplay> BuildRewardDisplays(IReadOnlyList<QuestReward> rewards)
        {
            if (rewards == null || rewards.Count == 0)
            {
                return null;
            }

            var displays = new List<RewardDisplay>(rewards.Count);
            foreach (var reward in rewards)
            {
                if (reward == null)
                {
                    continue;
                }

                displays.Add(new RewardDisplay
                {
                    Icon = reward.Icon,
                    Amount = reward.Amount > 0 ? reward.Amount.ConvertFromLongToString() : string.Empty
                });
            }

            return displays;
        }
    }
}
