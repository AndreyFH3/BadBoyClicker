using Leaderboards;
using TMPro;
using UnityEngine;

namespace LeaderboardMVP
{
    public class LeaderboardEntryViewElement : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _rankText;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private GameObject _currentPlayerHighlight;

        public void SetData(LeaderboardEntryViewData data)
        {
            if (data == null)
            {
                return;
            }

            if (_rankText != null)
            {
                _rankText.text = data.Rank.ToString();
            }

            if (_nameText != null)
            {
                _nameText.text = data.Name;
            }

            if (_scoreText != null)
            {
                _scoreText.text = data.Score.ToString();
            }

            if (_currentPlayerHighlight != null)
            {
                _currentPlayerHighlight.SetActive(data.IsCurrentPlayer);
            }
        }
    }
}
