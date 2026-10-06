using System;
using Leaderboards;
using Zenject;

namespace LeaderboardMVP
{
    public class LeaderboardPresenter : IInitializable, IDisposable
    {
        private ILeaderboardService _service;
        private ILeaderboardView _view;

        [Inject]
        public void Construct(ILeaderboardService service, ILeaderboardView view)
        {
            _service = service;
            _view = view;
        }

        public void Initialize()
        {
            _service.Changed += UpdateView;
            _view.Opened += OnOpened;
            _view.AuthRequested += OnAuthRequested;
            UpdateView();
        }

        public void Dispose()
        {
            _service.Changed -= UpdateView;
            _view.Opened -= OnOpened;
            _view.AuthRequested -= OnAuthRequested;
        }

        private void OnOpened()
        {
            UpdateView();
            _service.RequestData();
        }

        private void OnAuthRequested()
        {
            _service.RequestAuth();
        }

        private void UpdateView()
        {
            _view.SetData(_service.Data);
        }
    }
}
