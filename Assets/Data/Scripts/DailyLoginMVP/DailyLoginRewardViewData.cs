using UnityEngine;

namespace DailyLoginMVP
{
    public readonly struct DailyLoginRewardViewData
    {
        public readonly Sprite Icon;
        public readonly string Text;
        public readonly string DayText;
        public readonly bool IsResource;
        public readonly bool IsCurrent;
        public readonly bool IsClaimed;
        public readonly bool IsLocked;
        public readonly bool IsMilestone;

        public DailyLoginRewardViewData(Sprite icon, string text, string dayText, bool isResource, bool isCurrent, bool isClaimed, bool isLocked, bool isMilestone)
        {
            Icon = icon;
            Text = text;
            DayText = dayText;
            IsResource = isResource;
            IsCurrent = isCurrent;
            IsClaimed = isClaimed;
            IsLocked = isLocked;
            IsMilestone = isMilestone;
        }
    }
}
