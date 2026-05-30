using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Shop.Container
{

    public class ShopPostions : IInitializable
    {
        private List<ShopPositionLevelConainer> _positions;

        public System.Action OnChange;

        public void Initialize()
        {
            _positions.ForEach(x=>x.OnLevelChange += () => OnChange?.Invoke());
        }

        public void AddLevel(string id)
        {
            var position = _positions.Find(x => x.Id.Equals(id));
            if(position is null)
            {
                position = new(id);
                _positions.Add(position);
                position.OnLevelChange += () => OnChange?.Invoke();
            }
            else
            {
                position.AddLevel();
            }
        }
    }

    [Serializable]
    public class ShopPositionLevelConainer
    {
        [SerializeField] private string _id;
        [SerializeField] private int _level;

        public System.Action OnLevelChange;

        public string Id => _id;
        public int Level => _level;

        public ShopPositionLevelConainer(string id)
        {
            _id = id;
            _level = 1;
        }

        public ShopPositionLevelConainer(string id, int level)
        {
            _id = id;
            _level = level;
        }

        public void AddLevel()
        {
            _level++;
            OnLevelChange?.Invoke();
        }
    }
}