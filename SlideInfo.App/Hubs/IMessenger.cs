using System.Collections.Generic;
using SlideInfo.App.Models;

namespace SlideInfo.App.Hubs
{
    public interface IMessenger
    {
        void onConnected(string id, string userName, List<string> ConnectedUsers);

        void onNewUserConnected(string id, string userName);
        void onUserDisconnected(string id, string userName);

        void sendPrivateMessage(string id, string message);

        void messageReceived(string name, string message);
        void addNewMessageToPage(string name, string message);
    }
}