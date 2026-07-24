using Core;
using Zenject;
using UnityEngine;
using Core.View;
using Core.Presenter;
using Installer.Init;
using Shop;
using Core.Ads;
using Core.Time;
using OfflineIncome;
using DailyLogin;
using DailyLoginMVP;
using DailyQuests;
using DailyQuestMVP;
using PlayerProgression;
using PlayerFeatures;
using QuestSystem;
using CardCollections;
using CardCollectionMVP;
using Chests;
using ChestsMVP;
using GameLocalization;
using GameAudio;
using AdBonusOffers;
using Customization;
using Analytics;
using Purchases;
using RewardActivation;
using Tutorials;

namespace Installer
{
    public class GameInstaller : MonoInstaller
    {
        [SerializeField] private ClickableObject _clickObj;
        [SerializeField] private WalletView _walletView;
        [SerializeField] private ClickInfoShower _clickInfo;
        [SerializeField] private ShopView _shopView;
        [SerializeField] private ShopPurchaseConfirmationView _shopPurchaseConfirmationView;
        [SerializeField] private DailyLoginView _dailyLoginView;
        [SerializeField] private DailyQuestView _dailyQuestView;
        [SerializeField] private OfflineIncomeView _offlineIncomeView;
        [SerializeField] private PlayerProgressionView _playerProgressionView;
        [SerializeField] private CardCollectionsView _cardCollectionsView;
        [SerializeField] private CardCollectionCardsView _cardCollectionCardsView;
        [SerializeField] private CardCollectionRewardWindow _cardCollectionRewardWindow;
        [SerializeField] private ChestOpenView _chestOpenView;
        [SerializeField] private MonoBehaviour _adBonusOfferView;
        [SerializeField] private MonoBehaviour _rewardActivationView;
        [SerializeField] private AdBonusActiveEffectsView _adBonusActiveEffectsView;
        [SerializeField] private RewardedAdErrorView _rewardedAdErrorView;
        [SerializeField] private CustomizationView _customizationView;
        [SerializeField] private TutorialView _tutorialView;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ShopRuntimeSave>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<OfflineIncomeRuntimeSave>().AsSingle().NonLazy();
            Container.BindInterfacesTo<OfflineIncomeCustomRewardService>().AsSingle().NonLazy();
            Container.BindInterfacesTo<CardCollectionCustomRewardService>().AsSingle().NonLazy();
            Container.BindInterfacesTo<PlayerProgressionExperienceRewardService>().AsSingle().NonLazy();
            Container.BindInterfacesTo<QuestCustomRewardDispatcher>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<DailyLoginRuntimeSave>().AsSingle().NonLazy();
            Container.Bind<DailyQuestRuntimeSave>().AsSingle().NonLazy();
            Container.Bind<PlayerProgressionRuntimeSave>().AsSingle().NonLazy();
            Container.Bind<QuestRuntimeSave>().AsSingle().NonLazy();
            Container.Bind<CardCollectionRuntimeSave>().AsSingle().NonLazy();
            Container.Bind<CustomizationRuntimeSave>().AsSingle().NonLazy();
            Container.Bind<AdBonusOfferRuntimeSave>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<AnalyticsRuntimeSave>().AsSingle().NonLazy();
            Container.Bind<ILocalizationService>().To<LocalizationService>().AsSingle().NonLazy();
            Container.Bind<IAudioService>().To<AudioService>().AsSingle().NonLazy();
            Localization.SetService(Container.Resolve<ILocalizationService>());
            Container.BindInterfacesTo<LocalizationInitializer>().AsSingle().NonLazy();
            Container.BindInitializableExecutionOrder<LocalizationInitializer>(-10000);
            Container.BindInterfacesTo<StaticTextLocalizationInitializer>().AsSingle().NonLazy();
            Container.BindInitializableExecutionOrder<StaticTextLocalizationInitializer>(-9999);
            Container.Bind<QuestFactory>().AsSingle().NonLazy();
            Container.Bind<IQuestRewardService>().To<QuestRewardService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<RewardActivationService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ChestRewardService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PlayerProgressionService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PlayerFeatureUnlockService>().AsSingle().NonLazy();
            Container.Bind<ITimeService>().To<LocalTimeService>().AsSingle().NonLazy();
            Container.Bind<IRewardService>().To<RewardService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<DailyLoginService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<DailyQuestService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<CardCollectionService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<CardCollectionBonusService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<CustomizationService>().AsSingle().NonLazy();
            Container.Bind<IDailyLoginStartupGate>().To<DailyLoginStartupGate>().AsSingle().NonLazy();
            Container.Bind<IRewardedAdsService>().To<YGRewardedAdsService>().AsSingle().NonLazy();
            Container.BindInterfacesTo<PurchaseSystem>().AsSingle().NonLazy();
            Container.Bind<IRewardOfferUiGate>().To<RewardOfferUiGate>().AsSingle().NonLazy();
            Container.Bind<IRewardedAdErrorView>().FromInstance(GetRewardedAdErrorView()).AsSingle().NonLazy();
            Container.BindInterfacesTo<BuffService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<AdBonusOfferService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<Timer>().AsSingle().NonLazy();

