using System.Collections.Generic;
using Rewards;
using UnityEngine;
using UnityEngine.UI;

namespace CardCollectionMVP
{
    public class CardCollectionRewardWindow : MonoBehaviour, ICardCollectionRewardWindow
    {
        [SerializeField] private GameObject _windowRoot;
        [SerializeField] private Transform _rewardsRoot;
        [SerializeField] private RewardView _rewardReference;
        [SerializeField] private Button _closeButton;

        private readonly List<RewardView> _rewardViews = new();

        private void Awake()
        {
            if (_windowRoot == null)
            {
                _windowRoot = gameObject;
            }

            if (_rewardReference != null)
            {
                _rewardReference.gameObject.SetActive(false);
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(Hide);
            }

            Hide();
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
            _windowRoot.SetActive(true);
        }

        public void Hide()
        {
            _windowRoot.SetActive(false);
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
                RewardView view = isNew ? Instantiate(_rewardReference, _rewardsRoot) : _rewardViews[i];

                view.Set(rewards[i]);
                view.gameObject.SetActive(true);

                if (isNew)
                {
                    _rewardViews.Add(view);
                }
            }
        }
    }
}
