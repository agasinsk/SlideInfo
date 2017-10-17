using System.Collections.Generic;
using SlideInfo.App.Models;

namespace SlideInfo.App.Hubs
{
    public interface IMessenger
    {
        void onConnected(IEnumerable<string> ConnectedUsers);

        void onNewUserConnected(string userName);
        void onUserDisconnected(string userName);

        void addNewMessageToPage(string messageJson);
    }
}