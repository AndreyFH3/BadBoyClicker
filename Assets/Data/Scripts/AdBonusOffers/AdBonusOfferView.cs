using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace AdBonusOffers
{
    public class AdBonusOfferView : MonoBehaviour, IAdBonusOfferView
    {
        [Header("Card")]
        [SerializeField] private GameObject _cardRoot;
        [SerializeField] private Button _cardButton;
        [SerializeField] private Image _cardIcon;
        [SerializeField] private TextMeshProUGUI _cardTimerText;

        [Header("Confirmation")]
        [SerializeField] private GameObject _confirmationRoot;
        [SerializeField] private Image _confirmationIcon;
        [SerializeField] private TextMeshProUGUI _confirmationTitleText;
        [SerializeField] private TextMeshProUGUI _confirmationValueText;
        [SerializeField] private TextMeshProUGUI _confirmationDescriptionText;
        [SerializeField] private Button _confirmAdButton;
        [SerializeField] private Button _confirmHardButton;
        [SerializeField] private TextMeshProUGUI _confirmHardText;
        [SerializeField] private Button _confirmationCloseButton;

        [Header("Result")]
        [SerializeField] private GameObject _resultRoot;
        [SerializeField] private Image _resultIcon;
        [SerializeField] private TextMeshProUGUI _resultTitleText;
        [SerializeField] private TextMeshProUGUI _resultValueText;
        [SerializeField] private TextMeshProUGUI _resultDescriptionText;
        [SerializeField] private Button _resultCloseButton;

        public event Action CardClicked;
        public event Action ConfirmRequested;
        public event Action HardClaimRequested;
        public event Action ClosedRequested;
        public event Action ResultClosedRequested;

        private void Awake()
        {
            BuildRuntimeUiIfNeeded();
            AddListeners();
            HideCard();
            HideConfirmation();
            HideRewardResult();
        }

        private void OnDestroy()
        {
            RemoveListeners();
        }

        public void ShowCard(AdBonusOfferViewData data)
        {
            SetImage(_cardIcon, data.Icon);
            SetActive(_cardRoot, true);
        }

        public void SetCardRemainingSeconds(float seconds)
        {
            SetTexts(_cardTimerText, FormatTime(seconds));
        }

        public void HideCard()
        {
            SetActive(_cardRoot, false);
        }

        public void ShowConfirmation(AdBonusOfferViewData data)
        {
            HideCard();
            SetTexts(_confirmationTitleText, ComposeTitle(data, _confirmationValueText));
            SetTexts(_confirmationValueText, data.RewardValueText);
            SetTexts(_confirmationDescriptionText, data.Description);
            SetImage(_confirmationIcon, data.Icon);

            if (_confirmAdButton != null)
            {
                _confirmAdButton.interactable = data.CanClaimWithAd;
            }

            if (_confirmHardButton != null)
            {
                _confirmHardButton.gameObject.SetActive(data.CanClaimForHard);
            }

            if (_confirmHardText != null)
            {
                _confirmHardText.text = data.CanClaimForHard
                    ? $"Get for {data.HardPrice.ConvertFromLongToString()}"
                    : string.Empty;
            }

            SetActive(_confirmationRoot, true);
        }

        public void HideConfirmation()
        {
            SetActive(_confirmationRoot, false);
        }

        public void ShowRewardResult(AdBonusOfferViewData data)
        {
            SetTexts(_resultTitleText, ComposeTitle(data, _resultValueText));
            SetTexts(_resultValueText, data.RewardValueText);
            SetTexts(_resultDescriptionText, data.ResultDescription);
            SetImage(_resultIcon, data.Icon);
            SetActive(_resultRoot, true);
        }

        public void HideRewardResult()
        {
            SetActive(_resultRoot, false);
        }

        private void AddListeners()
        {
            if (_cardButton != null)
                _cardButton.onClick.AddListener(RequestCardClick);
            if (_confirmAdButton != null)
                _confirmAdButton.onClick.AddListener(RequestConfirm);
            if (_confirmHardButton != null)
                _confirmHardButton.onClick.AddListener(RequestHardClaim);
            if (_confirmationCloseButton != null)
                _confirmationCloseButton.onClick.AddListener(RequestCloseConfirmation);
            if (_resultCloseButton != null)
                _resultCloseButton.onClick.AddListener(RequestCloseResult);
        }

        private void RemoveListeners()
        {
            if (_cardButton != null)
                _cardButton.onClick.RemoveListener(RequestCardClick);
            if (_confirmAdButton != null)
                _confirmAdButton.onClick.RemoveListener(RequestConfirm);
            if (_confirmHardButton != null)
                _confirmHardButton.onClick.RemoveListener(RequestHardClaim);
            if (_confirmationCloseButton != null)
                _confirmationCloseButton.onClick.RemoveListener(RequestCloseConfirmation);
            if (_resultCloseButton != null)
                _resultCloseButton.onClick.RemoveListener(RequestCloseResult);
        }

        private void RequestCardClick()
        {
            CardClicked?.Invoke();
        }

        private void RequestConfirm()
        {
            ConfirmRequested?.Invoke();
        }

        private void RequestHardClaim()
        {
            HardClaimRequested?.Invoke();
        }

        private void RequestCloseConfirmation()
        {
            ClosedRequested?.Invoke();
        }

        private void RequestCloseResult()
        {
            ResultClosedRequested?.Invoke();
        }

        private void BuildRuntimeUiIfNeeded()
        {
            if (_cardRoot != null && _confirmationRoot != null && _resultRoot != null)
            {
                return;
            }

            var canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                var canvasObject = new GameObject("AdBonusOfferCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                var scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080f, 1920f);
            }

            transform.SetParent(canvas.transform, false);

            if (_cardRoot == null)
            {
                BuildCard(canvas.transform);
            }

            if (_confirmationRoot == null)
            {
                BuildConfirmation(canvas.transform);
            }

            if (_resultRoot == null)
            {
                BuildResult(canvas.transform);
            }
        }

        private void BuildCard(Transform parent)
        {
            _cardRoot = CreatePanel("AdBonusCard", parent, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -140f), new Vector2(300f, 92f), new Color(0.08f, 0.1f, 0.12f, 0.94f));
            _cardButton = _cardRoot.AddComponent<Button>();
            _cardIcon = CreateImage("Icon", _cardRoot.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(44f, 0f), new Vector2(56f, 56f));
            _cardTimerText = CreateText("Timer", _cardRoot.transform, 14, TextAlignmentOptions.Right, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-18f, 18f), new Vector2(70f, 28f));
        }

        private void BuildConfirmation(Transform parent)
        {
            _confirmationRoot = CreatePanel("AdBonusConfirmation", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(460f, 360f), new Color(0.06f, 0.07f, 0.09f, 0.98f));
            _confirmationIcon = CreateImage("Icon", _confirmationRoot.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -58f), new Vector2(72f, 72f));
            _confirmationTitleText = CreateText("Title", _confirmationRoot.transform, 24, TextAlignmentOptions.Center, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -116f), new Vector2(-48f, 40f));
            _confirmationValueText = CreateText("Value", _confirmationRoot.transform, 20, TextAlignmentOptions.Center, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -152f), new Vector2(-48f, 30f));
            _confirmationDescriptionText = CreateText("Description", _confirmationRoot.transform, 17, TextAlignmentOptions.Center, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -186f), new Vector2(-52f, 64f));
            _confirmAdButton = CreateButton("AdButton", _confirmationRoot.transform, "Watch ad", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 82f), new Vector2(240f, 48f));
            _confirmHardButton = CreateButton("HardButton", _confirmationRoot.transform, "Get for hard", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 28f), new Vector2(240f, 42f));
            _confirmHardText = _confirmHardButton.GetComponentInChildren<TextMeshProUGUI>();
            _confirmationCloseButton = CreateButton("Close", _confirmationRoot.transform, "X", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -24f), new Vector2(36f, 36f));
        }

        private void BuildResult(Transform parent)
        {
            _resultRoot = CreatePanel("AdBonusResult", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(380f, 280f), new Color(0.06f, 0.07f, 0.09f, 0.98f));
            _resultIcon = CreateImage("Icon", _resultRoot.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -52f), new Vector2(68f, 68f));
            _resultTitleText = CreateText("Title", _resultRoot.transform, 24, TextAlignmentOptions.Center, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -112f), new Vector2(-48f, 40f));
            _resultValueText = CreateText("Value", _resultRoot.transform, 20, TextAlignmentOptions.Center, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -148f), new Vector2(-48f, 30f));
            _resultDescriptionText = CreateText("Description", _resultRoot.transform, 16, TextAlignmentOptions.Center, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -182f), new Vector2(-48f, 42f));
            _resultCloseButton = CreateButton("Ok", _resultRoot.transform, "OK", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(160f, 44f));
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

        private string ComposeTitle(AdBonusOfferViewData data, TextMeshProUGUI valueText)
        {
            if (valueText == null && !string.IsNullOrEmpty(data.RewardValueText))
            {
                return $"{data.RewardTitle} {data.RewardValueText}";
            }

            return data.RewardTitle;
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

        private void SetTexts(TextMeshProUGUI text, string value)
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

        private string FormatTime(float seconds)
        {
            int totalSeconds = Mathf.Max(0, Mathf.CeilToInt(seconds));
            return $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
        }
    }
}
