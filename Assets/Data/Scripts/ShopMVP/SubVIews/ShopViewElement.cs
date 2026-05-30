using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Shop
{
    public class ShopViewElement : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private TextMeshProUGUI _level;
        [SerializeField] private Image _priceIcon;
        [SerializeField] private TextMeshProUGUI _price;
        [SerializeField] private TextMeshProUGUI _bonus;
        [SerializeField] private Button _buyButton;
        public System.Action<string> OnClick;

        private string _id;
        public void Init(ShopElementData data)
        {
            if (_icon != null)
                _icon.sprite = data.Icon;
            if (_name != null)
                _name.text = data.Name;
            if (_level != null)
                _level.text = data.Level;

            if (_priceIcon != null)
                _priceIcon.sprite = data.PriceIcon;
            if (_price != null)
                _price.text = data.Price;
            if (_bonus != null)
                _bonus.text = data.Bonus;
            if (_buyButton != null)
                _buyButton.interactable = data.CanBuy;

            _id = data.Id;
        }

        void Start()
        {
            if (_buyButton != null)
                _buyButton.onClick.AddListener(Click);
        }

        void OnDestroy()
        {
            if (_buyButton != null)
                _buyButton.onClick.RemoveListener(Click);
        }

        private void Click()
        {
            OnClick?.Invoke(_id);
        }
    }
}
