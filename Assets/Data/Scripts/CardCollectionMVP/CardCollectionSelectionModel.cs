using System;

namespace CardCollectionMVP
{
    public class CardCollectionSelectionModel
    {
        public string SelectedCollectionId { get; private set; }

        public event Action<string> SelectedCollectionChanged;

        public void Select(string collectionId)
        {
            if (SelectedCollectionId == collectionId)
            {
                return;
            }

            SelectedCollectionId = collectionId;
            SelectedCollectionChanged?.Invoke(SelectedCollectionId);
        }

        public void Clear()
        {
            Select(null);
        }
    }
}
