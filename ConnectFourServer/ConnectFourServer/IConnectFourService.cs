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

        [FaultContract(typeof(UserNotFoundFault))]
        [OperationContract]
        PlayersDetails getPlayerDetails(string username);

        [OperationContract]
        IEnumerable<GameDetails> getGames();

        [OperationContract]
        IEnumerable<PlayingGames> getCurrentGames();

        //Game Functions

        [FaultContract(typeof(UserNotFoundFault))]
        [OperationContract]
        bool SendRequestForGameToUser(string opponentUserName, string myUserName);

        [OperationContract]
        int InitGame(string player1, string player2);

        [FaultContract(typeof(UserNotFoundFault))]
        [OperationContract]
        bool IsMyTurn(string playerName, int gameId);

        //returns the row where the circle inserted
        [FaultContract(typeof(UserNotFoundFault))] 
        [OperationContract]
        InsertResult Insert(int column, string playerName, int gameId);

        //if the player decided to give up the game while playing
        [OperationContract]
        void GiveupGame(string playerName, int gameId);
    }
}
