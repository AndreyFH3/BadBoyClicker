using System;
using Core;
using Core.Ads;
using DailyLoginMVP;
using PlayerFeatures;
using Zenject;

namespace OfflineIncome
{
    public class OfflineIncomePresenter : IInitializable, IDisposable
    {
        private const string RewardedPlacementId = "offline_income_x2";

        private DiContainer _container;
        private OfflineIncomeModel _model;
        private IOfflineIncomeView _view;
        private IRewardedAdsService _adsService;
        private Wallet _wallet;
        private GameConfig _config;
        private IDailyLoginStartupGate _dailyLoginStartupGate;
        private IPlayerFeatureUnlockService _featureUnlockService;
        private bool _isCompleted;

        [Inject]
        public void Construct(
            DiContainer container,
            OfflineIncomeModel model,
            IOfflineIncomeView view,
            IRewardedAdsService adsService,
            Wallet wallet,
            GameConfig config,
            IDailyLoginStartupGate dailyLoginStartupGate,
            IPlayerFeatureUnlockService featureUnlockService)
        {
            _container = container;
            _model = model;
            _view = view;
            _adsService = adsService;
            _wallet = wallet;
            _config = config;
            _dailyLoginStartupGate = dailyLoginStartupGate;
            _featureUnlockService = featureUnlockService;
        }

        public void Initialize()
        {
            _view.ClaimRequested += Claim;
            _view.ClaimForHardRequested += ClaimForHard;
            _view.ClaimWithAdRequested += ClaimWithAd;

            if (!_dailyLoginStartupGate.IsCompleted)
            {
                _dailyLoginStartupGate.Completed += StartOfflineIncomeFlow;
                return;
            }

            StartOfflineIncomeFlow();
        }

        private void StartOfflineIncomeFlow()
        {
            _dailyLoginStartupGate.Completed -= StartOfflineIncomeFlow;

            if (!_featureUnlockService.IsUnlocked(PlayerFeatureType.OfflineIncome))
            {
                Complete();
                return;
            }

            if (_model.HasReward)
            {
                _view.Show(_model.CreateViewData(_wallet));
                return;
            }

            Complete();
        }

        public void Dispose()
        {
            _view.ClaimRequested -= Claim;
            _view.ClaimForHardRequested -= ClaimForHard;
            _view.ClaimWithAdRequested -= ClaimWithAd;
            _dailyLoginStartupGate.Completed -= StartOfflineIncomeFlow;
        }

        private void Claim()
        {
            AddReward(1);
        }

        private void ClaimForHard()
        {
            if (!_wallet.SpendHard(_config.OfflineIncome.HardClaimCost))
            {
                _view.Show(_model.CreateViewData(_wallet));
                return;
            }

            AddReward(_config.OfflineIncome.PaidMultiplier);
        }

        private void ClaimWithAd()
        {
            _adsService.Show(
                RewardedPlacementId,
                () => AddReward(_config.OfflineIncome.AdMultiplier),
                () => _view.Show(_model.CreateViewData(_wallet)));
        }

        private void AddReward(int multiplier)
        {
            long reward = _model.ConsumeReward(multiplier);
            if (reward > 0)
            {
                _wallet.AddSoft(reward);
            }

            Complete();
        }

        private void Complete()
        {
            if (_isCompleted)
            {
                return;
            }

            _isCompleted = true;
            _view.Hide();
            Dispose();

            _container.Unbind<OfflineIncomePresenter>();
            _container.Unbind<OfflineIncomeModel>();
        }
    }
}
