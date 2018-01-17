using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using ConnectFourDBCore;
namespace ConnectFourServer
{
    [ServiceBehavior(
    InstanceContextMode = InstanceContextMode.Single,
    ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ConnectFourService : IConnectFourService
    {
        ConnectFourDBService cs = new ConnectFourDBService();

        public bool login(string username, string password)
        {
            return cs.CheckIfValidLogin(username, password);
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

        
    }
}
