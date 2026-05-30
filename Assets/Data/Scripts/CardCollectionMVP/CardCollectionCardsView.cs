using System;
using System.Collections.Generic;
using CardCollections;
using DG.Tweening;
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
        [SerializeField] private GameObject _completedMarker;
        [SerializeField] private GameObject _rewardClaimedMarker;
        [SerializeField] private Transform _cardsRoot;
        [SerializeField] private CardItemView _cardReference;
        [SerializeField] private Button _closeButton;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _animationDuration = 0.2f;
        [SerializeField] private Ease _showEase = Ease.OutQuad;
        [SerializeField] private Ease _hideEase = Ease.InQuad;

        private readonly Dictionary<string, CardItemView> _cards = new();
        private Tween _visibilityTween;
        private bool _isVisible;

        public event Action CloseRequested;

        private void Awake()
        {
            if (_cardReference != null)
            {
                _cardReference.gameObject.SetActive(false);
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(RequestClose);
            }
        }

        private void OnDestroy()
        {
            _visibilityTween?.Kill();
            ClearCards();

            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveListener(RequestClose);
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
                    ClearCards();
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
                        ClearCards();
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

            if (_completedMarker != null)
            {
                _completedMarker.SetActive(collection.IsCompleted);
            }

            if (_rewardClaimedMarker != null)
            {
                _rewardClaimedMarker.SetActive(collection.IsRewardClaimed);
            }

            SetCards(collection.Cards);
        }

        public void UpdateCard(CardViewData card)
        {
            if (card == null || string.IsNullOrEmpty(card.Id))
            {
                return;
            }

            GetOrCreateCard(card.Id).SetData(card);
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

        private void SetCards(IReadOnlyList<CardViewData> cards)
        {
            if (cards == null || _cardReference == null)
            {
                ClearCards();
                return;
            }

            ClearCards();

            foreach (var card in cards)
            {
                if (card == null || string.IsNullOrEmpty(card.Id))
                {
                    continue;
                }

                GetOrCreateCard(card.Id).SetData(card);
            }
        }

        private CardItemView GetOrCreateCard(string id)
        {
            if (_cards.TryGetValue(id, out var item) && item != null)
            {
                return item;
            }

            Transform root = _cardsRoot != null ? _cardsRoot : transform;
            item = Instantiate(_cardReference, root);
            item.gameObject.SetActive(true);
            _cards[id] = item;
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
    }
}
