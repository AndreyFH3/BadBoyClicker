using System;
using System.Collections.Generic;
using Core;
using Core.Ads;
using GameLocalization;
using QuestSystem;
using Utils;
using UnityEngine;
using Zenject;

namespace AdBonusOffers
{
    public class AdBonusOfferService : IAdBonusOfferService, IInitializable, ITickable, IDisposable
    {
        private readonly Dictionary<string, float> _cooldowns = new();
        private readonly List<string> _expiredCooldowns = new();
        private readonly List<string> _cooldownIds = new();
        private AdBonusOfferConfig _config;
        private IRewardedAdsService _adsService;
        private IQuestRewardService _rewardService;
        private IAdBonusEffectService _effectService;
        private ILocalizationService _localization;
        private Wallet _wallet;
        private AdBonusOfferConfig.AdBonusOfferData _currentOffer;
        private AdBonusOfferConfig.AdBonusOfferData _pendingRewardOffer;
        private float _nextOfferTimer;
        private float _visibleTimer;
        private bool _isClaimInProgress;
        private bool _isCurrentOfferTimerPaused;

        public event Action<AdBonusOfferViewData> OfferShown;
        public event Action OfferHidden;
        public event Action<AdBonusOfferViewData> RewardGranted;
        public event Action<AdBonusOfferViewData> RewardFailed;

        public bool HasActiveOffer => _currentOffer != null;
        public AdBonusOfferViewData CurrentOffer => CreateViewData(_currentOffer);
        public float CurrentOfferRemainingSeconds => HasActiveOffer ? Mathf.Max(0f, _visibleTimer) : 0f;
        private bool HasAnyActiveEffect => _effectService.ActiveEffects.Count > 0;

        [Inject]
        public void Construct(
            AdBonusOfferConfig config,
            IRewardedAdsService adsService,
            IQuestRewardService rewardService,
            IAdBonusEffectService effectService,
            ILocalizationService localization,
            Wallet wallet)
        {
            _config = config;
            _adsService = adsService;
            _rewardService = rewardService;
            _effectService = effectService;
            _localization = localization;
            _wallet = wallet;
        }

        public void Initialize()
        {
            _nextOfferTimer = _config.InitialDelaySeconds;
            _effectService.Changed += OnActiveEffectsChanged;
            Debug.Log($"Ad bonus offers initialized. Offers: {_config.Offers?.Count ?? 0}, first offer in: {_nextOfferTimer:0.#}s.");
        }

        public void Dispose()
        {
            _effectService.Changed -= OnActiveEffectsChanged;
        }

        private void OnActiveEffectsChanged()
        {
            if (HasActiveOffer && !_isClaimInProgress && HasAnyActiveEffect)
            {
                HideCurrentOffer();
            }
        }

        public void Tick()
        {
            TickCooldowns();

            if (_isClaimInProgress)
            {
                return;
            }

            if (HasActiveOffer)
            {
                if (!_isCurrentOfferTimerPaused)
                {
                    TickVisibleOffer();
                }

                return;
            }

            TickNextOffer();
        }

        public bool TryShowNextOffer()
        {
            if (HasActiveOffer || _isClaimInProgress || HasAnyActiveEffect)
            {
                return false;
            }

            _currentOffer = SelectOffer();
            if (_currentOffer == null)
            {
                ResetNextOfferTimer();
                return false;
            }

            _visibleTimer = _config.VisibleDurationSeconds;
            Debug.Log($"Ad bonus offer shown: {_currentOffer.Id}. Visible for {_visibleTimer:0.#}s.");
            OfferShown?.Invoke(CurrentOffer);
            return true;
        }

        public void HideCurrentOffer()
        {
            if (!HasActiveOffer)
            {
                return;
            }

            _currentOffer = null;
            _visibleTimer = 0f;
            _isCurrentOfferTimerPaused = false;
            ResetNextOfferTimer();
            OfferHidden?.Invoke();
        }

        public void SetCurrentOfferTimerPaused(bool isPaused)
        {
            _isCurrentOfferTimerPaused = isPaused && HasActiveOffer;
        }

        public void ClaimCurrentOffer()
        {
            if (!HasActiveOffer || _isClaimInProgress)
            {
                return;
            }

            if (HasAnyActiveEffect)
            {
                HideCurrentOffer();
                return;
            }

            var offer = _currentOffer;
            var viewData = CurrentOffer;

            if (!_adsService.IsAvailable(offer.PlacementId))
            {
                FailClaim(viewData);
                return;
            }

            _isClaimInProgress = true;

            _adsService.Show(
                offer.PlacementId,
                () => CompleteClaim(offer, viewData),
                () => FailClaim(viewData));
        }

