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

namespace Installer
{
    public class GameInstaller : MonoInstaller
    {
        [SerializeField] private ClickableObject _clickObj;
        [SerializeField] private WalletView _walletView;
        [SerializeField] private ClickInfoShower _clickInfo;
        [SerializeField] private ShopView _shopView;
        [SerializeField] private DailyLoginView _dailyLoginView;
        [SerializeField] private DailyQuestView _dailyQuestView;
        [SerializeField] private OfflineIncomeView _offlineIncomeView;
        [SerializeField] private PlayerProgressionView _playerProgressionView;
        [SerializeField] private CardCollectionsView _cardCollectionsView;
        [SerializeField] private CardCollectionCardsView _cardCollectionCardsView;
        [SerializeField] private ChestOpenView _chestOpenView;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ShopRuntimeSave>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<OfflineIncomeRuntimeSave>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<DailyLoginRuntimeSave>().AsSingle().NonLazy();
            Container.Bind<DailyQuestRuntimeSave>().AsSingle().NonLazy();
            Container.Bind<PlayerProgressionRuntimeSave>().AsSingle().NonLazy();
            Container.Bind<QuestRuntimeSave>().AsSingle().NonLazy();
            Container.Bind<CardCollectionRuntimeSave>().AsSingle().NonLazy();
            Container.Bind<ILocalizationService>().To<LocalizationService>().AsSingle().NonLazy();
            Localization.SetService(Container.Resolve<ILocalizationService>());
            Container.BindInterfacesTo<LocalizationInitializer>().AsSingle().NonLazy();
            Container.BindInitializableExecutionOrder<LocalizationInitializer>(-10000);
            Container.BindInterfacesTo<StaticTextLocalizationInitializer>().AsSingle().NonLazy();
            Container.BindInitializableExecutionOrder<StaticTextLocalizationInitializer>(-9999);
            Container.Bind<QuestFactory>().AsSingle().NonLazy();
            Container.Bind<IQuestRewardService>().To<QuestRewardService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ChestRewardService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PlayerProgressionService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PlayerFeatureUnlockService>().AsSingle().NonLazy();
            Container.Bind<ITimeService>().To<LocalTimeService>().AsSingle().NonLazy();
            Container.Bind<IRewardService>().To<RewardService>().AsSingle().NonLazy();
            Container.Bind<IDailyLoginService>().To<DailyLoginService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<DailyQuestService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<CardCollectionService>().AsSingle().NonLazy();
            Container.Bind<IDailyLoginStartupGate>().To<DailyLoginStartupGate>().AsSingle().NonLazy();
            Container.Bind<IRewardedAdsService>().To<YGRewardedAdsService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<Timer>().AsSingle().NonLazy();

            Container.BindInterfacesAndSelfTo<Wallet>().AsSingle().NonLazy();
            Container.Bind<WalletView>().FromInstance(_walletView).AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<WalletPresenter>().AsSingle().NonLazy();
            
            Container.BindInterfacesAndSelfTo<SaveYGController>().AsSingle().NonLazy();
            
            Container.Bind<ClickableObject>().FromInstance(_clickObj).AsSingle().NonLazy();
            Container.Bind<ClickInfoShower>().FromInstance(_clickInfo).AsSingle().NonLazy();

            Container.BindInterfacesAndSelfTo<ShopModel>().AsSingle().NonLazy();
            Container.Bind<IShopView>().FromInstance(_shopView).AsSingle().NonLazy();
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

            if (_chestOpenView != null)
                Container.Bind<IChestOpenView>().FromInstance(_chestOpenView).AsSingle().NonLazy();
            else
                Container.Bind<IChestOpenView>().To<ChestOpenNullView>().AsSingle().NonLazy();

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
            Container.BindInterfacesAndSelfTo<OfflineIncomeActivityTracker>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<GameStartRouter>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<QuestService>().AsSingle().NonLazy();
        }
    }
}
