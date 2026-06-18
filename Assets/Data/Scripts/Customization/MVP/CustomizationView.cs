using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Customization
{
    public class CustomizationView : MonoBehaviour, ICustomizationView
    {
        [Header("Window")]
        [SerializeField] private GameObject _windowRoot;
        [SerializeField] private bool _hideOnAwake = true;
        [SerializeField] private Button _openButton;
        [SerializeField] private Button _closeButton;

        [Header("Tabs")]
        [SerializeField] private Button _backgroundTabButton;
        [SerializeField] private Button _catTabButton;

        [Header("Items")]
        [SerializeField] private Transform _backgroundRoot;
        [SerializeField] private Transform _catRoot;
        [SerializeField] private CustomizationViewElement _reference;

        private readonly Dictionary<CustomizationItemType, Dictionary<string, CustomizationViewElement>> _elements = new();
        private CustomizationItemType _activeTab = CustomizationItemType.Background;

        public event Action OpenRequested;
        public event Action CloseRequested;
        public event Action<CustomizationItemType, string> ItemClicked;

        public bool IsActive => _windowRoot != null ? _windowRoot.activeInHierarchy : gameObject.activeInHierarchy;

        private void Awake()
        {
            if (_windowRoot == null)
            {
                _windowRoot = gameObject;
            }

            EnsureElements();

            if (_reference != null)
            {
                _reference.gameObject.SetActive(false);
            }

            BindButtons();
            SetActiveTab(_activeTab);

            if (_hideOnAwake)
            {
                _windowRoot.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            UnbindButtons();
        }

        public void SetOpenState(bool isOpen)
        {
            if (_windowRoot == null || _windowRoot.activeSelf == isOpen)
            {
                return;
            }

            _windowRoot.SetActive(isOpen);
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

        public void RequestOpen()
        {
            OpenRequested?.Invoke();
        }

        public void RequestClose()
        {
            CloseRequested?.Invoke();
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
                Transform root = GetRoot(data.Type);
                if (root == null)
                {
                    return;
                }

                element = Instantiate(_reference, root);
                element.Clicked += OnElementClicked;
                element.gameObject.SetActive(true);
                elements.Add(data.Id, element);
            }

            element.Init(data);
        }

        private Transform GetRoot(CustomizationItemType type)
        {
            return type == CustomizationItemType.Background ? _backgroundRoot : _catRoot;
        }

        private void SetActiveTab(CustomizationItemType type)
        {
            _activeTab = type;
            SetActive(_backgroundRoot != null ? _backgroundRoot.gameObject : null, type == CustomizationItemType.Background);
            SetActive(_catRoot != null ? _catRoot.gameObject : null, type == CustomizationItemType.Cat);
        }

        private void BindButtons()
        {
            if (_openButton != null)
            {
                _openButton.onClick.AddListener(RequestOpen);
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(RequestClose);
            }

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
            if (_openButton != null)
            {
                _openButton.onClick.RemoveListener(RequestOpen);
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveListener(RequestClose);
            }

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

        private void SetActive(GameObject target, bool isActive)
        {
            if (target != null)
            {
                target.SetActive(isActive);
            }
        }
    }
}
