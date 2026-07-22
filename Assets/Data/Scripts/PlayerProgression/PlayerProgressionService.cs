using System;
using System.Collections.Generic;
using AdBonusOffers;
using Core;
using Core.Ads;
using Shop;
using UnityEngine;
using Zenject;
using GameLocalization;
using QuestSystem;
using PlayerFeatures;

namespace PlayerProgression
{
    public class PlayerProgressionService : IPlayerProgressionService, IInitializable, IDisposable
    {
        private GameConfig _config;
        private Wallet _wallet;
        private PlayerProgressionRuntimeSave _save;
        private LazyInject<IShopRuntimeSave> _shopSave;
        private LazyInject<IRewardedAdsService> _rewardedAds;
        private ILocalizationService _localization;
        private IQuestRewardService _rewardService;
        private IBuffService _buffService;
        private PlayerFeatureUnlockConfig _featureUnlockConfig;

        public int CurrentLevel => _save.IsTutorialCompleted ? _save.CompletedLevels + 1 : 0;
        public long CurrentExperience => _save.IsTutorialCompleted ? _save.CurrentExperience : _save.TutorialClicks;
        public long ExperienceToNextLevel => _save.IsTutorialCompleted
            ? GetLevelExperienceRequirement(_save.CompletedLevels)
            : (_config?.PlayerProgression?.TutorialClickTarget ?? 100);
        public float CurrentProgress => ExperienceToNextLevel <= 0 ? 0f : Math.Min(1f, (float)CurrentExperience / ExperienceToNextLevel);
        public bool CanCompleteLevel => _save.IsTutorialCompleted && ExperienceToNextLevel > 0 && CurrentExperience >= ExperienceToNextLevel;
        public IReadOnlyList<LevelRewardEntry> NextLevelRewards => CreateRewardEntries(_save.CompletedLevels);
        public string NextLevelLossText => _localization.Format("You_want_new_level", CurrentLevel + 1);
        public float ClickIncomeMultiplier => 1f + GetBonusPercent(PlayerProgressBonusType.ClickIncomePercent) / 100f;
        public float PassiveIncomeMultiplier => 1f + GetBonusPercent(PlayerProgressBonusType.PassiveIncomePercent) / 100f;

        public event Action Changed;
        public event Action<long> ExperienceAdded;
        public event Action<int> LevelCompleted;

        [Inject]
        public void Construct(GameConfig config, Wallet wallet, PlayerProgressionRuntimeSave save, LazyInject<IShopRuntimeSave> shopSave, LazyInject<IRewardedAdsService> rewardedAds, ILocalizationService localization, IQuestRewardService rewardService, IBuffService buffService, PlayerFeatureUnlockConfig featureUnlockConfig)
        {
            _config = config;
            _wallet = wallet;
            _save = save;
            _shopSave = shopSave;
            _rewardedAds = rewardedAds;
            _localization = localization;
            _rewardService = rewardService;
            _buffService = buffService;
            _featureUnlockConfig = featureUnlockConfig;
        }

        public void Initialize()
        {
            _save.Changed += OnSaveChanged;
            _rewardedAds.Value.AdRewarded += OnAdRewarded;
            Changed?.Invoke();
        }

        public void Dispose()
        {
            _save.Changed -= OnSaveChanged;
            _rewardedAds.Value.AdRewarded -= OnAdRewarded;
        }

        public void AddExperience(PlayerExperienceSource source, long contextAmount = 0)
        {
            if (!_save.IsTutorialCompleted)
            {
                AddTutorialClick(source);
                return;
            }

            if (CanCompleteLevel)
            {
                return;
            }

            long amount = ResolveExperienceAmount(source, contextAmount);
            if (amount <= 0)
            {
                return;
            }

            float experienceMultiplier = _buffService?.ExperienceMultiplier ?? 1f;
            amount = Math.Max(1, (long)Math.Ceiling(amount * Math.Max(0f, experienceMultiplier)));

            int completedLevels = _save.CompletedLevels;
            long experience = _save.CurrentExperience + amount;

            ExperienceAdded?.Invoke(amount);

            long requirement = GetLevelExperienceRequirement(completedLevels);
            if (requirement > 0 && experience > requirement)
            {
                experience = requirement;
            }

            _save.SetState(completedLevels, experience);
        }

