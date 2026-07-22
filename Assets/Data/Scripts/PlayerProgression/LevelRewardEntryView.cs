using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlayerProgression
{
    /// <summary>
    /// Single reward row (icon + value + optional description) instantiated inside the
    /// level-up windows. Assign this prefab to PlayerProgressionView so each reward gets
    /// its own object.
    /// </summary>
    [RequireComponent(typeof(LayoutElement))]
    public class LevelRewardEntryView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private TextMeshProUGUI _description;

        [Header("Layout")]
        [Min(1f)]
        [SerializeField] private float _preferredHeight = 100f;

        private LayoutElement _layoutElement;

        private void Awake()
        {
            ApplyLayout();
        }

        private void OnValidate()
        {
            ApplyLayout();
        }

        public void Setup(LevelRewardEntry entry)
        {
            ApplyLayout();

            if (_icon != null)
            {
                _icon.sprite = entry.Icon;
                _icon.enabled = entry.Icon != null;
            }

            if (_text != null)
            {
                _text.text = entry.Text;
            }

            if (_description != null)
            {
                bool hasDescription = !string.IsNullOrEmpty(entry.Description);
                _description.text = hasDescription ? entry.Description : string.Empty;
                _description.gameObject.SetActive(hasDescription);
            }
        }

        private void ApplyLayout()
        {
            if (_layoutElement == null)
            {
                _layoutElement = GetComponent<LayoutElement>();
            }

            if (_layoutElement == null && Application.isPlaying)
            {
                _layoutElement = gameObject.AddComponent<LayoutElement>();
            }

            if (_layoutElement == null)
            {
                return;
            }

            float height = Mathf.Max(1f, _preferredHeight);
            _layoutElement.minHeight = height;
            _layoutElement.preferredHeight = height;
            _layoutElement.flexibleHeight = 0f;
            _layoutElement.flexibleWidth = 1f;
        }
    }
}
