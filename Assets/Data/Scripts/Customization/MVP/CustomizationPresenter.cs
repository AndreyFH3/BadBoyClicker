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
            _view.ItemClicked += OnItemClicked;
            Localization.LanguageChanged += UpdateView;

            UpdateView();
        }

        public void Dispose()
        {
            _model.StateChanged -= UpdateView;
            _view.ItemClicked -= OnItemClicked;
            Localization.LanguageChanged -= UpdateView;
        }

        private void UpdateView()
        {
            _view.SetData(_model.GetAllData());
        }

        private void OnItemClicked(CustomizationItemType type, string id)
        {
            _model.BuyOrSelect(type, id);
        }
    }
}
