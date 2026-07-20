using UnityEngine;
using Zenject;

namespace Customization
{
    /// <summary>
    /// Shows an attention sign (e.g. a badge on a customization tab button)
    /// whenever there is a skin of the given category (Background/Cat) the
    /// player can afford to buy, or already owns (bought or received as a
    /// reward) but hasn't equipped yet.
    /// </summary>
    public class CustomizationTabSignShower : MonoBehaviour
    {
        [SerializeField] private GameObject _sign;
        [SerializeField] private CustomizationItemType _type;

        private ICustomizationModel _customizationModel;

        [Inject]
        public void Construct(ICustomizationModel customizationModel)
        {
            _customizationModel = customizationModel;
        }

        private void OnEnable()
        {
            if (_customizationModel != null)
            {
                _customizationModel.StateChanged += UpdateSign;
                UpdateSign();
            }
        }

        private void OnDisable()
        {
            if (_customizationModel != null)
            {
                _customizationModel.StateChanged -= UpdateSign;
            }
        }

        private void UpdateSign()
        {
            if (_sign == null || _customizationModel == null)
            {
                return;
            }

            bool needShow = _customizationModel.HasAnyActionable(_type);
            _sign.SetActive(needShow);
        }
    }
}
