using System;
using CardCollections;
using GameLocalization;
using Zenject;

namespace CardCollectionMVP
{
    public class CardCollectionCardsPresenter : IInitializable, IDisposable
    {
        private ICardCollectionService _service;
        private ICardCollectionCardsView _view;
        private CardCollectionSelectionModel _selectionModel;

        [Inject]
        public void Construct(
            ICardCollectionService service,
            ICardCollectionCardsView view,
            CardCollectionSelectionModel selectionModel)
        {
            _service = service;
            _view = view;
            _selectionModel = selectionModel;
        }

        public void Initialize()
        {
            _view.CloseRequested += OnCloseRequested;
            _view.CollectRequested += OnCollectRequested;
            _selectionModel.SelectedCollectionChanged += OnSelectedCollectionChanged;
            _service.CollectionChanged += OnCollectionChanged;
            _service.CardChanged += OnCardChanged;
            _service.Changed += UpdateSelectedCollection;
            Localization.LanguageChanged += UpdateSelectedCollection;

            UpdateSelectedCollection();
        }

        public void Dispose()
        {
            _view.CloseRequested -= OnCloseRequested;
            _view.CollectRequested -= OnCollectRequested;
            _selectionModel.SelectedCollectionChanged -= OnSelectedCollectionChanged;
            _service.CollectionChanged -= OnCollectionChanged;
            _service.CardChanged -= OnCardChanged;
            _service.Changed -= UpdateSelectedCollection;
            Localization.LanguageChanged -= UpdateSelectedCollection;
        }

        private void OnSelectedCollectionChanged(string collectionId)
        {
            UpdateSelectedCollection();
        }

        private void UpdateSelectedCollection()
        {
            if (string.IsNullOrEmpty(_selectionModel.SelectedCollectionId))
            {
                _view.SetVisible(false);
                return;
            }

            CardCollectionViewData collection = _service.GetViewData(_selectionModel.SelectedCollectionId);
            if (collection == null)
            {
                _view.SetVisible(false);
                return;
            }

            _view.SetVisible(true);
            _view.SetData(collection);
        }

        private void OnCollectionChanged(CardCollectionViewData collection)
        {
            if (collection == null || collection.Id != _selectionModel.SelectedCollectionId)
            {
                return;
            }

            _view.SetData(collection);
        }

        private void OnCardChanged(CardViewData card)
        {
            if (card == null || card.CollectionId != _selectionModel.SelectedCollectionId)
            {
                return;
            }

            _view.UpdateCard(card);
        }

        private void OnCloseRequested()
        {
            _selectionModel.Clear();
        }

        private void OnCollectRequested()
        {
            _service.TryClaimReward(_selectionModel.SelectedCollectionId);
        }
    }
}
