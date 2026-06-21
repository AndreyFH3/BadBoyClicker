using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Customization
{
    [RequireComponent(typeof(Button))]
    public class CustomizationViewElement : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _description;
        [SerializeField] private Image _priceIcon;
        [SerializeField] private TMP_Text _price;
        [SerializeField] private TMP_Text _actionText;
        [SerializeField] private GameObject _selectedMarker;
        [SerializeField] private GameObject _purchasedMarker;
        [SerializeField] private GameObject _lockedMarker;

        private string _id;
        private CustomizationItemType _type;

        public event System.Action<CustomizationItemType, string> Clicked;

        private void Awake()
        {
            if (_button == null)
            {
                _button = GetComponent<Button>();
            }
        }

        public void Init(CustomizationElementViewData data)
        {
            if (data == null)
            {
                return;
            }

            _id = data.Id;
            _type = data.Type;

            SetImage(_icon, data.Sprite);
            SetText(_title, data.Title);
            SetText(_description, data.Description);
            SetImage(_priceIcon, data.PriceIcon);
            SetText(_price, data.IsPurchased ? string.Empty : data.Price);

            if (_button != null)
            {
                _button.interactable = !data.IsSelected && (data.IsPurchased || data.CanBuy);
            }

            SetText(_actionText, GetActionText(data));
            SetActive(_selectedMarker, data.IsSelected);
            SetActive(_purchasedMarker, data.IsPurchased && !data.IsSelected);
            SetActive(_lockedMarker, !data.IsPurchased);
        }

        private void OnEnable()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(RequestClick);
            }
        }

        private void OnDisable()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(RequestClick);
            }
        }

        private string GetActionText(CustomizationElementViewData data)
        {
            if (data.IsSelected)
            {
                return GameLocalization.Localization.Tr("customization.selected");
            }

            if (data.IsPurchased)
            {
                return GameLocalization.Localization.Tr("customization.select");
            }

            return GameLocalization.Localization.Tr("customization.buy");
        }

        private void RequestClick()
        {
            Clicked?.Invoke(_type, _id);
        }

        private void SetImage(Image image, Sprite sprite)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = sprite;
            image.enabled = sprite != null;
        }

        private void SetText(TMP_Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
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
