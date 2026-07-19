using System;
using Core;

namespace Analytics
{
    public interface IAnalyticsRuntimeSave : ISavable<AnalyticsSaveData>
    {
        long TotalClicks { get; }
        int NextClickMilestoneIndex { get; }
        event Action Changed;
        void SetClickProgress(long totalClicks, int nextClickMilestoneIndex);
    }
}
