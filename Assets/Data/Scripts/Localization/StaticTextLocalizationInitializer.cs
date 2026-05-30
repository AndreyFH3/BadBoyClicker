using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Zenject;

namespace GameLocalization
{
    public class StaticTextLocalizationInitializer : IInitializable
    {
        private readonly ILocalizationService _localization;
        private readonly Dictionary<string, string> _keysBySourceText = new()
        {
            { "\u041c\u0430\u0433\u0430\u0437\u0438\u043d", "ui.shop" },
            { "\u041f\u043e\u043a\u0443\u043f\u043a\u0438", "ui.purchases" },
            { "\u041d\u0430\u0441\u0442\u0440\u043e\u0439\u043a\u0438", "ui.settings" },
            { "\u041a\u043b\u0438\u043a\u0430\u0442\u044c", "ui.click" },
            { "Daily", "ui.daily" },
            { "Ok", "common.ok" },
            { "Done", "common.done" },
            { "Take", "common.take" },
            { "Level Up", "player_progression.level_up" },
            { "Cancel", "common.cancel" },
            { "Offline Earn", "offline_income.title" }
        };

        public StaticTextLocalizationInitializer(ILocalizationService localization)
        {
            _localization = localization;
        }

        public void Initialize()
        {
            TMP_Text[] texts = Resources.FindObjectsOfTypeAll<TMP_Text>();
            foreach (TMP_Text text in texts)
            {
                if (text == null || !text.gameObject.scene.IsValid())
                {
                    continue;
                }

                string source = text.text?.Trim();
                if (string.IsNullOrEmpty(source) || !_keysBySourceText.TryGetValue(source, out string key))
                {
                    continue;
                }

                text.text = _localization.Localize(key, source);
            }
        }
    }
}
