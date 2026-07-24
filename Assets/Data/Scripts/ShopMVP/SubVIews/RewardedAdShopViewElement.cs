using System.Collections.Generic;
using Rewards;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shop
{
    /// <summary>
    /// Dedicated shop card for a repeatable rewarded-ad offer. Its button can be
    /// authored with an ad icon and without a price label independently of paid cards.
    /// </summary>
    public sealed class RewardedAdShopViewElement : ShopViewElementBase
    {
        [Header("Offer")]
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private TextMeshProUGUI _description;

        [Header("What you get")]
        [SerializeField] private Transform _rewardsRoot;
        [SerializeField] private RewardView _rewardReference;

        [Header("Ad button")]
        [SerializeField] private Button _watchAdButton;
        [Tooltip("Optional text on the button. Leave unassigned when the prefab uses only an ad icon.")]
        [SerializeField] private TextMeshProUGUI _watchAdText;
        [Tooltip("Optional ad icon configured directly in the prefab.")]
        [SerializeField] private Image _watchAdIcon;

        private readonly List<RewardView> _rewardViews = new();

        private void Awake()
        {
            if (_rewardReference != null)
                _rewardReference.gameObject.SetActive(false);

            if (_watchAdIcon != null)
                _watchAdIcon.gameObject.SetActive(true);
        }

        private void Start()
        {
            if (_watchAdButton != null)
                _watchAdButton.onClick.AddListener(RaiseClick);
        }

        private void OnDestroy()
        {
            if (_watchAdButton != null)
                _watchAdButton.onClick.RemoveListener(RaiseClick);
        }

        protected override void Apply(ShopElementData data)
        {
            if (_icon != null)
                _icon.sprite = data.Icon;
            if (_name != null)
                _name.text = data.Name;
            if (_description != null)
                _description.text = data.Bonus;
            if (_watchAdText != null)
                _watchAdText.text = data.Price;
            if (_watchAdButton != null)
                _watchAdButton.interactable = data.CanBuy;

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
    }
}
