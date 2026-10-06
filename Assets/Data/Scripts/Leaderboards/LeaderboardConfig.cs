using UnityEngine;

namespace Leaderboards
{
    [CreateAssetMenu(fileName = "LeaderboardConfig", menuName = "Configs/LeaderboardConfig")]
    public class LeaderboardConfig : ScriptableObject
    {
        [Tooltip("Техническое имя лидерборда, заданное в консоли разработчика площадки.")]
        [SerializeField] private string _technoName = "level";
        [Tooltip("Сколько игроков из топа запрашивать.")]
        [SerializeField] private int _quantityTop = 25;
        [Tooltip("Сколько игроков вокруг текущего запрашивать (в каждую сторону).")]
        [SerializeField] private int _quantityAround = 1;
        [Tooltip("Сколько строк показывать в окне. Если игрок не попал в них, его строка добавляется в конец.")]
        [SerializeField] private int _visiblePlayersCount = 25;
        [Tooltip("Отправлять счёт автоматически при повышении уровня.")]
        [SerializeField] private bool _submitOnLevelUp = true;

        public string TechnoName => _technoName;
        public int QuantityTop => Mathf.Max(0, _quantityTop);
        public int QuantityAround => Mathf.Max(0, _quantityAround);
        public int VisiblePlayersCount => Mathf.Max(1, _visiblePlayersCount);
        public bool SubmitOnLevelUp => _submitOnLevelUp;
    }
}
