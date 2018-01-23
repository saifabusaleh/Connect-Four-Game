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

        [FaultContract(typeof(UserAlreadyLoggedInFault))]
        [OperationContract]
        void Connect(string username);

        [FaultContract(typeof(UserNotFoundFault))]
        [OperationContract]
        void Disconnect(string userName);

        [FaultContract(typeof(UserNotFoundFault))]
        [OperationContract]
        bool SendRequestForGameToUser(string opponentUserName, string myUserName);

        //Game
        [OperationContract]
        void InitGame(string player1, string player2);

        [OperationContract]
        bool IsMyTurn(string playerName);

        //returns the row where the circle inserted
        [OperationContract]
        int Insert(int column, string playerName);

        [OperationContract]
        bool checkIfIWin(string playerName, int row, int col);
    }
}
