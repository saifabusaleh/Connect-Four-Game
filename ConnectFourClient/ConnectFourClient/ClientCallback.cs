using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConnectFourClient.ConnectFourService;

namespace ConnectFourClient
{
    public class ClientCallback : IConnectFourServiceCallback
    {

        public delegate void AddToClientListDelegate(string[] users);
        public event AddToClientListDelegate addUsers;

        public void addUsersToList(string[] users)
        {
             addUsers(users);
        }

        public delegate void RemoveFromClientListDelegate(string user);
        public event RemoveFromClientListDelegate removeUser;

        public void removeUsersFromList(string user)
        {
            removeUser(user);
        }

        public delegate bool SendGameRequestToUserDelegate(string users);
        public event SendGameRequestToUserDelegate sendGameRequestToUserFunc;

        public bool sendGameRequestToUser(string user)
        {
            return sendGameRequestToUserFunc(user);
        }

        public delegate void UpdateCell(int row, int col, MOVE_RESULT result);
        public event UpdateCell updateCellFunc;

        public void updateCell(int row, int col, MOVE_RESULT result)
        {
            updateCellFunc(row,col, result);
        }

        public delegate void SendGameInfo(InitGameResult game);
        public event SendGameInfo SendGameInfoFunc;

        public void sendGameInfo(InitGameResult game)
        {
            SendGameInfoFunc(game);
        }

        public delegate void AnnouceWinnerBecauseOtherPlayerLeft();
        public event AnnouceWinnerBecauseOtherPlayerLeft AnnouceWinnerBecauseOtherPlayerLeftFunc;

        public void annouceWinnerBecauseOtherPlayerLeft()
        {
            AnnouceWinnerBecauseOtherPlayerLeftFunc();
        }
    }
}