        private void AddTutorialClick(PlayerExperienceSource source)
        {
            if (source != PlayerExperienceSource.Click)
            {
                return;
            }

            int target = _config?.PlayerProgression?.TutorialClickTarget ?? 100;
            int clicks = Math.Min(target, _save.TutorialClicks + 1);
            _save.SetTutorialClicks(clicks);
            ExperienceAdded?.Invoke(1);

            if (clicks >= target)
            {
                _save.CompleteTutorial();
                LevelCompleted?.Invoke(CurrentLevel);
            }
        }

        public bool CompleteLevel()
        {
            if (!CanCompleteLevel)
            {
                return false;
            }

            int completedLevels = _save.CompletedLevels + 1;
            _wallet.ResetSoft();
            _shopSave.Value.ResetLevels();
            _wallet.AddMiddle(GetDecorReward(_save.CompletedLevels));
            _rewardService.GiveRewards(GetLevelData(_save.CompletedLevels)?.Rewards);
            _save.SetState(completedLevels, 0);
            _shopSave.Value.Recalculate(_config);
            LevelCompleted?.Invoke(completedLevels);
            return true;
        }

        public void Set(PlayerProgressionRuntimeSave.SaveData data)
        {
            _save.Set(data);
        }

        public PlayerProgressionRuntimeSave.SaveData Get()
        {
            return _save.Get();
        }

        private void OnSaveChanged()
        {
            Changed?.Invoke();
        }

        private void OnAdRewarded()
        {
            AddExperience(PlayerExperienceSource.AdWatched);
        }

        private bool HasEnoughExperienceForLevel(int completedLevels, long experience)
        {
            long requirement = GetLevelExperienceRequirement(completedLevels);
            return requirement > 0 && experience >= requirement;
        }

        private long ResolveExperienceAmount(PlayerExperienceSource source, long contextAmount)
        {
            return source switch
            {
                PlayerExperienceSource.ShopPurchase => GetPurchaseExperience(contextAmount),
                PlayerExperienceSource.QuestCompleted => Math.Max(0, contextAmount),
                PlayerExperienceSource.PercentBonus => Math.Max(0, contextAmount),
                _ => GetExperienceReward(source)
            };
        }

        private long GetPurchaseExperience(long priceSpent)
        {
            if (priceSpent <= 0)
            {
                return GetExperienceReward(PlayerExperienceSource.ShopPurchase);
            }

            float percent = Math.Max(0f, _config?.PlayerProgression?.PurchaseExperiencePercent ?? 0f);
            return (long)Math.Round(priceSpent * percent / 100f, MidpointRounding.AwayFromZero);
        }

        private long GetExperienceReward(PlayerExperienceSource source)
        {
            var rewards = _config?.PlayerProgression?.ExperienceRewards;
            if (rewards == null)
            {
                return 0;
            }

            foreach (var reward in rewards)
            {
                if (reward != null && reward.Source == source)
                {
                    return Math.Max(0, reward.Experience);
                }
            }

            return 0;
        }

        private long GetLevelExperienceRequirement(int completedLevels)
        {
            long baseExperience = Math.Max(1, _config?.PlayerProgression?.BaseExperienceToComplete ?? 1000);
            float growth = Math.Max(1f, _config?.PlayerProgression?.ExperienceGrowth ?? 1f);
            return (long)Math.Ceiling(baseExperience * Math.Pow(growth, completedLevels));
        }

