using System;
using System.Collections.Generic;

namespace Customization
{
    public interface ICustomizationView
    {
        event Action<CustomizationItemType, string> ItemClicked;

        void SetData(List<CustomizationElementViewData> data);
    }
}
