using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RewardActivation
{
    public class RewardActivationView : MonoBehaviour, IRewardActivationView
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private Button _activateButton;
        [SerializeField] private TextMeshProUGUI _activateButtonText;
        [SerializeField] private Button _postponeButton;
        [SerializeField] private TextMeshProUGUI _postponeButtonText;
        [SerializeField, Min(0f)] private float _fadeDuration = 0.2f;

        private CanvasGroup _canvasGroup;

        public event Action ActivateRequested;
        public event Action PostponeRequested;

        private void Awake()
        {
            BuildRuntimeUiIfNeeded();
            _canvasGroup = GetOrAddCanvasGroup(_root);
            AddListeners();
            HideImmediate();
        }

        private void OnDestroy()
        {
            RemoveListeners();
            DOTween.Kill(_canvasGroup);
        }

        public void Show(RewardActivationViewData data)
        {
            SetImage(_icon, data.Icon);
            SetText(_titleText, data.Title);
            SetText(_descriptionText, data.Description);
            SetText(_activateButtonText, data.ClaimButtonText);
            SetText(_postponeButtonText, data.PostponeButtonText);
            SetActive(_postponeButton != null ? _postponeButton.gameObject : null, data.ShowPostponeButton);
            SetActive(_activateButton != null ? _activateButton.gameObject : null, true);
            SetSingleButtonLayout(!data.ShowPostponeButton);
            SetActive(_root, true);
            FadeTo(1f, true);
        }

        public void Hide()
        {
            FadeTo(0f, false);
        }

        private void AddListeners()
        {
            if (_activateButton != null)
            {
                _activateButton.onClick.AddListener(RequestActivate);
            }

            if (_postponeButton != null)
            {
                _postponeButton.onClick.AddListener(RequestPostpone);
            }
        }

        private void RemoveListeners()
        {
            if (_activateButton != null)
            {
                _activateButton.onClick.RemoveListener(RequestActivate);
            }

            if (_postponeButton != null)
            {
                _postponeButton.onClick.RemoveListener(RequestPostpone);
            }
        }

        private void RequestActivate()
        {
            SetActive(_activateButton != null ? _activateButton.gameObject : null, false);
            ActivateRequested?.Invoke();
        }

        private void RequestPostpone()
        {
            SetInteractable(false);
            PostponeRequested?.Invoke();
        }

        private void BuildRuntimeUiIfNeeded()
        {
            if (_root != null)
            {
                return;
            }

            var canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                var canvasObject = new GameObject("RewardActivationCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                var scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080f, 1920f);
            }

            transform.SetParent(canvas.transform, false);

            _root = CreatePanel("RewardActivationPanel", canvas.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(440f, 340f), new Color(0.06f, 0.07f, 0.09f, 0.98f));
            _icon = CreateImage("Icon", _root.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -58f), new Vector2(72f, 72f));
            _titleText = CreateText("Title", _root.transform, 24, TextAlignmentOptions.Center, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -116f), new Vector2(-48f, 40f));
            _descriptionText = CreateText("Description", _root.transform, 17, TextAlignmentOptions.Center, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -160f), new Vector2(-52f, 100f));
            _activateButton = CreateButton("ActivateButton", _root.transform, string.Empty, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(75f, 36f), new Vector2(130f, 48f), out _activateButtonText);
            _postponeButton = CreateButton("PostponeButton", _root.transform, string.Empty, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-75f, 36f), new Vector2(130f, 48f), out _postponeButtonText);
        }

        private GameObject CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            panel.GetComponent<Image>().color = color;
            return panel;
        }

        private Image CreateImage(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 size)
        {
            var imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            var rect = imageObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            imageObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.95f);
            return imageObject.GetComponent<Image>();
        }

        private TextMeshProUGUI CreateText(string name, Transform parent, int size, TextAlignmentOptions alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            var rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            var text = textObject.GetComponent<TextMeshProUGUI>();
            text.fontSize = size;
            text.color = Color.white;
            text.alignment = alignment;
            text.textWrappingMode = TextWrappingModes.Normal;
            return text;
        }

        private Button CreateButton(string name, Transform parent, string text, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 size, out TextMeshProUGUI label)
        {
            var buttonObject = CreatePanel(name, parent, anchorMin, anchorMax, pivot, anchoredPosition, size, new Color(0.18f, 0.38f, 0.7f, 1f));
            var button = buttonObject.AddComponent<Button>();
            label = CreateText("Text", buttonObject.transform, 17, TextAlignmentOptions.Center, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            label.text = text;
            return button;
        }

        private void HideImmediate()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
            }

            SetInteractable(false);
            SetActive(_root, false);
        }

        private void FadeTo(float alpha, bool interactableAfterFade)
        {
            if (_canvasGroup == null)
            {
                SetActive(_root, alpha > 0f);
                return;
            }

            DOTween.Kill(_canvasGroup);
            SetInteractable(false);
            _canvasGroup.DOFade(alpha, _fadeDuration)
                .SetTarget(_canvasGroup)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    if (alpha <= 0f)
                    {
                        SetActive(_root, false);
                    }
                    else
                    {
                        SetInteractable(interactableAfterFade);
                    }
                });
        }

        private void SetInteractable(bool value)
        {
            if (_canvasGroup == null)
            {
                return;
            }

            _canvasGroup.interactable = value;
            _canvasGroup.blocksRaycasts = value;
        }

        private void SetSingleButtonLayout(bool singleButton)
        {
            if (_activateButton == null || !(_activateButton.transform is RectTransform rect))
            {
                return;
            }

            rect.anchoredPosition = new Vector2(singleButton ? 0f : 75f, rect.anchoredPosition.y);
            rect.sizeDelta = new Vector2(singleButton ? 240f : 130f, rect.sizeDelta.y);
        }

        private static CanvasGroup GetOrAddCanvasGroup(GameObject target)
        {
            if (target == null)
            {
                return null;
            }

            return target.TryGetComponent(out CanvasGroup group) ? group : target.AddComponent<CanvasGroup>();
        }

        private void SetImage(Image image, Sprite sprite)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = sprite;
            image.enabled = sprite != null;
        }

        private void SetText(TextMeshProUGUI text, string value)
        {
            if (text != null)
            {
                text.text = value ?? string.Empty;
            }
        }

        private void SetActive(GameObject target, bool value)
        {
            if (target != null)
            {
                target.SetActive(value);
            }
        }
    }
}
