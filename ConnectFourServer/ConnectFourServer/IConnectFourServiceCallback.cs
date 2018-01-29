using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ConnectFourServer
{
    [ServiceContract]
    public interface IConnectFourServiceCallback
    {
        //this function gets updated list of clients that is currently connected and it send it to the client
        [OperationContract(IsOneWay = true)]
        void addUsersToList(IEnumerable<string> newUser);

        [OperationContract(IsOneWay = true)]
        void removeUsersFromList(string connclients);

        // this function gets as parameter user name that want to start game and it send it to the
        //opponent user
        [OperationContract]
        bool sendGameRequestToUser(string fromUser);

        [OperationContract(IsOneWay = true)]
        void updateCell(int row, int col, MOVE_RESULT move_result);

        [OperationContract(IsOneWay = true)]
        void sendGameId(int gameId);

        [OperationContract(IsOneWay = true)]
        void annouceWinnerBecauseOtherPlayerLeft();

    }
}
