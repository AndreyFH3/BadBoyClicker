using Core;
using System;
using Shop;
using Zenject;
using PlayerProgression;
using AdBonusOffers;

namespace Installer.Init
{    
    public class GameStartRouter : IInitializable, IDisposable
    {
        private Wallet _wallet;
        private ClickableObject _clickable;
        private IShopRuntimeSave _shopSave;
        private IPlayerProgressionService _playerProgression;
        private IBuffService _buffService;
        public System.Action<long> OnClickValueEvent;

        private long CalculateValue => ApplyMultiplier(
            _shopSave.ClickValue,
            (_buffService?.ClickIncomeMultiplier ?? 1f) * (_buffService?.AllIncomeMultiplier ?? 1f));

        [Inject]
        public void StartGame(
            Wallet wallet,
            ClickableObject clickable,
            IShopRuntimeSave shopSave,
            IPlayerProgressionService playerProgression,
            IBuffService buffService)
        {
            _wallet = wallet;
            _clickable = clickable;
            _shopSave = shopSave;
            _playerProgression = playerProgression;
            _buffService = buffService;
        }

        public void Initialize()
        {
            _clickable.OnClick += AddMoney;
        }

        private void AddMoney()
        {
            _wallet.AddSoft(CalculateValue);
            _playerProgression.AddExperience(PlayerExperienceSource.Click);
            OnClickValueEvent?.Invoke(CalculateValue);
        }

        private long ApplyMultiplier(long value, float multiplier)
        {
            if (value <= 0)
            {
                return 0;
            }

            return Math.Max(1, (long)Math.Ceiling(value * Math.Max(0f, multiplier)));
        }

        public void StopGame()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_clickable != null)
            {
                _clickable.OnClick -= AddMoney;
            }
        }
    }
}
