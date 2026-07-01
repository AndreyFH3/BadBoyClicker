using System;
using UnityEngine;

namespace Shop
{
    /// <summary>
    /// Common base for every shop card. Owns the id/click plumbing so that
    /// <see cref="ShopView"/> can store soft-currency and paid offers in the same
    /// collection while each concrete element controls its own presentation.
    /// </summary>
    public abstract class ShopViewElementBase : MonoBehaviour
    {
        public Action<string> OnClick;

        private string _id;

        public void Init(ShopElementData data)
        {
            if (data == null)
                return;

            _id = data.Id;
            Apply(data);
        }

        protected abstract void Apply(ShopElementData data);

        protected void RaiseClick()
        {
            OnClick?.Invoke(_id);
        }
    }
}
