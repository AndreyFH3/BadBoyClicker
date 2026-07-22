using System.Collections.Generic;
using GameLocalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace AdBonusOffers
{
    public class AdBonusActiveEffectsView : MonoBehaviour, IInitializable, ITickable, System.IDisposable
    {
        [SerializeField] private RectTransform _root;
        [SerializeField] private AdBonusActiveEffectItemView _itemTemplate;

        private readonly List<AdBonusActiveEffectItemView> _items = new();
        private IBuffService _buffService;
        private ILocalizationService _localization;

        [Inject]
        public void Construct(IBuffService buffService, ILocalizationService localization)
        {
            _buffService = buffService;
            _localization = localization;
        }

        public void Initialize()
        {
            BuildRuntimeUiIfNeeded();
            _buffService.Changed += Rebuild;
            Rebuild();
        }

        public void Dispose()
        {
            if (_buffService != null)
            {
                _buffService.Changed -= Rebuild;
            }
        }

        public void Tick()
        {
            UpdateItems();
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

            foreach (var effect in _buffService.ActiveEffects)
            {
                var item = Instantiate(_itemTemplate, _root);
                item.gameObject.SetActive(true);
                _items.Add(item);
            }

            _root.gameObject.SetActive(_items.Count > 0);
            UpdateItems();
        }

        private void UpdateItems()
        {
            var effects = _buffService.ActiveEffects;
            int count = Mathf.Min(_items.Count, effects.Count);

            for (int i = 0; i < count; i++)
            {
                _items[i].SetText(FormatEffect(effects[i]));
                _items[i].SetProgress(effects[i].Progress01);
            }
        }

        private string FormatEffect(AdBonusActiveEffectViewData effect)
        {
            string name = effect.Type switch
            {
                AdBonusEffectType.ClickIncomeMultiplier => _localization.Format("ad_bonus.effect.click_multiplier", $"{effect.Multiplier:0.#}"),
                AdBonusEffectType.PassiveIncomeMultiplier => _localization.Format("ad_bonus.effect.passive_multiplier", $"{effect.Multiplier:0.#}"),
                AdBonusEffectType.ShopDiscountPercent => _localization.Format("ad_bonus.effect.shop_discount", $"{effect.DiscountPercent:0.#}"),
                AdBonusEffectType.AllIncomeMultiplier => _localization.Format("ad_bonus.effect.all_income_multiplier", $"{effect.Multiplier:0.#}"),
                AdBonusEffectType.ExperienceMultiplier => _localization.Format("ad_bonus.effect.experience_multiplier", $"{effect.Multiplier:0.#}"),
                _ => effect.Type.ToString()
            };

            return _localization.Format(
                "ad_bonus.effect.active_format",
                name,
                FormatTime(effect.RemainingSeconds));
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
                _itemTemplate = BuildItemTemplate();
            }
        }

        private AdBonusActiveEffectItemView BuildItemTemplate()
        {
            var itemObject = new GameObject("EffectItemTemplate", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            itemObject.transform.SetParent(_root, false);
            var rect = itemObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(260f, 34f);
            itemObject.GetComponent<Image>().color = new Color(0.05f, 0.08f, 0.1f, 0.9f);
            itemObject.GetComponent<LayoutElement>().preferredHeight = 34f;

            var fillObject = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillObject.transform.SetParent(itemObject.transform, false);
            var fillRect = fillObject.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(2f, 2f);
            fillRect.offsetMax = new Vector2(-2f, -2f);

            var fillImage = fillObject.GetComponent<Image>();
            fillImage.color = new Color(0.18f, 0.38f, 0.7f, 0.85f);
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImage.fillAmount = 1f;

            var textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(itemObject.transform, false);
            var textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(8f, 2f);
            textRect.offsetMax = new Vector2(-8f, -2f);

            var label = textObject.GetComponent<TextMeshProUGUI>();
            label.fontSize = 15;
            label.color = Color.white;
            label.alignment = TextAlignmentOptions.Center;

            var itemView = itemObject.AddComponent<AdBonusActiveEffectItemView>();
            itemView.SetReferences(label, fillImage);
            itemObject.SetActive(false);
            return itemView;
        }

        private string FormatTime(float seconds)
        {
            int totalSeconds = Mathf.Max(0, Mathf.CeilToInt(seconds));
            return $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
        }
    }
}
