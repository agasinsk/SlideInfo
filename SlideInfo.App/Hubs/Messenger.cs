using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace SlideInfo.App.Hubs
{
    [HubName("messenger")]
    public class Messenger : Hub<IMessenger>
    {
        public void Send(string name, string message)
        {
            // Call the addNewMessageToPage method to update clients.
            Clients.All.addNewMessageToPage(name, message);
        }

        public void SendMessageToAll(string userName, string message)
        {
            // Broad cast message
            Clients.All.messageReceived(userName, message);
        }
    }
}

