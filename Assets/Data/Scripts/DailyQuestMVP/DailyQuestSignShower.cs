using System.Linq;
using DailyQuests;
using UnityEngine;
using Zenject;

namespace DailyQuestMVP
{
    /// <summary>
    /// Shows an attention sign (e.g. a badge on a nav button) whenever there is
    /// a completed quest or milestone whose reward hasn't been claimed yet.
    /// </summary>
    public class DailyQuestSignShower : MonoBehaviour
    {
        [SerializeField] private GameObject _sign;

        private IDailyQuestService _dailyQuestService;

        [Inject]
        public void Construct(IDailyQuestService dailyQuestService)
        {
            _dailyQuestService = dailyQuestService;
        }

        private void OnEnable()
        {
            if (_dailyQuestService != null)
            {
                _dailyQuestService.Changed += UpdateSign;
                _dailyQuestService.QuestPointsClaimed += OnQuestPointsClaimed;
                _dailyQuestService.MilestoneClaimed += OnMilestoneClaimed;
                UpdateSign();
            }
        }

        private void OnDisable()
        {
            if (_dailyQuestService != null)
            {
                _dailyQuestService.Changed -= UpdateSign;
                _dailyQuestService.QuestPointsClaimed -= OnQuestPointsClaimed;
                _dailyQuestService.MilestoneClaimed -= OnMilestoneClaimed;
            }
        }

        private void OnQuestPointsClaimed(string questId, int points)
        {
            UpdateSign();
        }

        private void OnMilestoneClaimed(int requiredPoints)
        {
            UpdateSign();
        }

        private void UpdateSign()
        {
            if (_sign == null || _dailyQuestService == null)
            {
                return;
            }

            DailyQuestBoardViewData data = _dailyQuestService.GetViewData();
            bool needShow = data.Quests.Any(quest => quest.CanClaimPoints)
                || data.Milestones.Any(milestone => milestone.CanClaim);
            _sign.SetActive(needShow);
        }
    }
}
