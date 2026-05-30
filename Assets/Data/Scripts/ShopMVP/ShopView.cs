using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Shop
{
    public class ShopView : MonoBehaviour, IShopView
    {
        [SerializeField] private Transform _shopRoot;
        [SerializeField] private Transform _clicksRoot;
        [SerializeField] private Transform _autoBuysRoot;
        [SerializeField] private Transform _paidBuysRoot;
        [SerializeField] private CanvasGroup _clicksCanvasGroup;
        [SerializeField] private CanvasGroup _autoBuysCanvasGroup;
        [SerializeField] private CanvasGroup _paidBuysCanvasGroup;
        [SerializeField] private Button _clicksTabButton;
        [SerializeField] private Button _autoBuysTabButton;
        [SerializeField] private Button _paidBuysTabButton;
        [SerializeField] private float _tabFadeDuration = 0.2f;
        [SerializeField] private ShopViewElement _reference;
        [SerializeField] private TextMeshProUGUI _earnPerSecond;
        
        private readonly Dictionary<ShopItemType, Dictionary<string, ShopViewElement>> _shopElements = new();
        private ShopItemType _activeTab = ShopItemType.Click;

        public event Action OpenRequested;
        public event Action CloseRequested;
        public event Action<string> OnBuy;

        public bool IsActive => gameObject.activeInHierarchy;

        private void Awake()
        {
            EnsureElements();
            if (_reference != null)
                _reference.gameObject.SetActive(false);
            AddTabListeners();
            SetActiveTab(_activeTab);
        }

        private void OnDestroy()
        {
            RemoveTabListeners();
        }

        public void SetOpenState(bool isOpen)
        {
            if (gameObject.activeSelf == isOpen)
                return;

            gameObject.SetActive(isOpen);
        }

        public void SetData(List<ShopElementData> datas)
        {
            EnsureElements();
            if (_reference == null || datas == null)
            {
                return;
            }

            foreach (var data in datas)
            {
                SetElementData(data);
            }
        }

        public void SetEarnPerSecond(long value)
        {
            if (_earnPerSecond == null)
                return;

            _earnPerSecond.text = value.ConvertFromLongToString();
        }

        private void UpdateElement(string id)
        {
            OnBuy?.Invoke(id);
        }

        public void UpdateCard(ShopElementData data)
        {
            EnsureElements();
            if (data == null)
                return;

            SetElementData(data);
        }

        public void RequestOpen()
        {
            OpenRequested?.Invoke();
        }

        public void RequestClose()
        {
            CloseRequested?.Invoke();
        }

        public void RequestClickTab()
        {
            SetActiveTab(ShopItemType.Click);
        }

        public void RequestAutoBuyTab()
        {
            SetActiveTab(ShopItemType.AutoBuy);
        }

        public void RequestPaidBuyTab()
        {
            SetActiveTab(ShopItemType.PaidBuy);
        }

        private void SetElementData(ShopElementData data)
        {
            if (data == null)
                return;

            var elements = _shopElements[data.Type];
            if (elements.TryGetValue(data.Id, out var element))
            {
                element.Init(data);
                return;
            }

            if (_reference == null)
                return;

            Transform root = GetRoot(data.Type);
            if (root == null)
                return;

            var instance = Instantiate(_reference, root);
            instance.Init(data);
            instance.gameObject.SetActive(true);
            instance.OnClick += UpdateElement;
            elements.Add(data.Id, instance);
        }

        private void SetActiveTab(ShopItemType type)
        {
            _activeTab = type;
            SetRootState(_clicksCanvasGroup, type == ShopItemType.Click);
            SetRootState(_autoBuysCanvasGroup, type == ShopItemType.AutoBuy);
            SetRootState(_paidBuysCanvasGroup, type == ShopItemType.PaidBuy);
        }

        private Transform GetRoot(ShopItemType type)
        {
            switch (type)
            {
                case ShopItemType.Click:
                    return _clicksRoot != null ? _clicksRoot : _shopRoot;
                case ShopItemType.AutoBuy:
                    return _autoBuysRoot != null ? _autoBuysRoot : _shopRoot;
                case ShopItemType.PaidBuy:
                    return _paidBuysRoot != null ? _paidBuysRoot : _shopRoot;
                default:
                    return _shopRoot;
            }
        }

        private void SetRootState(CanvasGroup canvasGroup, bool isActive)
        {
            if (canvasGroup == null)
                return;

            canvasGroup.DOKill();
            canvasGroup.interactable = isActive;
            canvasGroup.blocksRaycasts = isActive;

            if (_tabFadeDuration <= 0f)
            {
                canvasGroup.alpha = isActive ? 1f : 0f;
                return;
            }

            canvasGroup
                .DOFade(isActive ? 1f : 0f, _tabFadeDuration)
                .SetEase(Ease.OutQuad);
        }

        private void AddTabListeners()
        {
            if (_clicksTabButton != null)
                _clicksTabButton.onClick.AddListener(RequestClickTab);
            if (_autoBuysTabButton != null)
                _autoBuysTabButton.onClick.AddListener(RequestAutoBuyTab);
            if (_paidBuysTabButton != null)
                _paidBuysTabButton.onClick.AddListener(RequestPaidBuyTab);
        }

        private void RemoveTabListeners()
        {
            if (_clicksTabButton != null)
                _clicksTabButton.onClick.RemoveListener(RequestClickTab);
            if (_autoBuysTabButton != null)
                _autoBuysTabButton.onClick.RemoveListener(RequestAutoBuyTab);
            if (_paidBuysTabButton != null)
                _paidBuysTabButton.onClick.RemoveListener(RequestPaidBuyTab);
        }

        private void EnsureElements()
        {
            EnsureElements(ShopItemType.Click);
            EnsureElements(ShopItemType.AutoBuy);
            EnsureElements(ShopItemType.PaidBuy);
        }

        private void EnsureElements(ShopItemType type)
        {
            if (!_shopElements.ContainsKey(type))
                _shopElements.Add(type, new Dictionary<string, ShopViewElement>());
        }
    }
}
