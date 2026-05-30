using TMPro;
using UnityEngine;
using Utils;

namespace Core.View
{    
    public class WalletView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _sofrText;       
        [SerializeField] private TextMeshProUGUI _middleText;       
        [SerializeField] private TextMeshProUGUI _hardText;

        public void UpdateSoft(long value)
        {
            _sofrText.text = value.ConvertFromLongToString();
        }
        public void UpdateMiddle(long value)
        {
            _middleText.text = value.ConvertFromLongToString();
        }
        public void UpdateHard(long value)
        {
            _hardText.text = value.ConvertFromLongToString();            
        }
    }
}
