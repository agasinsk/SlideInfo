using System.Collections.Generic;
using SlideInfo.App.Models;

namespace SlideInfo.App.Hubs
{
    public interface IMessenger
    {
        void onConnected(IEnumerable<string> ConnectedUsers);

        void onNewUserConnected(string userName);
        void onUserDisconnected(string userName);

        void sendPrivateMessage(string id, string message);

        void messageReceived(string name, string message);
        void addNewMessageToPage(string name, string message);
    }
}