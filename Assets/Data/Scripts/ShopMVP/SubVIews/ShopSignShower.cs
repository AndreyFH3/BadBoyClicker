using UnityEngine;
using Zenject;

namespace Shop
{
    /// <summary>
    /// Shows an attention sign (e.g. a badge on a nav button) whenever there is
    /// at least one shop item the player can currently afford/buy.
    /// </summary>
    public class ShopSignShower : MonoBehaviour
    {
        [SerializeField] private GameObject _sign;

        private IShopModel _shopModel;

        [Inject]
        public void Construct(IShopModel shopModel)
        {
            _shopModel = shopModel;
        }

        private void OnEnable()
        {
            if (_shopModel != null)
            {
                _shopModel.StateChanged += UpdateSign;
                _shopModel.ItemBought += OnItemBought;
                UpdateSign();
            }
        }

        private void OnDisable()
        {
            if (_shopModel != null)
            {
                _shopModel.StateChanged -= UpdateSign;
                _shopModel.ItemBought -= OnItemBought;
            }
        }

        private void OnItemBought(string id)
        {
            UpdateSign();
        }

        private void UpdateSign()
        {
            if (_sign == null || _shopModel == null)
            {
                return;
            }

            bool needShow = _shopModel.HasAnyBuyable();
            _sign.SetActive(needShow);
        }
    }
}
