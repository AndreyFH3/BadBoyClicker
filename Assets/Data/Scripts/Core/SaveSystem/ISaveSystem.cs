namespace Core
{    
    public interface ISaveSystem
    {
        bool IsLoaded { get; }
        event System.Action Loaded;

        void Load();
        void Save();
    }
}
