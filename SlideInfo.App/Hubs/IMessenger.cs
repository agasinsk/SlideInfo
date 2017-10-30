using System.Collections.Generic;
using SlideInfo.App.Models;

namespace SlideInfo.App.Hubs
{
    public interface IMessenger
    {
        void onConnected(IEnumerable<string> connectedUsers);

        void onNewUserConnected(string userId);
        void onUserDisconnected(string userId);

        void onUserTyping(string fromUserId);
        void addNewMessageToPage(string messageJson);
    }
}