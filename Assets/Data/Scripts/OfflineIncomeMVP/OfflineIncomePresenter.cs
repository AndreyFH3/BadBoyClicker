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
        private IRewardedAdErrorView _adErrorView;
        private IRewardOfferUiGate _rewardOfferUiGate;
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
            IRewardedAdErrorView adErrorView,
            IRewardOfferUiGate rewardOfferUiGate,
            Wallet wallet,
            GameConfig config,
            IDailyLoginStartupGate dailyLoginStartupGate,
            IPlayerFeatureUnlockService featureUnlockService)
        {
            _container = container;
            _model = model;
            _view = view;
            _adsService = adsService;
            _adErrorView = adErrorView;
            _rewardOfferUiGate = rewardOfferUiGate;
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
                DestroyView();
                return;
            }

            if (_model.HasReward)
            {
                _rewardOfferUiGate.Block(this);
                _view.Show(_model.CreateViewData(_wallet));
                return;
            }

            Complete();
            DestroyView();
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
            Complete();
            _view.Hide(DestroyView);
        }

        private void ClaimForHard()
        {
            if (!_wallet.SpendHard(_config.OfflineIncome.HardClaimCost))
            {
                _view.Show(_model.CreateViewData(_wallet));
                return;
            }

            long reward = AddReward(_config.OfflineIncome.PaidMultiplier);
            Complete();
            _view.ShowClaimedReward(reward, DestroyView);
        }

        private void ClaimWithAd()
        {
            _adsService.Show(
                RewardedPlacementId,
                () =>
                {
                    long reward = AddReward(_config.OfflineIncome.AdMultiplier);
                    Complete();
                    _view.ShowClaimedReward(reward, DestroyView);
                },
                OnRewardedAdFailed);
        }

        private void OnRewardedAdFailed(Core.Ads.RewardedAdFailureReason reason)
        {
            _view.Show(_model.CreateViewData(_wallet));
            _adErrorView.Show();
        }

        private long AddReward(int multiplier)
        {
            long reward = _model.ConsumeReward(multiplier);
            if (reward > 0)
            {
                _wallet.AddSoft(reward);
            }

            return reward;
        }

        private void Complete()
        {
            if (_isCompleted)
            {
                return;
            }

            _isCompleted = true;
            _rewardOfferUiGate.Unblock(this);
            Dispose();

            _container.Unbind<OfflineIncomePresenter>();
            _container.Unbind<OfflineIncomeModel>();
        }

        // The popup is a one-time (or zero-time) piece of UI: either it is claimed once and
        // never needed again this session, or the reward wasn't available at all. Destroying
        // it after the close animation finishes (rather than leaving it inactive) frees its
        // whole UI subtree for the rest of the session.
        private void DestroyView()
        {
            _view.DestroyView();
            _container.Unbind<IOfflineIncomeView>();
        }
    }
}
