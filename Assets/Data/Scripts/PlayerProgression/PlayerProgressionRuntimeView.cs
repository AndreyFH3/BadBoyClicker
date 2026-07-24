using System;
using System.Collections.Generic;
using System.Text;
using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Zenject;
using GameLocalization;

namespace PlayerProgression
{
    public class PlayerProgressionRuntimeView : IPlayerProgressionView, IInitializable, IDisposable
    {
        private GameObject _root;
        private Image _fill;
        private TextMeshProUGUI _levelText;
        private TextMeshProUGUI _experienceText;
        private TextMeshProUGUI _addedExperienceText;
        private Button _newLevelButton;
        private GameObject _popupRoot;
        private TextMeshProUGUI _popupText;
        private Image _popupIcon;
        private Button _popupConfirmButton;
        private Button _popupCancelButton;
        private Action _confirmAction;
        private bool _isShowingLevelUpResult;

        public event Action NewLevelRequested;
        public event Action LevelUpResultClosed;

        public void Initialize()
        {
            EnsureCreated();
        }

        public void Dispose()
        {
            if (_root != null)
            {
                UnityEngine.Object.Destroy(_root);
            }
        }

        public void UpdateState(int level, long experience, long experienceToNextLevel, float progress)
        {
            EnsureCreated();

            _levelText.text = Localization.Format("player_progression.level", level);
            _experienceText.text = $"{experience.ConvertFromLongToString()} / {experienceToNextLevel.ConvertFromLongToString()}";
            _fill.fillAmount = Mathf.Clamp01(progress);
        }

        public void SetNewLevelAvailable(bool isAvailable)
        {
            EnsureCreated();
            _newLevelButton.gameObject.SetActive(isAvailable);
            _newLevelButton.interactable = isAvailable;
        }

        public void ShowAddedExperience(long amount)
        {
            EnsureCreated();

            if (amount <= 0)
            {
                return;
            }

            _addedExperienceText.text = $"+{amount.ConvertFromLongToString()}";
            _addedExperienceText.gameObject.SetActive(true);
            _addedExperienceText.canvasRenderer.SetAlpha(1f);
            _addedExperienceText.CrossFadeAlpha(0f, 0.45f, false);
        }

        public void ShowLevelUpOffer(Action confirmAction, IReadOnlyList<LevelRewardEntry> rewards, string lossText, bool showLossWarning)
        {
            EnsureCreated();
            _isShowingLevelUpResult = false;
            _confirmAction = confirmAction;
            string rewardsText = BuildRewardsText(rewards);
            string content = showLossWarning
                ? $"{lossText}\n\n{rewardsText}"
                : rewardsText;
            _popupText.text = Localization.Format("player_progression.level_up_offer_runtime", content);
            SetPopupIcon(null);
            _popupConfirmButton.gameObject.SetActive(true);
            _popupCancelButton.GetComponentInChildren<TextMeshProUGUI>().text = Localization.Tr("common.cancel");
            _popupRoot.SetActive(true);
        }

        public void ShowLevelUpResult(int previousLevel, int newLevel, IReadOnlyList<LevelRewardEntry> rewards)
        {
            EnsureCreated();
            _isShowingLevelUpResult = true;
            _confirmAction = null;
            string from = Localization.Format("player_progression.level", previousLevel);
            string to = Localization.Format("player_progression.level", newLevel);
            _popupText.text = Localization.Format(
                "player_progression.level_up_result_runtime",
                $"{from} → {to}\n\n{BuildRewardsText(rewards)}");
            SetPopupIcon(null);
            _popupConfirmButton.gameObject.SetActive(false);
            _popupCancelButton.GetComponentInChildren<TextMeshProUGUI>().text = Localization.Tr("common.ok");
            _popupRoot.SetActive(true);
        }

        public void RefreshLocalization()
        {
            EnsureCreated();
            _newLevelButton.GetComponentInChildren<TextMeshProUGUI>().text = Localization.Tr("player_progression.new_level_button");
            _popupConfirmButton.GetComponentInChildren<TextMeshProUGUI>().text = Localization.Tr("player_progression.new_level_button");
        }

