using System;
using Zenject;

namespace Core
{
    public sealed class StartupLoadingController : IInitializable, IDisposable
    {
        private readonly ISaveSystem _saveSystem;
        private readonly StartupLoadingView _view;

        public StartupLoadingController(ISaveSystem saveSystem, StartupLoadingView view)
        {
            _saveSystem = saveSystem;
            _view = view;
        }

        public void Initialize()
        {
            _saveSystem.Loaded += OnLoaded;

            if (_saveSystem.IsLoaded)
            {
                OnLoaded();
            }
        }

        public void Dispose()
        {
            _saveSystem.Loaded -= OnLoaded;
        }

        private void OnLoaded()
        {
            _saveSystem.Loaded -= OnLoaded;

            if (_view != null)
            {
                _view.AllowStart();
            }
        }
    }
}