            Container.BindInterfacesAndSelfTo<Wallet>().AsSingle().NonLazy();
            Container.Bind<WalletView>().FromInstance(_walletView).AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<WalletPresenter>().AsSingle().NonLazy();
            
            Container.BindInterfacesAndSelfTo<SaveYGController>().AsSingle().NonLazy();
            
            Container.Bind<ClickableObject>().FromInstance(_clickObj).AsSingle().NonLazy();
            Container.Bind<ClickInfoShower>().FromInstance(_clickInfo).AsSingle().NonLazy();

            Container.BindInterfacesAndSelfTo<ShopModel>().AsSingle().NonLazy();
            Container.Bind<IShopView>().FromInstance(_shopView).AsSingle().NonLazy();
            if (_shopPurchaseConfirmationView != null)
                Container.Bind<IShopPurchaseConfirmationView>().FromInstance(_shopPurchaseConfirmationView).AsSingle().NonLazy();
            else
                Container.Bind<IShopPurchaseConfirmationView>().To<ShopPurchaseConfirmationNullView>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ShopPresenter>().AsSingle().NonLazy();

            if (_dailyLoginView != null)
                Container.Bind<IDailyLoginView>().FromInstance(_dailyLoginView).AsSingle().NonLazy();
            else
                Container.Bind<IDailyLoginView>().To<DailyLoginNullView>().AsSingle().NonLazy();

            if (_dailyQuestView != null)
                Container.Bind<IDailyQuestView>().FromInstance(_dailyQuestView).AsSingle().NonLazy();
            else
                Container.Bind<IDailyQuestView>().To<DailyQuestNullView>().AsSingle().NonLazy();

            if (_offlineIncomeView != null)
                Container.Bind<IOfflineIncomeView>().FromInstance(_offlineIncomeView).AsSingle().NonLazy();
            else
                Container.Bind<IOfflineIncomeView>().To<OfflineIncomeNullView>().AsSingle().NonLazy();

            if (_playerProgressionView != null)
                Container.Bind<IPlayerProgressionView>().FromInstance(_playerProgressionView).AsSingle().NonLazy();
            else
                Container.BindInterfacesTo<PlayerProgressionRuntimeView>().AsSingle().NonLazy();

            if (_cardCollectionsView != null)
                Container.Bind<ICardCollectionsView>().FromInstance(_cardCollectionsView).AsSingle().NonLazy();
            else
                Container.Bind<ICardCollectionsView>().To<CardCollectionsNullView>().AsSingle().NonLazy();

            if (_cardCollectionCardsView != null)
                Container.Bind<ICardCollectionCardsView>().FromInstance(_cardCollectionCardsView).AsSingle().NonLazy();
            else
                Container.Bind<ICardCollectionCardsView>().To<CardCollectionCardsNullView>().AsSingle().NonLazy();