        private static string BuildRewardsText(IReadOnlyList<LevelRewardEntry> rewards)
        {
            if (rewards == null)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            foreach (LevelRewardEntry reward in rewards)
            {
                if (builder.Length > 0)
                {
                    builder.Append('\n');
                }

                builder.Append(reward.Text);
                if (!string.IsNullOrEmpty(reward.Description))
                {
                    builder.Append(" — ");
                    builder.Append(reward.Description);
                }
            }

            return builder.ToString();
        }

        private void EnsureCreated()
        {
            if (_root != null)
            {
                return;
            }

            Canvas canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("Player progression view could not find a Canvas.");
                return;
            }

            _root = new GameObject("PlayerProgressionRuntimeView", typeof(RectTransform));
            _root.transform.SetParent(canvas.transform, false);

            RectTransform rootTransform = _root.GetComponent<RectTransform>();
            rootTransform.anchorMin = new Vector2(0.5f, 1f);
            rootTransform.anchorMax = new Vector2(0.5f, 1f);
            rootTransform.pivot = new Vector2(0.5f, 1f);
            rootTransform.anchoredPosition = new Vector2(0f, -24f);
            rootTransform.sizeDelta = new Vector2(420f, 54f);

            Image background = _root.AddComponent<Image>();
            background.color = new Color(0.05f, 0.06f, 0.08f, 0.86f);

            _fill = CreateImage("Fill", _root.transform, new Color(0.23f, 0.72f, 1f, 0.95f));
            _fill.type = Image.Type.Filled;
            _fill.fillMethod = Image.FillMethod.Horizontal;
            _fill.fillAmount = 0f;
            RectTransform fillTransform = _fill.rectTransform;
            fillTransform.anchorMin = Vector2.zero;
            fillTransform.anchorMax = Vector2.one;
            fillTransform.offsetMin = new Vector2(4f, 4f);
            fillTransform.offsetMax = new Vector2(-4f, -4f);

            _levelText = CreateText("Level", _root.transform, 24, TextAlignmentOptions.Left);
            RectTransform levelTransform = _levelText.rectTransform;
            levelTransform.anchorMin = new Vector2(0f, 0f);
            levelTransform.anchorMax = new Vector2(0f, 1f);
            levelTransform.pivot = new Vector2(0f, 0.5f);
            levelTransform.anchoredPosition = new Vector2(14f, 0f);
            levelTransform.sizeDelta = new Vector2(120f, 0f);

            _experienceText = CreateText("Experience", _root.transform, 20, TextAlignmentOptions.Right);
            RectTransform experienceTransform = _experienceText.rectTransform;
            experienceTransform.anchorMin = new Vector2(1f, 0f);
            experienceTransform.anchorMax = new Vector2(1f, 1f);
            experienceTransform.pivot = new Vector2(1f, 0.5f);
            experienceTransform.anchoredPosition = new Vector2(-14f, 0f);
            experienceTransform.sizeDelta = new Vector2(220f, 0f);

            _addedExperienceText = CreateText("AddedExperience", _root.transform, 22, TextAlignmentOptions.Center);
            RectTransform addedTransform = _addedExperienceText.rectTransform;
            addedTransform.anchorMin = new Vector2(0.5f, 0.5f);
            addedTransform.anchorMax = new Vector2(0.5f, 0.5f);
            addedTransform.pivot = new Vector2(0.5f, 0.5f);
            addedTransform.anchoredPosition = new Vector2(0f, 0f);
            addedTransform.sizeDelta = new Vector2(140f, 40f);
            _addedExperienceText.gameObject.SetActive(false);

            _newLevelButton = CreateButton("NewLevelButton", _root.transform, Localization.Tr("player_progression.new_level_button"), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(180f, 34f));
            _newLevelButton.onClick.AddListener(() => NewLevelRequested?.Invoke());
            _newLevelButton.gameObject.SetActive(false);

            CreatePopup(canvas.transform);
        }

