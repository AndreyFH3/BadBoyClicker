using System;

namespace Tutorials
{
    public interface ITutorialView
    {
        event Action Closed;

        void Show(TutorialConfig.TutorialData tutorial);
    }
}
