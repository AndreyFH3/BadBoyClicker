using System;
using CardCollections;
using Zenject;

namespace CardCollectionMVP
{
    public class CardCollectionsPresenter : IInitializable, IDisposable
    {
        private ICardCollectionService _service;
        private ICardCollectionsView _view;
        private CardCollectionSelectionModel _selectionModel;

        [Inject]
        public void Construct(
            ICardCollectionService service,
            ICardCollectionsView view,
            CardCollectionSelectionModel selectionModel)
        {
            _service = service;
            _view = view;
            _selectionModel = selectionModel;
        }

        public void Initialize()
        {
            _service.Changed += UpdateView;
            _service.CollectionChanged += OnCollectionChanged;
            _view.CollectionSelected += OnCollectionSelected;
            _selectionModel.SelectedCollectionChanged += OnSelectedCollectionChanged;

            UpdateView();
        }

        public void Dispose()
        {
            _service.Changed -= UpdateView;
            _service.CollectionChanged -= OnCollectionChanged;
            _view.CollectionSelected -= OnCollectionSelected;
            _selectionModel.SelectedCollectionChanged -= OnSelectedCollectionChanged;
        }

        private void UpdateView()
        {
            bool isUnlocked = _service.IsUnlocked;
            _view.SetVisible(isUnlocked && string.IsNullOrEmpty(_selectionModel.SelectedCollectionId));

            if (!isUnlocked)
            {
                _selectionModel.Clear();
                return;
            }

            var collections = _service.GetAllViewData();
            _view.SetData(collections);
        }

        private void OnCollectionChanged(CardCollectionViewData collection)
        {
            if (!_service.IsUnlocked)
            {
                return;
            }

            _view.UpdateCollection(collection);
        }

        private void OnCollectionSelected(string collectionId)
        {
            if (!_service.IsUnlocked || _service.GetViewData(collectionId) == null)
            {
                return;
            }

            _selectionModel.Select(collectionId);
        }

        private void OnSelectedCollectionChanged(string collectionId)
        {
            _view.SetVisible(_service.IsUnlocked && string.IsNullOrEmpty(collectionId));
        }
    }
}
