using UnityEngine;

namespace RewardActivation
{
    public readonly struct RewardActivationViewData
    {
        public readonly Sprite Icon;
        public readonly string Title;
        public readonly string Description;
        public readonly string ClaimButtonText;
        public readonly string PostponeButtonText;
        public readonly bool ShowPostponeButton;

        public RewardActivationViewData(
            Sprite icon,
            string title,
            string description,
            string claimButtonText,
            string postponeButtonText,
            bool showPostponeButton = true)
        {
            Icon = icon;
            Title = title;
            Description = description;
            ClaimButtonText = claimButtonText;
            PostponeButtonText = postponeButtonText;
            ShowPostponeButton = showPostponeButton;
        }
    }
}
