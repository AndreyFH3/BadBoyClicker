using System;
using Zenject;
namespace Shop
{
    public class ShopPresenter : IInitializable, IDisposable
    {
        private IShopModel _model;
        private IShopView _view;

        [Inject]
        public void Construct(IShopModel model, IShopView view)
        {
            _model = model;
            _view = view;
        }

        public void Initialize()
        {
            _model.StateChanged += OnStateChanged;
            _view.OpenRequested += OnOpenRequested;
            _view.CloseRequested += OnCloseRequested;
            _view.OnBuy += OnBuyRequested;

            UpdateView(true);
        }

        public void Dispose()
        {
            _model.StateChanged -= OnStateChanged;
            _view.OpenRequested -= OnOpenRequested;
            _view.CloseRequested -= OnCloseRequested;
            _view.OnBuy -= OnBuyRequested;
        }

        private void OnStateChanged()
        {
            UpdateView();
        }


        private void UpdateView(bool isForec = false)
        {
            _view.SetOpenState(_model.IsOpen);
            _view.SetEarnPerSecond(_model.AutoIncomePerSecond);

            if (!_view.IsActive && !isForec)
                return;

            _view.SetData(_model.GetAllData());
        }

        private void OnOpenRequested()
        {
            _model.Open();
        }

        private void OnCloseRequested()
        {
            _model.Close();
        }

        private void OnBuyRequested(string id)
        {
            _model.Buy(id);
        }
    }
}
