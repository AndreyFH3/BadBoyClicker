using System;

namespace Customization
{
    [Serializable]
    public class CustomizationSaveData
    {
        public string ActiveBackgroundId;
        public string ActiveCatId;
        public string[] PurchasedBackgroundIds;
        public string[] PurchasedCatIds;
        public string[] UnseenBackgroundIds;
        public string[] UnseenCatIds;
    }
}
