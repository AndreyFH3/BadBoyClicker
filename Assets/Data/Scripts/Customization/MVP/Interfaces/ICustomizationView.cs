using System;
using System.Collections.Generic;

namespace Customization
{
    public interface ICustomizationView
    {
        event Action<CustomizationItemType, string> ItemClicked;
        event Action<CustomizationItemType> TabOpened;

        void SetData(List<CustomizationElementViewData> data);
    }
}
