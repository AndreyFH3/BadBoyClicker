using System.Collections.Generic;
using PlayerProgression;
using Zenject;

namespace PlayerFeatures
{
    public class PlayerFeatureUnlockService : IPlayerFeatureUnlockService, IInitializable, System.IDisposable
    {
        private PlayerFeatureUnlockConfig _config;
        private IPlayerProgressionService _progression;
        private readonly Dictionary<PlayerFeatureType, bool> _knownUnlockStates = new();

        public event System.Action<PlayerFeatureType> FeatureUnlocked;

        [Inject]
        public void Construct(PlayerFeatureUnlockConfig config, IPlayerProgressionService progression)
        {
            _config = config;
            _progression = progression;
        }

        public void Initialize()
        {
            CacheStates();
            _progression.Changed += CheckUnlocks;
        }

        public void Dispose()
        {
            _progression.Changed -= CheckUnlocks;
        }

        public bool IsUnlocked(PlayerFeatureType feature)
        {
            return _progression.CurrentLevel >= GetRequiredLevel(feature);
        }

        public int GetRequiredLevel(PlayerFeatureType feature)
        {
            var features = _config?.Features;
            if (features == null)
            {
                return 1;
            }

            foreach (var data in features)
            {
                if (data != null && data.Feature == feature)
                {
                    return System.Math.Max(1, data.RequiredLevel);
                }
            }

            return int.MaxValue;
        }

        private void CacheStates()
        {
            _knownUnlockStates.Clear();
            var features = _config?.Features;
            if (features == null)
            {
                return;
            }

            foreach (var data in features)
            {
                if (data != null)
                {
                    _knownUnlockStates[data.Feature] = IsUnlocked(data.Feature);
                }
            }
        }

        private void CheckUnlocks()
        {
            var features = _config?.Features;
            if (features == null)
            {
                return;
            }

            foreach (var data in features)
            {
                if (data == null)
                {
                    continue;
                }

                bool wasUnlocked = _knownUnlockStates.TryGetValue(data.Feature, out bool value) && value;
                bool isUnlocked = IsUnlocked(data.Feature);
                _knownUnlockStates[data.Feature] = isUnlocked;

                if (!wasUnlocked && isUnlocked)
                {
                    FeatureUnlocked?.Invoke(data.Feature);
                }
            }
        }
    }
}
