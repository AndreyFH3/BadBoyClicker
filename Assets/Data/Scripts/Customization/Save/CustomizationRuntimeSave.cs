using System;
using System.Collections.Generic;
using Core;

namespace Customization
{
    public class CustomizationRuntimeSave : ISavable<CustomizationSaveData>
    {
        private readonly HashSet<string> _purchasedBackgrounds = new();
        private readonly HashSet<string> _purchasedCats = new();
        private readonly HashSet<string> _unseenBackgrounds = new();
        private readonly HashSet<string> _unseenCats = new();

        public string ActiveBackgroundId { get; private set; }
        public string ActiveCatId { get; private set; }

        public event Action Changed;

        public bool IsPurchased(CustomizationItemType type, string id)
        {
            return GetPurchasedSet(type).Contains(id);
        }

        public bool HasUnseen(CustomizationItemType type)
        {
            return GetUnseenSet(type).Count > 0;
        }

        public void MarkUnseen(CustomizationItemType type, string id)
        {
            if (!string.IsNullOrEmpty(id) && GetUnseenSet(type).Add(id))
            {
                Changed?.Invoke();
            }
        }

        public void MarkSeen(CustomizationItemType type)
        {
            HashSet<string> unseen = GetUnseenSet(type);
            if (unseen.Count == 0)
            {
                return;
            }

            unseen.Clear();
            Changed?.Invoke();
        }

        public void AddPurchased(CustomizationItemType type, string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return;
            }

            if (GetPurchasedSet(type).Add(id))
            {
                Changed?.Invoke();
            }
        }

        public void SetActive(CustomizationItemType type, string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return;
            }

            if (type == CustomizationItemType.Background)
            {
                if (ActiveBackgroundId == id)
                {
                    return;
                }

                ActiveBackgroundId = id;
            }
            else
            {
                if (ActiveCatId == id)
                {
                    return;
                }

                ActiveCatId = id;
            }

            Changed?.Invoke();
        }

        public void Set(CustomizationSaveData data)
        {
            _purchasedBackgrounds.Clear();
            _purchasedCats.Clear();
            _unseenBackgrounds.Clear();
            _unseenCats.Clear();

            if (data != null)
            {
                AddRange(_purchasedBackgrounds, data.PurchasedBackgroundIds);
                AddRange(_purchasedCats, data.PurchasedCatIds);
                AddRange(_unseenBackgrounds, data.UnseenBackgroundIds);
                AddRange(_unseenCats, data.UnseenCatIds);
                ActiveBackgroundId = data.ActiveBackgroundId;
                ActiveCatId = data.ActiveCatId;
            }
            else
            {
                ActiveBackgroundId = null;
                ActiveCatId = null;
            }

            Changed?.Invoke();
        }

        public CustomizationSaveData Get()
        {
            return new CustomizationSaveData
            {
                ActiveBackgroundId = ActiveBackgroundId,
                ActiveCatId = ActiveCatId,
                PurchasedBackgroundIds = ToArray(_purchasedBackgrounds),
                PurchasedCatIds = ToArray(_purchasedCats),
                UnseenBackgroundIds = ToArray(_unseenBackgrounds),
                UnseenCatIds = ToArray(_unseenCats)
            };
        }

        private HashSet<string> GetPurchasedSet(CustomizationItemType type)
        {
            return type == CustomizationItemType.Background ? _purchasedBackgrounds : _purchasedCats;
        }

        private HashSet<string> GetUnseenSet(CustomizationItemType type)
        {
            return type == CustomizationItemType.Background ? _unseenBackgrounds : _unseenCats;
        }

        private void AddRange(HashSet<string> target, string[] ids)
        {
            if (ids == null)
            {
                return;
            }

            foreach (string id in ids)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    target.Add(id);
                }
            }
        }

        private string[] ToArray(HashSet<string> values)
        {
            var result = new string[values.Count];
            values.CopyTo(result);
            return result;
        }
    }
}
