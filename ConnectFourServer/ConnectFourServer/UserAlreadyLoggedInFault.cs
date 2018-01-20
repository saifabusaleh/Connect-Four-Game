using System.Runtime.Serialization;

namespace ConnectFourServer
{
    [DataContract]
    class UserAlreadyLoggedInFault
    {
        [DataMember]
        public string Message { get; set; }
    }
}