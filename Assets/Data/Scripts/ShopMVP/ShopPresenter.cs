using System;
using GameLocalization;
using Zenject;
namespace Shop
{
    public class ShopPresenter : IInitializable, IDisposable
    {
        private IShopModel _model;
        private IShopView _view;
        private IShopPurchaseConfirmationView _confirmationView;
        private string _pendingPurchaseId;

        [Inject]
        public void Construct(
            IShopModel model,
            IShopView view,
            IShopPurchaseConfirmationView confirmationView)
        {
            _model = model;
            _view = view;
            _confirmationView = confirmationView;
        }

        public void Initialize()
        {
            _model.StateChanged += OnStateChanged;
            _view.OpenRequested += OnOpenRequested;
            _view.CloseRequested += OnCloseRequested;
            _view.OnBuy += OnBuyRequested;
            _confirmationView.ConfirmRequested += OnPurchaseConfirmed;
            _confirmationView.CancelRequested += OnPurchaseCanceled;
            Localization.LanguageChanged += OnLanguageChanged;

            UpdateView(true);
        }

        public void Dispose()
        {
            _model.StateChanged -= OnStateChanged;
            _view.OpenRequested -= OnOpenRequested;
            _view.CloseRequested -= OnCloseRequested;
            _view.OnBuy -= OnBuyRequested;
            _confirmationView.ConfirmRequested -= OnPurchaseConfirmed;
            _confirmationView.CancelRequested -= OnPurchaseCanceled;
            Localization.LanguageChanged -= OnLanguageChanged;
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
            ShopPurchaseConfirmationData confirmationData = _model.GetPurchaseConfirmationData(id);
            if (confirmationData != null)
            {
                _pendingPurchaseId = id;
                _confirmationView.Show(confirmationData);
                return;
            }

            _model.Buy(id);
        }

        private void OnPurchaseConfirmed()
        {
            string id = _pendingPurchaseId;
            _pendingPurchaseId = null;
            _confirmationView.Hide();

            if (!string.IsNullOrEmpty(id))
            {
                _model.Buy(id);
            }
        }

        private void OnPurchaseCanceled()
        {
            _pendingPurchaseId = null;
            _confirmationView.Hide();
        }

        private void OnLanguageChanged()
        {
            UpdateView(true);
        }
    }
}