        private Image CreateImage(string name, Transform parent, Color color)
        {
            var gameObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            gameObject.transform.SetParent(parent, false);
            Image image = gameObject.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private TextMeshProUGUI CreateText(string name, Transform parent, int size, TextAlignmentOptions alignment)
        {
            var gameObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            gameObject.transform.SetParent(parent, false);
            TextMeshProUGUI text = gameObject.GetComponent<TextMeshProUGUI>();
            text.fontSize = size;
            text.fontStyle = FontStyles.Bold;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private Button CreateButton(string name, Transform parent, string textValue, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size)
        {
            var gameObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            gameObject.transform.SetParent(parent, false);

            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = pivot;
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = size;

            gameObject.GetComponent<Image>().color = new Color(0.12f, 0.49f, 0.9f, 1f);
            Button button = gameObject.GetComponent<Button>();

            TextMeshProUGUI buttonText = CreateText("Text", gameObject.transform, 18, TextAlignmentOptions.Center);
            buttonText.text = textValue;
            buttonText.rectTransform.anchorMin = Vector2.zero;
            buttonText.rectTransform.anchorMax = Vector2.one;
            buttonText.rectTransform.offsetMin = Vector2.zero;
            buttonText.rectTransform.offsetMax = Vector2.zero;

            return button;
        }

        private void CreatePopup(Transform canvasTransform)
        {
            _popupRoot = new GameObject("PlayerProgressionPopup", typeof(RectTransform), typeof(Image));
            _popupRoot.transform.SetParent(canvasTransform, false);

            RectTransform rootTransform = _popupRoot.GetComponent<RectTransform>();
            rootTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rootTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rootTransform.pivot = new Vector2(0.5f, 0.5f);
            rootTransform.anchoredPosition = Vector2.zero;
            rootTransform.sizeDelta = new Vector2(420f, 260f);
            _popupRoot.GetComponent<Image>().color = new Color(0.05f, 0.06f, 0.08f, 0.96f);

            _popupIcon = CreateImage("Icon", _popupRoot.transform, Color.white);
            RectTransform iconTransform = _popupIcon.rectTransform;
            iconTransform.anchorMin = new Vector2(0.5f, 1f);
            iconTransform.anchorMax = new Vector2(0.5f, 1f);
            iconTransform.pivot = new Vector2(0.5f, 1f);
            iconTransform.anchoredPosition = new Vector2(0f, -18f);
            iconTransform.sizeDelta = new Vector2(48f, 48f);

            _popupText = CreateText("Text", _popupRoot.transform, 20, TextAlignmentOptions.Center);
            RectTransform textTransform = _popupText.rectTransform;
            textTransform.anchorMin = new Vector2(0f, 0f);
            textTransform.anchorMax = new Vector2(1f, 1f);
            textTransform.offsetMin = new Vector2(24f, 68f);
            textTransform.offsetMax = new Vector2(-24f, -72f);

            _popupConfirmButton = CreateButton("Confirm", _popupRoot.transform, Localization.Tr("player_progression.new_level_button"), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(28f, 22f), new Vector2(180f, 42f));
            _popupConfirmButton.onClick.AddListener(ConfirmPopup);

            _popupCancelButton = CreateButton("Cancel", _popupRoot.transform, Localization.Tr("common.cancel"), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-28f, 22f), new Vector2(140f, 42f));
            _popupCancelButton.onClick.AddListener(HidePopup);

            _popupRoot.SetActive(false);
        }

        private void ConfirmPopup()
        {
            HidePopup();
            _confirmAction?.Invoke();
            _confirmAction = null;
        }

        private void HidePopup()
        {
            _popupRoot.SetActive(false);

            if (_isShowingLevelUpResult)
            {
                _isShowingLevelUpResult = false;
                LevelUpResultClosed?.Invoke();
            }
        }

        private void SetPopupIcon(Sprite sprite)
        {
            _popupIcon.sprite = sprite;
            _popupIcon.enabled = sprite != null;
        }
    }
}
