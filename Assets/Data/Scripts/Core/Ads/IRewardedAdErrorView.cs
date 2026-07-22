namespace Core.Ads
{
    public interface IRewardedAdErrorView
    {
        void Show();
        void Show(string title, string message);
        void Hide();
    }
}
