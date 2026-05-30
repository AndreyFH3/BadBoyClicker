using Core;
using DailyLogin;
using DailyQuests;
using PlayerFeatures;
using QuestSystem;
using CardCollections;
using GameLocalization;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "ScriptableInstaller", menuName = "Installers/ScriptableInstaller")]
public class ScriptableInstaller : ScriptableObjectInstaller
{
    [SerializeField] protected GameConfig _config;
    [SerializeField] protected DailyLoginConfig _dailyLoginConfig;
    [SerializeField] protected PlayerFeatureUnlockConfig _featureUnlockConfig;
    [SerializeField] protected QuestConfig _questConfig;
    [SerializeField] protected DailyQuestConfig _dailyQuestConfig;
    [SerializeField] protected CardCollectionConfig _cardCollectionConfig;
    [SerializeField] protected LocalizationConfig _localizationConfig;
    
    public override void InstallBindings()
    {
        Container.Bind<GameConfig>().FromInstance(_config).AsSingle().NonLazy();

        DailyLoginConfig dailyLoginConfig = _dailyLoginConfig != null
            ? _dailyLoginConfig
            : ScriptableObject.CreateInstance<DailyLoginConfig>();

        Container.Bind<DailyLoginConfig>().FromInstance(dailyLoginConfig).AsSingle().NonLazy();

        PlayerFeatureUnlockConfig featureUnlockConfig = _featureUnlockConfig != null
            ? _featureUnlockConfig
            : ScriptableObject.CreateInstance<PlayerFeatureUnlockConfig>();

        Container.Bind<PlayerFeatureUnlockConfig>().FromInstance(featureUnlockConfig).AsSingle().NonLazy();

        QuestConfig questConfig = _questConfig != null
            ? _questConfig
            : ScriptableObject.CreateInstance<QuestConfig>();

        Container.Bind<QuestConfig>().FromInstance(questConfig).AsSingle().NonLazy();

        DailyQuestConfig dailyQuestConfig = _dailyQuestConfig != null
            ? _dailyQuestConfig
            : ScriptableObject.CreateInstance<DailyQuestConfig>();

        Container.Bind<DailyQuestConfig>().FromInstance(dailyQuestConfig).AsSingle().NonLazy();

        CardCollectionConfig cardCollectionConfig = _cardCollectionConfig != null
            ? _cardCollectionConfig
            : ScriptableObject.CreateInstance<CardCollectionConfig>();

        Container.Bind<CardCollectionConfig>().FromInstance(cardCollectionConfig).AsSingle().NonLazy();

        LocalizationConfig localizationConfig = _localizationConfig != null
            ? _localizationConfig
            : ScriptableObject.CreateInstance<LocalizationConfig>();

        Container.Bind<LocalizationConfig>().FromInstance(localizationConfig).AsSingle().NonLazy();
    }
}
