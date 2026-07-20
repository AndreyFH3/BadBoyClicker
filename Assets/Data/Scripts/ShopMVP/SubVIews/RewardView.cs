using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rewards
{
    /// <summary>
    /// A single "what you get" chip: an icon and an amount. Reusable across the
    /// game (shop offers, daily quest milestones, …) to communicate a reward the
    /// way f2p games usually present them.
    /// </summary>
    public class RewardView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _amount;
        [Tooltip("Backdrop shown behind the amount text; hidden when the reward has no amount (e.g. a chest).")]
        [SerializeField] private GameObject _amountBackdrop;

        public void Set(RewardDisplay reward)
        {
            if (_icon != null)
            {
                _icon.sprite = reward.Icon;
                _icon.enabled = reward.Icon != null;
            }

            bool hasAmount = !string.IsNullOrEmpty(reward.Amount);

            if (_amount != null)
            {
                _amount.text = reward.Amount;
                _amount.gameObject.SetActive(hasAmount);
            }

            if (_amountBackdrop != null)
            {
                _amountBackdrop.SetActive(hasAmount);
            }
        }
    }
}
