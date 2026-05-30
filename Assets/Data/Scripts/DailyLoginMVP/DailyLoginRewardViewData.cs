using UnityEngine;

namespace DailyLoginMVP
{
    public readonly struct DailyLoginRewardViewData
    {
        public readonly Sprite Icon;
        public readonly string Text;
        public readonly bool IsCurrent;

        public DailyLoginRewardViewData(Sprite icon, string text, bool isCurrent)
        {
            Icon = icon;
            Text = text;
            IsCurrent = isCurrent;
        }
    }
}
