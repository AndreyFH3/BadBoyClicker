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
        [SerializeField] private Image _icon;
        [SerializeField] private Sprite _lockedIcon;
        [SerializeField] private Sprite _unlockedIcon;
        [SerializeField] private TextMeshProUGUI _lockedText;
        [SerializeField] private string _lockedTextLocalizationKey = "player_features.locked_level";

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

            if (_icon != null)
            {
                Sprite sprite = isUnlocked ? _unlockedIcon : _lockedIcon;
                if (sprite != null)
                {
                    _icon.sprite = sprite;
                }
            }

            if (_lockedText != null)
            {
                _lockedText.gameObject.SetActive(!isUnlocked);
                _lockedText.text = Localization.Format(_lockedTextLocalizationKey, requiredLevel);
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
