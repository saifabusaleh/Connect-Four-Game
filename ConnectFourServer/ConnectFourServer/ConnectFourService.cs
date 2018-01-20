using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using ConnectFourDBCore;
using System.Threading;

namespace ConnectFourServer
{
    [ServiceBehavior(
    InstanceContextMode = InstanceContextMode.Single,
    ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ConnectFourService : IConnectFourService
    {
        private ConnectFourDBService cs = new ConnectFourDBService();
        private SortedDictionary<string, IConnectFourServiceCallback> clients;

        public ConnectFourService()
        {
            clients = new SortedDictionary<string, IConnectFourServiceCallback>();
        }


        public bool login(string username, string password)
        {
            var loginResult = cs.CheckIfValidLogin(username, password);
            return loginResult;
        }

        private void UpdateClientsListsThreadingFunction()
        {
            foreach (var callback in clients.Values)
            {
                callback.UpdateClientsList(clients.Keys);
            }
        }

        public void register(string username, string password)
        {
            bool isRegisterSuccess = cs.RegisterUser(username, password);
            if (isRegisterSuccess == false)
            {
                UserExistsFault fault = new UserExistsFault()
                { Message = "Username " + username + " already exists" };
                throw new FaultException<UserExistsFault>(fault);
            }
        }

        public IEnumerable<String> getConnectedUsersByUsernameThatWaitingForPartner(string username)
        {
            return cs.getConnectedUsersByUsernameThatWaitingForPartner(username);
        }

        public void Connect(string username)
        {
            if (clients.ContainsKey(username))
            {
                UserAlreadyLoggedInFault fault = new UserAlreadyLoggedInFault()
                { Message = "Username " + username + " already logged in, exiting.." };
                throw new FaultException<UserAlreadyLoggedInFault>(fault);
            }

            IConnectFourServiceCallback callback =
OperationContext.Current.GetCallbackChannel<IConnectFourServiceCallback>();
            clients.Add(username, callback);
            Thread updateThread = new Thread(UpdateClientsListsThreadingFunction);
            updateThread.Start();
        }

        public void Disconnect(string userName)
        {
            if(!clients.ContainsKey(userName))
            {
                UserNotFoundFault fault = new UserNotFoundFault()
                { Message = "Username " + userName + " is not found!" };
                throw new FaultException<UserNotFoundFault>(fault);
            }
            clients.Remove(userName);
            Thread updateThread = new Thread(UpdateClientsListsThreadingFunction);
            updateThread.Start();
        }

        public void SendRequestForGameToUser(string opponentUserName, string myUserName)
        {
            foreach (KeyValuePair<string, IConnectFourServiceCallback> client in clients)
            {
                if (client.Key == opponentUserName)
                {
                    client.Value.sendGameRequestToUser(myUserName);
                    return;
                }
            }
            // if user not found
            UserNotFoundFault fault = new UserNotFoundFault()
            { Message = "Username " + opponentUserName + " is not found!" };
            throw new FaultException<UserNotFoundFault>(fault);
        }

        public void SendRejectForGameToUser(string opponentUserName)
        {
            foreach (KeyValuePair<string, IConnectFourServiceCallback> client in clients)
            {
                if (client.Key == opponentUserName)
                {
                    client.Value.sendRejectRequestToUser();
                    return;
                }
                
            }
            // if user not found
            UserNotFoundFault fault = new UserNotFoundFault()
            { Message = "Username " + opponentUserName + " is not found!" };
            throw new FaultException<UserNotFoundFault>(fault);
        }
    }
}