        public void ClaimCurrentOfferForHard()
        {
            if (!HasActiveOffer || _isClaimInProgress || !CanClaimForHard(_currentOffer))
            {
                return;
            }

            if (!_wallet.SpendHard(_currentOffer.HardPrice))
            {
                RewardFailed?.Invoke(CurrentOffer);
                return;
            }

            CompleteClaim(_currentOffer, CurrentOffer);
        }

        private void CompleteClaim(AdBonusOfferConfig.AdBonusOfferData offer, AdBonusOfferViewData viewData)
        {
            _pendingRewardOffer = offer;
            StartCooldown(offer);
            _isClaimInProgress = false;
            _currentOffer = null;
            _visibleTimer = 0f;
            _isCurrentOfferTimerPaused = false;
            ResetNextOfferTimer();
            OfferHidden?.Invoke();
            RewardGranted?.Invoke(viewData);
        }

        public void CompleteRewardPresentation()
        {
            if (_pendingRewardOffer == null)
            {
                return;
            }

            var offer = _pendingRewardOffer;
            _pendingRewardOffer = null;

            _rewardService.GiveRewards(offer.Rewards);
            GiveEffects(offer.Effects);
        }

        private void FailClaim(AdBonusOfferViewData viewData)
        {
            _isClaimInProgress = false;
            RewardFailed?.Invoke(viewData);
        }

        private void TickCooldowns()
        {
            if (_cooldowns.Count == 0)
            {
                return;
            }

            _expiredCooldowns.Clear();
            _cooldownIds.Clear();

            foreach (var pair in _cooldowns)
            {
                _cooldownIds.Add(pair.Key);
            }

            foreach (string id in _cooldownIds)
            {
                float value = _cooldowns[id] - Time.deltaTime;
                _cooldowns[id] = value;

                if (value <= 0f)
                {
                    _expiredCooldowns.Add(id);
                }
            }

            foreach (string id in _expiredCooldowns)
            {
                _cooldowns.Remove(id);
            }

            _expiredCooldowns.Clear();
            _cooldownIds.Clear();
        }

        private void TickVisibleOffer()
        {
            _visibleTimer -= Time.deltaTime;
            if (_visibleTimer <= 0f)
            {
                HideCurrentOffer();
            }
        }

        private void TickNextOffer()
        {
            _nextOfferTimer -= Time.deltaTime;
            if (_nextOfferTimer <= 0f)
            {
                TryShowNextOffer();
            }
        }

        private void ResetNextOfferTimer()
        {
            _nextOfferTimer = _config.OfferIntervalSeconds;
        }

        private AdBonusOfferConfig.AdBonusOfferData SelectOffer()
        {
            var offers = _config.Offers;
            if (offers == null || offers.Count == 0)
            {
                return null;
            }

            int totalWeight = 0;
            foreach (var offer in offers)
            {
                if (IsOfferAvailable(offer))
                {
                    totalWeight += offer.Weight;
                }
            }

            if (totalWeight <= 0)
            {
                return null;
            }

            int roll = UnityEngine.Random.Range(0, totalWeight);
            foreach (var offer in offers)
            {
                if (!IsOfferAvailable(offer))
                {
                    continue;
                }

                roll -= offer.Weight;
                if (roll < 0)
                {
                    return offer;
                }
            }

            return null;
        }

        private bool IsOfferAvailable(AdBonusOfferConfig.AdBonusOfferData offer)
        {
            return offer != null &&
                   !string.IsNullOrEmpty(offer.Id) &&
                   !_cooldowns.ContainsKey(offer.Id) &&
                   (HasRewards(offer) || HasEffects(offer));
        }

        private bool HasRewards(AdBonusOfferConfig.AdBonusOfferData offer)
        {
            return offer.Rewards != null && offer.Rewards.Count > 0;
        }

        private bool HasEffects(AdBonusOfferConfig.AdBonusOfferData offer)
        {
            return offer.Effects != null && offer.Effects.Count > 0;
        }

        private void GiveEffects(IReadOnlyList<AdBonusOfferConfig.AdBonusEffectData> effects)
        {
            if (effects == null)
            {
                return;
            }

            foreach (var effect in effects)
            {
                _effectService.Apply(effect);
            }
        }

        private void StartCooldown(AdBonusOfferConfig.AdBonusOfferData offer)
        {
            if (offer.CooldownSeconds > 0f)
            {
                _cooldowns[offer.Id] = offer.CooldownSeconds;
            }
        }

