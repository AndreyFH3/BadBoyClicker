using System.Collections.Generic;
using DailyLogin;
using QuestSystem;
using UnityEngine;
using Zenject;

namespace AdBonusOffers
{
    public class AdBonusEffectService : IAdBonusEffectService, IBoostRewardService, ITickable
    {
        private const float MaxTickDeltaSeconds = 1f;

        private readonly List<ActiveEffect> _activeEffects = new();
        private readonly List<AdBonusActiveEffectViewData> _activeEffectViewData = new();
        private AdBonusOfferConfig _config;
        private IQuestRewardService _rewardService;

        public event System.Action Changed;

        public float ClickIncomeMultiplier => GetMaxMultiplier(AdBonusEffectType.ClickIncomeMultiplier);
        public float PassiveIncomeMultiplier => GetMaxMultiplier(AdBonusEffectType.PassiveIncomeMultiplier);
        public float ShopPriceMultiplier => 1f - GetMaxDiscountPercent() / 100f;
        public IReadOnlyList<AdBonusActiveEffectViewData> ActiveEffects => _activeEffectViewData;

        [Inject]
        public void Construct(AdBonusOfferConfig config, IQuestRewardService rewardService)
        {
            _config = config;
            _rewardService = rewardService;
        }

        public void Tick()
        {
            TickEffectTimers();

            bool changed = RemoveExpiredEffects();

            RebuildViewData();

            if (changed)
            {
                Changed?.Invoke();
            }
        }

        private void TickEffectTimers()
        {
            if (Time.timeScale <= 0f)
            {
                return;
            }

            float delta = Mathf.Min(Time.unscaledDeltaTime, MaxTickDeltaSeconds);

            foreach (var effect in _activeEffects)
            {
                effect.RemainingSeconds -= delta;
            }
        }

        private bool RemoveExpiredEffects()
        {
            bool removed = false;

            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                if (_activeEffects[i].IsExpired)
                {
                    _activeEffects.RemoveAt(i);
                    removed = true;
                }
            }

            return removed;
        }

        public void Apply(AdBonusOfferConfig.AdBonusEffectData effect)
        {
            if (effect == null)
            {
                return;
            }

            switch (effect.EffectType)
            {
                case AdBonusEffectType.QuestReward:
                    _rewardService.GiveReward(effect.Reward);
                    break;
                case AdBonusEffectType.ClickIncomeMultiplier:
                case AdBonusEffectType.PassiveIncomeMultiplier:
                case AdBonusEffectType.ShopDiscountPercent:
                    AddTimedEffect(effect);
                    break;
                default:
                    Debug.LogWarning($"Unsupported ad bonus effect type: {effect.EffectType}");
                    break;
            }
        }

        public void GiveBoost(string boostId)
        {
            var boost = FindBoost(boostId);
            if (boost == null)
            {
                Debug.LogWarning($"Ad bonus boost config was not found: {boostId}");
                return;
            }

            Apply(boost);
        }

        private void AddTimedEffect(AdBonusOfferConfig.AdBonusEffectData effect)
        {
            if (effect.DurationSeconds <= 0f)
            {
                return;
            }

            _activeEffects.Add(new ActiveEffect(effect));
            RebuildViewData();
            Changed?.Invoke();
        }

        private void RebuildViewData()
        {
            _activeEffectViewData.Clear();

            foreach (var effect in _activeEffects)
            {
                if (effect.IsExpired)
                {
                    continue;
                }

                _activeEffectViewData.Add(new AdBonusActiveEffectViewData(
                    effect.Id,
                    effect.Type,
                    effect.Multiplier,
                    effect.DiscountPercent,
                    effect.RemainingSeconds,
                    effect.DurationSeconds));
            }
        }

        private AdBonusOfferConfig.AdBonusEffectData FindBoost(string boostId)
        {
            var boosts = _config?.Boosts;
            if (string.IsNullOrEmpty(boostId) || boosts == null)
            {
                return null;
            }

            foreach (var boost in boosts)
            {
                if (boost != null && boost.Id == boostId)
                {
                    return boost;
                }
            }

            return null;
        }

        private float GetMaxMultiplier(AdBonusEffectType type)
        {
            float result = 1f;

            foreach (var effect in _activeEffects)
            {
                if (effect.Type == type && !effect.IsExpired)
                {
                    result = Mathf.Max(result, effect.Multiplier);
                }
            }

            return result;
        }

        private float GetMaxDiscountPercent()
        {
            float result = 0f;

            foreach (var effect in _activeEffects)
            {
                if (effect.Type == AdBonusEffectType.ShopDiscountPercent && !effect.IsExpired)
                {
                    result = Mathf.Max(result, effect.DiscountPercent);
                }
            }

            return result;
        }

        private class ActiveEffect
        {
            public readonly AdBonusEffectType Type;
            public readonly string Id;
            public readonly float Multiplier;
            public readonly float DiscountPercent;
            public readonly float DurationSeconds;
            public float RemainingSeconds;

            public ActiveEffect(AdBonusOfferConfig.AdBonusEffectData data)
            {
                Id = data.Id;
                Type = data.EffectType;
                Multiplier = data.Multiplier;
                DiscountPercent = data.DiscountPercent;
                DurationSeconds = data.DurationSeconds;
                RemainingSeconds = data.DurationSeconds;
            }

            public bool IsExpired => RemainingSeconds <= 0f;
        }
    }
}
