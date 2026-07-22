using System.Collections.Generic;
using Rewards;
using PlayerProgression;
using UIAnimations;
using UnityEngine;
using UnityEngine.UI;

namespace CardCollectionMVP
{
    public class CardCollectionRewardWindow : MonoBehaviour, ICardCollectionRewardWindow
    {
        [SerializeField] private GameObject _windowRoot;
        [SerializeField] private Transform _rewardsRoot;
        [SerializeField] private LevelRewardEntryView _rewardReference;
        [SerializeField] private Button _closeButton;
        [SerializeField] private CanvasGroupFade _fade;

        private readonly List<LevelRewardEntryView> _rewardViews = new();

        private void Awake()
        {
            if (_windowRoot == null)
            {
                _windowRoot = gameObject;
            }

            if (_fade == null)
            {
                _fade = _windowRoot.TryGetComponent(out CanvasGroupFade fade)
                    ? fade
                    : _windowRoot.AddComponent<CanvasGroupFade>();
            }

            if (_rewardReference != null)
            {
                _rewardReference.gameObject.SetActive(false);
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(Hide);
            }

            _fade.HideImmediately();
        }

        private void OnDestroy()
        {
            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveListener(Hide);
            }
        }

        public void Show(IReadOnlyList<RewardDisplay> rewards)
        {
            RebuildRewards(rewards);
            _fade.Show();
        }

        public void Hide()
        {
            _fade.Hide();
        }

        private void RebuildRewards(IReadOnlyList<RewardDisplay> rewards)
        {
            if (_rewardsRoot == null || _rewardReference == null)
            {
                return;
            }

            int count = rewards?.Count ?? 0;

            for (int i = 0; i < _rewardViews.Count; i++)
            {
                _rewardViews[i].gameObject.SetActive(i < count);
            }

            for (int i = 0; i < count; i++)
            {
                bool isNew = i >= _rewardViews.Count;
                LevelRewardEntryView view = isNew ? Instantiate(_rewardReference, _rewardsRoot) : _rewardViews[i];

                RewardDisplay reward = rewards[i];
                view.Setup(new LevelRewardEntry(reward.Icon, reward.Amount, reward.Description));
                view.gameObject.SetActive(true);

                if (isNew)
                {
                    _rewardViews.Add(view);
                }
            }
        }
    }
}
