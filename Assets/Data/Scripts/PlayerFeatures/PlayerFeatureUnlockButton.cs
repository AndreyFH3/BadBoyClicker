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

            // Subscribed here (not OnEnable/OnDisable): when this button has no dedicated
            // locked/unlocked art, UpdateState() deactivates this very GameObject while
            // locked. OnDisable would then unsubscribe and the button could never hear
            // about a later unlock, staying hidden forever.
            if (_featureUnlockService != null)
            {
                _featureUnlockService.FeatureUnlocked += OnFeatureUnlocked;
            }
        }

        private void OnEnable()
        {
            Localization.LanguageChanged += UpdateState;
            UpdateState();
        }

        private void Start()
        {
            UpdateState();
        }

        private void OnDisable()
        {
            Localization.LanguageChanged -= UpdateState;
        }

        private void OnDestroy()
        {
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

            // Buttons without dedicated locked/unlocked art (e.g. plain nav buttons) fall
            // back to hiding the whole button while locked, matching "hidden" gates.
            if (_unlockedObject == null && _lockedObject == null)
            {
                gameObject.SetActive(isUnlocked);
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
