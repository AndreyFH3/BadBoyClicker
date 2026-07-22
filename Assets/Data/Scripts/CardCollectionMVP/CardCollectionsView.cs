using System;
using System.Collections.Generic;
using System.Linq;
using CardCollections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Shop;
using Zenject;

namespace CardCollectionMVP
{
    public class CardCollectionsView : MonoBehaviour, ICardCollectionsView
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Transform _collectionsRoot;
        [SerializeField] private CardCollectionItemView _collectionReference;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _animationDuration = 0.2f;
        [SerializeField] private Ease _showEase = Ease.OutQuad;
        [SerializeField] private Ease _hideEase = Ease.InQuad;

        [Header("Card chest purchase")]
        [SerializeField] private GameObject _cardChestPurchaseRoot;
        [SerializeField] private Button _cardChestPurchaseButton;
        [SerializeField] private Image _cardChestPurchaseIcon;
        [SerializeField] private TextMeshProUGUI _cardChestPurchaseName;
        [SerializeField] private TextMeshProUGUI _cardChestPurchaseDescription;
        [SerializeField] private Image _cardChestPurchasePriceIcon;
        [SerializeField] private TextMeshProUGUI _cardChestPurchasePrice;

        [Header("Card chest rewarded ad")]
        [SerializeField] private Button _cardChestAdButton;

        private readonly Dictionary<string, CardCollectionItemView> _items = new();
        private DiContainer _container;
        private Tween _visibilityTween;
        private bool _isVisible;

        public event Action<string> CollectionSelected;
        public event Action<string> CollectRequested;
        public event Action CardChestPurchaseRequested;
        public event Action CardChestAdRequested;

        [Inject]
        private void Construct(DiContainer container)
        {
            _container = container;
        }

        private void Awake()
        {
            if (_collectionReference != null)
            {
                _collectionReference.gameObject.SetActive(false);
            }

            if (_cardChestPurchaseButton != null)
            {
                _cardChestPurchaseButton.onClick.AddListener(OnCardChestPurchaseRequested);
            }

            if (_cardChestAdButton != null)
            {
                _cardChestAdButton.onClick.AddListener(OnCardChestAdRequested);
            }

            SetCardChestPurchaseData(null);
            SetCardChestAdAvailable(false);
        }

        private void OnDestroy()
        {
            _visibilityTween?.Kill();

            if (_cardChestPurchaseButton != null)
            {
                _cardChestPurchaseButton.onClick.RemoveListener(OnCardChestPurchaseRequested);
            }

            if (_cardChestAdButton != null)
            {
                _cardChestAdButton.onClick.RemoveListener(OnCardChestAdRequested);
            }

            foreach (var item in _items.Values)
            {
                if (item != null)
                {
                    item.Selected -= OnCollectionSelected;
                    item.CollectRequested -= OnCollectRequested;
                }
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
                    }

                    _visibilityTween = null;
                });
        }

        public void SetData(IReadOnlyList<CardCollectionViewData> collections)
        {
            if (collections == null || _collectionReference == null)
            {
                return;
            }

            foreach (var collection in collections)
            {
                if (collection == null || string.IsNullOrEmpty(collection.Id))
                {
                    continue;
                }

                GetOrCreateItem(collection.Id).SetData(collection);
            }

            ApplySortOrder();
        }

        public void UpdateCollection(CardCollectionViewData collection)
        {
            if (collection == null || string.IsNullOrEmpty(collection.Id))
            {
                return;
            }

            GetOrCreateItem(collection.Id).SetData(collection);
            ApplySortOrder();
        }

        public void SetCardChestPurchaseData(ShopElementData data)
        {
            GameObject purchaseRoot = _cardChestPurchaseRoot != null
                ? _cardChestPurchaseRoot
                : _cardChestPurchaseButton != null ? _cardChestPurchaseButton.gameObject : null;
            if (purchaseRoot == null)
            {
                return;
            }

            purchaseRoot.SetActive(data != null);
            if (data == null)
            {
                return;
            }

            if (_cardChestPurchaseButton != null)
            {
                _cardChestPurchaseButton.interactable = data.CanBuy;
            }

            SetImage(_cardChestPurchaseIcon, data.Icon);
            SetImage(_cardChestPurchasePriceIcon, data.PriceIcon);

            if (_cardChestPurchaseName != null)
            {
                _cardChestPurchaseName.text = data.Name;
            }

            if (_cardChestPurchaseDescription != null)
            {
                _cardChestPurchaseDescription.text = data.Bonus;
            }

            if (_cardChestPurchasePrice != null)
            {
                _cardChestPurchasePrice.text = data.Price;
            }
        }

        private static void SetImage(Image image, Sprite sprite)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = sprite;
            image.enabled = sprite != null;
        }

        public void SetCardChestAdAvailable(bool isAvailable)
        {
            if (_cardChestAdButton == null)
            {
                return;
            }

            _cardChestAdButton.gameObject.SetActive(isAvailable);
            _cardChestAdButton.interactable = isAvailable;
        }

        private void OnCardChestPurchaseRequested()
        {
            CardChestPurchaseRequested?.Invoke();
        }

        private void OnCardChestAdRequested()
        {
            CardChestAdRequested?.Invoke();
        }

        // Ready-to-collect first, then by collected card count (descending), fully claimed ones last.
        private void ApplySortOrder()
        {
            List<CardCollectionItemView> ordered = _items.Values
                .Where(item => item != null)
                .OrderBy(item => GetSortGroup(item.Data))
                .ThenByDescending(item => item.Data?.CollectedCards ?? 0)
                .ToList();

            for (int i = 0; i < ordered.Count; i++)
            {
                ordered[i].transform.SetSiblingIndex(i);
            }
        }

        private static int GetSortGroup(CardCollectionViewData data)
        {
            if (data == null)
            {
                return 1;
            }

            if (data.RewardAvailable)
            {
                return 0;
            }

            if (data.IsRewardClaimed)
            {
                return 2;
            }

            return 1;
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

        private CardCollectionItemView GetOrCreateItem(string id)
        {
            if (_items.TryGetValue(id, out var item) && item != null)
            {
                return item;
            }

            Transform root = _collectionsRoot != null ? _collectionsRoot : transform;
            item = Instantiate(_collectionReference, root);
            _container?.Inject(item);
            item.gameObject.SetActive(true);
            item.Selected += OnCollectionSelected;
            item.CollectRequested += OnCollectRequested;
            _items[id] = item;
            return item;
        }

        private void OnCollectionSelected(string collectionId)
        {
            CollectionSelected?.Invoke(collectionId);
        }

        private void OnCollectRequested(string collectionId)
        {
            CollectRequested?.Invoke(collectionId);
        }
    }
}
