using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlayerProgression
{
    public class LevelRewardEntryView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private TextMeshProUGUI _description;

        public void Setup(LevelRewardEntry entry)
        {

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
    }
}
