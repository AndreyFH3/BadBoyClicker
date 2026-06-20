using TMPro;
using UnityEngine;

namespace GameLocalization
{
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] private string _key;
        [TextArea]
        [SerializeField] private string _fallback;

        private TMP_Text _text;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            Localization.ServiceChanged += OnLocalizationServiceChanged;
            Localization.LanguageChanged += OnLanguageChanged;

            if (Localization.IsReady)
            {
                Refresh();
            }
        }

        private void Start()
        {
            if (Localization.IsReady)
            {
                Refresh();
            }
        }

        private void OnDisable()
        {
            Localization.ServiceChanged -= OnLocalizationServiceChanged;
            Localization.LanguageChanged -= OnLanguageChanged;
        }

        public void Configure(string key, string fallback = null)
        {
            _key = key;
            _fallback = fallback;
            Refresh();
        }

        [ContextMenu("Refresh")]
        public void Refresh()
        {
            if (_text == null)
            {
                _text = GetComponent<TMP_Text>();
            }

            if (_text != null)
            {
                _text.text = Localization.Tr(_key, _fallback);
            }
        }

        private void OnLocalizationServiceChanged()
        {
            if (Localization.IsReady)
            {
                Refresh();
            }
        }

        private void OnLanguageChanged()
        {
            Refresh();
        }
    }
}
