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

        public void Set(RewardDisplay reward)
        {
            if (_icon != null)
            {
                _icon.sprite = reward.Icon;
                _icon.enabled = reward.Icon != null;
            }

            if (_amount != null)
            {
                _amount.text = reward.Amount;
                _amount.gameObject.SetActive(!string.IsNullOrEmpty(reward.Amount));
            }
        }
    }
}
