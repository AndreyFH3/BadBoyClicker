using System;
using System.Collections.Generic;

namespace Customization
{
    public class CustomizationNullView : ICustomizationView
    {
        public event Action OpenRequested;
        public event Action CloseRequested;
        public event Action<CustomizationItemType, string> ItemClicked;

        public bool IsActive => false;

        public void SetOpenState(bool isOpen)
        {
        }

        public void SetData(List<CustomizationElementViewData> data)
        {
        }
    }
}
