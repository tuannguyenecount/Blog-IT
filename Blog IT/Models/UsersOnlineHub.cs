using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Blog_IT.Models
{
    [HubName("usersOnlineHub")]
    public class UsersOnlineHub : Hub
    {
        public static List<string> Users = new List<string>();
        //public static int dem = ;
        /// <summary>
        /// Sends the update user count to the listening view.
        /// </summary>
        /// <param name="count">
        /// The count.
        /// </param>
        public void Send(int count)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<UsersOnlineHub>();
            context.Clients.All.updateUsersOnlineCount(count);

        }
        /// <summary>
        /// The OnConnected event.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// 
        public override Task OnConnected()
        {

            string clientID = GetClientId();

            if (Users.IndexOf(clientID) == -1)
            {
                Users.Add(clientID);
            }

            Send(Users.Count);
            return base.OnConnected();
        }
        /// <summary>
        /// The OnReconnected event.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        ///
        public override Task OnReconnected()
        {
            string clientID = GetClientId();
            if (Users.IndexOf(clientID) == -1)
            {
                Users.Add(clientID);
               
            }

            Send(Users.Count);

            return base.OnReconnected();
        }

        /// <summary>
        /// The OnDisconnected event.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// 
        public override Task OnDisconnected(bool stopCalled)
        {
            string clientId = GetClientId();

            if (Users.IndexOf(clientId) != -1)
            {
                Users.Remove(clientId);
               
            }
            // Send the current count of users
            Send(Users.Count);
            return base.OnDisconnected(stopCalled);
        }
        /// <summary>
        /// Get's the currently connected Id of the client.
        /// This is unique for each client and is used to identify
        /// a connection.
        /// </summary>
        /// <returns>The client Id.</returns>
        private string GetClientId()
        {
            string clientId = "";
            if (Context.QueryString["clientId"] != null)
            {
                // clientId passed from application 
                clientId = this.Context.QueryString["clientId"];
            }
            if (string.IsNullOrEmpty(clientId.Trim()))
            {
                clientId = Context.ConnectionId;
            }
            return clientId;
        }
    }
}