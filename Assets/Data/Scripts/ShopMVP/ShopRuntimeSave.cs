using System;
using System.Collections.Generic;
using Core;
using PlayerProgression;
using Zenject;

namespace Shop
{
    public class ShopRuntimeSave : IShopRuntimeSave, ISavable<ShopRuntimeSave.SaveData>
    {
        private readonly Dictionary<string, int> _levels = new();
        private IPlayerProgressionService _playerProgression;

        public long ClickValue { get; private set; } = 1;
        public long AutoIncomePerSecond { get; private set; }
        public event Action Changed;

        [Inject]
        public void Construct(IPlayerProgressionService playerProgression)
        {
            _playerProgression = playerProgression;
        }

        public int GetLevel(ShopItemType type, string id)
        {
            return _levels.TryGetValue(GetKey(type, id), out int level) ? level : 0;
        }

        public void AddLevel(ShopItemType type, string id)
        {
            string key = GetKey(type, id);
            _levels[key] = GetLevel(type, id) + 1;
            Changed?.Invoke();
        }

        public void ResetLevels()
        {
            if (_levels.Count == 0)
            {
                return;
            }

            _levels.Clear();
            Changed?.Invoke();
        }

        public void Recalculate(GameConfig config)
        {
            if (config == null)
            {
                ClickValue = 1;
                AutoIncomePerSecond = 0;
                return;
            }

            ClickValue = ApplyMultiplier(1 + CalculateBonus(config.Clicks, ShopItemType.Click), _playerProgression?.ClickIncomeMultiplier ?? 1f);
            AutoIncomePerSecond = ApplyMultiplier(CalculateBonus(config.AutoBuys, ShopItemType.AutoBuy), _playerProgression?.PassiveIncomeMultiplier ?? 1f);
        }

        private long ApplyMultiplier(long value, float multiplier)
        {
            if (value <= 0)
            {
                return 0;
            }

            return Math.Max(1, (long)Math.Ceiling(value * Math.Max(0f, multiplier)));
        }

        private long CalculateBonus(IReadOnlyList<GameConfig.ShopDataClick> items, ShopItemType type)
        {
            if (items == null)
            {
                return 0;
            }

            long value = 0;
            foreach (var item in items)
            {
                if (item == null)
                {
                    continue;
                }

                value += item.BaseBonus * GetLevel(type, item.Id);
            }

            return value;
        }

        private string GetKey(ShopItemType type, string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Shop item id cannot be empty.", nameof(id));
            }

            return $"{type}:{id}";
        }

        public void Set(SaveData data)
        {
            _levels.Clear();

            if (data.Levels == null)
            {
                Changed?.Invoke();
                return;
            }

            foreach (var item in data.Levels)
            {
                if (string.IsNullOrEmpty(item.Key))
                {
                    continue;
                }

                _levels[item.Key] = item.Level;
            }

            Changed?.Invoke();
        }

        public SaveData Get()
        {
            var levels = new ShopItemLevel[_levels.Count];
            int index = 0;

            foreach (var pair in _levels)
            {
                levels[index] = new ShopItemLevel
                {
                    Key = pair.Key,
                    Level = pair.Value
                };

                index++;
            }

            return new SaveData
            {
                Levels = levels
            };
        }

        [Serializable]
        public struct SaveData
        {
            public ShopItemLevel[] Levels;
        }

        [Serializable]
        public struct ShopItemLevel
        {
            public string Key;
            public int Level;
        }
    }
}
