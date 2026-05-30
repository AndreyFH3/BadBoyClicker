using UnityEngine;
using UnityEngine.UI;

namespace YG.Example
{
    public class SaverTest : MonoBehaviour
    {
        public InputField stringifyText;
        public InputField integerText;
        public Toggle[] booleanArrayToggle;

        // Подписываемся на ивент onGetSDKData
        // Ивент onGetSDKData срабатывает при загрузке сохранений и при других обновлениях данных
        // В данном случае, при нажатии кнопки Set Default Saves будет вызов ивента onGetSDKData и мы обновим данные при сбросе сохранений
        private void OnEnable()
        {
            YG2.onGetSDKData += GetData;
        }

        // Отписываемся от ивента onGetSDKData
        private void OnDisable()
        {
            YG2.onGetSDKData -= GetData;
        }

        private void Awake()
        {
            GetData();
        }

        public void SetData()
        {
            
        }

        public void GetData()
        {

        }

        public void Save()
        {
            YG2.SaveProgress();
        }
    }
}