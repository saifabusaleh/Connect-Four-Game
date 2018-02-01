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
        private Dictionary<int, PlayingGame> currentGames;
        private int currentGameId = 0;
        public ConnectFourService()
        {
            clients = new SortedDictionary<string, IConnectFourServiceCallback>();
            currentGames = new Dictionary<int, PlayingGame>();
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
                    return requestResult;
                }
            }
            // if user not found
            UserNotFoundFault fault = new UserNotFoundFault()
            { Message = "Username " + opponentUserName + " is not found!" };
            throw new FaultException<UserNotFoundFault>(fault);
        }

        private int initGameThread(string player1, string player2)
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


            currentGames.Add(currentGameId, initPlayingGame(player1, player2, player1CallBack, player2CallBack));
            //Send to player2 the gameId, player1 will get the gameId from function return value
            InitGameResult game = new InitGameResult();
            game.Player1 = player1;
            game.Player2 = player2;
            game.gameId = currentGameId;
            player2CallBack.sendGameInfo(game);

            return currentGameId++;
        }

        // Input: Two players
        // Output: the game Id which identifies the key of the dictionary of the game between the two players

        public InitGameResult InitGame(string player1, string player2)
        {
            int gameId = 0;
            Thread t = new Thread(() => { gameId = initGameThread(player1, player2); });
            t.Start();
            t.Join();
            InitGameResult res = new InitGameResult();
            res.Player1 = player1;
            res.Player2 = player2;
            res.gameId = gameId;
            return res;
        }

        private PlayingGame initPlayingGame(string player1, string player2, IConnectFourServiceCallback player1CallBack, IConnectFourServiceCallback player2CallBack)
        {
            PlayingGame game = new PlayingGame();
            game.GameStartTime = DateTime.Now;
            game.Turn = player1;
            game.Player1 = player1;
            game.Player2 = player2;
            game.CallBackPlayer1 = player1CallBack;
            game.CallBackPlayer2 = player2CallBack;
            game.Board = new ConnectFourBoard(6, 7);
            return game;
        }

        public bool IsMyTurn(string playerName, int gameId)
        {
            PlayingGame value;
            if (currentGames.TryGetValue(gameId, out value))
            {
                return value.Turn == playerName;
            }
            else
            { // if game ID not found throw exception, TODO: change exception type
                throwUserNotFoundFault(playerName);
            }
            return false;
        }

        public InsertResult Insert(int column, string playerName, int gameId)
        {
            InsertResult result = new InsertResult();
            int insertionRowIndex = -1;

            PlayingGame currentGame;
            if (currentGames.TryGetValue(gameId, out currentGame))
            {
                // game is found
                if (playerName == currentGame.Player1)
                {
                    insertionRowIndex = currentGame.Board.Insert(Side.Red, column);
                    Side winner = currentGame.Board.Winner(insertionRowIndex, column);
                    // if player 1 is winner
                    if (winner != Side.None)
                    {
                        updateCellForAnotherPlayer(playerName, insertionRowIndex, column, currentGame, MOVE_RESULT.Win);
                        annoucePlayerWinner(playerName, currentGame.Player2, currentGame);
                        disconnectClientsAndRemoveGame(currentGame, gameId);
                        return createInsertResult(insertionRowIndex, column, MOVE_RESULT.Win);
                    }
                    // if there is tie
                    if (currentGame.Board.Tied())
                    {
                        updateCellForAnotherPlayer(playerName, insertionRowIndex, column, currentGame, MOVE_RESULT.Draw);
                        announceDraw(playerName, currentGame.Player2, currentGame);
                        disconnectClientsAndRemoveGame(currentGame, gameId);
                        return createInsertResult(insertionRowIndex, column, MOVE_RESULT.Draw);
                    }
                    //if not win or draw, update turn
                    updateCellForAnotherPlayer(playerName, insertionRowIndex, column, currentGame, MOVE_RESULT.Nothing);
                    updateTurn(playerName, currentGame);
                    return createInsertResult(insertionRowIndex, column, MOVE_RESULT.Nothing);
                }
                else if (playerName == currentGame.Player2)
                {
                    insertionRowIndex = currentGame.Board.Insert(Side.Black, column);
                    Side winner = currentGame.Board.Winner(insertionRowIndex, column);

                    if (winner != Side.None)
                    {
                        // if player 2 winner
                        updateCellForAnotherPlayer(playerName, insertionRowIndex, column, currentGame, MOVE_RESULT.Win);
                        annoucePlayerWinner(playerName, currentGame.Player1, currentGame);
                        disconnectClientsAndRemoveGame(currentGame, gameId);
                        return createInsertResult(insertionRowIndex, column, MOVE_RESULT.Win);
                    }

                    if (currentGame.Board.Tied())
                    {
                        // if tie
                        updateCellForAnotherPlayer(playerName, insertionRowIndex, column, currentGame, MOVE_RESULT.Draw);
                        announceDraw(playerName, currentGame.Player1, currentGame);
                        disconnectClientsAndRemoveGame(currentGame, gameId);
                        return createInsertResult(insertionRowIndex, column, MOVE_RESULT.Draw);
                    }

                    //if not win or draw, update turn
                    updateCellForAnotherPlayer(playerName, insertionRowIndex, column, currentGame, MOVE_RESULT.Nothing);
                    updateTurn(playerName, currentGame);
                    return createInsertResult(insertionRowIndex, column, MOVE_RESULT.Nothing);
                }
            }

            throwUserNotFoundFault(playerName);
            return result;
        }

        private void updateTurn(string playerWithTurn, PlayingGame currentGame)
        {
            if (playerWithTurn == currentGame.Player1)
            {
                currentGame.Turn = currentGame.Player2;
            }
            else
            {
                currentGame.Turn = currentGame.Player1;
            }
        }

        private InsertResult createInsertResult(int insertionRowIndex, int column, MOVE_RESULT move)
        {
            InsertResult result = new InsertResult();
            result.Row_index = insertionRowIndex;
            result.Move_result = move;
            return result;
        }

        private void updateCellForAnotherPlayer(string playername, int insertionRowIndex, int column, PlayingGame currentGame, MOVE_RESULT move)
        {
            // update other player that the other one has own
            if (playername == currentGame.Player1)
            {
                currentGame.CallBackPlayer2.updateCell(insertionRowIndex, column, move);
            }
            else if (playername == currentGame.Player2)
            {
                currentGame.CallBackPlayer1.updateCell(insertionRowIndex, column, move);
            }
            //else
            //{
            //    throw exception
            //}
        }
        private void annoucePlayerWinner(string winner, string loser, PlayingGame currentGame)
        {
            bool retValue = cs.addGameWithWinToDB(winner, loser, winner);
            if (retValue == false)
            {
                UserNotFoundFault fault = new UserNotFoundFault()
                { Message = "Username " + winner + " or " + loser + " not found" };
                throw new FaultException<UserNotFoundFault>(fault);
            }
        }

        private void announceDraw(string playerWithTurn, string otherPlayer, PlayingGame currentGame)
        {
            bool retValue = cs.addGameWithDrawToDB(playerWithTurn, otherPlayer);
            if (retValue == false)
            {
                UserNotFoundFault fault = new UserNotFoundFault()
                { Message = "Username " + playerWithTurn + " or " + otherPlayer + " not found" };
                throw new FaultException<UserNotFoundFault>(fault);
            }
        }

        private void disconnectClientsAndRemoveGame(PlayingGame currentGame, int gameId)
        {
            Disconnect(currentGame.Player1);
            Disconnect(currentGame.Player2);

            currentGames.Remove(gameId);
        }
        // get all players details
        public IEnumerable<PlayersDetails> getPlayers()
        {
            IEnumerable<PlayersDetails> users = null;
            Thread t = new Thread(() => { users = cs.getPlayers(); });
            t.Start();
            t.Join();
            return users;
        }
        //get player with name username details
        public PlayersDetails getPlayerDetails(string username)
        {
            PlayersDetails player = null;
            Thread t = new Thread(() => { player = cs.getPlayerDetails(username); });
            t.Start();
            t.Join();
            if (player == null)
            {
                throwUserNotFoundFault(username);
            }
            return player;
        }

        private void throwUserNotFoundFault(string username)
        {
            UserNotFoundFault fault = new UserNotFoundFault()
            { Message = "Username " + username + " not found" };
            throw new FaultException<UserNotFoundFault>(fault);
        }
        //get current games
        public IEnumerable<PlayingGames> getCurrentGames()
        {
            List<PlayingGames> currentGames = new List<PlayingGames>();
            Thread t = new Thread(() =>
            {
                foreach (KeyValuePair<int, PlayingGame> currentGame in this.currentGames)
                {
                    PlayingGames game = new PlayingGames();
                    game.player1 = currentGame.Value.Player1;
                    game.player2 = currentGame.Value.Player2;
                    game.startTime = currentGame.Value.GameStartTime;
                    currentGames.Add(game);
                }
            });
            t.Start();
            t.Join();
            return currentGames;
        }

        // get all games
        public IEnumerable<GameDetails> getGames()
        {
            IEnumerable<GameDetails> games = null;

            Thread t = new Thread(() => { games = cs.getGames(); });
            t.Start();
            t.Join();
            return games;
        }


        public void GiveupGame(string playerNameWhoGaveUp, int gameId)
        {
            PlayingGame currentGame;
            if (currentGames.TryGetValue(gameId, out currentGame))
            {
                // announce winner to other player
                if (playerNameWhoGaveUp == currentGame.Player1)
                {
                    annoucePlayerWinner(currentGame.Player2, playerNameWhoGaveUp, currentGame);
                    currentGame.CallBackPlayer2.annouceWinnerBecauseOtherPlayerLeft();
                    disconnectClientsAndRemoveGame(currentGame, gameId);
                }
                else if (playerNameWhoGaveUp == currentGame.Player2)
                {
                    annoucePlayerWinner(currentGame.Player1, playerNameWhoGaveUp, currentGame);
                    currentGame.CallBackPlayer1.annouceWinnerBecauseOtherPlayerLeft();
                    disconnectClientsAndRemoveGame(currentGame, gameId);

                }
                //else
                //{
                //    throw exception
                //}
            }
            else
            {
                //throw exception
            }
        }

    }
}
