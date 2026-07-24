using System.Collections;
using GameLocalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YG;

namespace Core.Ads
{
    public class AdLoadingOverlay : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _progressText;
        [Min(1f)] [SerializeField] private float _timeoutSeconds = 15f;

        private Coroutine _timeoutCoroutine;
        private float _animationTime;

        private void Awake()
        {
            BuildRuntimeUiIfNeeded();
            Hide();
            YG2.onAdvNotification += Show;
            YG2.onOpenAnyAdv += Hide;
            YG2.onCloseAnyAdv += Hide;
            YG2.onErrorAnyAdv += Hide;
            Localization.LanguageChanged += RefreshText;
        }

        private void OnDestroy()
        {
            YG2.onAdvNotification -= Show;
            YG2.onOpenAnyAdv -= Hide;
            YG2.onCloseAnyAdv -= Hide;
            YG2.onErrorAnyAdv -= Hide;
            Localization.LanguageChanged -= RefreshText;
        }

        private void Update()
        {
            if (_root == null || !_root.activeSelf || _progressText == null) return;
            _animationTime += UnityEngine.Time.unscaledDeltaTime;
            _progressText.text = new string('\u2022', 1 + Mathf.FloorToInt(_animationTime * 2f) % 3);
        }

        public void Show()
        {
            BuildRuntimeUiIfNeeded();
            RefreshText();
            _animationTime = 0f;
            _root.transform.SetAsLastSibling();
            _root.SetActive(true);
            if (_timeoutCoroutine != null) StopCoroutine(_timeoutCoroutine);
            _timeoutCoroutine = StartCoroutine(HideAfterTimeout());
        }

        public void Hide()
        {
            if (_timeoutCoroutine != null)
            {
                StopCoroutine(_timeoutCoroutine);
                _timeoutCoroutine = null;
            }
            if (_root != null) _root.SetActive(false);
        }

        private IEnumerator HideAfterTimeout()
        {
            yield return new WaitForSecondsRealtime(_timeoutSeconds);
            _timeoutCoroutine = null;
            Hide();
        }

        private void RefreshText()
        {
            if (_titleText != null) _titleText.text = Localization.Tr("ad.loading.title");
        }

        private void BuildRuntimeUiIfNeeded()
        {
            if (_root != null) return;

            var canvasObject = new GameObject("AdLoadingCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32000;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);

            _root = CreateImage("Blocker", canvasObject.transform, new Color(0f, 0f, 0f, 0.82f));
            StretchToParent(_root.GetComponent<RectTransform>());
            GameObject panel = CreateImage("Message", _root.transform, new Color(0.08f, 0.09f, 0.12f, 0.98f));
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = panelRect.anchorMax = panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(620f, 240f);
            _titleText = CreateText("Title", panel.transform, 34f, new Vector2(0f, 0.42f), new Vector2(1f, 0.88f));
            _progressText = CreateText("Progress", panel.transform, 42f, new Vector2(0f, 0.12f), new Vector2(1f, 0.45f));
            RefreshText();
        }

        private static GameObject CreateImage(string name, Transform parent, Color color)
        {
            var result = new GameObject(name, typeof(RectTransform), typeof(Image));
            result.transform.SetParent(parent, false);
            result.GetComponent<Image>().color = color;
            return result;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, float fontSize, Vector2 anchorMin, Vector2 anchorMax)
        {
            var result = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            result.transform.SetParent(parent, false);
            RectTransform rect = result.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = new Vector2(36f, 0f);
            rect.offsetMax = new Vector2(-36f, 0f);
            var text = result.GetComponent<TextMeshProUGUI>();
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.fontSize = fontSize;
            text.textWrappingMode = TextWrappingModes.Normal;
            return text;
        }

        private static void StretchToParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
