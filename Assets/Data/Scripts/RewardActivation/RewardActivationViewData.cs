using UnityEngine;

namespace RewardActivation
{
    public readonly struct RewardActivationViewData
    {
        public readonly Sprite Icon;
        public readonly string Title;
        public readonly string Description;

        public RewardActivationViewData(Sprite icon, string title, string description)
        {
            Icon = icon;
            Title = title;
            Description = description;
        }
    }
}
