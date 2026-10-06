using System;
using System.Collections.Generic;
using PlayerProgression;
using UnityEngine;
using YG;
using YG.Utils.LB;
using Zenject;

namespace Leaderboards
{
    /// <summary>
    /// Мост между прогрессией игрока и лидербордом площадки: отправляет текущий уровень как счёт
    /// и отдаёт данные рейтинга в формате, не зависящем от типов PluginYG2.
    /// </summary>
    public class LeaderboardService : ILeaderboardService, IInitializable, IDisposable
    {
        private LeaderboardConfig _config;
        private IPlayerProgressionService _progression;

        private LeaderboardViewData _data = new LeaderboardViewData();
        private int _submittedScore;
        private bool _isLoading;

        public bool IsAuthorized => YG2.player != null && YG2.player.auth;
        public bool IsLoading => _isLoading;
        public LeaderboardViewData Data => _data;
        public int SubmittedScore => _submittedScore;

        public event Action<LeaderboardViewData> DataReceived;
        public event Action Changed;

        [Inject]
        public void Construct(LeaderboardConfig config, IPlayerProgressionService progression)
        {
            _config = config;
            _progression = progression;
        }

        public void Initialize()
        {
            YG2.onGetLeaderboard += OnGetLeaderboard;
            YG2.onGetSDKData += OnSdkDataReceived;
            _progression.LevelCompleted += OnLevelCompleted;

            _data.IsAuthorized = IsAuthorized;

            if (YG2.isSDKEnabled)
            {
                SubmitCurrentLevel();
            }
        }

        public void Dispose()
        {
            YG2.onGetLeaderboard -= OnGetLeaderboard;
            YG2.onGetSDKData -= OnSdkDataReceived;
            _progression.LevelCompleted -= OnLevelCompleted;
        }

        public void SubmitCurrentLevel()
        {
            SubmitScore(_progression.CurrentLevel);
        }

        public void SubmitScore(int score)
        {
            if (string.IsNullOrWhiteSpace(_config.TechnoName))
            {
                Debug.LogWarning("LeaderboardConfig.TechnoName is empty, score is not submitted.");
                return;
            }

            // Счёт монотонно растёт вместе с уровнем, поэтому отправлять уже отправленное значение
            // (или меньшее — например при повторной инициализации) смысла нет.
            if (score <= 0 || score <= _submittedScore || !IsAuthorized)
            {
                return;
            }

            YG2.SetLeaderboard(_config.TechnoName, score);
            _submittedScore = score;
        }

        public void RequestData()
        {
            if (string.IsNullOrWhiteSpace(_config.TechnoName))
            {
                return;
            }

            _isLoading = true;
            _data.IsLoading = true;
            _data.IsAuthorized = IsAuthorized;
            Changed?.Invoke();

            YG2.GetLeaderboard(_config.TechnoName, _config.QuantityTop, _config.QuantityAround);
        }

        public void RequestAuth()
        {
            if (IsAuthorized)
            {
                return;
            }

            YG2.OpenAuthDialog();
        }

        private void OnSdkDataReceived()
        {
            // Данные SDK приходят и после успешной авторизации, так что это же место
            // догоняет счёт для игрока, который залогинился уже в процессе игры.
            SubmitCurrentLevel();

            if (_data.IsAuthorized != IsAuthorized)
            {
                _data.IsAuthorized = IsAuthorized;
                Changed?.Invoke();
            }
        }

        private void OnLevelCompleted(int completedLevels)
        {
            if (!_config.SubmitOnLevelUp)
            {
                return;
            }

            SubmitCurrentLevel();
        }

        private void OnGetLeaderboard(LBData lbData)
        {
            if (lbData == null || lbData.technoName != _config.TechnoName)
            {
                return;
            }

            _isLoading = false;
            _data = Convert(lbData);
            DataReceived?.Invoke(_data);
            Changed?.Invoke();
        }

        private LeaderboardViewData Convert(LBData lbData)
        {
            string currentPlayerId = YG2.player != null ? YG2.player.id : string.Empty;
            var entries = new List<LeaderboardEntryViewData>();

            if (lbData.players != null)
            {
                foreach (LBPlayerData player in lbData.players)
                {
                    if (player == null || player.name == InfoYG.NO_DATA)
                    {
                        continue;
                    }

                    bool isCurrentPlayer = !string.IsNullOrEmpty(currentPlayerId) &&
                                           player.uniqueID == currentPlayerId;

                    entries.Add(new LeaderboardEntryViewData
                    {
                        Rank = player.rank,
                        Name = LBMethods.AnonymousName(player.name),
                        Score = player.score,
                        IsCurrentPlayer = isCurrentPlayer
                    });
                }
            }

            int currentRank = lbData.currentPlayer?.rank ?? 0;
            int currentScore = lbData.currentPlayer?.score ?? 0;

            // Площадка не всегда отдаёт uniqueID текущего игрока — тогда опираемся на его место.
            if (currentRank > 0)
            {
                foreach (LeaderboardEntryViewData entry in entries)
                {
                    if (entry.Rank == currentRank)
                    {
                        entry.IsCurrentPlayer = true;
                        break;
                    }
                }
            }

            entries.Sort((first, second) => first.Rank.CompareTo(second.Rank));
            List<LeaderboardEntryViewData> visibleEntries = TakeVisible(entries);

            return new LeaderboardViewData
            {
                Entries = visibleEntries,
                CurrentPlayerRank = currentRank,
                CurrentPlayerScore = currentScore,
                HasData = visibleEntries.Count > 0,
                IsLoading = false,
                IsAuthorized = IsAuthorized
            };
        }

        /// <summary>
        /// Оставляет первые VisiblePlayersCount мест. Если игрок в них не попал,
        /// его строка добавляется последней, чтобы он всегда видел себя в списке.
        /// </summary>
        private List<LeaderboardEntryViewData> TakeVisible(List<LeaderboardEntryViewData> entries)
        {
            int limit = _config.VisiblePlayersCount;
            var visible = new List<LeaderboardEntryViewData>(Math.Min(entries.Count, limit + 1));
            LeaderboardEntryViewData currentPlayerEntry = null;

            foreach (LeaderboardEntryViewData entry in entries)
            {
                if (entry.IsCurrentPlayer)
                {
                    currentPlayerEntry = entry;
                }

                if (visible.Count < limit)
                {
                    visible.Add(entry);
                }
            }

            if (currentPlayerEntry != null && !visible.Contains(currentPlayerEntry))
            {
                visible.Add(currentPlayerEntry);
            }

            return visible;
        }
    }
}
