using System;
using System.Collections.Generic;
using CardCollections;
using DG.Tweening;
using UnityEngine;
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

        private readonly Dictionary<string, CardCollectionItemView> _items = new();
        private DiContainer _container;
        private Tween _visibilityTween;
        private bool _isVisible;

        public event Action<string> CollectionSelected;
        public event Action<string> CollectRequested;

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
        }

        private void OnDestroy()
        {
            _visibilityTween?.Kill();

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
        }

        public void UpdateCollection(CardCollectionViewData collection)
        {
            if (collection == null || string.IsNullOrEmpty(collection.Id))
            {
                return;
            }

            GetOrCreateItem(collection.Id).SetData(collection);
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
