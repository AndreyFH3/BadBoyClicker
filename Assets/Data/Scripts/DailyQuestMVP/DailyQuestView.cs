using System;
using System.Collections.Generic;
using System.Linq;
using DailyQuests;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DailyQuestMVP
{
    public class DailyQuestView : MonoBehaviour, IDailyQuestView
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Transform _questsRoot;
        [SerializeField] private DailyQuestViewElement _questReference;
        [SerializeField] private Transform _milestonesRoot;
        [SerializeField] private DailyQuestMilestoneViewElement _milestoneReference;
        [SerializeField] private Image _pointsProgressFill;
        [SerializeField] private TextMeshProUGUI _pointsProgressText;
        [SerializeField] private float _pointsFillDuration = 0.2f;

        private readonly Dictionary<string, DailyQuestViewElement> _questElements = new();
        private readonly Dictionary<int, DailyQuestMilestoneViewElement> _milestoneElements = new();
        private Tween _pointsFillTween;

        public event Action<string> ClaimQuestPointsRequested;
        public event Action<int> ClaimMilestoneRequested;

        private void Awake()
        {
            if (_questReference != null)
            {
                _questReference.gameObject.SetActive(false);
            }

            if (_milestoneReference != null)
            {
                _milestoneReference.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            foreach (var element in _milestoneElements.Values)
            {
                if (element != null)
                {
                    element.ClaimRequested -= OnClaimRequested;
                }
            }

            foreach (var element in _questElements.Values)
            {
                if (element != null)
                {
                    element.ClaimPointsRequested -= OnQuestClaimPointsRequested;
                }
            }

            _pointsFillTween?.Kill();
        }

        public void SetData(DailyQuestBoardViewData data)
        {
            Root.SetActive(true);

            if (_pointsProgressText != null)
            {
                _pointsProgressText.text = data.MaxPoints > 0
                    ? $"{data.Points}/{data.MaxPoints}"
                    : data.Points.ToString();
            }

            if (_pointsProgressFill != null)
            {
                _pointsFillTween?.Kill();
                _pointsFillTween = _pointsProgressFill
                    .DOFillAmount(Mathf.Clamp01(data.PointsProgress), _pointsFillDuration)
                    .SetEase(Ease.OutQuad);
            }

            SetQuests(data.Quests);
            SetMilestones(data.Milestones);
            PositionMilestonesByProgress(data.Milestones, data.MaxPoints);
        }

        private GameObject Root => _root != null ? _root : gameObject;

        private void SetQuests(IReadOnlyList<DailyQuestViewData> quests)
        {
            if (quests == null || _questReference == null)
            {
                return;
            }

            var orderedQuests = quests
                .Where(quest => quest != null && !string.IsNullOrEmpty(quest.Id))
                .OrderBy(GetQuestSortOrder);

            int siblingIndex = 0;
            foreach (var quest in orderedQuests)
            {
                DailyQuestViewElement element = GetOrCreateQuestElement(quest.Id);
                element.SetData(quest);
                element.transform.SetSiblingIndex(siblingIndex);
                siblingIndex++;
            }
        }

        private static int GetQuestSortOrder(DailyQuestViewData quest)
        {
            if (quest.CanClaimPoints)
            {
                return 0;
            }

            return quest.IsPointsClaimed ? 2 : 1;
        }

        private void SetMilestones(IReadOnlyList<DailyQuestMilestoneViewData> milestones)
        {
            if (milestones == null || _milestoneReference == null)
            {
                return;
            }

            foreach (var milestone in milestones)
            {
                if (milestone == null || milestone.RequiredPoints <= 0)
                {
                    continue;
                }

                DailyQuestMilestoneViewElement element = GetOrCreateMilestoneElement(milestone.RequiredPoints);
                element.SetData(milestone);
            }
        }

        private DailyQuestViewElement GetOrCreateQuestElement(string id)
        {
            if (_questElements.TryGetValue(id, out var element) && element != null)
            {
                return element;
            }

            Transform root = _questsRoot != null ? _questsRoot : transform;
            element = Instantiate(_questReference, root);
            element.gameObject.SetActive(true);
            element.ClaimPointsRequested += OnQuestClaimPointsRequested;
            _questElements[id] = element;
            return element;
        }

        private DailyQuestMilestoneViewElement GetOrCreateMilestoneElement(int requiredPoints)
        {
            if (_milestoneElements.TryGetValue(requiredPoints, out var element) && element != null)
            {
                return element;
            }

            Transform root = _milestonesRoot != null ? _milestonesRoot : transform;
            element = Instantiate(_milestoneReference, root);
            element.gameObject.SetActive(true);
            element.ClaimRequested += OnClaimRequested;
            _milestoneElements[requiredPoints] = element;
            return element;
        }

        private void OnClaimRequested(int requiredPoints)
        {
            ClaimMilestoneRequested?.Invoke(requiredPoints);
        }

        private void OnQuestClaimPointsRequested(string questId)
        {
            ClaimQuestPointsRequested?.Invoke(questId);
        }

        private void PositionMilestonesByProgress(IReadOnlyList<DailyQuestMilestoneViewData> milestones, int maxPoints)
        {
            if (milestones == null || milestones.Count == 0 || maxPoints <= 0)
            {
                return;
            }

            for (int i = 0; i < milestones.Count; i++)
            {
                var milestone = milestones[i];
                if (milestone == null || !_milestoneElements.TryGetValue(milestone.RequiredPoints, out var element) || element == null)
                {
                    continue;
                }

                RectTransform rect = element.transform as RectTransform;
                if (rect == null)
                {
                    continue;
                }

                float position = Mathf.Clamp01((float)milestone.RequiredPoints / maxPoints);
                rect.anchorMin = new Vector2(position, rect.anchorMin.y);
                rect.anchorMax = new Vector2(position, rect.anchorMax.y);
                rect.anchoredPosition = new Vector2(0f, rect.anchoredPosition.y);
            }
        }
    }
}
