using System;
using Core;

namespace PlayerProgression
{
    public class PlayerProgressionRuntimeSave : ISavable<PlayerProgressionRuntimeSave.SaveData>
    {
        public int CompletedLevels { get; private set; }
        public long CurrentExperience { get; private set; }
        public event Action Changed;

        public void SetState(int completedLevels, long currentExperience)
        {
            CompletedLevels = Math.Max(0, completedLevels);
            CurrentExperience = Math.Max(0, currentExperience);
            Changed?.Invoke();
        }

        public void Set(SaveData data)
        {
            CompletedLevels = Math.Max(0, data.CompletedLevels);
            CurrentExperience = Math.Max(0, data.CurrentExperience);
            Changed?.Invoke();
        }

        public SaveData Get()
        {
            return new SaveData
            {
                CompletedLevels = CompletedLevels,
                CurrentExperience = CurrentExperience
            };
        }

        [Serializable]
        public struct SaveData
        {
            public int CompletedLevels;
            public long CurrentExperience;
        }
    }
}
