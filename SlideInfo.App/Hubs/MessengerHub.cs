using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;
using SlideInfo.App.Models;

namespace SlideInfo.App.Hubs
{
    [Authorize]
    [HubName("messenger")]
    public class MessengerHub : Hub<IMessenger>
    {
        private static readonly ConnectionMapping<string> Connections =
            new ConnectionMapping<string>();

        public void Send(string messageJson)
        {
            Console.WriteLine(messageJson);

            var message = JsonConvert.DeserializeObject<Message>(messageJson);

            //TODO: add message to db
            //sending message to receiver connections
            foreach (var connectionId in Connections.GetConnections(message.ToId))
            {
                Clients.Client(connectionId).addNewMessageToPage(messageJson);
            }
        }

        public override Task OnConnected()
        {
            var userName = Context.User.Identity.Name;

            Connections.Add(userName, Context.ConnectionId);
            Clients.Caller.onConnected(Connections.GetKeys());
            Clients.AllExcept(Context.ConnectionId).onNewUserConnected(userName);

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var userName = Context.User.Identity.Name;

            Connections.Remove(userName, Context.ConnectionId);
            Clients.AllExcept(Context.ConnectionId).onUserDisconnected(userName);

            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            var userName = Context.User.Identity.Name;

            if (!Connections.GetConnections(userName).Contains(Context.ConnectionId))
            {
                Connections.Add(userName, Context.ConnectionId);
            }

            return base.OnReconnected();
        }
    }
}

