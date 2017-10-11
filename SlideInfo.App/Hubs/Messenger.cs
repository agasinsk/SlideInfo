using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace SlideInfo.App.Hubs
{
    [Authorize]
    [HubName("messenger")]
    public class Messenger : Hub<IMessenger>
    {
        private static readonly ConnectionMapping<string> Connections =
            new ConnectionMapping<string>();

        public void Send(string who, string message)
        {
            var userName = Context.User.Identity.Name;

            foreach (var connectionId in Connections.GetConnections(who))
            {
                Clients.Client(connectionId).addNewMessageToPage(userName, message);
            }
        }

        public override Task OnConnected()
        {
            var userName = Context.User.Identity.Name;

            Connections.Add(userName, Context.ConnectionId);

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var userName = Context.User.Identity.Name;

            Connections.Remove(userName, Context.ConnectionId);

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

