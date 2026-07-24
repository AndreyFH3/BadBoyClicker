using System;
using System.Collections.Generic;
using PlayerFeatures;
using PlayerProgression;
using Zenject;

namespace Tutorials
{
    public class TutorialModel : IInitializable, IDisposable
    {
        private readonly Queue<TutorialConfig.TutorialData> _pendingTutorials = new();

        private TutorialConfig _config;
        private IPlayerFeatureUnlockService _featureUnlockService;
        private IPlayerProgressionView _playerProgressionView;
        private ITutorialView _view;

        [Inject]
        public void Construct(
            TutorialConfig config,
            IPlayerFeatureUnlockService featureUnlockService,
            IPlayerProgressionView playerProgressionView,
            ITutorialView view)
        {
            _config = config;
            _featureUnlockService = featureUnlockService;
            _playerProgressionView = playerProgressionView;
            _view = view;
        }

        public void Initialize()
        {
            _featureUnlockService.FeatureUnlocked += OnFeatureUnlocked;
            _playerProgressionView.LevelUpResultClosed += OnLevelUpResultClosed;
        }

        public void Dispose()
        {
            _featureUnlockService.FeatureUnlocked -= OnFeatureUnlocked;
            _playerProgressionView.LevelUpResultClosed -= OnLevelUpResultClosed;
        }

        private void OnFeatureUnlocked(PlayerFeatureType feature)
        {
            if (_config.TryGet(feature, out TutorialConfig.TutorialData tutorial))
            {
                _pendingTutorials.Enqueue(tutorial);
            }
        }

        private void OnLevelUpResultClosed()
        {
            while (_pendingTutorials.Count > 0)
            {
                _view.Show(_pendingTutorials.Dequeue());
            }
        }
    }
}
