using System;
using System.Text;
using Core;
using Shop;
using UnityEngine;
using Zenject;
using GameLocalization;

namespace PlayerProgression
{
    public class PlayerProgressionService : IPlayerProgressionService, IInitializable, IDisposable
    {
        private GameConfig _config;
        private Wallet _wallet;
        private PlayerProgressionRuntimeSave _save;
        private LazyInject<IShopRuntimeSave> _shopSave;
        private ILocalizationService _localization;

        public int CurrentLevel => _save.CompletedLevels + 1;
        public long CurrentExperience => _save.CurrentExperience;
        public long ExperienceToNextLevel => GetLevelExperienceRequirement(_save.CompletedLevels);
        public float CurrentProgress => ExperienceToNextLevel <= 0 ? 0f : Math.Min(1f, (float)CurrentExperience / ExperienceToNextLevel);
        public bool CanCompleteLevel => ExperienceToNextLevel > 0 && CurrentExperience >= ExperienceToNextLevel;
        public string NextLevelRewardDescription => CreateRewardDescription(_save.CompletedLevels);
        public Sprite NextLevelRewardIcon => GetLevelData(_save.CompletedLevels)?.RewardIcon;
        public float ClickIncomeMultiplier => 1f + GetBonusPercent(PlayerProgressBonusType.ClickIncomePercent) / 100f;
        public float PassiveIncomeMultiplier => 1f + GetBonusPercent(PlayerProgressBonusType.PassiveIncomePercent) / 100f;

        public event Action Changed;
        public event Action<long> ExperienceAdded;
        public event Action<int> LevelCompleted;

        [Inject]
        public void Construct(GameConfig config, Wallet wallet, PlayerProgressionRuntimeSave save, LazyInject<IShopRuntimeSave> shopSave, ILocalizationService localization)
        {
            _config = config;
            _wallet = wallet;
            _save = save;
            _shopSave = shopSave;
            _localization = localization;
        }

        public void Initialize()
        {
            _save.Changed += OnSaveChanged;
            Changed?.Invoke();
        }

        public void Dispose()
        {
            _save.Changed -= OnSaveChanged;
        }

        public void AddExperience(PlayerExperienceSource source)
        {
            if (CanCompleteLevel)
            {
                return;
            }

            long amount = GetExperienceReward(source);
            if (amount <= 0)
            {
                return;
            }

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

        public bool CompleteLevel()
        {
            if (!CanCompleteLevel)
            {
                return false;
            }

            int completedLevels = _save.CompletedLevels + 1;
            _wallet.ResetSoft();
            _shopSave.Value.ResetLevels();
            _wallet.AddMiddle(GetMiddleRewardPerLevel());
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

        private bool HasEnoughExperienceForLevel(int completedLevels, long experience)
        {
            long requirement = GetLevelExperienceRequirement(completedLevels);
            return requirement > 0 && experience >= requirement;
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
            var level = GetLevelData(completedLevels);
            return level == null ? 0 : Math.Max(0, level.ExperienceToComplete);
        }

        private long GetMiddleRewardPerLevel()
        {
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
                        result += Math.Max(0f, bonus.Percent);
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

        private string CreateRewardDescription(int completedLevels)
        {
            var builder = new StringBuilder();
            long middleReward = GetMiddleRewardPerLevel();

            if (middleReward > 0)
            {
                builder.Append(_localization.Format("player_progression.reward.middle_currency", "+{0} decor currency", middleReward));
            }

            var level = GetLevelData(completedLevels);
            var bonuses = level?.Bonuses;
            if (bonuses != null)
            {
                foreach (var bonus in bonuses)
                {
                    if (bonus == null || bonus.Percent <= 0)
                    {
                        continue;
                    }

                    if (builder.Length > 0)
                    {
                        builder.Append("\n");
                    }

                    builder.Append(_localization.Format("player_progression.reward.bonus_percent", "+{0:0.#}% {1}", bonus.Percent, GetBonusName(bonus.Type)));
                }
            }

            return builder.Length > 0 ? builder.ToString() : _localization.Localize("player_progression.reward.new_level", "New level");
        }

        private string GetBonusName(PlayerProgressBonusType type)
        {
            switch (type)
            {
                case PlayerProgressBonusType.ClickIncomePercent:
                    return _localization.Localize("player_progression.bonus.click_income", "to click income");
                case PlayerProgressBonusType.PassiveIncomePercent:
                    return _localization.Localize("player_progression.bonus.passive_income", "to passive income");
                default:
                    return type.ToString();
            }
        }
    }
}
