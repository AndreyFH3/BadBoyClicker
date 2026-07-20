using UnityEngine;
using Zenject;

namespace Customization
{
    /// <summary>
    /// Shows an attention sign (e.g. a badge on a nav button) whenever the
    /// player has a skin they can afford to buy, or already own (bought or
    /// received as a reward) but haven't equipped yet.
    /// </summary>
    public class CustomizationSignShower : MonoBehaviour
    {
        [SerializeField] private GameObject _sign;

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

            bool needShow = _customizationModel.HasAnyActionable();
            _sign.SetActive(needShow);
        }
    }
}
