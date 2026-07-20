using System.Linq;
using CardCollections;
using UnityEngine;
using Zenject;

namespace CardCollectionMVP
{
    /// <summary>
    /// Shows an attention sign (e.g. a badge on a nav button) whenever the
    /// player has a completed card collection whose reward hasn't been claimed yet.
    /// </summary>
    public class CardCollectionSignShower : MonoBehaviour
    {
        [SerializeField] private GameObject _sign;

        private ICardCollectionService _cardCollectionService;

        [Inject]
        public void Construct(ICardCollectionService cardCollectionService)
        {
            _cardCollectionService = cardCollectionService;
        }

        private void OnEnable()
        {
            if (_cardCollectionService != null)
            {
                _cardCollectionService.Changed += UpdateSign;
                _cardCollectionService.CollectionChanged += OnCollectionChanged;
                _cardCollectionService.CollectionRewardClaimed += OnCollectionChanged;
                UpdateSign();
            }
        }

        private void OnDisable()
        {
            if (_cardCollectionService != null)
            {
                _cardCollectionService.Changed -= UpdateSign;
                _cardCollectionService.CollectionChanged -= OnCollectionChanged;
                _cardCollectionService.CollectionRewardClaimed -= OnCollectionChanged;
            }
        }

        private void OnCollectionChanged(CardCollectionViewData data)
        {
            UpdateSign();
        }

        private void UpdateSign()
        {
            if (_sign == null || _cardCollectionService == null)
            {
                return;
            }

            bool needShow = _cardCollectionService.GetAllViewData().Any(collection => collection.RewardAvailable);
            _sign.SetActive(needShow);
        }
    }
}