            if (_cardCollectionRewardWindow != null)
                Container.Bind<ICardCollectionRewardWindow>().FromInstance(_cardCollectionRewardWindow).AsSingle().NonLazy();
            else
                Container.Bind<ICardCollectionRewardWindow>().To<CardCollectionRewardNullWindow>().AsSingle().NonLazy();

            if (_chestOpenView != null)
                Container.Bind<IChestOpenView>().FromInstance(_chestOpenView).AsSingle().NonLazy();
            else
                Container.Bind<IChestOpenView>().To<ChestOpenNullView>().AsSingle().NonLazy();

            if (_adBonusOfferView is IAdBonusOfferView adBonusOfferView)
                Container.Bind<IAdBonusOfferView>().FromInstance(adBonusOfferView).AsSingle().NonLazy();
            else
                Container.Bind<IAdBonusOfferView>().FromInstance(CreateRuntimeAdBonusOfferView()).AsSingle().NonLazy();

            Container.Bind<IRewardActivationView>()
                .FromInstance(GetRewardActivationView())
                .AsSingle()
                .NonLazy();

            if (_adBonusActiveEffectsView != null)
                Container.BindInterfacesAndSelfTo<AdBonusActiveEffectsView>().FromInstance(_adBonusActiveEffectsView).AsSingle().NonLazy();
            else
                Container.BindInterfacesAndSelfTo<AdBonusActiveEffectsView>().FromInstance(CreateRuntimeAdBonusActiveEffectsView()).AsSingle().NonLazy();

            if (_customizationView != null)
                Container.Bind<ICustomizationView>().FromInstance(_customizationView).AsSingle().NonLazy();
            else
                Container.Bind<ICustomizationView>().To<CustomizationNullView>().AsSingle().NonLazy();

            if (_tutorialView != null)
                Container.Bind<ITutorialView>().FromInstance(_tutorialView).AsSingle().NonLazy();
            else
                Container.Bind<ITutorialView>().To<TutorialNullView>().AsSingle().NonLazy();

            Container.BindInterfacesAndSelfTo<DailyLoginModel>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<DailyLoginPresenter>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<DailyQuestPresenter>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<OfflineIncomeModel>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<OfflineIncomePresenter>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PlayerProgressionPresenter>().AsSingle().NonLazy();
            Container.Bind<CardCollectionSelectionModel>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<CardCollectionsPresenter>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<CardCollectionCardsPresenter>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ChestOpenPresenter>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<AdBonusOfferPresenter>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<CustomizationModel>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<CustomizationPresenter>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<OfflineIncomeActivityTracker>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<GameStartRouter>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<QuestService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<GameAnalyticsController>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<TutorialModel>().AsSingle().NonLazy();
        }

        private AdBonusOfferView CreateRuntimeAdBonusOfferView()
        {
            return new GameObject("AdBonusOfferView").AddComponent<AdBonusOfferView>();
        }

        private RewardActivationView CreateRuntimeRewardActivationView()
        {
            return new GameObject("RewardActivationView").AddComponent<RewardActivationView>();
        }

        private IRewardActivationView GetRewardActivationView()
        {
            if (_rewardActivationView is IRewardActivationView view)
            {
                return view;
            }

            if (_rewardActivationView != null &&
                _rewardActivationView.TryGetComponent(out RewardActivationView siblingView))
            {
                return siblingView;
            }

            Debug.LogWarning(
                "RewardActivationView is not assigned in GameInstaller. A runtime fallback view will be created.",
                this);
            return CreateRuntimeRewardActivationView();
        }

        private AdBonusActiveEffectsView CreateRuntimeAdBonusActiveEffectsView()
        {
            return new GameObject("AdBonusActiveEffectsView").AddComponent<AdBonusActiveEffectsView>();
        }

        private RewardedAdErrorView CreateRuntimeRewardedAdErrorView()
        {
            return new GameObject("RewardedAdErrorView").AddComponent<RewardedAdErrorView>();
        }

        private RewardedAdErrorView GetRewardedAdErrorView()
        {
            return _rewardedAdErrorView != null ? _rewardedAdErrorView : CreateRuntimeRewardedAdErrorView();
        }
    }
}
