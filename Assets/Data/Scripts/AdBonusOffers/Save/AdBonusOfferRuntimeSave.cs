using System;
using Core;

namespace AdBonusOffers
{
    public class AdBonusOfferRuntimeSave : ISavable<AdBonusOfferSaveData>
    {
        public string DayKey { get; private set; }
        public int ClaimsToday { get; private set; }
        public event Action Changed;

        public void ResetForDay(string dayKey)
        {
            if (DayKey == dayKey)
            {
                return;
            }

            DayKey = dayKey;
            ClaimsToday = 0;
            Changed?.Invoke();
        }

        public void AddClaim()
        {
            ClaimsToday++;
            Changed?.Invoke();
        }

        public void Set(AdBonusOfferSaveData data)
        {
            DayKey = data?.DayKey;
            ClaimsToday = Math.Max(0, data?.ClaimsToday ?? 0);
            Changed?.Invoke();
        }

        public AdBonusOfferSaveData Get()
        {
            return new AdBonusOfferSaveData
            {
                DayKey = DayKey,
                ClaimsToday = ClaimsToday
            };
        }
    }
}
