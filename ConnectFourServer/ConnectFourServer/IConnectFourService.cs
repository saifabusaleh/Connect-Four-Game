using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ConnectFourServer
{
    [ServiceContract(CallbackContract = typeof(IConnectFourServiceCallback))]
    public interface IConnectFourService
    {
        [FaultContract(typeof(UserExistsFault))]
        [OperationContract]
        void register(string username, string password);

        [OperationContract]
        bool login(string username, string password);

        [OperationContract]
        void updateClients(string username);

        [OperationContract]
        void Disconnect(string userName);

    }
}
