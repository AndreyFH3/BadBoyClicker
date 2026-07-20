using UnityEngine;
using Zenject;

namespace Shop
{
    /// <summary>
    /// Shows an attention sign (e.g. a badge on a shop tab button) whenever
    /// there is at least one item of the given tab's type the player can
    /// currently afford/buy.
    /// </summary>
    public class ShopTabSignShower : MonoBehaviour
    {
        [SerializeField] private GameObject _sign;
        [SerializeField] private ShopItemType _type;

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

            bool needShow = _shopModel.HasAnyBuyable(_type);
            _sign.SetActive(needShow);
        }
    }
}
