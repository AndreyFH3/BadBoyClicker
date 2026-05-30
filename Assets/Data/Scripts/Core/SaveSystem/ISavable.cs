namespace Core
{
    public interface ISavable<TSaveData>
    {
        void Set(TSaveData data);
        TSaveData Get();
    }
}
