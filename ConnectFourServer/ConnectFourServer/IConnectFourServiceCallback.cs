using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ConnectFourServer
{
    [ServiceContract]
    interface IConnectFourServiceCallback
    {
        //this function gets updated list of clients that is currently connected and it send it to the client
        [OperationContract(IsOneWay = true)]
        void UpdateClientsList(IEnumerable<string> users);


        // this function gets as parameter user name that want to start game and it send it to the
        //opponent user
        [OperationContract(IsOneWay = true)]
        void sendGameRequestToUser(string fromUser);
    }
}
