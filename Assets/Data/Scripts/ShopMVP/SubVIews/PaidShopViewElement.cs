using System.Collections.Generic;
using Rewards;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shop
{
    /// <summary>
    /// Dedicated card for paid offers. Unlike the plain upgrade card it makes the
    /// purchase unambiguous: the bundle contents (reward icons + amounts), the offer
    /// name, the price with its currency badge, and a clear real-money vs in-game
    /// indicator so the two payment types never blend together.
    /// </summary>
    public class PaidShopViewElement : ShopViewElementBase
    {
        [Header("Offer")]
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private TextMeshProUGUI _description;

        [Header("What you get")]
        [SerializeField] private Transform _rewardsRoot;
        [SerializeField] private RewardView _rewardReference;

        [Header("Price")]
        [SerializeField] private Image _priceIcon;
        [SerializeField] private TextMeshProUGUI _price;
        [SerializeField] private Button _buyButton;

        [Header("Payment type indicator (optional)")]
        [Tooltip("Enabled when the offer is bought with real money.")]
        [SerializeField] private GameObject _realMoneyBadge;
        [Tooltip("Enabled when the offer is bought with in-game currency.")]
        [SerializeField] private GameObject _inGameBadge;

        private readonly List<RewardView> _rewardViews = new();

        protected override void Apply(ShopElementData data)
        {
            if (_icon != null)
                _icon.sprite = data.Icon;
            if (_name != null)
                _name.text = data.Name;
            if (_description != null)
                _description.text = data.Bonus;
            if (_priceIcon != null)
            {
                // Real-money offers (YAN) show only the price text + "YAN"; there is
                // no currency icon for them. In-game purchases keep their resource icon.
                _priceIcon.gameObject.SetActive(!data.IsRealMoney);
                _priceIcon.sprite = data.PriceIcon;
            }
            if (_price != null)
                _price.text = data.Price;
            if (_buyButton != null)
                _buyButton.interactable = data.CanBuy;

            if (_realMoneyBadge != null)
                _realMoneyBadge.SetActive(data.IsRealMoney);
            if (_inGameBadge != null)
                _inGameBadge.SetActive(!data.IsRealMoney);

            RebuildRewards(data.Rewards);
        }

        private void RebuildRewards(IReadOnlyList<RewardDisplay> rewards)
        {
            if (_rewardsRoot == null || _rewardReference == null)
                return;

            int count = rewards?.Count ?? 0;

            for (int i = 0; i < _rewardViews.Count; i++)
                _rewardViews[i].gameObject.SetActive(i < count);

            for (int i = 0; i < count; i++)
            {
                RewardView view;
                if (i < _rewardViews.Count)
                {
                    view = _rewardViews[i];
                }
                else
                {
                    view = Instantiate(_rewardReference, _rewardsRoot);
                    _rewardViews.Add(view);
                }

                view.gameObject.SetActive(true);
                view.Set(rewards[i]);
            }
        }

        private void Awake()
        {
            if (_rewardReference != null)
                _rewardReference.gameObject.SetActive(false);
        }

        private void Start()
        {
            if (_buyButton != null)
                _buyButton.onClick.AddListener(RaiseClick);
        }

        private void OnDestroy()
        {
            if (_buyButton != null)
                _buyButton.onClick.RemoveListener(RaiseClick);
        }
    }
}
