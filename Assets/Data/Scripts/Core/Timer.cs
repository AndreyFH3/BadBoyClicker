using UnityEngine;
using Zenject;
using Shop;
using PlayerProgression;
using AdBonusOffers;
using System;

namespace Core
{    
    public class Timer : IInitializable, ITickable
    {
        private const float IncomeInterval = 1f;

        private Wallet _wallet;
        private IShopRuntimeSave _shopSave;
        private IPlayerProgressionService _playerProgression;
        private IBuffService _buffService;
        private float _elapsed;

        [Inject]
        public void Construct(
            Wallet wallet,
            IShopRuntimeSave shopSave,
            IPlayerProgressionService playerProgression,
            IBuffService buffService)
        {
            _wallet = wallet;
            _shopSave = shopSave;
            _playerProgression = playerProgression;
            _buffService = buffService;
        }

        public void Initialize()
        {
            Debug.Log("Timer Initialized!");
        }

        public void Tick()
        {
            long income = ApplyMultiplier(
                _shopSave.AutoIncomePerSecond,
                (_buffService?.PassiveIncomeMultiplier ?? 1f) * (_buffService?.AllIncomeMultiplier ?? 1f));
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

        private long ApplyMultiplier(long value, float multiplier)
        {
            if (value <= 0)
            {
                return 0;
            }

            return Math.Max(1, (long)Math.Ceiling(value * Math.Max(0f, multiplier)));
        }
    }
}
