using System;

namespace Leaderboards
{
    public interface ILeaderboardService
    {
        /// <summary>Игрок авторизован на площадке: счёт можно отправлять и получать.</summary>
        bool IsAuthorized { get; }

        bool IsLoading { get; }

        /// <summary>Последние полученные данные лидерборда. Никогда не null.</summary>
        LeaderboardViewData Data { get; }

        /// <summary>Счёт, который сервис уже отправил в этой сессии. 0, если ещё ничего не отправлял.</summary>
        int SubmittedScore { get; }

        /// <summary>Данные лидерборда обновились (пришёл ответ, сменился статус загрузки/авторизации).</summary>
        event Action<LeaderboardViewData> DataReceived;

        event Action Changed;

        /// <summary>Отправить текущий уровень игрока как счёт. Вызывается автоматически при апе уровня.</summary>
        void SubmitCurrentLevel();

        /// <summary>Отправить произвольный счёт (например, для другого лидерборда в будущем).</summary>
        void SubmitScore(int score);

        /// <summary>Запросить топ и окружение текущего игрока. Ответ придёт в <see cref="DataReceived"/>.</summary>
        void RequestData();

        /// <summary>Открыть диалог авторизации площадки, если игрок не авторизован.</summary>
        void RequestAuth();
    }
}
