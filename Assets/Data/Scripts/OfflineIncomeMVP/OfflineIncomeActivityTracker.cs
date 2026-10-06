using Core.Time;
using UnityEngine;
using Zenject;

namespace OfflineIncome
{
    public class OfflineIncomeActivityTracker : IInitializable, ITickable, System.IDisposable
    {
        private const float SaveInterval = 30f;

        private ITimeService _timeService;
        private IOfflineIncomeRuntimeSave _save;
        private float _elapsed;

        [Inject]
        public void Construct(ITimeService timeService, IOfflineIncomeRuntimeSave save)
        {
            _timeService = timeService;
            _save = save;
        }

        public void Initialize()
        {
            Application.quitting += RecordCurrentTime;
        }

        public void Tick()
        {
            _elapsed += UnityEngine.Time.deltaTime;
            if (_elapsed < SaveInterval)
            {
                return;
            }

            _elapsed = 0f;
            RecordCurrentTime();
        }

        public void Dispose()
        {
            Application.quitting -= RecordCurrentTime;
            RecordCurrentTime();
        }

        private void RecordCurrentTime()
        {
            _save.SetLastOnlineTicks(_timeService.CurrentUtcTicks);
        }
    }
}
