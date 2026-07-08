using UnityEngine;

namespace AdBonusOffers
{
    public readonly struct AdBonusActiveEffectViewData
    {
        public AdBonusActiveEffectViewData(
            string id,
            AdBonusEffectType type,
            float multiplier,
            float discountPercent,
            float remainingSeconds,
            float durationSeconds)
        {
            Id = id;
            Type = type;
            Multiplier = multiplier;
            DiscountPercent = discountPercent;
            RemainingSeconds = remainingSeconds;
            DurationSeconds = durationSeconds;
        }

        public string Id { get; }
        public AdBonusEffectType Type { get; }
        public float Multiplier { get; }
        public float DiscountPercent { get; }
        public float RemainingSeconds { get; }
        public float DurationSeconds { get; }

        public float Progress01 => DurationSeconds > 0f
            ? Mathf.Clamp01(RemainingSeconds / DurationSeconds)
            : 0f;
    }
}
