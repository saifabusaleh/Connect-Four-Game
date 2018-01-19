using System.Runtime.Serialization;

namespace ConnectFourServer
{
    [DataContract]
    class UserNotFoundFault
    {
        [DataMember]
        public string Message { get; set; }
    }
}