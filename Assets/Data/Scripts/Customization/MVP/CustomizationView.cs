using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Customization
{
    public class CustomizationView : MonoBehaviour, ICustomizationView
    {
        [Header("Tabs")]
        [SerializeField] private Button _backgroundTabButton;
        [SerializeField] private Button _catTabButton;
        [SerializeField] private float _fadeDuration = 0.25f;

        [Header("Items")]
        [SerializeField] private CanvasGroup _backgroundRoot;
        [SerializeField] private CanvasGroup _catRoot;
        [SerializeField] private CustomizationViewElement _reference;

        private readonly Dictionary<CustomizationItemType, Dictionary<string, CustomizationViewElement>> _elements = new();
        private CustomizationItemType _activeTab = CustomizationItemType.Background;

        public event Action<CustomizationItemType, string> ItemClicked;

        private void Awake()
        {
            EnsureElements();

            if (_reference != null)
            {
                _reference.gameObject.SetActive(false);
            }

            BindButtons();
            SetActiveTab(_activeTab, animate: false);
        }

        private void OnDestroy()
        {
            UnbindButtons();
        }

        public void SetData(List<CustomizationElementViewData> data)
        {
            EnsureElements();
            if (data == null || _reference == null)
            {
                return;
            }

            for (int i = 0; i < data.Count; i++)
            {
                SetElementData(data[i]);
            }
        }

        public void RequestBackgroundTab()
        {
            SetActiveTab(CustomizationItemType.Background);
        }

        public void RequestCatTab()
        {
            SetActiveTab(CustomizationItemType.Cat);
        }

        private void SetElementData(CustomizationElementViewData data)
        {
            if (data == null || string.IsNullOrEmpty(data.Id))
            {
                return;
            }

            Dictionary<string, CustomizationViewElement> elements = _elements[data.Type];
            if (!elements.TryGetValue(data.Id, out CustomizationViewElement element))
            {
                CanvasGroup root = GetRoot(data.Type);
                if (root == null)
                {
                    return;
                }

                element = Instantiate(_reference, root.transform);
                element.Clicked += OnElementClicked;
                element.gameObject.SetActive(true);
                elements.Add(data.Id, element);
            }

            element.Init(data);
        }

        private CanvasGroup GetRoot(CustomizationItemType type)
        {
            return type == CustomizationItemType.Background ? _backgroundRoot : _catRoot;
        }

        private void SetActiveTab(CustomizationItemType type, bool animate = true)
        {
            _activeTab = type;
            SetTabVisibility(_backgroundRoot, type == CustomizationItemType.Background, animate);
            SetTabVisibility(_catRoot, type == CustomizationItemType.Cat, animate);
        }

        private void SetTabVisibility(CanvasGroup group, bool isActive, bool animate)
        {
            if (group == null)
            {
                return;
            }

            group.DOKill();
            group.interactable = isActive;
            group.blocksRaycasts = isActive;

            float targetAlpha = isActive ? 1f : 0f;

            if (!animate)
            {
                group.alpha = targetAlpha;
                return;
            }

            group.DOFade(targetAlpha, _fadeDuration).OnComplete(() =>
            {
                group.interactable = isActive;
                group.blocksRaycasts = isActive;
            });
        }

        private void BindButtons()
        {
            if (_backgroundTabButton != null)
            {
                _backgroundTabButton.onClick.AddListener(RequestBackgroundTab);
            }

            if (_catTabButton != null)
            {
                _catTabButton.onClick.AddListener(RequestCatTab);
            }
        }

        private void UnbindButtons()
        {
            if (_backgroundTabButton != null)
            {
                _backgroundTabButton.onClick.RemoveListener(RequestBackgroundTab);
            }

            if (_catTabButton != null)
            {
                _catTabButton.onClick.RemoveListener(RequestCatTab);
            }

            foreach (Dictionary<string, CustomizationViewElement> typedElements in _elements.Values)
            {
                foreach (CustomizationViewElement element in typedElements.Values)
                {
                    if (element == null)
                    {
                        continue;
                    }

                    element.Clicked -= OnElementClicked;
                }
            }
        }

        private void EnsureElements()
        {
            EnsureElements(CustomizationItemType.Background);
            EnsureElements(CustomizationItemType.Cat);
        }

        private void EnsureElements(CustomizationItemType type)
        {
            if (!_elements.ContainsKey(type))
            {
                _elements.Add(type, new Dictionary<string, CustomizationViewElement>());
            }
        }

        private void OnElementClicked(CustomizationItemType type, string id)
        {
            ItemClicked?.Invoke(type, id);
        }
    }
}
