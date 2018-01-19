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
            var loginResult =  cs.CheckIfValidLogin(username, password);
            if (loginResult == true)
            {
            //    updateClientsList(username);
            }
            return loginResult;
        }

        private void updateClientsList(string username)
        {
            IConnectFourServiceCallback callback =
OperationContext.Current.GetCallbackChannel<IConnectFourServiceCallback>();
            clients.Add(username, callback);
            Thread updateThread = new Thread(UpdateClientsListsThreadingFunction);
            updateThread.Start();
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
            if(isRegisterSuccess == false)
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

        public void updateClients(string username)
        {
            updateClientsList(username);
        }

        public void Disconnect(string userName)
        {
            clients.Remove(userName);
            Thread updateThread = new Thread(UpdateClientsListsThreadingFunction);
            updateThread.Start(); 
        }
    }
}
