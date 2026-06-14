namespace AdBonusOffers
{
    public readonly struct AdBonusActiveEffectViewData
    {
        public AdBonusActiveEffectViewData(
            string id,
            AdBonusEffectType type,
            float multiplier,
            float discountPercent,
            float remainingSeconds)
        {
            Id = id;
            Type = type;
            Multiplier = multiplier;
            DiscountPercent = discountPercent;
            RemainingSeconds = remainingSeconds;
        }

        public string Id { get; }
        public AdBonusEffectType Type { get; }
        public float Multiplier { get; }
        public float DiscountPercent { get; }
        public float RemainingSeconds { get; }
    }
}
