using System;
using Leaderboards;

namespace LeaderboardMVP
{
    public interface ILeaderboardView
    {
        /// <summary>Игрок открыл окно лидерборда — презентер запросит свежие данные.</summary>
        event Action Opened;

        /// <summary>Нажата кнопка авторизации (показывается неавторизованным игрокам).</summary>
        event Action AuthRequested;

        void SetData(LeaderboardViewData data);
    }
}
