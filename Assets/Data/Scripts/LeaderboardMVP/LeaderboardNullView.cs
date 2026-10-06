using System;
using Leaderboards;

namespace LeaderboardMVP
{
    public class LeaderboardNullView : ILeaderboardView
    {
        public event Action Opened
        {
            add { }
            remove { }
        }

        public event Action AuthRequested
        {
            add { }
            remove { }
        }

        public void SetData(LeaderboardViewData data)
        {
        }
    }
}
