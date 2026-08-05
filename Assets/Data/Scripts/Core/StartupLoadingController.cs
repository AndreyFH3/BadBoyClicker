using System;
using Purchases;
using Zenject;

namespace Core
{
    public sealed class StartupLoadingController : IInitializable, IDisposable
    {
        private readonly ISaveSystem _saveSystem;
        private readonly StartupLoadingView _view;
        private readonly IPurchaseSystem _purchaseSystem;

        public StartupLoadingController(
            ISaveSystem saveSystem,
            StartupLoadingView view,
            IPurchaseSystem purchaseSystem)
        {
            _saveSystem = saveSystem;
            _view = view;
            _purchaseSystem = purchaseSystem;
        }

        public void Initialize()
        {
            _saveSystem.Loaded += OnLoaded;
            _view.StartRequested += OnStartRequested;

            if (_saveSystem.IsLoaded)
            {
                OnLoaded();
            }
        }

        public void Dispose()
        {
            _saveSystem.Loaded -= OnLoaded;
            _view.StartRequested -= OnStartRequested;
        }

        private void OnLoaded()
        {
            _saveSystem.Loaded -= OnLoaded;

            if (_view != null)
            {
                _view.AllowStart();
            }
        }

        private void OnStartRequested()
        {
            _purchaseSystem.RecoverPurchases();
        }
    }
}
