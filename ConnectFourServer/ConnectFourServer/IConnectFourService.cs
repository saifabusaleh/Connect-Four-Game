using ConnectFourDBCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ConnectFourServer
{
    [ServiceContract(CallbackContract = typeof(IConnectFourServiceCallback))]
    public interface IConnectFourService
    {

        //Login and register functions


        [FaultContract(typeof(UserExistsFault))]
        [OperationContract]
        void register(string username, string password);

        [OperationContract]
        bool login(string username, string password);

        //Connection functions
        [FaultContract(typeof(UserAlreadyLoggedInFault))]
        [OperationContract]
        void Connect(string username);

        [FaultContract(typeof(UserNotFoundFault))]
        [OperationContract]
        void Disconnect(string userName);




        //Getters
        [OperationContract]
        IEnumerable<PlayersDetails> getPlayers();

        [OperationContract]
        IEnumerable<GameDetails> getGames();

        [OperationContract]
        IEnumerable<PlayingGames> getCurrentGames();

        //Game Functions

        [FaultContract(typeof(UserNotFoundFault))]
        [OperationContract]
        bool SendRequestForGameToUser(string opponentUserName, string myUserName);

        [OperationContract]
        void InitGame(string player1, string player2);

        [OperationContract]
        bool IsMyTurn(string playerName);

        //returns the row where the circle inserted
        [OperationContract]
        InsertResult Insert(int column, string playerName);

        //[OperationContract]
        //bool checkIfIWin(string playerName, int row, int col);
    }
}
