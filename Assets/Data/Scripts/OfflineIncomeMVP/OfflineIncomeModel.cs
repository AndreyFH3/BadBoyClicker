using System;
using Core;
using Core.Time;
using PlayerFeatures;
using Shop;
using Zenject;

namespace OfflineIncome
{
    public class OfflineIncomeModel : IInitializable
    {
        private GameConfig _config;
        private ITimeService _timeService;
        private IShopRuntimeSave _shopSave;
        private IOfflineIncomeRuntimeSave _save;
        private IPlayerFeatureUnlockService _featureUnlockService;
        private bool _pendingRewardCalculated;

        public long PendingReward { get; private set; }
        public bool HasReward => PendingReward > 0;

        [Inject]
        public void Construct(
            GameConfig config,
            ITimeService timeService,
            IShopRuntimeSave shopSave,
            IOfflineIncomeRuntimeSave save,
            IPlayerFeatureUnlockService featureUnlockService)
        {
            _config = config;
            _timeService = timeService;
            _shopSave = shopSave;
            _save = save;
            _featureUnlockService = featureUnlockService;
        }

        public void Initialize()
        {
        }

        // Called once right after the save data is loaded (see SaveYGController.Load).
        // Computing the snapshot in Initialize() is unsafe: on platforms where the SDK
        // data arrives asynchronously, Initialize() runs before the save is applied, so
        // LastOnlineTicks / AutoIncomePerSecond would still be empty and the reward would
        // silently be zero on every re-entry.
        public void RecalculateFromSave()
        {
            if (_pendingRewardCalculated)
            {
                return;
            }

            _pendingRewardCalculated = true;
            CalculatePendingReward();
        }

        public OfflineIncomeViewData CreateViewData(Wallet wallet)
        {
            return new OfflineIncomeViewData(
                PendingReward,
                _config.OfflineIncome.HardClaimCost,
                wallet.CanSpendHard(_config.OfflineIncome.HardClaimCost));
        }

        public long ConsumeReward(int multiplier = 1)
        {
            if (PendingReward <= 0)
            {
                RecordCurrentTime();
                return 0;
            }

            long reward = PendingReward * Math.Max(1, multiplier);
            PendingReward = 0;
            RecordCurrentTime();
            return reward;
        }

        public void RecordCurrentTime()
        {
            _save.SetLastOnlineTicks(_timeService.CurrentUtcTicks);
        }

        private void CalculatePendingReward()
        {
            long nowTicks = _timeService.CurrentUtcTicks;

            if (!_featureUnlockService.IsUnlocked(PlayerFeatureType.OfflineIncome))
            {
                _save.SetLastOnlineTicks(nowTicks);
                PendingReward = 0;
                return;
            }

            long lastTicks = _save.LastOnlineTicks;

            if (lastTicks <= 0 || lastTicks >= nowTicks)
            {
                _save.SetLastOnlineTicks(nowTicks);
                PendingReward = 0;
                return;
            }

            TimeSpan elapsed = TimeSpan.FromTicks(nowTicks - lastTicks);
            long elapsedSeconds = (long)Math.Floor(elapsed.TotalSeconds);
            long minSeconds = Math.Max(0, _config.OfflineIncome.MinSecondsToShow);

            if (elapsedSeconds < minSeconds || _shopSave.AutoIncomePerSecond <= 0)
            {
                _save.SetLastOnlineTicks(nowTicks);
                PendingReward = 0;
                return;
            }

            PendingReward = elapsedSeconds * _shopSave.AutoIncomePerSecond;
            _save.SetLastOnlineTicks(nowTicks);
        }
    }
}
