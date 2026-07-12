using System;
using System.Collections.Generic;

namespace Customization
{
    public interface ICustomizationModel
    {
        event Action StateChanged;

        List<CustomizationElementViewData> GetAllData();
        void BuyOrSelect(CustomizationItemType type, string id);
    }
}
