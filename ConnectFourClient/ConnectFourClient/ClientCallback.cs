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

        public delegate void UpdateListDelegate(string[] users);
        public event UpdateListDelegate updateUsers;

        public void UpdateClientsList(string[] users)
        {
            updateUsers(users);
        }

        public delegate void SendGameRequestToUserDelegate(string users);
        public event SendGameRequestToUserDelegate sendGameRequestToUserFunc;

        public void sendGameRequestToUser(string user)
        {
            sendGameRequestToUserFunc(user);
        }

        public delegate void SendAcceptRequest();
        public event SendAcceptRequest sendAcceptRequestToUserFunc;

        public void sendAcceptRequestToUser()
        {
            sendAcceptRequestToUserFunc();
        }

        public delegate void SendRejectRequest();
        public event SendRejectRequest sendRejectRequestToUserFunc;

        public void sendRejectRequestToUser()
        {
            sendRejectRequestToUserFunc();
        }

        public delegate void UpdateCell(int row, int col);
        public event UpdateCell updateCellFunc;

        public void updateCell(int row, int col)
        {
            updateCellFunc(row,col);
        }


        public delegate void AnnouceWinner(string winnerName);
        public event AnnouceWinner annouceWinnerFunc;

        public void annouceWinner(string winnerName)
        {
            annouceWinnerFunc(winnerName);
        }
    }
}
