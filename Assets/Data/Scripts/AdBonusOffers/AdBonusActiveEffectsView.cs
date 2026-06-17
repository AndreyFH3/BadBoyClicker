using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace AdBonusOffers
{
    public class AdBonusActiveEffectsView : MonoBehaviour, IInitializable, ITickable, System.IDisposable
    {
        [SerializeField] private RectTransform _root;
        [SerializeField] private TextMeshProUGUI _itemTemplate;

        private readonly List<TextMeshProUGUI> _items = new();
        private IAdBonusEffectService _effectService;

        [Inject]
        public void Construct(IAdBonusEffectService effectService)
        {
            _effectService = effectService;
        }

        public void Initialize()
        {
            BuildRuntimeUiIfNeeded();
            _effectService.Changed += Rebuild;
            Rebuild();
        }

        public void Dispose()
        {
            if (_effectService != null)
            {
                _effectService.Changed -= Rebuild;
            }
        }

        public void Tick()
        {
            UpdateTexts();
        }

        private void Rebuild()
        {
            BuildRuntimeUiIfNeeded();

            foreach (var item in _items)
            {
                if (item != null)
                {
                    Destroy(item.gameObject);
                }
            }

            _items.Clear();

            foreach (var effect in _effectService.ActiveEffects)
            {
                var item = Instantiate(_itemTemplate, _root);
                item.gameObject.SetActive(true);
                _items.Add(item);
            }

            _root.gameObject.SetActive(_items.Count > 0);
            UpdateTexts();
        }

        private void UpdateTexts()
        {
            var effects = _effectService.ActiveEffects;
            int count = Mathf.Min(_items.Count, effects.Count);

            for (int i = 0; i < count; i++)
            {
                _items[i].text = FormatEffect(effects[i]);
            }
        }

        private string FormatEffect(AdBonusActiveEffectViewData effect)
        {
            string name = effect.Type switch
            {
                AdBonusEffectType.ClickIncomeMultiplier => $"x{effect.Multiplier:0.#} clicks",
                AdBonusEffectType.PassiveIncomeMultiplier => $"x{effect.Multiplier:0.#} income",
                AdBonusEffectType.ShopDiscountPercent => $"-{effect.DiscountPercent:0.#}% shop",
                _ => effect.Type.ToString()
            };

            return $"{name} {FormatTime(effect.RemainingSeconds)}";
        }

        private void BuildRuntimeUiIfNeeded()
        {
            if (_root != null && _itemTemplate != null)
            {
                return;
            }

            var canvas = GetComponentInParent<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                var canvasObject = new GameObject("AdBonusEffectsCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                var scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080f, 1920f);
            }

            transform.SetParent(canvas.transform, false);

            if (_root == null)
            {
                var rootObject = new GameObject("AdBonusActiveEffects", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
                rootObject.transform.SetParent(canvas.transform, false);
                _root = rootObject.GetComponent<RectTransform>();
                _root.anchorMin = new Vector2(1f, 1f);
                _root.anchorMax = new Vector2(1f, 1f);
                _root.pivot = new Vector2(1f, 1f);
                _root.anchoredPosition = new Vector2(-24f, -250f);
                _root.sizeDelta = new Vector2(260f, 120f);

                var layout = rootObject.GetComponent<VerticalLayoutGroup>();
                layout.spacing = 6f;
                layout.childControlHeight = true;
                layout.childControlWidth = true;

                var fitter = rootObject.GetComponent<ContentSizeFitter>();
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            if (_itemTemplate == null)
            {
                var itemObject = new GameObject("EffectItemTemplate", typeof(RectTransform), typeof(Image));
                itemObject.transform.SetParent(_root, false);
                var rect = itemObject.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(260f, 34f);
                itemObject.GetComponent<Image>().color = new Color(0.05f, 0.08f, 0.1f, 0.9f);

                var textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                textObject.transform.SetParent(itemObject.transform, false);
                var textRect = textObject.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(8f, 2f);
                textRect.offsetMax = new Vector2(-8f, -2f);

                _itemTemplate = textObject.GetComponent<TextMeshProUGUI>();
                _itemTemplate.fontSize = 15;
                _itemTemplate.color = Color.white;
                _itemTemplate.alignment = TextAlignmentOptions.Center;
                itemObject.SetActive(false);
            }
        }

        private string FormatTime(float seconds)
        {
            int totalSeconds = Mathf.Max(0, Mathf.CeilToInt(seconds));
            return $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
        }
    }
}
