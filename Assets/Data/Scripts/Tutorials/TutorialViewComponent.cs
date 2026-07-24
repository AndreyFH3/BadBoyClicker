using GameLocalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tutorials
{
    public class TutorialViewComponent : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _headerText;
        [SerializeField] private TextMeshProUGUI _descriptionText;

        public void Show(TutorialConfig.TutorialData tutorial)
        {
            if (_headerText != null)
            {
                _headerText.text = Localization.Tr(tutorial.HeaderLocalizationKey);
            }

            if (_descriptionText != null)
            {
                _descriptionText.text = Localization.Tr(tutorial.DescriptionLocalizationKey);
            }

            if (_icon != null)
            {
                _icon.sprite = tutorial.Icon;
                _icon.enabled = tutorial.Icon != null;
            }

            gameObject.SetActive(true);
        }
    }
}
