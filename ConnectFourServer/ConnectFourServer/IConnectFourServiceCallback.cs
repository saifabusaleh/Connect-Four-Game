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
        [OperationContract(IsOneWay = true)]
        void UpdateClientsList(IEnumerable<string> users);
    }
}
