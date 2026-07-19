using System;
using System.Collections.Generic;
using System.Linq;
using CardCollections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace CardCollectionMVP
{
    public class CardCollectionItemView : MonoBehaviour
    {
        private const string CardsProgressFormat = "{0} из {1} карточек";

        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private TextMeshProUGUI _cardsProgressText;
        [SerializeField] private Image _progressFill;
        [SerializeField] private Button _selectButton;
        [SerializeField] private Button _collectButton;

        [Header("Toggled when the reward is claimed")]
        [SerializeField] private List<GameObject> _rewardClaimedEnableObjects = new();
        [SerializeField] private List<GameObject> _rewardClaimedDisableObjects = new();

        [Header("Cards")]
        [SerializeField] private Transform _cardsRoot;
        [SerializeField] private CardStateView _cardReference;

        private readonly List<CardStateView> _cardViews = new();
        private string _id;
        private CardCollectionViewData _data;
        private ICardCollectionRewardWindow _rewardWindow;

        public event Action<string> Selected;
        public event Action<string> CollectRequested;

        public CardCollectionViewData Data => _data;

        [Inject]
        private void Construct(ICardCollectionRewardWindow rewardWindow)
        {
            _rewardWindow = rewardWindow;
        }

        private void Awake()
        {
            if (_selectButton != null)
            {
                _selectButton.onClick.AddListener(RequestSelect);
            }

            if (_collectButton != null)
            {
                _collectButton.onClick.AddListener(RequestCollect);
            }


            if (_cardReference != null)
            {
                _cardReference.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (_selectButton != null)
            {
                _selectButton.onClick.RemoveListener(RequestSelect);
            }

            if (_collectButton != null)
            {
                _collectButton.onClick.RemoveListener(RequestCollect);
            }
        }

        public void SetData(CardCollectionViewData data)
        {
            if (data == null)
            {
                return;
            }

            _data = data;
            _id = data.Id;

            if (_title != null)
            {
                _title.text = data.Title;
            }

            if (_cardsProgressText != null)
            {
                _cardsProgressText.text = string.Format(CardsProgressFormat, data.CollectedCards, data.TotalCards);
            }

            if (_progressFill != null)
            {
                _progressFill.fillAmount = data.TotalCards > 0
                    ? Mathf.Clamp01((float)data.CollectedCards / data.TotalCards)
                    : 0f;
            }

            if (_collectButton != null)
            {
                _collectButton.gameObject.SetActive(data.RewardAvailable && data.TotalCards >= 6);
            }

            SetRewardClaimedObjects(data.IsRewardClaimed);
            SetCards(data.Cards);
        }

        private void SetCards(IReadOnlyList<CardViewData> cards)
        {
            ClearCards();

            if (cards == null || _cardReference == null)
            {
                return;
            }

            Transform root = _cardsRoot != null ? _cardsRoot : transform;
            _cardReference.gameObject.SetActive(false);

            // Collected cards first, uncollected last; original config order preserved within each group.
            foreach (var card in cards.OrderByDescending(card => card.IsCollected))
            {
                if (card == null)
                {
                    continue;
                }

                CardStateView view = Instantiate(_cardReference, root);
                view.SetData(card);
                view.gameObject.SetActive(true);
                _cardViews.Add(view);
            }
        }

        private void ClearCards()
        {
            foreach (var view in _cardViews)
            {
                if (view != null)
                {
                    Destroy(view.gameObject);
                }
            }

            _cardViews.Clear();
        }

        private void SetRewardClaimedObjects(bool isRewardClaimed)
        {
            foreach (var go in _rewardClaimedEnableObjects)
            {
                if (go != null)
                {
                    go.SetActive(isRewardClaimed);
                }
            }

            foreach (var go in _rewardClaimedDisableObjects)
            {
                if (go != null)
                {
                    go.SetActive(!isRewardClaimed);
                }
            }
        }

        private void RequestSelect()
        {
            Selected?.Invoke(_id);
        }

        private void RequestCollect()
        {
            CollectRequested?.Invoke(_id);
        }
    }
}
