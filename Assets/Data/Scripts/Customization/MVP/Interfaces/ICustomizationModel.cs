using System;
using System.Collections.Generic;

namespace Customization
{
    public interface ICustomizationModel
    {
        bool IsOpen { get; }
        event Action StateChanged;

        List<CustomizationElementViewData> GetAllData();
        void Open();
        void Close();
        void BuyOrSelect(CustomizationItemType type, string id);
    }
}
