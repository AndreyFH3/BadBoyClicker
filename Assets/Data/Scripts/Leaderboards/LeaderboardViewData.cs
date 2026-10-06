using System.Collections.Generic;

namespace Leaderboards
{
    public class LeaderboardEntryViewData
    {
        public int Rank;
        public string Name;
        public int Score;
        public bool IsCurrentPlayer;
    }

    public class LeaderboardViewData
    {
        public IReadOnlyList<LeaderboardEntryViewData> Entries = new List<LeaderboardEntryViewData>();

        /// <summary>Место текущего игрока в общем рейтинге. 0, если места ещё нет.</summary>
        public int CurrentPlayerRank;

        /// <summary>Счёт текущего игрока по данным площадки (может отставать от локального уровня).</summary>
        public int CurrentPlayerScore;

        /// <summary>Данные пришли и лидерборд не пустой.</summary>
        public bool HasData;

        /// <summary>Запрос выполняется прямо сейчас.</summary>
        public bool IsLoading;

        /// <summary>Игрок не авторизован — площадка не отдаёт и не принимает счёт.</summary>
        public bool IsAuthorized;
    }
}
