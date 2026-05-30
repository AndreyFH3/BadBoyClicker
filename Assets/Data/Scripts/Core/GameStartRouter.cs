using Core;
using System;
using Shop;
using Zenject;
using PlayerProgression;

namespace Installer.Init
{    
    public class GameStartRouter : IInitializable, IDisposable
    {
        private Wallet _wallet;
        private ClickableObject _clickable;
        private IShopRuntimeSave _shopSave;
        private IPlayerProgressionService _playerProgression;
        public System.Action<long> OnClickValueEvent;

        private long CalculateValue => _shopSave.ClickValue;

        [Inject]
        public void StartGame(Wallet wallet, ClickableObject clickable, IShopRuntimeSave shopSave, IPlayerProgressionService playerProgression)
        {
            _wallet = wallet;
            _clickable = clickable;
            _shopSave = shopSave;
            _playerProgression = playerProgression;
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
