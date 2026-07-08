using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AdBonusOffers
{
    public class AdBonusActiveEffectItemView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private Image _fillImage;

        public void SetText(string value)
        {
            if (_label != null)
            {
                _label.text = value ?? string.Empty;
            }
        }

        public void SetProgress(float progress01)
        {
            if (_fillImage != null)
            {
                _fillImage.fillAmount = Mathf.Clamp01(progress01);
            }
        }

        public void SetReferences(TextMeshProUGUI label, Image fillImage)
        {
            _label = label;
            _fillImage = fillImage;
        }
    }
}
