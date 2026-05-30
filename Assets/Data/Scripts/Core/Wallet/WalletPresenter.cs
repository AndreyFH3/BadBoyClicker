using UnityEngine;
using Zenject;
using Core.View;

namespace Core.Presenter
{    
    public class WalletPresenter : IInitializable
    {
        private Wallet _wallet;
        private WalletView _view;

        [Inject]
        public void Init(Wallet wallet, WalletView view)
        {
            _wallet = wallet;
            _view = view;
        }

        public void Initialize()
        {
            _wallet.OnChanged += UpdateView;
            UpdateView();
        }

        private void UpdateView()
        {
            _view.UpdateSoft(_wallet.Soft);
            _view.UpdateMiddle(_wallet.Middle);
            _view.UpdateHard(_wallet.Hard);
        }
    }
}
