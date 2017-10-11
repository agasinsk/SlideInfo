using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.EntityFrameworkCore;
using SlideInfo.App.Data;
using SlideInfo.App.Models;

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

        public void SendChatMessage(string who, string message)
        {
            var name = Context.User.Identity.Name;
            using (var db = new SlideInfoDbContext())
            {
                var user = db.Users.Find(who);
                if (user == null) return;

                db.Entry(user)
                    .Collection(u => u.Connections)
                    .Query()
                    .Where(c => c.IsConnected == true)
                    .Load();

                if (user.Connections == null) return;
                foreach (var connection in user.Connections)
                {
                    Clients.Client(connection.Id)
                        .messageReceived(name, message);
                }
            }
        }

        public override Task OnConnected()
        {
            var name = Context.User.Identity.Name;
            using (var db = new SlideInfoDbContext())
            {
                var user = db.AppUsers
                    .Include(u => u.Connections)
                    .SingleOrDefault(u => u.UserName == name);

                if (user != null)
                {
                    user.Connections.Add(new Connection
                    {
                        Id = Context.ConnectionId,
                        UserAgent = Context.Request.Headers["User-Agent"],
                        IsConnected = true
                    });
                    db.SaveChanges();
                }
                return base.OnConnected();
            }
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            using (var db = new SlideInfoDbContext())
            {
                var connection = db.Connections.Find(Context.ConnectionId);
                connection.IsConnected = false;
                db.SaveChanges();
            }
            return base.OnDisconnected(stopCalled);
        }
    }
}

