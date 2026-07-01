using UnityEngine;

namespace PlayerProgression
{
    /// <summary>
    /// A single thing the player gains for completing a level: an icon, its value text
    /// and an optional short description that tells the player what the reward affects
    /// (e.g. "to click income" / "to passive income").
    /// </summary>
    public readonly struct LevelRewardEntry
    {
        public readonly Sprite Icon;
        public readonly string Text;
        public readonly string Description;

        public LevelRewardEntry(Sprite icon, string text, string description = null)
        {
            Icon = icon;
            Text = text;
            Description = description;
        }
    }
}
