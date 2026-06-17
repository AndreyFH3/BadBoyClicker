using GameLocalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Ads
{
    public class RewardedAdErrorView : MonoBehaviour, IRewardedAdErrorView
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Button _closeButton;

        private void Awake()
        {
            BuildRuntimeUiIfNeeded();
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

        public void Show()
        {
            BuildRuntimeUiIfNeeded();
            SetText(_titleText, Localization.Tr("error_title", "Ad error"));
            SetText(_messageText, Localization.Tr("error_description", "Something went wrong while showing the ad. Please try again later."));

            if (_root != null)
            {
                _root.transform.SetAsLastSibling();
                _root.SetActive(true);
            }
        }

        public void Hide()
        {
            if (_root != null)
            {
                _root.SetActive(false);
            }
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
                var canvasObject = new GameObject("RewardedAdErrorCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 10000;

                var scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080f, 1920f);
            }

            transform.SetParent(canvas.transform, false);
            _root = CreatePanel("RewardedAdErrorPopup", canvas.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(480f, 230f), new Color(0.06f, 0.07f, 0.09f, 0.98f));
            _titleText = CreateText("Title", _root.transform, 25, TextAlignmentOptions.Center, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -42f), new Vector2(-48f, 42f));
            _messageText = CreateText("Message", _root.transform, 18, TextAlignmentOptions.Center, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -105f), new Vector2(-56f, 70f));
            _closeButton = CreateButton("Close", _root.transform, "OK", new Vector2(0.5f, 0f), new Vector2(0f, 34f), new Vector2(160f, 44f));
        }

        private GameObject CreatePanel(string name, Transform parent, Vector2 anchor, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);

            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            panel.GetComponent<Image>().color = color;
            return panel;
        }

        private Button CreateButton(string name, Transform parent, string text, Vector2 anchor, Vector2 anchoredPosition, Vector2 size)
        {
            var buttonObject = CreatePanel(name, parent, anchor, anchoredPosition, size, new Color(0.18f, 0.38f, 0.7f, 1f));
            var button = buttonObject.AddComponent<Button>();
            var label = CreateText("Text", buttonObject.transform, 17, TextAlignmentOptions.Center, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            label.text = text;
            return button;
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

        private void SetText(TextMeshProUGUI text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }
    }
}
