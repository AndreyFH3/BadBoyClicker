using System.Collections.Generic;
using Rewards;
using UnityEngine;

namespace DailyQuests
{
    public class DailyQuestViewData
    {
        public string Id;
        public string Title;
        public string Description;
        public Sprite Icon;
        public string ProgressText;
        public long CurrentValue;
        public long TargetValue;
        public float Progress;
        public int Points;
        public bool IsCompleted;
        public bool IsPointsClaimed;
        public bool CanClaimPoints;
        public IReadOnlyList<RewardDisplay> Rewards;
    }

    public class DailyQuestMilestoneViewData
    {
        public int RequiredPoints;
        public bool IsClaimed;
        public bool CanClaim;
        public IReadOnlyList<RewardDisplay> Rewards;
    }

    public class DailyQuestBoardViewData
    {
        public int Points;
        public int MaxPoints;
        public float PointsProgress;
        public IReadOnlyList<DailyQuestViewData> Quests;
        public IReadOnlyList<DailyQuestMilestoneViewData> Milestones;
    }
}
