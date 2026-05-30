namespace PlayerFeatures
{
    public interface IPlayerFeatureUnlockService
    {
        event System.Action<PlayerFeatureType> FeatureUnlocked;

        bool IsUnlocked(PlayerFeatureType feature);
        int GetRequiredLevel(PlayerFeatureType feature);
    }
}
