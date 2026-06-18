using System;
using System.Collections.Generic;

namespace Customization
{
    public interface ICustomizationView
    {
        event Action OpenRequested;
        event Action CloseRequested;
        event Action<CustomizationItemType, string> ItemClicked;

        bool IsActive { get; }

        void SetOpenState(bool isOpen);
        void SetData(List<CustomizationElementViewData> data);
    }
}
