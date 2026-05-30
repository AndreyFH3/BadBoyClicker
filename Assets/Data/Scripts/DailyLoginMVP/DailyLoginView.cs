using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DailyLoginMVP
{
    public class DailyLoginView : MonoBehaviour, IDailyLoginView
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Transform _rewardsRoot;
        [SerializeField] private DailyLoginRewardItemView _rewardItemPrefab;
        [SerializeField] private Image _fallbackIcon;
        [SerializeField] private TextMeshProUGUI _fallbackText;
        [SerializeField] private Button _claimButton;

        private readonly List<DailyLoginRewardItemView> _spawnedRewardItems = new();

        public event Action ClaimRequested;

        private void Awake()
        {
            if (_claimButton != null)
            {
                _claimButton.onClick.AddListener(RequestClaim);
            }

            Hide();
        }

        private void OnDestroy()
        {
            if (_claimButton != null)
            {
                _claimButton.onClick.RemoveListener(RequestClaim);
            }
        }

        public void Show(DailyLoginViewData data)
        {
            Root.SetActive(true);
            ClearSpawnedItems();

            if (_rewardItemPrefab != null && _rewardsRoot != null && data.Rewards != null)
            {
                _rewardItemPrefab.gameObject.SetActive(true);
                for (int i = 0; i < data.Rewards.Count; i++)
                {
                    DailyLoginRewardItemView item = Instantiate(_rewardItemPrefab, _rewardsRoot);
                    item.gameObject.SetActive(true);
                    item.SetData(data.Rewards[i]);
                    _spawnedRewardItems.Add(item);
                }

                SetFallbackReward(default);
                return;
            }

            SetFallbackReward(GetCurrentReward(data));
        }

        public void Hide()
        {
            Root.SetActive(false);
        }

        private GameObject Root => _root != null ? _root : gameObject;

        private void ClearSpawnedItems()
        {
            for (int i = 0; i < _spawnedRewardItems.Count; i++)
            {
                if (_spawnedRewardItems[i] != null)
                {
                    Destroy(_spawnedRewardItems[i].gameObject);
                }
            }

            _spawnedRewardItems.Clear();
        }

        private void SetFallbackReward(DailyLoginRewardViewData data)
        {
            if (_fallbackIcon != null)
            {
                _fallbackIcon.sprite = data.Icon;
                _fallbackIcon.enabled = data.Icon != null;
            }

            if (_fallbackText != null)
            {
                _fallbackText.text = data.Text;
                _fallbackText.gameObject.SetActive(!string.IsNullOrEmpty(data.Text));
            }
        }

        private DailyLoginRewardViewData GetCurrentReward(DailyLoginViewData data)
        {
            if (data.Rewards == null)
            {
                return default;
            }

            for (int i = 0; i < data.Rewards.Count; i++)
            {
                if (data.Rewards[i].IsCurrent)
                {
                    return data.Rewards[i];
                }
            }

            return data.Rewards.Count > 0 ? data.Rewards[0] : default;
        }

        private void RequestClaim()
        {
            ClaimRequested?.Invoke();
        }
    }
}
