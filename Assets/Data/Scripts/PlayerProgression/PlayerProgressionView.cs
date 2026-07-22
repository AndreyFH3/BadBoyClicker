using System.Collections.Generic;
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
        [Header("Reward entry")]
        [SerializeField] private LevelRewardEntryView _rewardEntryPrefab;
        [Header("Warning")]
        [SerializeField] private Button _newLevelButton;
        [SerializeField] private GameObject _levelUpOfferRoot;
        [SerializeField] private TextMeshProUGUI _levelUpOfferLossText;
        [SerializeField] private Transform _levelUpOfferRewardsContainer;
        [SerializeField] private Button _levelUpConfirmButton;
        [SerializeField] private Button _levelUpCancelButton;
        [Header("LevelUp")]
        [SerializeField] private GameObject _levelUpResultRoot;
        [SerializeField] private TextMeshProUGUI _levelUpResultText;
        [SerializeField] private TextMeshProUGUI _levelUpTransitionText;
        [SerializeField] private Transform _levelUpResultRewardsContainer;
        [SerializeField] private Button _levelUpResultCloseButton;
        [Header("Window animation")]
        [Min(0f)]
        [SerializeField] private float _windowFadeDuration = 0.2f;

        private readonly List<LevelRewardEntryView> _offerEntries = new();
        private readonly List<LevelRewardEntryView> _resultEntries = new();

        private Tween _fillTween;
        private Sequence _addedExperienceSequence;
        private Tween _newLevelButtonTween;
        private Tween _offerFadeTween;
        private Tween _resultFadeTween;
        private Sequence _resultCelebrationSequence;
        private CanvasGroup _levelUpOfferCanvasGroup;
        private CanvasGroup _levelUpResultCanvasGroup;
        private System.Action _confirmLevelUpAction;

        private const float NewLevelButtonAnimationDuration = 0.2f;

        public event System.Action NewLevelRequested;

        private void Awake()
        {
            _levelUpOfferCanvasGroup = GetOrAddCanvasGroup(_levelUpOfferRoot);
            _levelUpResultCanvasGroup = GetOrAddCanvasGroup(_levelUpResultRoot);

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

            HideWindowImmediately(_levelUpOfferRoot, _levelUpOfferCanvasGroup);
            HideWindowImmediately(_levelUpResultRoot, _levelUpResultCanvasGroup);
        }

        private void OnDestroy()
        {
            _fillTween?.Kill();
            _addedExperienceSequence?.Kill();
            _newLevelButtonTween?.Kill();
            _offerFadeTween?.Kill();
            _resultFadeTween?.Kill();
            _resultCelebrationSequence?.Kill();

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
            if (_newLevelButton == null)
            {
                return;
            }

            _newLevelButtonTween?.Kill();

            RectTransform buttonTransform = _newLevelButton.transform as RectTransform;
            _newLevelButton.interactable = isAvailable;

            if (isAvailable)
            {
                _newLevelButton.gameObject.SetActive(true);
                Vector3 startScale = buttonTransform.localScale;
                startScale.x = 0f;
                buttonTransform.localScale = startScale;

                _newLevelButtonTween = buttonTransform
                    .DOScaleX(1f, NewLevelButtonAnimationDuration)
                    .SetEase(Ease.OutBack);
            }
            else
            {
                _newLevelButtonTween = buttonTransform
                    .DOScaleX(0f, NewLevelButtonAnimationDuration)
                    .SetEase(Ease.InBack)
                    .OnComplete(() => _newLevelButton.gameObject.SetActive(false));
            }
        }

        public void ShowLevelUpOffer(System.Action confirmAction, IReadOnlyList<LevelRewardEntry> rewards, string lossText)
        {
            _confirmLevelUpAction = confirmAction;

            PopulateRewards(_levelUpOfferRewardsContainer, _offerEntries, rewards);

            if (_levelUpOfferLossText != null)
            {
                _levelUpOfferLossText.text = lossText;
            }

            if (_levelUpOfferRoot != null)
            {
                ShowWindow(_levelUpOfferRoot, _levelUpOfferCanvasGroup, ref _offerFadeTween);
            }
            else
            {
                confirmAction?.Invoke();
            }
        }

        public void ShowLevelUpResult(int previousLevel, int newLevel, IReadOnlyList<LevelRewardEntry> rewards)
        {
            if (_levelUpResultText != null)
            {
                _levelUpResultText.text = Localization.Tr("player_progression.level_up_result_title");
            }

            if (_levelUpTransitionText != null)
            {
                string from = Localization.Format("player_progression.level", previousLevel);
                string to = Localization.Format("player_progression.level", newLevel);
                _levelUpTransitionText.text = $"{from} -> {to}";
            }

            PopulateRewards(_levelUpResultRewardsContainer, _resultEntries, rewards);

            if (_levelUpResultRoot != null)
            {
                ShowWindow(_levelUpResultRoot, _levelUpResultCanvasGroup, ref _resultFadeTween);
                PlayResultCelebration();
            }
        }

        public void RefreshLocalization()
        {
            SetButtonText(_newLevelButton, "player_progression.new_level_button");
            SetButtonText(_levelUpConfirmButton, "player_progression.new_level_button");
            SetButtonText(_levelUpCancelButton, "common.cancel");
            SetButtonText(_levelUpResultCloseButton, "common.ok");

            if (_levelUpResultRoot != null && _levelUpResultRoot.activeSelf && _levelUpResultText != null)
            {
                _levelUpResultText.text = Localization.Tr("player_progression.level_up_result_title");
            }
        }

        private void RequestNewLevel()
        {
            NewLevelRequested?.Invoke();
        }

        private void ConfirmLevelUp()
        {
            System.Action confirmAction = _confirmLevelUpAction;
            _confirmLevelUpAction = null;
            HideLevelUpOffer(confirmAction);
        }

        private void HideLevelUpOffer()
        {
            _confirmLevelUpAction = null;
            HideLevelUpOffer(null);
        }

        private void HideLevelUpOffer(System.Action onComplete)
        {
            HideWindow(_levelUpOfferRoot, _levelUpOfferCanvasGroup, ref _offerFadeTween, onComplete);
        }

        private void HideLevelUpResult()
        {
            _resultCelebrationSequence?.Kill();
            HideWindow(_levelUpResultRoot, _levelUpResultCanvasGroup, ref _resultFadeTween, null);
        }

        private void ShowWindow(GameObject root, CanvasGroup canvasGroup, ref Tween tween)
        {
            if (root == null)
            {
                return;
            }

            tween?.Kill();
            root.SetActive(true);

            if (canvasGroup == null || _windowFadeDuration <= 0f)
            {
                SetCanvasGroupState(canvasGroup, 1f, true);
                return;
            }

            SetCanvasGroupState(canvasGroup, 0f, false);
            tween = canvasGroup
                .DOFade(1f, _windowFadeDuration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true)
                .OnComplete(() => SetCanvasGroupState(canvasGroup, 1f, true));
        }

        private void HideWindow(
            GameObject root,
            CanvasGroup canvasGroup,
            ref Tween tween,
            System.Action onComplete)
        {
            tween?.Kill();

            if (root == null || !root.activeSelf)
            {
                HideWindowImmediately(root, canvasGroup);
                onComplete?.Invoke();
                return;
            }

            SetCanvasGroupInteractable(canvasGroup, false);
            if (canvasGroup == null || _windowFadeDuration <= 0f)
            {
                HideWindowImmediately(root, canvasGroup);
                onComplete?.Invoke();
                return;
            }

            tween = canvasGroup
                .DOFade(0f, _windowFadeDuration)
                .SetEase(Ease.InQuad)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    HideWindowImmediately(root, canvasGroup);
                    onComplete?.Invoke();
                });
        }

        private static CanvasGroup GetOrAddCanvasGroup(GameObject root)
        {
            if (root == null)
            {
                return null;
            }

            return root.TryGetComponent(out CanvasGroup canvasGroup)
                ? canvasGroup
                : root.AddComponent<CanvasGroup>();
        }

        private static void HideWindowImmediately(GameObject root, CanvasGroup canvasGroup)
        {
            SetCanvasGroupState(canvasGroup, 0f, false);
            root?.SetActive(false);
        }

        private static void SetCanvasGroupState(CanvasGroup canvasGroup, float alpha, bool interactable)
        {
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha = alpha;
            SetCanvasGroupInteractable(canvasGroup, interactable);
        }

        private static void SetCanvasGroupInteractable(CanvasGroup canvasGroup, bool interactable)
        {
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.interactable = interactable;
            canvasGroup.blocksRaycasts = interactable;
        }

        private void PopulateRewards(Transform container, List<LevelRewardEntryView> spawned, IReadOnlyList<LevelRewardEntry> rewards)
        {
            for (int i = 0; i < spawned.Count; i++)
            {
                if (spawned[i] != null)
                {
                    Destroy(spawned[i].gameObject);
                }
            }

            spawned.Clear();

            if (container == null || _rewardEntryPrefab == null || rewards == null)
            {
                return;
            }
            _rewardEntryPrefab.gameObject.SetActive(false);
            foreach (LevelRewardEntry reward in rewards)
            {
                LevelRewardEntryView entry = Instantiate(_rewardEntryPrefab, container);
                entry.gameObject.SetActive(true);
                entry.Setup(reward);
                spawned.Add(entry);
            }
        }

        private void PlayResultCelebration()
        {
            _resultCelebrationSequence?.Kill();

            Transform root = _levelUpResultRoot.transform;
            root.localScale = Vector3.one * 0.7f;

            _resultCelebrationSequence = DOTween.Sequence()
                .Append(root.DOScale(1f, 0.35f).SetEase(Ease.OutBack));

            for (int i = 0; i < _resultEntries.Count; i++)
            {
                LevelRewardEntryView entry = _resultEntries[i];
                if (entry == null)
                {
                    continue;
                }

                Transform entryTransform = entry.transform;
                entryTransform.localScale = Vector3.zero;
                _resultCelebrationSequence.Insert(
                    0.15f + i * 0.08f,
                    entryTransform.DOScale(1f, 0.25f).SetEase(Ease.OutBack));
            }
        }

        private void SetButtonText(Button button, string key)
        {
            if (button == null)
            {
                return;
            }

            TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = Localization.Tr(key);
            }
        }
    }
}
