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

        public delegate void SendGameId(int gameId);
        public event SendGameId SendGameIdFunc;

        public void sendGameId(int gameId)
        {
            SendGameIdFunc(gameId);
        }
    }
}
