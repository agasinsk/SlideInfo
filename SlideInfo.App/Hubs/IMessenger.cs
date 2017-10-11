namespace SlideInfo.App.Hubs
{
    public interface IMessenger
    {
        void messageReceived(string name, string message);
        void addNewMessageToPage(string name, string message);
    }
}