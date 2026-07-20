using UnityEngine;

namespace Customization
{
    public class CustomizationElementViewData
    {
        public string Id;
        public CustomizationItemType Type;
        public Sprite Sprite;
        public string Title;
        public string Description;
        public Sprite PriceIcon;
        public string Price;
        public bool IsPurchased;
        public bool IsSelected;
        public bool CanBuy;
        public bool IsRewardOnly;
    }
}
