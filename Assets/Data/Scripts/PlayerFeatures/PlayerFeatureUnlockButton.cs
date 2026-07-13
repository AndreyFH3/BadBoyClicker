using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using GameLocalization;

namespace PlayerFeatures
{
    public class PlayerFeatureUnlockButton : MonoBehaviour
    {
        [SerializeField] private PlayerFeatureType _feature;
        [SerializeField] private Button _button;
        [SerializeField] private GameObject _unlockedObject;
        [SerializeField] private GameObject _lockedObject;
        [SerializeField] private TextMeshProUGUI _lockedText;

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
            Localization.LanguageChanged += UpdateState;

            if (_featureUnlockService != null)
            {
                _featureUnlockService.FeatureUnlocked += OnFeatureUnlocked;
                UpdateState();
            }
        }

        private void Start()
        {
            UpdateState();
        }

        private void OnDisable()
        {
            Localization.LanguageChanged -= UpdateState;

            if (_featureUnlockService != null)
            {
                _featureUnlockService.FeatureUnlocked -= OnFeatureUnlocked;
            }
        }

        private void UpdateState()
        {
            if (_featureUnlockService == null)
            {
                return;
            }

            bool isUnlocked = _featureUnlockService.IsUnlocked(_feature);
            int requiredLevel = _featureUnlockService.GetRequiredLevel(_feature);

            if (_button != null)
            {
                _button.interactable = isUnlocked;
            }

            if (_unlockedObject != null)
            {
                _unlockedObject.SetActive(isUnlocked);
            }

            if (_lockedObject != null)
            {
                _lockedObject.SetActive(!isUnlocked);
            }

            if (_lockedText != null)
            {
                _lockedText.gameObject.SetActive(!isUnlocked);
                _lockedText.text = Localization.Format("player_progression.level", requiredLevel);
            }
        }

        private void OnFeatureUnlocked(PlayerFeatureType feature)
        {
            if (feature == _feature)
            {
                UpdateState();
            }
        }
    }
}
