using System;
using System.Collections.Generic;

namespace Customization
{
    public interface ICustomizationModel
    {
        event Action StateChanged;

        bool HasAnyActionable(CustomizationItemType? type = null);
        void MarkSeen(CustomizationItemType type);
        List<CustomizationElementViewData> GetAllData();
        void BuyOrSelect(CustomizationItemType type, string id);
    }
}
