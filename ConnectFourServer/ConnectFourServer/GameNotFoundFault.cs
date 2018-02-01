using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace ConnectFourServer
{
    [DataContract]
    class GameNotFoundFault
    {
        [DataMember]
        public string Message { get; set; }
    }
}
