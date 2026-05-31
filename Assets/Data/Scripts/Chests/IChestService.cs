using System;
using DailyLogin;

namespace Chests
{
    public interface IChestService : IChestRewardService
    {
        event Action<ChestOpenResult> ChestOpened;

        ChestConfig.ChestData GetChest(string chestId);
        bool TryOpenChest(string chestId, out ChestOpenResult result);
    }
}
