using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChestsMVP
{
    public class ChestOpenView : MonoBehaviour, IChestOpenView
    {
        [Header("Root")]
        [SerializeField] private GameObject _root;
        [SerializeField] private Image _chestIcon;
        [SerializeField] private TextMeshProUGUI _chestTitle;

        [Header("Opening state")]
        [Tooltip("Enabled and interactable right after Show(). Requires a CanvasGroup. Its own tween must use Manual play mode - it is triggered from code.")]
        [SerializeField] private GameObject _openStateRoot;
        [SerializeField] private TextMeshProUGUI _openHintText;
        [SerializeField] private Button _openButton;
        [Tooltip("Optional. Plays while the open button and hint text are hidden before the chest animation starts.")]
        [SerializeField] private SimpleTweenAnimation _openStateHideAnimation;

        [Header("Chest animation")]
        [Tooltip("Optional. Plays the chest opening itself (e.g. lid, shake, glow). If it is the same asset as _openStateHideAnimation it is only played once.")]
        [SerializeField] private SimpleTweenAnimation _chestOpenAnimation;
        [Tooltip("Enabled the moment the chest animation completes (e.g. reward burst/particles).")]
        [SerializeField] private GameObject _rewardRevealEffect;

        [Header("Reward state")]
        [Tooltip("Disabled right after Show() and enabled once the chest animation completes. Requires a CanvasGroup. Its own reveal tween should use OnEnable play mode - it is triggered by activating this object, not from code.")]
        [SerializeField] private GameObject _rewardRoot;
        [SerializeField] private Image _rewardIcon;
        [SerializeField] private TextMeshProUGUI _rewardText;
        [SerializeField] private Button _closeButton;

        private CanvasGroup _openStateCanvasGroup;
        private CanvasGroup _rewardCanvasGroup;

        private ChestOpenViewData _pendingData;
        private bool _isOpening;

        public event Action CloseRequested;

        private void Awake()
        {
            _openStateCanvasGroup = GetCanvasGroup(_openStateRoot);
            _rewardCanvasGroup = GetCanvasGroup(_rewardRoot);

            if (_openButton != null)
            {
                _openButton.onClick.AddListener(OnOpenButtonClicked);
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(RequestClose);
            }

            Hide();
        }

        private void OnDestroy()
        {
            if (_openButton != null)
            {
                _openButton.onClick.RemoveListener(OnOpenButtonClicked);
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveListener(RequestClose);
            }
        }

        public void Show(ChestOpenViewData data)
        {
            _pendingData = data;
            _isOpening = false;

            Root.SetActive(true);

            SetImage(_chestIcon, data?.ChestIcon);
            SetText(_chestTitle, data?.ChestTitle);
            SetText(_openHintText, data?.OpenHintText);

            SetActive(_rewardRevealEffect, false);

            // Opening state: back to fully visible and interactable, and its tween rewound
            // (restores the chest icon scale / alpha it captured the first time it played).
            SetActive(_openStateRoot, true);
            SetCanvasGroupState(_openStateCanvasGroup, visible: true);

            if (_openStateHideAnimation != null)
            {
                _openStateHideAnimation.Rewind();
            }

            // Reward state: disabled, not just faded out - so its own OnEnable reveal tween
            // is guaranteed to fire again the next time RevealReward() enables it.
            SetCanvasGroupState(_rewardCanvasGroup, visible: false);
            SetActive(_rewardRoot, false);
        }

        public void Hide()
        {
            _isOpening = false;
            _pendingData = null;

            Root.SetActive(false);
        }

        private GameObject Root => _root != null ? _root : gameObject;

        private async void OnOpenButtonClicked()
        {
            if (_isOpening)
            {
                return;
            }

            _isOpening = true;

            ChestOpenViewData data = _pendingData;

            await PlayChestOpeningAsync();

            RevealReward(data);

            _isOpening = false;
        }

        private async Awaitable PlayChestOpeningAsync()
        {
            SetCanvasGroupInteractable(_openStateCanvasGroup, false);

            await PlayTweenAsync(_openStateHideAnimation);

            // The chest-open animation is often the very same asset that already faded the
            // opening state out (one combined tween). Only play it again if it is a distinct one.
            if (_chestOpenAnimation != null && _chestOpenAnimation != _openStateHideAnimation)
            {
                await PlayTweenAsync(_chestOpenAnimation);
            }

            SetCanvasGroupAlpha(_openStateCanvasGroup, 0f);
            SetActive(_openStateRoot, false);
        }

        private void RevealReward(ChestOpenViewData data)
        {
            SetActive(_rewardRevealEffect, true);

            SetImage(_rewardIcon, data?.RewardIcon);
            SetText(_rewardText, data?.RewardText);

            SetCanvasGroupInteractable(_rewardCanvasGroup, true);

            // Enabling the object (rather than just flipping alpha) is what triggers its
            // own OnEnable reveal tween.
            SetActive(_rewardRoot, true);
        }

        private static Awaitable PlayTweenAsync(SimpleTweenAnimation animation)
        {
            var completionSource = new AwaitableCompletionSource();

            if (animation == null)
            {
                completionSource.SetResult();
                return completionSource.Awaitable;
            }

            animation.Play(() => completionSource.SetResult());
            return completionSource.Awaitable;
        }

        private static CanvasGroup GetCanvasGroup(GameObject go)
        {
            return go != null ? go.GetComponent<CanvasGroup>() : null;
        }

        private static void SetCanvasGroupState(CanvasGroup canvasGroup, bool visible)
        {
            SetCanvasGroupAlpha(canvasGroup, visible ? 1f : 0f);
            SetCanvasGroupInteractable(canvasGroup, visible);
        }

        private static void SetCanvasGroupInteractable(CanvasGroup canvasGroup, bool interactable)
        {
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.interactable = interactable;
            canvasGroup.blocksRaycasts = interactable;
        }

        private static void SetCanvasGroupAlpha(CanvasGroup canvasGroup, float alpha)
        {
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha = alpha;
        }

        private static void SetActive(GameObject go, bool active)
        {
            if (go != null)
            {
                go.SetActive(active);
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

        private static void SetText(TextMeshProUGUI text, string value)
        {
            if (text == null)
            {
                return;
            }

            text.text = value ?? string.Empty;
            text.gameObject.SetActive(!string.IsNullOrEmpty(value));
        }

        private void RequestClose()
        {
            CloseRequested?.Invoke();
        }
    }
}
