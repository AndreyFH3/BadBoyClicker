using UnityEngine;
using Zenject;

namespace Core
{
    public class Wallet : IInitializable, ISavable<Wallet.SaveData>
    {
        private long _softValue;
        private long _middleValue;
        private long _hardValue;

        public long Soft => _softValue;
        public long Middle => _middleValue;
        public long Hard => _hardValue;

        public System.Action OnChanged;
        public System.Action OnSoftChanged;
        public System.Action OnMiddleChanged;
        public System.Action OnHardChanged;
        public System.Action<long> SoftAdded;
        public System.Action<long> MiddleAdded;
        public System.Action<long> HardAdded;
        public System.Action<long> SoftSpent;
        public System.Action<long> MiddleSpent;
        public System.Action<long> HardSpent;

        public void Set(SaveData data) 
        {
            _softValue = data.Soft;
            _middleValue =  data.Middle;
            _hardValue =  data.Hard;

            OnSoftChanged?.Invoke();
            OnMiddleChanged?.Invoke();
            OnHardChanged?.Invoke();
            OnChanged?.Invoke();
        }

        public void Initialize()
        {
            OnSoftChanged?.Invoke();
            OnMiddleChanged?.Invoke();
            OnHardChanged?.Invoke();
            OnChanged?.Invoke();
        }

        public void AddSoft(long amount)
        {
            Add(ref _softValue, amount, OnSoftChanged, SoftAdded);
        }

        public void AddMiddle(long amount)
        {
            Add(ref _middleValue, amount, OnMiddleChanged, MiddleAdded);
        }

        public void AddHard(long amount)
        {
            Add(ref _hardValue, amount, OnHardChanged, HardAdded);
        }

        public bool CanSpendSoft(long amount)
        {
            return CanSpend(_softValue, amount);
        }

        public bool CanSpendMiddle(long amount)
        {
            return CanSpend(_middleValue, amount);
        }

        public bool CanSpendHard(long amount)
        {
            return CanSpend(_hardValue, amount);
        }

        public bool SpendSoft(long amount)
        {
            return Spend(ref _softValue, amount, OnSoftChanged, SoftSpent);
        }

        public void ResetSoft()
        {
            if (_softValue == 0)
            {
                return;
            }

            _softValue = 0;
            NotifyChanged(OnSoftChanged);
        }

        public bool SpendMiddle(long amount)
        {
            return Spend(ref _middleValue, amount, OnMiddleChanged, MiddleSpent);
        }

        public bool SpendHard(long amount)
        {
            return Spend(ref _hardValue, amount, OnHardChanged, HardSpent);
        }

        private bool CanSpend(long currentValue, long amount)
        {
            ValidateAmount(amount);
            return currentValue >= amount;
        }

        private void Add(ref long currentValue, long amount, System.Action currencyChanged, System.Action<long> added)
        {
            ValidateAmount(amount);
            if (amount == 0)
            {
                return;
            }

            currentValue += amount;
            NotifyChanged(currencyChanged);
            added?.Invoke(amount);
        }

        private bool Spend(ref long currentValue, long amount, System.Action currencyChanged, System.Action<long> spent)
        {
            ValidateAmount(amount);
            if (amount == 0)
            {
                return true;
            }

            if (currentValue < amount)
            {
                return false;
            }

            currentValue -= amount;
            NotifyChanged(currencyChanged);
            spent?.Invoke(amount);
            return true;
        }

        private void NotifyChanged(System.Action currencyChanged)
        {
            currencyChanged?.Invoke();
            OnChanged?.Invoke();
        }

        private void ValidateAmount(long amount)
        {
            if (amount < 0)
            {
                Debug.Log($"{nameof(amount)}, {amount}, Amount cannot be negative.");
            }
        }

        public SaveData Get()
        {
            return new()
            {
              Soft = _softValue,
              Middle = _middleValue,
              Hard = _hardValue  
            };
        }

        [System.Serializable]
        public struct SaveData
        {
            public long Soft;
            public long Middle;
            public long Hard; 
        }
    }
}
