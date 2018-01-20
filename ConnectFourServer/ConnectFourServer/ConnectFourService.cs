using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using ConnectFourDBCore;
using System.Threading;

namespace ConnectFourServer
{
    [ServiceBehavior(
    InstanceContextMode = InstanceContextMode.Single,
    ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ConnectFourService : IConnectFourService
    {
        private ConnectFourDBService cs = new ConnectFourDBService();
        private SortedDictionary<string, IConnectFourServiceCallback> clients;
        private Dictionary<PlayingPlayers, PlayingGame> currentGames;
        public ConnectFourService()
        {
            clients = new SortedDictionary<string, IConnectFourServiceCallback>();
            currentGames = new Dictionary<PlayingPlayers, PlayingGame>();
        }


        public bool login(string username, string password)
        {
            var loginResult = cs.CheckIfValidLogin(username, password);
            return loginResult;
        }

        private void UpdateClientsListsThreadingFunction()
        {
            foreach (var callback in clients.Values)
            {
                callback.UpdateClientsList(clients.Keys);
            }
        }

        public void register(string username, string password)
        {
            bool isRegisterSuccess = cs.RegisterUser(username, password);
            if (isRegisterSuccess == false)
            {
                UserExistsFault fault = new UserExistsFault()
                { Message = "Username " + username + " already exists" };
                throw new FaultException<UserExistsFault>(fault);
            }
        }

        public IEnumerable<String> getConnectedUsersByUsernameThatWaitingForPartner(string username)
        {
            return cs.getConnectedUsersByUsernameThatWaitingForPartner(username);
        }

        public void Connect(string username)
        {
            if (clients.ContainsKey(username))
            {
                UserAlreadyLoggedInFault fault = new UserAlreadyLoggedInFault()
                { Message = "Username " + username + " already logged in, exiting.." };
                throw new FaultException<UserAlreadyLoggedInFault>(fault);
            }

            IConnectFourServiceCallback callback =
OperationContext.Current.GetCallbackChannel<IConnectFourServiceCallback>();
            clients.Add(username, callback);
            Thread updateThread = new Thread(UpdateClientsListsThreadingFunction);
            updateThread.Start();
        }

        public void Disconnect(string userName)
        {
            if(!clients.ContainsKey(userName))
            {
                UserNotFoundFault fault = new UserNotFoundFault()
                { Message = "Username " + userName + " is not found!" };
                throw new FaultException<UserNotFoundFault>(fault);
            }
            clients.Remove(userName);
            Thread updateThread = new Thread(UpdateClientsListsThreadingFunction);
            updateThread.Start();
        }

        public void SendRequestForGameToUser(string opponentUserName, string myUserName)
        {
            foreach (KeyValuePair<string, IConnectFourServiceCallback> client in clients)
            {
                if (client.Key == opponentUserName)
                {
                    client.Value.sendGameRequestToUser(myUserName);
                    return;
                }
            }
            // if user not found
            UserNotFoundFault fault = new UserNotFoundFault()
            { Message = "Username " + opponentUserName + " is not found!" };
            throw new FaultException<UserNotFoundFault>(fault);
        }

        public void SendRejectForGameToUser(string opponentUserName)
        {
            foreach (KeyValuePair<string, IConnectFourServiceCallback> client in clients)
            {
                if (client.Key == opponentUserName)
                {
                    client.Value.sendRejectRequestToUser();
                    return;
                }
                
            }
            // if user not found
            UserNotFoundFault fault = new UserNotFoundFault()
            { Message = "Username " + opponentUserName + " is not found!" };
            throw new FaultException<UserNotFoundFault>(fault);
        }

        public void SendAcceptForGameToUser(string opponentUserName)
        {
            foreach (KeyValuePair<string, IConnectFourServiceCallback> client in clients)
            {
                if (client.Key == opponentUserName)
                {
                    client.Value.sendAcceptRequestToUser();
                    return;
                }

            }
            // if user not found
            UserNotFoundFault fault = new UserNotFoundFault()
            { Message = "Username " + opponentUserName + " is not found!" };
            throw new FaultException<UserNotFoundFault>(fault);
        }

        public void InitGame(string player1, string player2)
        {
            //Search player1
            IConnectFourServiceCallback player1CallBack = null;
            IConnectFourServiceCallback player2CallBack = null;

            foreach (KeyValuePair<string, IConnectFourServiceCallback> client in clients)
            {
                if (client.Key == player1)
                {
                    player1CallBack  = client.Value;
                }

                if (client.Key == player2)
                {
                    player2CallBack = client.Value;
                }
            }

            //if player1 or 2 callbacks null throw exception 
            currentGames.Add(initPlayingPlayers(player1,player2,player1CallBack,player2CallBack), initPlayingGame(player1));
        }

        private PlayingPlayers initPlayingPlayers(string player1, string player2, IConnectFourServiceCallback player1CallBack, IConnectFourServiceCallback player2CallBack)
        {
            PlayingPlayers players = new PlayingPlayers();
            players.Player1 = player1;
            players.Player2 = player2;
            players.CallBackPlayer1 = player1CallBack;
            players.CallBackPlayer2 = player2CallBack;
            return players;
        }

        private PlayingGame initPlayingGame(string player1)
        {
            PlayingGame game = new PlayingGame();
            game.GameStartTime = DateTime.Now;
            game.Turn = player1;
            game.Board = new ConnectFourBoard(6, 7);
            return game;
        }

        public bool IsMyTurn(string playerName)
        {
            foreach(KeyValuePair<PlayingPlayers, PlayingGame> currentGame in currentGames)
            {
                if(playerName == currentGame.Key.Player1 || playerName == currentGame.Key.Player2)
                {
                    return currentGame.Value.Turn == playerName;
                }
            }
            return false;
            //IF NOT throw exception that playerName not found
        }

        public int Insert(int column, string playerName)
        {
            int insertionRowIndex;
            foreach (KeyValuePair<PlayingPlayers, PlayingGame> currentGame in currentGames)
            {
                if (playerName == currentGame.Key.Player1)
                {
                    insertionRowIndex = currentGame.Value.Board.Insert(Side.Red, column);
                    currentGame.Value.Turn = currentGame.Key.Player2;
                    currentGame.Key.CallBackPlayer1.updateCell(insertionRowIndex, column);
                    currentGame.Key.CallBackPlayer2.updateCell(insertionRowIndex, column);
                    return insertionRowIndex;
                } else if (playerName == currentGame.Key.Player2)
                {
                    insertionRowIndex = currentGame.Value.Board.Insert(Side.Black, column);
                    currentGame.Value.Turn = currentGame.Key.Player1;
                    currentGame.Key.CallBackPlayer1.updateCell(insertionRowIndex, column);
                    currentGame.Key.CallBackPlayer2.updateCell(insertionRowIndex, column);
                    return insertionRowIndex;
                }
            }

            //Throw exception
            return -1;
        }
    }
}
