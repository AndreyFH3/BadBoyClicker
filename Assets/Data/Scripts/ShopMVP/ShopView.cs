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
        [Tooltip("Optional. When assigned, offers that reward crystals (hard) go here instead of the shared paid root.")]
        [SerializeField] private Transform _paidHardRoot;
        [Tooltip("Optional. When assigned, offers that reward decor currency go here instead of the shared paid root.")]
        [SerializeField] private Transform _paidDecorRoot;
        [Tooltip("Optional. When assigned, offers that reward soft currency go here instead of the shared paid root.")]
        [SerializeField] private Transform _paidSoftRoot;
        [Tooltip("Optional. When assigned, offers that reward a chest go here instead of the shared paid root.")]
        [SerializeField] private Transform _paidChestRoot;
        [Tooltip("Optional. Section header shown above the chest offers; hidden automatically while no chest offers are available (e.g. Chests feature still locked).")]
        [SerializeField] private GameObject _paidChestHeader;
        [Tooltip("Optional. When assigned, offers that reward experience go here instead of the shared paid root.")]
        [SerializeField] private Transform _paidExperienceRoot;
        [Tooltip("Optional. Section header shown above the experience offers; hidden automatically while no experience offers are available.")]
        [SerializeField] private GameObject _paidExperienceHeader;
        [SerializeField] private CanvasGroup _clicksCanvasGroup;
        [SerializeField] private CanvasGroup _autoBuysCanvasGroup;
        [SerializeField] private CanvasGroup _paidBuysCanvasGroup;
        [SerializeField] private Button _clicksTabButton;
        [SerializeField] private Button _autoBuysTabButton;
        [SerializeField] private Button _paidBuysTabButton;
        [SerializeField] private float _tabFadeDuration = 0.2f;
        [Tooltip("Prefab used for soft-currency upgrades (clicks / auto-buys).")]
        [SerializeField] private ShopViewElement _reference;
        [Tooltip("Prefab used for paid offers. Falls back to the standard reference when not assigned.")]
        [SerializeField] private PaidShopViewElement _paidReference;
        [Tooltip("Prefab used only for rewarded-ad offers. Falls back to the paid-offer prefab when not assigned.")]
        [SerializeField] private RewardedAdShopViewElement _rewardedAdReference;
        [SerializeField] private TextMeshProUGUI _earnPerSecond;
        [Header("Scrolling")]
        [Tooltip("Scroll views that return to the bottom after completing a player level.")]
        [SerializeField] private ScrollRect[] _scrollRects;

        private readonly Dictionary<ShopItemType, Dictionary<string, ShopViewElementBase>> _shopElements = new();
        private ShopItemType _activeTab = ShopItemType.Click;

        public event Action<string> OnBuy;

        public void ScrollToTop()
        {
            SetVerticalScrollPosition(1f);
        }

        public void ScrollToBottom()
        {
            SetVerticalScrollPosition(0f);
        }

        private void Awake()
        {
            EnsureElements();
            if (_reference != null)
                _reference.gameObject.SetActive(false);
            if (_paidReference != null)
                _paidReference.gameObject.SetActive(false);
            if (_rewardedAdReference != null)
                _rewardedAdReference.gameObject.SetActive(false);
            AddTabListeners();
            SetActiveTab(_activeTab);
        }

        private void OnDestroy()
        {
            RemoveTabListeners();
        }

        public void SetData(List<ShopElementData> datas)
        {
            EnsureElements();
            if (datas == null || (_reference == null && _paidReference == null))
            {
                return;
            }

            foreach (var data in datas)
            {
                SetElementData(data);
            }

            UpdateGroupSectionVisibility(datas, ShopRewardGroup.Chest, _paidChestHeader, _paidChestRoot);
            UpdateGroupSectionVisibility(datas, ShopRewardGroup.Experience, _paidExperienceHeader, _paidExperienceRoot);
        }

        // Some paid-offer groups (chests, experience) only ever hold a handful of
        // offers and can end up empty (e.g. every offer in the group is currently
        // feature-locked), so their header/root are hidden rather than left dangling.
        private void UpdateGroupSectionVisibility(List<ShopElementData> datas, ShopRewardGroup group, GameObject header, Transform root)
        {
            if (header == null && root == null)
            {
                return;
            }

            bool hasOffers = false;
            foreach (var data in datas)
            {
                if (data != null && data.Type == ShopItemType.PaidBuy && data.RewardGroup == group)
                {
                    hasOffers = true;
                    break;
                }
            }

            if (header != null)
            {
                header.SetActive(hasOffers);
            }

            if (root != null)
            {
                root.gameObject.SetActive(hasOffers);
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

            ShopViewElementBase reference = GetReference(data);
            if (reference == null)
                return;

            Transform root = GetRoot(data);
            if (root == null)
                return;

            var instance = Instantiate(reference, root);
            instance.Init(data);
            instance.gameObject.SetActive(true);
            instance.OnClick += UpdateElement;
            elements.Add(data.Id, instance);
        }

        private ShopViewElementBase GetReference(ShopElementData data)
        {
            if (data.IsRewardedAd && _rewardedAdReference != null)
                return _rewardedAdReference;

            if (data.Type == ShopItemType.PaidBuy && _paidReference != null)
                return _paidReference;

            return _reference;
        }

        private void SetActiveTab(ShopItemType type)
        {
            _activeTab = type;
            SetRootState(_clicksCanvasGroup, type == ShopItemType.Click);
            SetRootState(_autoBuysCanvasGroup, type == ShopItemType.AutoBuy);
            SetRootState(_paidBuysCanvasGroup, type == ShopItemType.PaidBuy);
        }

        private Transform GetRoot(ShopElementData data)
        {
            switch (data.Type)
            {
                case ShopItemType.Click:
                    return _clicksRoot != null ? _clicksRoot : _shopRoot;
                case ShopItemType.AutoBuy:
                    return _autoBuysRoot != null ? _autoBuysRoot : _shopRoot;
                case ShopItemType.PaidBuy:
                    return GetPaidRoot(data);
                default:
                    return _shopRoot;
            }
        }

        private Transform GetPaidRoot(ShopElementData data)
        {
            Transform groupRoot = GetRewardGroupRoot(data.RewardGroup);
            if (groupRoot != null)
                return groupRoot;

            return _paidBuysRoot != null ? _paidBuysRoot : _shopRoot;
        }

        private Transform GetRewardGroupRoot(ShopRewardGroup group)
        {
            switch (group)
            {
                case ShopRewardGroup.Hard:
                    return _paidHardRoot;
                case ShopRewardGroup.Decor:
                    return _paidDecorRoot;
                case ShopRewardGroup.Soft:
                    return _paidSoftRoot;
                case ShopRewardGroup.Chest:
                    return _paidChestRoot;
                case ShopRewardGroup.Experience:
                    return _paidExperienceRoot;
                default:
                    return null;
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
                _shopElements.Add(type, new Dictionary<string, ShopViewElementBase>());
        }

        private void SetVerticalScrollPosition(float normalizedPosition)
        {
            if (_scrollRects == null)
                return;

            foreach (ScrollRect scrollRect in _scrollRects)
            {
                if (scrollRect == null)
                    continue;

                scrollRect.StopMovement();
                scrollRect.verticalNormalizedPosition = normalizedPosition;
            }
        }
    }
}
