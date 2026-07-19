using PlayerFeatures;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Customization
{
    public class CustomizationOpenButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private OpenWindow _window;

        private IPlayerFeatureUnlockService _featureUnlockService;

        [Inject]
        public void Construct(IPlayerFeatureUnlockService featureUnlockService)
        {
            _featureUnlockService = featureUnlockService;
        }

        private void Awake()
        {
            if (_button == null)
            {
                _button = GetComponent<Button>();
            }
        }

        private void OnEnable()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(Open);
            }

            if (_featureUnlockService != null)
            {
                _featureUnlockService.FeatureUnlocked += OnFeatureUnlocked;
                UpdateInteractable();
            }
        }

        private void OnDisable()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(Open);
            }

            if (_featureUnlockService != null)
            {
                _featureUnlockService.FeatureUnlocked -= OnFeatureUnlocked;
            }
        }

        public void Open()
        {
            if (_featureUnlockService != null && !_featureUnlockService.IsUnlocked(PlayerFeatureType.Customization))
            {
                return;
            }

            if (_window != null)
            {
                _window.Show();
            }
        }

        private void UpdateInteractable()
        {
            if (_button != null)
            {
                _button.interactable = _featureUnlockService.IsUnlocked(PlayerFeatureType.Customization);
            }
        }

        private void OnFeatureUnlocked(PlayerFeatureType feature)
        {
            if (feature == PlayerFeatureType.Customization)
            {
                UpdateInteractable();
            }
        }
    }
}