        private AdBonusOfferViewData CreateViewData(AdBonusOfferConfig.AdBonusOfferData offer)
        {
            if (offer == null)
            {
                return default;
            }

            return new AdBonusOfferViewData(
                offer.Id,
                ResolveRewardTitle(offer),
                ResolveRewardValueText(offer),
                BuildConfirmationDescription(offer),
                BuildResultDescription(offer),
                offer.Icon,
                _adsService.IsAvailable(offer.PlacementId),
                CanClaimForHard(offer),
                offer.HardPrice);
        }

        private string ResolveRewardTitle(AdBonusOfferConfig.AdBonusOfferData offer)
        {
            if (HasRewards(offer))
            {
                var reward = offer.Rewards[0];
                if (reward == null)
                {
                    return LocalizeOrFallback(offer.ConfirmationTitleLocalizationKey, offer.ConfirmationTitle);
                }

                if (reward.RewardType == QuestRewardType.Currency)
                {
                    return _localization.Localize(GetCurrencyNameKey(reward.CurrencyType));
                }

                if (!string.IsNullOrEmpty(reward.DisplayTextLocalizationKey))
                {
                    return _localization.Localize(reward.DisplayTextLocalizationKey);
                }

                return string.IsNullOrEmpty(reward.DisplayText) ? reward.RewardId : reward.DisplayText;
            }

            return LocalizeOrFallback(offer.ConfirmationTitleLocalizationKey, offer.ConfirmationTitle);
        }

        private string ResolveRewardValueText(AdBonusOfferConfig.AdBonusOfferData offer)
        {
            if (!HasRewards(offer))
            {
                return string.Empty;
            }

            var reward = offer.Rewards[0];
            if (reward != null && reward.RewardType == QuestRewardType.Currency && reward.Amount > 0)
            {
                return $"+{reward.Amount.ConvertFromLongToString()}";
            }

            return string.Empty;
        }

        private string BuildConfirmationDescription(AdBonusOfferConfig.AdBonusOfferData offer)
        {
            string description = LocalizeOrFallback(offer.ConfirmationDescriptionLocalizationKey, offer.ConfirmationDescription);
            return AppendDuration(offer, description);
        }

        private string BuildResultDescription(AdBonusOfferConfig.AdBonusOfferData offer)
        {
            string description = LocalizeOrFallback(offer.ResultDescriptionLocalizationKey, offer.ResultDescription);
            return AppendDuration(offer, description);
        }

        private string AppendDuration(AdBonusOfferConfig.AdBonusOfferData offer, string description)
        {
            var effect = GetFirstTimedEffect(offer);
            if (effect == null || effect.DurationSeconds <= 0f)
            {
                return description;
            }

            string durationLine = _localization.Format("ad_bonus.duration", Mathf.RoundToInt(effect.DurationSeconds));
            return string.IsNullOrEmpty(description) ? durationLine : $"{description}\n{durationLine}";
        }

        private AdBonusOfferConfig.AdBonusEffectData GetFirstTimedEffect(AdBonusOfferConfig.AdBonusOfferData offer)
        {
            if (!HasEffects(offer))
            {
                return null;
            }

            foreach (var effect in offer.Effects)
            {
                if (effect != null && effect.EffectType != AdBonusEffectType.QuestReward)
                {
                    return effect;
                }
            }

            return null;
        }

        private string GetCurrencyNameKey(QuestRewardCurrencyType currencyType)
        {
            switch (currencyType)
            {
                case QuestRewardCurrencyType.Soft:
                    return "currency.soft";
                case QuestRewardCurrencyType.Decor:
                    return "currency.decor";
                case QuestRewardCurrencyType.Hard:
                    return "currency.hard";
                case QuestRewardCurrencyType.Yan:
                    return "currency.yan";
                default:
                    return "currency.soft";
            }
        }

        private string LocalizeOrFallback(string key, string fallback)
        {
            if (string.IsNullOrEmpty(key))
            {
                return fallback;
            }

            string localized = _localization.Localize(key);
            return string.IsNullOrEmpty(localized) || localized == key ? fallback : localized;
        }

        private bool CanClaimForHard(AdBonusOfferConfig.AdBonusOfferData offer)
        {
            if (offer == null || offer.HardPrice <= 0 || HasEffects(offer))
            {
                return false;
            }

            if (offer.Rewards == null || offer.Rewards.Count != 1)
            {
                return false;
            }

            var reward = offer.Rewards[0];
            return reward != null &&
                   reward.RewardType == QuestRewardType.Currency &&
                   reward.CurrencyType == QuestRewardCurrencyType.Soft &&
                   reward.Amount > 0;
        }
    }
}
