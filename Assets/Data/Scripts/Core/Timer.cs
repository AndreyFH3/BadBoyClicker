using UnityEngine;
using Zenject;
using Shop;
using PlayerProgression;

namespace Core
{    
    public class Timer : IInitializable, ITickable
    {
        private const float IncomeInterval = 1f;

        private Wallet _wallet;
        private IShopRuntimeSave _shopSave;
        private IPlayerProgressionService _playerProgression;
        private float _elapsed;

        [Inject]
        public void Construct(Wallet wallet, IShopRuntimeSave shopSave, IPlayerProgressionService playerProgression)
        {
            _wallet = wallet;
            _shopSave = shopSave;
            _playerProgression = playerProgression;
        }

        public void Initialize()
        {
            Debug.Log("Timer Initialized!");
        }

        public void Tick()
        {
            long income = _shopSave.AutoIncomePerSecond;
            if (income <= 0)
            {
                _elapsed = 0f;
                return;
            }

            _elapsed += UnityEngine.Time.deltaTime;
            while (_elapsed >= IncomeInterval)
            {
                _elapsed -= IncomeInterval;
                _wallet.AddSoft(income);
                _playerProgression.AddExperience(PlayerExperienceSource.PassiveIncomeTick);
            }
        }
    }
}
