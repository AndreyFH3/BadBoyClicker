using System;
using System.Collections.Generic;

namespace Customization
{
    public class CustomizationNullView : ICustomizationView
    {
        public event Action<CustomizationItemType, string> ItemClicked;

        public void SetData(List<CustomizationElementViewData> data)
        {
        }
    }
}
