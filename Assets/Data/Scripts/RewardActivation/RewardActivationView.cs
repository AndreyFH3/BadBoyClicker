using System;
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

        public event Action ActivateRequested;

        private void Awake()
        {
            BuildRuntimeUiIfNeeded();
            AddListeners();
            SetActive(_root, false);
        }

        private void OnDestroy()
        {
            RemoveListeners();
        }

        public void Show(RewardActivationViewData data)
        {
            SetImage(_icon, data.Icon);
            SetText(_titleText, data.Title);
            SetText(_descriptionText, data.Description);
            SetActive(_root, true);
        }

        public void Hide()
        {
            SetActive(_root, false);
        }

        private void AddListeners()
        {
            if (_activateButton != null)
            {
                _activateButton.onClick.AddListener(RequestActivate);
            }
        }

        private void RemoveListeners()
        {
            if (_activateButton != null)
            {
                _activateButton.onClick.RemoveListener(RequestActivate);
            }
        }

        private void RequestActivate()
        {
            ActivateRequested?.Invoke();
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
            _activateButton = CreateButton("ActivateButton", _root.transform, "Activate", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 36f), new Vector2(240f, 48f));
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

        private Button CreateButton(string name, Transform parent, string text, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 size)
        {
            var buttonObject = CreatePanel(name, parent, anchorMin, anchorMax, pivot, anchoredPosition, size, new Color(0.18f, 0.38f, 0.7f, 1f));
            var button = buttonObject.AddComponent<Button>();
            var label = CreateText("Text", buttonObject.transform, 17, TextAlignmentOptions.Center, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            label.text = text;
            return button;
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
