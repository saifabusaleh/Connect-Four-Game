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

    public enum MOVE_RESULT { Win, Draw, Nothing };


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

        private void AddToClientListThreadingFunction(string username, IConnectFourServiceCallback callback)
        {

            //Update rest clients with the new client instead of new one
            List<string> connclients = new List<String>();
            connclients.Add(username);
            foreach (KeyValuePair<string, IConnectFourServiceCallback> client in clients)
            {
                if (client.Key != username)
                {
                    client.Value.addUsersToList(connclients);
                }

            }

            //update new one with the rest of clients
            connclients = new List<String>(clients.Keys);
            connclients.Remove(username);
            callback.addUsersToList(connclients);

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
            Thread updateThread = new Thread(() => AddToClientListThreadingFunction(username, callback));
            updateThread.Start();
        }

        public void Disconnect(string userName)
        {
            if (!clients.ContainsKey(userName))
            {
                UserNotFoundFault fault = new UserNotFoundFault()
                { Message = "Username " + userName + " is not found!" };
                throw new FaultException<UserNotFoundFault>(fault);
            }
            clients.Remove(userName);
            Thread updateThread = new Thread(() => RemoveFromClientListThreadingFunction(userName));
            updateThread.Start();
        }

        private void RemoveFromClientListThreadingFunction(string userName)
        {

            //update all clients without the client that will be deleted with the client that will be deleted
            foreach (KeyValuePair<string, IConnectFourServiceCallback> client in clients)
            {
                if (client.Key != userName)
                {
                    client.Value.removeUsersFromList(userName);
                }
            }
        }

        public bool SendRequestForGameToUser(string opponentUserName, string myUserName)
        {
            bool requestResult = false;
            Thread t = new Thread(() => { requestResult = sendRequestForGameToUserThread(opponentUserName, myUserName); });
            t.Start();
            t.Join();
            return requestResult;
        }

        private bool sendRequestForGameToUserThread(string opponentUserName, string myUserName)
        {
            foreach (KeyValuePair<string, IConnectFourServiceCallback> client in clients)
            {
                if (client.Key == opponentUserName)
                {
                    bool requestResult = client.Value.sendGameRequestToUser(myUserName);
                    if (requestResult == true)
                    {
                        initGameThread(myUserName, opponentUserName);
                    }
                    return requestResult;
                }
            }
            // if user not found
            UserNotFoundFault fault = new UserNotFoundFault()
            { Message = "Username " + opponentUserName + " is not found!" };
            throw new FaultException<UserNotFoundFault>(fault);
        }
        private void initGameThread(string player1, string player2)
        {
            IConnectFourServiceCallback player1CallBack = null;
            IConnectFourServiceCallback player2CallBack = null;

            foreach (KeyValuePair<string, IConnectFourServiceCallback> client in clients)
            {
                if (client.Key == player1)
                {
                    player1CallBack = client.Value;
                }

                if (client.Key == player2)
                {
                    player2CallBack = client.Value;
                }
            }

            //if player1 or 2 callbacks null throw exception 
            currentGames.Add(initPlayingPlayers(player1, player2, player1CallBack, player2CallBack), initPlayingGame(player1));
        }

        public void InitGame(string player1, string player2)
        {
            //Search player1
            Thread t = new Thread(() => initGameThread(player1, player2));
            t.Start();
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
            foreach (KeyValuePair<PlayingPlayers, PlayingGame> currentGame in currentGames)
            {
                if (playerName == currentGame.Key.Player1 || playerName == currentGame.Key.Player2)
                {
                    return currentGame.Value.Turn == playerName;
                }
            }
            return false;
            //IF NOT throw exception that playerName not found
        }

        public InsertResult Insert(int column, string playerName)
        {
            InsertResult result = new InsertResult();
            int insertionRowIndex = -1;

            foreach (KeyValuePair<PlayingPlayers, PlayingGame> currentGame in currentGames)
            {
                if (playerName == currentGame.Key.Player1)
                {
                    insertionRowIndex = currentGame.Value.Board.Insert(Side.Red, column);
                    Side winner = currentGame.Value.Board.Winner(insertionRowIndex, column);
                    if (winner != Side.None)
                    {
                        currentGame.Key.CallBackPlayer2.updateCell(insertionRowIndex, column, MOVE_RESULT.Win);
                        result.Move_result = MOVE_RESULT.Win;
                        cs.addGameWithWinToDB(playerName, currentGame.Key.Player2, playerName);
                        result.Row_index = insertionRowIndex;
                        disconnectClientsAndRemoveGame(currentGame);

                        return result;
                    }

                    if(currentGame.Value.Board.Tied())
                    {
                        currentGame.Key.CallBackPlayer2.updateCell(insertionRowIndex, column, MOVE_RESULT.Draw);
                        result.Move_result = MOVE_RESULT.Draw;
                        result.Row_index = insertionRowIndex;
                        cs.addGameWithDrawToDB(playerName, currentGame.Key.Player2);
                        disconnectClientsAndRemoveGame(currentGame);

                        return result;
                    }
                    //if not win or draw, update turn
                    currentGame.Value.Turn = currentGame.Key.Player2;

                    currentGame.Key.CallBackPlayer2.updateCell(insertionRowIndex, column, MOVE_RESULT.Nothing);
                    result.Move_result = MOVE_RESULT.Nothing;
                    result.Row_index = insertionRowIndex;

                    return result;
                }
                else if (playerName == currentGame.Key.Player2)
                {
                    insertionRowIndex = currentGame.Value.Board.Insert(Side.Black, column);
                    Side winner = currentGame.Value.Board.Winner(insertionRowIndex, column);
                    if (winner != Side.None)
                    {
                        currentGame.Key.CallBackPlayer1.updateCell(insertionRowIndex, column, MOVE_RESULT.Win);
                        result.Move_result = MOVE_RESULT.Win;
                        result.Row_index = insertionRowIndex;
                        cs.addGameWithWinToDB(playerName, currentGame.Key.Player1, playerName);
                        disconnectClientsAndRemoveGame(currentGame);
                        return result;
                    }

                    if (currentGame.Value.Board.Tied())
                    {
                        currentGame.Key.CallBackPlayer1.updateCell(insertionRowIndex, column, MOVE_RESULT.Draw);
                        result.Move_result = MOVE_RESULT.Draw;
                        result.Row_index = insertionRowIndex;
                        cs.addGameWithDrawToDB(playerName, currentGame.Key.Player1);
                        disconnectClientsAndRemoveGame(currentGame);


                        return result;
                    }

                    //if not win or draw, update turn
                    currentGame.Value.Turn = currentGame.Key.Player1;
                    result.Move_result = MOVE_RESULT.Nothing;
                    result.Row_index = insertionRowIndex;
                    currentGame.Key.CallBackPlayer1.updateCell(insertionRowIndex, column, MOVE_RESULT.Nothing);
                    return result;
                }
            }

            //Throw exception
            return result;
        }
        //public bool checkIfIWin(string playerName, int row, int col)
        //{
        //    foreach (KeyValuePair<PlayingPlayers, PlayingGame> currentGame in currentGames)
        //    {
        //        if (playerName == currentGame.Key.Player1 || playerName == currentGame.Key.Player2)
        //        {
        //            Side winner = currentGame.Value.Board.Winner(row, col);
        //            if (winner != Side.None)
        //            {
        //                //Send to another player that this player won
        //                if (playerName == currentGame.Key.Player1)
        //                {
        //                    currentGame.Key.CallBackPlayer2.annouceWinner(playerName);

        //                    cs.addGameWithWinToDB(playerName, currentGame.Key.Player2, playerName);
        //                }
        //                else
        //                {
        //                    currentGame.Key.CallBackPlayer1.annouceWinner(playerName);

        //                    cs.addGameWithWinToDB(currentGame.Key.Player1, playerName, playerName);

        //                }
        //                Disconnect(currentGame.Key.Player1);
        //                Disconnect(currentGame.Key.Player2);

        //                currentGames.Remove(currentGame.Key);
        //                // add the game to finished games..
        //                return true;
        //            }
        //            else
        //            {
        //                if (playerName == currentGame.Key.Player1)
        //                {
        //                    currentGame.Value.Turn = currentGame.Key.Player2;
        //                }
        //                else
        //                {
        //                    currentGame.Value.Turn = currentGame.Key.Player1;

        //                }
        //                return false;
        //            }
        //        }

        //    }
        //    return false;
        //}

        private void disconnectClientsAndRemoveGame(KeyValuePair<PlayingPlayers, PlayingGame> currentGame)
        {
            Disconnect(currentGame.Key.Player1);
            Disconnect(currentGame.Key.Player2);

            currentGames.Remove(currentGame.Key);
        }

        public IEnumerable<PlayersDetails> getPlayers()
        {
            IEnumerable<PlayersDetails> users = null;
            Thread t = new Thread(() => { users = cs.getPlayers();  });
            t.Start();
            t.Join();
            return users;
        }

        public IEnumerable<PlayingGames> getCurrentGames()
        {
            List<PlayingGames> currentGames = new List<PlayingGames>();
            Thread t = new Thread(() => {
                foreach(KeyValuePair<PlayingPlayers, PlayingGame> currentGame in this.currentGames)
                {
                    PlayingGames game = new PlayingGames();
                    game.player1 = currentGame.Key.Player1;
                    game.player2 = currentGame.Key.Player2;
                    game.startTime = currentGame.Value.GameStartTime;
                    currentGames.Add(game);
                }
            });
            t.Start();
            t.Join();
            return currentGames;
        }

        public IEnumerable<GameDetails> getGames()
        {
            IEnumerable<GameDetails> games = null;

            Thread t = new Thread(() => { games = cs.getGames(); });
            t.Start();
            t.Join();
            return games;
        }
    }
}