        private long GetDecorReward(int completedLevels)
        {
            var level = GetLevelData(completedLevels);
            if (level != null && level.DecorReward > 0)
            {
                return level.DecorReward;
            }

            return Math.Max(0, _config?.PlayerProgression?.MiddleRewardPerLevel ?? 0);
        }

        private float GetBonusPercent(PlayerProgressBonusType type)
        {
            float result = 0f;
            for (int i = 0; i < _save.CompletedLevels; i++)
            {
                var level = GetLevelData(i);
                var bonuses = level?.Bonuses;
                if (bonuses == null)
                {
                    continue;
                }

                foreach (var bonus in bonuses)
                {
                    if (bonus != null && bonus.Type == type)
                    {
                        result += Math.Max(0f, bonus.Percent) *
                                  (_config?.PlayerProgression?.LevelBonusScale ?? 1f);
                    }
                }
            }

            return result;
        }

        private GameConfig.PlayerLevelData GetLevelData(int completedLevels)
        {
            var levels = _config?.PlayerProgression?.Levels;
            if (levels == null || levels.Count == 0)
            {
                return null;
            }

            return levels[completedLevels % levels.Count];
        }

        private List<LevelRewardEntry> CreateRewardEntries(int completedLevels)
        {
            var entries = new List<LevelRewardEntry>();

            long middleReward = GetDecorReward(completedLevels);
            if (middleReward > 0)
            {
                entries.Add(new LevelRewardEntry(
                    _config?.PlayerProgression?.MiddleRewardIcon,
                    _localization.Format("player_progression.reward.amount", middleReward),
                    _localization.Localize("player_progression.reward.middle_currency.description")));
            }

            var bonuses = GetLevelData(completedLevels)?.Bonuses;
            if (bonuses != null)
            {
                foreach (var bonus in bonuses)
                {
                    if (bonus == null || bonus.Percent <= 0)
                    {
                        continue;
                    }

                    entries.Add(new LevelRewardEntry(
                        bonus.Icon,
                        _localization.Format(
                            "player_progression.reward.percent",
                            bonus.Percent * (_config?.PlayerProgression?.LevelBonusScale ?? 1f)),
                        GetBonusDescription(bonus.Type)));
                }
            }

            var rewards = GetLevelData(completedLevels)?.Rewards;
            if (rewards != null)
            {
                foreach (var reward in rewards)
                {
                    if (reward == null)
                    {
                        continue;
                    }

                    string rewardText = !string.IsNullOrEmpty(reward.DisplayTextLocalizationKey)
                        ? _localization.Localize(reward.DisplayTextLocalizationKey)
                        : reward.DisplayText;
                    entries.Add(new LevelRewardEntry(reward.Icon, rewardText));
                }
            }

            AddFeatureUnlockEntries(entries, completedLevels + 2);

            return entries;
        }

        private void AddFeatureUnlockEntries(List<LevelRewardEntry> entries, int targetLevel)
        {
            var features = _featureUnlockConfig?.Features;
            if (features == null)
            {
                return;
            }

            foreach (PlayerFeatureUnlockData feature in features)
            {
                if (feature == null || !feature.IsShowable || feature.RequiredLevel != targetLevel)
                {
                    continue;
                }

                entries.Add(new LevelRewardEntry(
                    feature.Icon,
                    _localization.Localize($"player_features.unlock.{feature.Feature}"),
                    string.IsNullOrEmpty(feature.DescriptionLocalizationKey)
                        ? null
                        : _localization.Localize(feature.DescriptionLocalizationKey)));
            }
        }

        private string GetBonusDescription(PlayerProgressBonusType type)
        {
            return type switch
            {
                PlayerProgressBonusType.ClickIncomePercent => _localization.Localize("player_progression.bonus.click_income"),
                PlayerProgressBonusType.PassiveIncomePercent => _localization.Localize("player_progression.bonus.passive_income"),
                _ => null
            };
        }
    }
}
