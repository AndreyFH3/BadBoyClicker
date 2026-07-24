using System;

namespace Tutorials
{
    public class TutorialNullView : ITutorialView
    {
        public event Action Closed;

        public void Show(TutorialConfig.TutorialData tutorial)
        {
            Closed?.Invoke();
        }
    }
}
