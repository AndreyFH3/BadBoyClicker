using Core;

namespace Shop
{
    public interface IShopRuntimeSave : ISavable<ShopRuntimeSave.SaveData>
    {
        long ClickValue { get; }
        long AutoIncomePerSecond { get; }
        event System.Action Changed;

        int GetLevel(ShopItemType type, string id);
        void AddLevel(ShopItemType type, string id);
        void ResetLevels();
        void Recalculate(Core.GameConfig config);
    }
}
