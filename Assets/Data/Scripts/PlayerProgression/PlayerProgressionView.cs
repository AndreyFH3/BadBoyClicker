using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using GameLocalization;

namespace PlayerProgression
{
    public class PlayerProgressionView : MonoBehaviour, IPlayerProgressionView
    {
        [SerializeField] private Image _experienceFill;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _experienceText;
        [SerializeField] private TextMeshProUGUI _addedExperienceText;
        [SerializeField] private RectTransform _addedExperienceStartPoint;
        [Header("Warning")]
        [SerializeField] private Button _newLevelButton;
        [SerializeField] private GameObject _levelUpOfferRoot;
        [SerializeField] private TextMeshProUGUI _levelUpOfferText;
        [SerializeField] private Image _levelUpOfferIcon;
        [SerializeField] private Button _levelUpConfirmButton;
        [SerializeField] private Button _levelUpCancelButton;
        [Header("LevelUp")] 
        [SerializeField] private GameObject _levelUpResultRoot;
        [SerializeField] private TextMeshProUGUI _levelUpResultText;
        [SerializeField] private Image _levelUpResultIcon;
  
        [SerializeField] private Button _levelUpResultCloseButton;

        private Tween _fillTween;
        private Sequence _addedExperienceSequence;
        private System.Action _confirmLevelUpAction;

        public event System.Action NewLevelRequested;

        private void Awake()
        {
            if (_newLevelButton != null)
            {
                _newLevelButton.onClick.AddListener(RequestNewLevel);
                SetButtonText(_newLevelButton, "player_progression.new_level_button");
            }

            if (_levelUpConfirmButton != null)
            {
                _levelUpConfirmButton.onClick.AddListener(ConfirmLevelUp);
                SetButtonText(_levelUpConfirmButton, "player_progression.new_level_button");
            }

            if (_levelUpCancelButton != null)
            {
                _levelUpCancelButton.onClick.AddListener(HideLevelUpOffer);
                SetButtonText(_levelUpCancelButton, "common.cancel");
            }

            if (_levelUpResultCloseButton != null)
            {
                _levelUpResultCloseButton.onClick.AddListener(HideLevelUpResult);
                SetButtonText(_levelUpResultCloseButton, "common.ok");
            }

            HideLevelUpOffer();
            HideLevelUpResult();
        }

        private void OnDestroy()
        {
            _fillTween?.Kill();
            _addedExperienceSequence?.Kill();

            if (_newLevelButton != null)
            {
                _newLevelButton.onClick.RemoveListener(RequestNewLevel);
            }

            if (_levelUpConfirmButton != null)
            {
                _levelUpConfirmButton.onClick.RemoveListener(ConfirmLevelUp);
            }

            if (_levelUpCancelButton != null)
            {
                _levelUpCancelButton.onClick.RemoveListener(HideLevelUpOffer);
            }

            if (_levelUpResultCloseButton != null)
            {
                _levelUpResultCloseButton.onClick.RemoveListener(HideLevelUpResult);
            }
        }

        public void UpdateState(int level, long experience, long experienceToNextLevel, float progress)
        {
            if (_levelText != null)
            {
                _levelText.text = Localization.Format("player_progression.level", level);
            }

            if (_experienceText != null)
            {
                _experienceText.text = $"{experience.ConvertFromLongToString()} / {experienceToNextLevel.ConvertFromLongToString()}";
            }

            if (_experienceFill != null)
            {
                _fillTween?.Kill();
                _fillTween = _experienceFill
                    .DOFillAmount(Mathf.Clamp01(progress), 0.2f)
                    .SetEase(Ease.OutQuad);
            }
        }

        public void ShowAddedExperience(long amount)
        {
            if (_addedExperienceText == null || amount <= 0)
            {
                return;
            }

            _addedExperienceSequence?.Kill();
            _addedExperienceText.gameObject.SetActive(true);
            _addedExperienceText.text = $"+{amount.ConvertFromLongToString()}";
            _addedExperienceText.color = new Color(
                _addedExperienceText.color.r,
                _addedExperienceText.color.g,
                _addedExperienceText.color.b,
                1f);

            if (_addedExperienceStartPoint != null)
            {
                _addedExperienceText.transform.position = _addedExperienceStartPoint.position;
            }

            _addedExperienceSequence = DOTween.Sequence()
                .Append(_addedExperienceText.transform.DOLocalMoveY(_addedExperienceText.transform.localPosition.y + 24f, 0.35f))
                .Join(_addedExperienceText.DOFade(0f, 0.35f))
                .OnComplete(() => _addedExperienceText.gameObject.SetActive(false));
        }

        public void SetNewLevelAvailable(bool isAvailable)
        {
            if (_newLevelButton != null)
            {
                _newLevelButton.gameObject.SetActive(isAvailable);
                _newLevelButton.interactable = isAvailable;
            }
        }

        public void ShowLevelUpOffer(System.Action confirmAction, string rewardDescription, Sprite rewardIcon)
        {
            _confirmLevelUpAction = confirmAction;

            if (_levelUpOfferText != null)
            {
                _levelUpOfferText.text = Localization.Format(
                    "player_progression.level_up_offer",
                    rewardDescription);
            }

            SetIcon(_levelUpOfferIcon, rewardIcon);

            if (_levelUpOfferRoot != null)
            {
                _levelUpOfferRoot.SetActive(true);
            }
            else
            {
                confirmAction?.Invoke();
            }
        }

        public void ShowLevelUpResult(string rewardDescription, Sprite rewardIcon)
        {
            if (_levelUpResultText != null)
            {
                _levelUpResultText.text = Localization.Format(
                    "player_progression.level_up_result",
                    rewardDescription);
            }

            SetIcon(_levelUpResultIcon, rewardIcon);

            if (_levelUpResultRoot != null)
            {
                _levelUpResultRoot.SetActive(true);
            }
        }

        private void RequestNewLevel()
        {
            NewLevelRequested?.Invoke();
        }

        private void ConfirmLevelUp()
        {
            HideLevelUpOffer();
            _confirmLevelUpAction?.Invoke();
            _confirmLevelUpAction = null;
        }

        private void HideLevelUpOffer()
        {
            if (_levelUpOfferRoot != null)
            {
                _levelUpOfferRoot.SetActive(false);
            }
        }

        private void HideLevelUpResult()
        {
            if (_levelUpResultRoot != null)
            {
                _levelUpResultRoot.SetActive(false);
            }
        }

        private void SetIcon(Image icon, Sprite sprite)
        {
            if (icon == null)
            {
                return;
            }

            icon.sprite = sprite;
            icon.enabled = sprite != null;
        }

        private void SetButtonText(Button button, string key)
        {
            TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = Localization.Tr(key);
            }
        }
    }
}
