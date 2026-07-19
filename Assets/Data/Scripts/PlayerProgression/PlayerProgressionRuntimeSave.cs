using System;
using Core;

namespace PlayerProgression
{
    public class PlayerProgressionRuntimeSave : ISavable<PlayerProgressionRuntimeSave.SaveData>
    {
        public int CompletedLevels { get; private set; }
        public long CurrentExperience { get; private set; }
        public bool IsTutorialCompleted { get; private set; }
        public int TutorialClicks { get; private set; }
        public event Action Changed;

        public void SetState(int completedLevels, long currentExperience)
        {
            CompletedLevels = Math.Max(0, completedLevels);
            CurrentExperience = Math.Max(0, currentExperience);
            Changed?.Invoke();
        }

        public void SetTutorialClicks(int tutorialClicks)
        {
            tutorialClicks = Math.Max(0, tutorialClicks);
            if (TutorialClicks == tutorialClicks)
            {
                return;
            }

            TutorialClicks = tutorialClicks;
            Changed?.Invoke();
        }

        public void CompleteTutorial()
        {
            if (IsTutorialCompleted)
            {
                return;
            }

            IsTutorialCompleted = true;
            Changed?.Invoke();
        }

        public void Set(SaveData data)
        {
            CompletedLevels = Math.Max(0, data.CompletedLevels);
            CurrentExperience = Math.Max(0, data.CurrentExperience);
            TutorialClicks = Math.Max(0, data.TutorialClicks);
            // Saves created before the tutorial gate existed already have real progress;
            // treat them as having finished it instead of sending returning players back to level 0.
            IsTutorialCompleted = data.IsTutorialCompleted || CompletedLevels > 0 || data.CurrentExperience > 0;
            Changed?.Invoke();
        }

        public SaveData Get()
        {
            return new SaveData
            {
                CompletedLevels = CompletedLevels,
                CurrentExperience = CurrentExperience,
                TutorialClicks = TutorialClicks,
                IsTutorialCompleted = IsTutorialCompleted
            };
        }

        [Serializable]
        public struct SaveData
        {
            public int CompletedLevels;
            public long CurrentExperience;
            public int TutorialClicks;
            public bool IsTutorialCompleted;
        }
    }
}
