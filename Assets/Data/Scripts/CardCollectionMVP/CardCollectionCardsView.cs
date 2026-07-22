using System;
using System.Collections.Generic;
using System.Linq;
using CardCollections;
using DG.Tweening;
using PlayerProgression;
using Rewards;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardCollectionMVP
{
    public class CardCollectionCardsView : MonoBehaviour, ICardCollectionCardsView
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private TextMeshProUGUI _description;
        [SerializeField] private TextMeshProUGUI _cardsProgressText;
        [SerializeField] private TextMeshProUGUI _starsProgressText;
        [SerializeField] private Image _progressFill;

        [Header("Cards")]
        [SerializeField] private Transform _cardsRoot;
        [SerializeField] private CardItemView _cardReference;

        [Header("Rewards")]
        [SerializeField] private Transform _rewardsRoot;
        [SerializeField] private LevelRewardEntryView _rewardReference;

        [Header("Collect")]
        [SerializeField] private Button _collectButton;

        [Header("Toggled when the reward is claimed")]
        [SerializeField] private List<GameObject> _rewardClaimedEnableObjects = new();
        [SerializeField] private List<GameObject> _rewardClaimedDisableObjects = new();

        [SerializeField] private Button _closeButton;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _animationDuration = 0.2f;
        [SerializeField] private Ease _showEase = Ease.OutQuad;
        [SerializeField] private Ease _hideEase = Ease.InQuad;

        private readonly Dictionary<string, CardItemView> _cards = new();
        private readonly List<LevelRewardEntryView> _rewardViews = new();
        private Tween _visibilityTween;
        private bool _isVisible;

        public event Action CloseRequested;
        public event Action CollectRequested;

        private void Awake()
        {
            if (_cardReference != null)
            {
                _cardReference.gameObject.SetActive(false);
            }

            if (_rewardReference != null)
            {
                _rewardReference.gameObject.SetActive(false);
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(RequestClose);
            }

            if (_collectButton != null)
            {
                _collectButton.onClick.AddListener(RequestCollect);
            }
        }

        private void OnDestroy()
        {
            _visibilityTween?.Kill();
            ClearContent();

            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveListener(RequestClose);
            }

            if (_collectButton != null)
            {
                _collectButton.onClick.RemoveListener(RequestCollect);
            }
        }

        public void SetVisible(bool isVisible)
        {
            if (_isVisible == isVisible && Root.activeSelf == isVisible && _visibilityTween == null)
            {
                return;
            }

            _isVisible = isVisible;
            _visibilityTween?.Kill();
            _visibilityTween = null;

            CanvasGroup canvasGroup = CanvasGroup;

            if (_animationDuration <= 0f)
            {
                canvasGroup.alpha = isVisible ? 1f : 0f;
                canvasGroup.interactable = isVisible;
                canvasGroup.blocksRaycasts = isVisible;
                Root.SetActive(isVisible);

                if (!isVisible)
                {
                    ClearContent();
                }

                return;
            }

            if (isVisible)
            {
                Root.SetActive(true);
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;

                _visibilityTween = canvasGroup
                    .DOFade(1f, _animationDuration)
                    .SetEase(_showEase)
                    .OnComplete(() => _visibilityTween = null);
                return;
            }

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            _visibilityTween = canvasGroup
                .DOFade(0f, _animationDuration)
                .SetEase(_hideEase)
                .OnComplete(() =>
                {
                    if (!_isVisible)
                    {
                        Root.SetActive(false);
                        ClearContent();
                    }

                    _visibilityTween = null;
                });
        }

        public void SetData(CardCollectionViewData collection)
        {
            if (collection == null)
            {
                return;
            }

            if (_title != null)
            {
                _title.text = collection.Title;
            }

            if (_description != null)
            {
                _description.text = collection.Description;
            }

            if (_cardsProgressText != null)
            {
                _cardsProgressText.text = $"{collection.CollectedCards}/{collection.TotalCards}";
            }

            if (_starsProgressText != null)
            {
                _starsProgressText.text = $"{collection.CollectedStars}/{collection.TotalStars}";
            }

            if (_progressFill != null)
            {
                _progressFill.fillAmount = collection.TotalCards > 0
                    ? Mathf.Clamp01((float)collection.CollectedCards / collection.TotalCards)
                    : 0f;
            }

            UpdateCollectButton(collection);
            SetRewards(collection.Rewards);
            SetCards(collection.Cards);
        }

        public void UpdateCard(CardViewData card)
        {
            if (card == null || string.IsNullOrEmpty(card.Id))
            {
                return;
            }

            GetOrCreateCard(card);
        }

        private GameObject Root => _root != null ? _root : gameObject;

        private CanvasGroup CanvasGroup
        {
            get
            {
                if (_canvasGroup != null)
                {
                    return _canvasGroup;
                }

                if (!Root.TryGetComponent(out _canvasGroup))
                {
                    _canvasGroup = Root.AddComponent<CanvasGroup>();
                }

                return _canvasGroup;
            }
        }

        private void RequestClose()
        {
            CloseRequested?.Invoke();
        }

        private void RequestCollect()
        {
            CollectRequested?.Invoke();
        }

        private void UpdateCollectButton(CardCollectionViewData collection)
        {
            if (_collectButton != null)
            {
                _collectButton.interactable = collection.RewardAvailable;
            }

            SetRewardClaimedObjects(collection.IsRewardClaimed);
        }

        private void SetRewardClaimedObjects(bool isRewardClaimed)
        {
            foreach (var go in _rewardClaimedDisableObjects)
            {
                if (go != null)
                {
                    go.SetActive(!isRewardClaimed);
                }
            }

            foreach (var go in _rewardClaimedEnableObjects)
            {
                if (go != null)
                {
                    go.SetActive(isRewardClaimed);
                }
            }
        }

        private void SetCards(IReadOnlyList<CardViewData> cards)
        {
            if (cards == null || _cardReference == null)
            {
                ClearCards();
                return;
            }

            ClearCards();

            // Collected cards first, uncollected last; original config order preserved within each group.
            foreach (var card in cards.OrderByDescending(card => card.IsCollected))
            {
                if (card == null || string.IsNullOrEmpty(card.Id))
                {
                    continue;
                }

                GetOrCreateCard(card);
            }
        }

        private CardItemView GetOrCreateCard(CardViewData card)
        {
            if (_cards.TryGetValue(card.Id, out var item) && item != null)
            {
                item.SetData(card);
                return item;
            }

            Transform root = _cardsRoot != null ? _cardsRoot : transform;
            item = Instantiate(_cardReference, root);
            item.SetData(card);
            item.gameObject.SetActive(true);
            _cards[card.Id] = item;
            return item;
        }

        private void ClearCards()
        {
            foreach (var card in _cards.Values)
            {
                if (card != null)
                {
                    Destroy(card.gameObject);
                }
            }

            _cards.Clear();
        }

        private void SetRewards(IReadOnlyList<RewardDisplay> rewards)
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

        private void ClearRewards()
        {
            foreach (var view in _rewardViews)
            {
                if (view != null)
                {
                    Destroy(view.gameObject);
                }
            }

            _rewardViews.Clear();
        }

        private void ClearContent()
        {
            ClearCards();
            ClearRewards();
        }
    }
}
