#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using YG;
using YG.Utils.LB;

namespace Leaderboards.EditorTools
{
    /// <summary>
    /// Наполняет симуляцию лидерборда в InfoYG (SettingsYG2), чтобы окно можно было
    /// проверить прямо в редакторе: в билде эти данные не используются — там отвечает площадка.
    /// </summary>
    public static class LeaderboardSimulationSetup
    {
        private const int PlayersCount = 30;
        private const string DefaultTechnoName = "level";

        [MenuItem("Tools/Leaderboard/Симуляция: игрок вне топ-25 (27 место)")]
        private static void SetupOutsideTop() => Setup(27);

        [MenuItem("Tools/Leaderboard/Симуляция: игрок в топ-25 (5 место)")]
        private static void SetupInsideTop() => Setup(5);

        private static void Setup(int currentPlayerRank)
        {
            InfoYG info = YG2.infoYG;
            if (info == null)
            {
                Debug.LogError("InfoYG (SettingsYG2) не найден.");
                return;
            }

            info.Leaderboards.enable = true;
            info.Authorization.authorized = true;

            string technoName = GetTechnoName();
            string currentPlayerName = info.Authorization.playerName;
            string currentPlayerId = info.Authorization.uniqueID;

            var players = new List<LBPlayerData>(PlayersCount);
            for (int i = 0; i < PlayersCount; i++)
            {
                int rank = i + 1;
                bool isCurrentPlayer = rank == currentPlayerRank;

                players.Add(new LBPlayerData
                {
                    rank = rank,
                    // Счёт = уровень игрока, поэтому убывает вместе с местом.
                    score = PlayersCount * 4 - i * 4,
                    name = isCurrentPlayer ? currentPlayerName : $"Player {rank}",
                    uniqueID = isCurrentPlayer ? currentPlayerId : $"sim_{rank}",
                    photo = null
                });
            }

            LBPlayerData currentPlayer = players[currentPlayerRank - 1];

            var simulation = new LBData
            {
                technoName = technoName,
                type = "numeric",
                entries = string.Join("\n", players.Select(p => $"{p.rank}. {p.name}: {p.score}")),
                players = players.ToArray(),
                currentPlayer = new LBCurrentPlayerData
                {
                    rank = currentPlayer.rank,
                    score = currentPlayer.score
                }
            };

            List<LBData> list = info.Leaderboards.listLBSim != null
                ? info.Leaderboards.listLBSim.ToList()
                : new List<LBData>();

            int existingIndex = list.FindIndex(lb => lb != null && lb.technoName == technoName);
            if (existingIndex >= 0)
            {
                list[existingIndex] = simulation;
            }
            else
            {
                list.Add(simulation);
            }

            info.Leaderboards.listLBSim = list.ToArray();

            EditorUtility.SetDirty(info);
            AssetDatabase.SaveAssets();

            Debug.Log($"Симуляция лидерборда «{technoName}» готова: {PlayersCount} игроков, " +
                      $"«{currentPlayerName}» на {currentPlayerRank} месте со счётом {currentPlayer.score}.");
        }

        private static string GetTechnoName()
        {
            string[] guids = AssetDatabase.FindAssets("t:LeaderboardConfig");
            foreach (string guid in guids)
            {
                var config = AssetDatabase.LoadAssetAtPath<LeaderboardConfig>(AssetDatabase.GUIDToAssetPath(guid));
                if (config != null && !string.IsNullOrWhiteSpace(config.TechnoName))
                {
                    return config.TechnoName;
                }
            }

            Debug.LogWarning($"Ассет LeaderboardConfig не найден, используется имя «{DefaultTechnoName}».");
            return DefaultTechnoName;
        }
    }
}
#endif
