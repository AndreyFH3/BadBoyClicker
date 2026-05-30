using System.Collections.Generic;
using UnityEngine;

namespace DailyLogin
{
    [CreateAssetMenu(fileName = "DailyLoginConfig", menuName = "Configs/Daily Login")]
    public class DailyLoginConfig : ScriptableObject
    {
        [SerializeField] private List<DailyLoginDayConfig> _days = new();

        public IReadOnlyList<DailyLoginDayConfig> Days => _days;
        public int DaysCount => _days?.Count ?? 0;

        public bool HasRewards => DaysCount > 0;
    }
}
