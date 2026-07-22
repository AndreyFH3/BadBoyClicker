using System;
using GameLocalization;
using Core.Ads;
using RewardActivation;
using Zenject;
namespace Shop
{
    public class ShopPresenter : IInitializable, IDisposable
    {
        private IShopModel _model;
        private IShopView _view;
        private IShopPurchaseConfirmationView _confirmationView;
        private IRewardActivationService _rewardActivationService;
        private IRewardedAdErrorView _errorView;
        private ILocalizationService _localization;
        private string _pendingPurchaseId;

        [Inject]
        public void Construct(
            IShopModel model,
            IShopView view,
            IShopPurchaseConfirmationView confirmationView,
            IRewardActivationService rewardActivationService,
            IRewardedAdErrorView errorView,
            ILocalizationService localization)
        {
            _model = model;
            _view = view;
            _confirmationView = confirmationView;
            _rewardActivationService = rewardActivationService;
            _errorView = errorView;
            _localization = localization;
        }

        public void Initialize()
        {
            _model.StateChanged += OnStateChanged;
            _model.PaidPurchaseSucceeded += OnPaidPurchaseSucceeded;
            _model.PaidPurchaseFailed += OnPaidPurchaseFailed;
            _view.OnBuy += OnBuyRequested;
            _confirmationView.ConfirmRequested += OnPurchaseConfirmed;
            _confirmationView.CancelRequested += OnPurchaseCanceled;
            Localization.LanguageChanged += OnLanguageChanged;

            UpdateView();
        }

        public void Dispose()
        {
            _model.StateChanged -= OnStateChanged;
            _model.PaidPurchaseSucceeded -= OnPaidPurchaseSucceeded;
            _model.PaidPurchaseFailed -= OnPaidPurchaseFailed;
            _view.OnBuy -= OnBuyRequested;
            _confirmationView.ConfirmRequested -= OnPurchaseConfirmed;
            _confirmationView.CancelRequested -= OnPurchaseCanceled;
            Localization.LanguageChanged -= OnLanguageChanged;
        }

        private void OnStateChanged()
        {
            UpdateView();
        }


        private void UpdateView()
        {
            _view.SetEarnPerSecond(_model.AutoIncomePerSecond);
            _view.SetData(_model.GetAllData());
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

        private void OnPaidPurchaseSucceeded(string id)
        {
            ShopElementData item = _model.GetShopPositionData(id);
            string itemName = item?.Name ?? string.Empty;
            string reward = item?.Bonus ?? string.Empty;

            _rewardActivationService.Enqueue(new RewardActivationViewData(
                item?.Icon,
                _localization.Localize("shop.purchase.success.title"),
                _localization.Format("shop.purchase.success.description", itemName, reward),
                _localization.Localize("common.ok"),
                string.Empty,
                false));
        }

        private void OnPaidPurchaseFailed(string _)
        {
            _errorView.Show(
                _localization.Localize("shop.purchase.failed.title"),
                _localization.Localize("shop.purchase.failed.description"));
        }

        private void OnLanguageChanged()
        {
            UpdateView();
        }
    }
}
