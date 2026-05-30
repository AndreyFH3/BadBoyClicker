using System;

namespace QuestSystem
{
    public abstract class Quest
    {
        private readonly QuestConfig.QuestData _data;
        private long _currentValue;
        private bool _isCompleted;
        private bool _isRewardClaimed;

        public string Id => _data.Id;
        public QuestConfig.QuestData Data => _data;
        public long CurrentValue => _currentValue;
        public long TargetValue => Math.Max(1, _data.TargetValue);
        public bool IsCompleted => _isCompleted;
        public bool IsRewardClaimed => _isRewardClaimed;

        public event Action<Quest> Changed;

        protected Quest(QuestConfig.QuestData data, long currentValue, bool isCompleted, bool isRewardClaimed)
        {
            _data = data;
            _currentValue = Math.Max(0, currentValue);
            _isCompleted = isCompleted || _currentValue >= TargetValue;
            _isRewardClaimed = isRewardClaimed;
            if (_isCompleted)
            {
                _currentValue = TargetValue;
            }
        }

        public void MarkRewardClaimed()
        {
            _isRewardClaimed = true;
        }

        public QuestViewData CreateViewData()
        {
            return new QuestViewData
            {
                Id = Id,
                Title = _data.Title,
                Description = _data.Description,
                Progress = $"{_currentValue}/{TargetValue}",
                CurrentValue = _currentValue,
                TargetValue = TargetValue,
                IsCompleted = _isCompleted,
                IsRewardClaimed = _isRewardClaimed
            };
        }

        protected void AddProgress(long amount)
        {
            if (_isCompleted || amount <= 0)
            {
                return;
            }

            _currentValue = Math.Min(TargetValue, _currentValue + amount);
            if (_currentValue >= TargetValue)
            {
                _isCompleted = true;
            }

            Changed?.Invoke(this);
        }
    }
}
