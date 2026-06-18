using UnityEngine;
using UnityEngine.UI;

namespace Customization
{
    public class CustomizationOpenButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private CustomizationView _view;

        private void Awake()
        {
            if (_button == null)
            {
                _button = GetComponent<Button>();
            }
        }

        private void OnEnable()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(Open);
            }
        }

        private void OnDisable()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(Open);
            }
        }

        public void Open()
        {
            if (_view != null)
            {
                _view.RequestOpen();
            }
        }
    }
}
