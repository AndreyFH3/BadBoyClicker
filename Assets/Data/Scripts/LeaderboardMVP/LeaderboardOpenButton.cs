using UnityEngine;
using UnityEngine.UI;

namespace LeaderboardMVP
{
    public class LeaderboardOpenButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private LeaderboardView _view;

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
                _view.Show();
            }
        }
    }
}
