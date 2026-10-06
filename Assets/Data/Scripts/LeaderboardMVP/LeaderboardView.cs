using System;
using System.Collections.Generic;
using DG.Tweening;
using Leaderboards;
using UnityEngine;
using UnityEngine.UI;

namespace LeaderboardMVP
{
    public class LeaderboardView : MonoBehaviour, ILeaderboardView
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _animationDuration = 0.2f;
        [SerializeField] private Ease _showEase = Ease.OutQuad;
        [SerializeField] private Ease _hideEase = Ease.InQuad;

        [Header("Entries")]
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private Transform _entriesRoot;
        [SerializeField] private LeaderboardEntryViewElement _entryReference;

        [Header("States")]
        [SerializeField] private GameObject _loadingRoot;
        [SerializeField] private GameObject _emptyRoot;
        [SerializeField] private GameObject _unauthorizedRoot;

        [Header("Buttons")]
        [SerializeField] private Button _authButton;
        [SerializeField] private Button _closeButton;

        private readonly List<LeaderboardEntryViewElement> _entryElements = new();
        private Tween _visibilityTween;
        private bool _isVisible;

        public event Action Opened;
        public event Action AuthRequested;

        private GameObject Root => _root != null ? _root : gameObject;

        private CanvasGroup CanvasGroup
        {
            get
            {
                if (_canvasGroup == null)
                {
                    _canvasGroup = Root.GetComponent<CanvasGroup>();
                    if (_canvasGroup == null)
                    {
                        _canvasGroup = Root.AddComponent<CanvasGroup>();
                    }
                }

                return _canvasGroup;
            }
        }

        private void Awake()
        {
            if (_entryReference != null)
            {
                _entryReference.gameObject.SetActive(false);
            }

            if (_authButton != null)
            {
                _authButton.onClick.AddListener(OnAuthClicked);
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(Hide);
            }

            HideImmediately();
        }

        private void OnDestroy()
        {
            _visibilityTween?.Kill();

            if (_authButton != null)
            {
                _authButton.onClick.RemoveListener(OnAuthClicked);
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveListener(Hide);
            }
        }

        /// <summary>Открыть окно. Презентер по этому событию запросит свежие данные.</summary>
        public void Show()
        {
            SetVisible(true);
            Opened?.Invoke();
        }

        public void Hide()
        {
            SetVisible(false);
        }

        public void SetData(LeaderboardViewData data)
        {
            if (data == null)
            {
                return;
            }

            if (_loadingRoot != null)
            {
                _loadingRoot.SetActive(data.IsLoading);
            }

            if (_unauthorizedRoot != null)
            {
                _unauthorizedRoot.SetActive(!data.IsAuthorized);
            }

            if (_emptyRoot != null)
            {
                _emptyRoot.SetActive(data.IsAuthorized && !data.IsLoading && !data.HasData);
            }

            int currentPlayerIndex = SetEntries(data.Entries);
            ScrollTo(currentPlayerIndex);
        }

        private void SetVisible(bool isVisible)
        {
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

        private void HideImmediately()
        {
            _isVisible = false;
            CanvasGroup canvasGroup = CanvasGroup;
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            Root.SetActive(false);
        }

        /// <returns>Индекс строки текущего игрока или -1, если его нет в списке.</returns>
        private int SetEntries(IReadOnlyList<LeaderboardEntryViewData> entries)
        {
            if (_entryReference == null)
            {
                return -1;
            }

            int count = entries?.Count ?? 0;
            int currentPlayerIndex = -1;

            for (int i = 0; i < count; i++)
            {
                LeaderboardEntryViewData entry = entries[i];
                LeaderboardEntryViewElement element = GetOrCreateElement(i);
                element.gameObject.SetActive(true);
                element.SetData(entry);

                if (entry != null && entry.IsCurrentPlayer)
                {
                    currentPlayerIndex = i;
                }
            }

            for (int i = count; i < _entryElements.Count; i++)
            {
                if (_entryElements[i] != null)
                {
                    _entryElements[i].gameObject.SetActive(false);
                }
            }

            return currentPlayerIndex;
        }

        private LeaderboardEntryViewElement GetOrCreateElement(int index)
        {
            if (index < _entryElements.Count && _entryElements[index] != null)
            {
                return _entryElements[index];
            }

            Transform root = _entriesRoot != null
                ? _entriesRoot
                : _scrollRect != null && _scrollRect.content != null
                    ? _scrollRect.content
                    : transform;

            LeaderboardEntryViewElement element = Instantiate(_entryReference, root);
            element.gameObject.SetActive(true);
            if (index < _entryElements.Count)
            {
                _entryElements[index] = element;
            }
            else
            {
                _entryElements.Add(element);
            }

            element.transform.SetSiblingIndex(index);
            return element;
        }

        /// <summary>Проматывает список к строке игрока, чтобы он сразу видел себя при открытии.</summary>
        private void ScrollTo(int index)
        {
            if (_scrollRect == null || index < 0 || index >= _entryElements.Count)
            {
                return;
            }

            RectTransform target = _entryElements[index] != null
                ? _entryElements[index].transform as RectTransform
                : null;
            RectTransform content = _scrollRect.content;
            RectTransform viewport = _scrollRect.viewport != null
                ? _scrollRect.viewport
                : _scrollRect.transform as RectTransform;

            if (target == null || content == null || viewport == null)
            {
                return;
            }

            // Карточки только что созданы: без принудительного пересчёта лэйаута
            // их позиции ещё нулевые и промотать по ним нельзя.
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);

            float scrollableHeight = content.rect.height - viewport.rect.height;
            if (scrollableHeight <= 0f)
            {
                return;
            }

            float targetCenterY = content.InverseTransformPoint(target.position).y;
            float distanceFromTop = content.rect.yMax - targetCenterY;
            float offset = distanceFromTop - viewport.rect.height * 0.5f;

            _scrollRect.verticalNormalizedPosition = 1f - Mathf.Clamp01(offset / scrollableHeight);
        }

        private void OnAuthClicked()
        {
            AuthRequested?.Invoke();
        }
    }
}
