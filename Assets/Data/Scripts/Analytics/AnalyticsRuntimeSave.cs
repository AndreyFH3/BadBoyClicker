using System;

namespace Analytics
{
    public class AnalyticsRuntimeSave : IAnalyticsRuntimeSave
    {
        public long TotalClicks { get; private set; }
        public int NextClickMilestoneIndex { get; private set; }
        public event Action Changed;

        public void SetClickProgress(long totalClicks, int nextClickMilestoneIndex)
        {
            totalClicks = Math.Max(0, totalClicks);
            nextClickMilestoneIndex = Math.Max(0, nextClickMilestoneIndex);

            if (TotalClicks == totalClicks && NextClickMilestoneIndex == nextClickMilestoneIndex)
            {
                return;
            }

            TotalClicks = totalClicks;
            NextClickMilestoneIndex = nextClickMilestoneIndex;
            Changed?.Invoke();
        }

        public void Set(AnalyticsSaveData data)
        {
            if (data == null)
            {
                TotalClicks = 0;
                NextClickMilestoneIndex = 0;
            }
            else
            {
                TotalClicks = Math.Max(0, data.TotalClicks);
                NextClickMilestoneIndex = Math.Max(0, data.NextClickMilestoneIndex);
            }

            Changed?.Invoke();
        }

        public AnalyticsSaveData Get()
        {
            return new AnalyticsSaveData
            {
                TotalClicks = TotalClicks,
                NextClickMilestoneIndex = NextClickMilestoneIndex
            };
        }
    }
}
