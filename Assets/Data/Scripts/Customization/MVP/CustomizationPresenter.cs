using System;
using GameLocalization;
using Zenject;

namespace Customization
{
    public class CustomizationPresenter : IInitializable, IDisposable
    {
        private ICustomizationModel _model;
        private ICustomizationView _view;

        [Inject]
        public void Construct(ICustomizationModel model, ICustomizationView view)
        {
            _model = model;
            _view = view;
        }

        public void Initialize()
        {
            _model.StateChanged += UpdateView;
            _view.OpenRequested += OnOpenRequested;
            _view.CloseRequested += OnCloseRequested;
            _view.ItemClicked += OnItemClicked;
            Localization.LanguageChanged += ForceUpdateView;

            ForceUpdateView();
        }

        public void Dispose()
        {
            _model.StateChanged -= UpdateView;
            _view.OpenRequested -= OnOpenRequested;
            _view.CloseRequested -= OnCloseRequested;
            _view.ItemClicked -= OnItemClicked;
            Localization.LanguageChanged -= ForceUpdateView;
        }

        private void UpdateView()
        {
            _view.SetOpenState(_model.IsOpen);

            if (!_view.IsActive)
            {
                return;
            }

            _view.SetData(_model.GetAllData());
        }

        private void ForceUpdateView()
        {
            _view.SetOpenState(_model.IsOpen);
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

        private void OnItemClicked(CustomizationItemType type, string id)
        {
            _model.BuyOrSelect(type, id);
        }
    }
}
