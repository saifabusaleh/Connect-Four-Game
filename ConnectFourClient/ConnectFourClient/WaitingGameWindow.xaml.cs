using ConnectFourClient.ConnectFourService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ConnectFourClient
{
    /// <summary>
    /// Interaction logic for WaitingGameWindow.xaml
    /// </summary>
    public partial class WaitingGameWindow : Window
    {
        private bool isClosingFromGui = false;

        public ClientCallback Callback { get; set; }
        public string currentUser { get; set; }
        public ConnectFourServiceClient client { get; set; }

        public ObservableCollection<string> connectedUsers { get; set; }

        public WaitingGameWindow()
        {
            InitializeComponent();
            connectedUsers = new ObservableCollection<string>();
            lbUsers.ItemsSource = connectedUsers;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // call update clients which iterates through all the online clients
            //and add this user to their list
            Callback.addUsers += AddUsers;
            Callback.removeUser += removeUser;
            Callback.sendGameRequestToUserFunc += RecieveGameRequest;
            this.Title = "Waiting window, connected as: " + currentUser;
        }

        private void removeUser(string user)
        {
            if (!connectedUsers.Contains(user))
            {
                MessageBox.Show("connected users update problem..");
                return;
            }
            connectedUsers.Remove(user);
        }

        private void initGameWindow(GameWindow.Side Side)
        {
            GameWindow gWindow = new GameWindow(Side);
            gWindow.client = this.client;
            gWindow.Callback = this.Callback;
            gWindow.currentUser = this.currentUser;
            gWindow.Show();
            isClosingFromGui = true;
            this.Close();
        }

        private void RecieveReject()
        {
            string selectedOponent = (string)lbUsers.SelectedItem;
            MessageBox.Show("user: " + selectedOponent + " has reject request for game...");
            lbUsers.IsEnabled = true;
            btnPick.IsEnabled = true;
        }

        private bool RecieveGameRequest(string user)
        {
            MessageBoxResult dialogResult = MessageBox.Show("user: " + user + " want to play game with you, do you want to play?", "Game request", MessageBoxButton.YesNo);
            switch (dialogResult)
            {
                case MessageBoxResult.Yes:
                    initGameWindow(GameWindow.Side.Black);
                    return true;
                case MessageBoxResult.No:
                    return false;
            }
            return true;
        }
        private void initGameThread(string player1, string player2)
        {
            client.InitGame(player1, player2);
        }
        private void AddUsers(string[] users)
        {
            for (int i = 0; i < users.Length; i++)
            {
                connectedUsers.Add(users[i]);
            }
        }

        private void btnPick_Click(object sender, RoutedEventArgs e)
        {
            var selectedOponent = lbUsers.SelectedItem;
            if (selectedOponent == null)
            {
                MessageBox.Show("you need to pick opponent to play with!");
                return;
            }
            string selectedOponentString = (string)selectedOponent;
            try
            {
                bool gameRequestResult = client.SendRequestForGameToUser(selectedOponentString, currentUser);
                lbUsers.IsEnabled = false;
                btnPick.IsEnabled = false;
                if (gameRequestResult == true)
                {
                    initGameWindow(GameWindow.Side.Red);
                }
                else
                {
                    MessageBox.Show("Player: " + selectedOponentString + " refused to play with you.. :(");
                    lbUsers.IsEnabled = true;
                    btnPick.IsEnabled = true;
                }

            }
            catch (FaultException<UserNotFoundFault> ex)
            {

                MessageBox.Show(ex.Detail.Message);
            }

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            if (!isClosingFromGui)
            {

                try
                {
                    client.Disconnect(currentUser);

                }
                catch (FaultException<UserNotFoundFault> ex)
                {
                    MessageBox.Show(ex.Detail.Message);

                }
                System.Environment.Exit(System.Environment.ExitCode);

            }
        }

        private void lbUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var curItem = lbUsers.SelectedItem;
            if (curItem == null) // handling the case that nothing is selected

            {
                setTextBoxesToEmpty();
                return;
            }
            string playername = (string)curItem;
            PlayersDetails playerDetails = null;
            Thread t = new Thread(() => {
                try
                {

                
                playerDetails = client.getPlayerDetails(playername);
                }   catch (FaultException<UserAlreadyLoggedInFault> ex)
                {
                    MessageBox.Show(ex.Detail.Message);
                    return;
                }
            });
            t.Start();
            t.Join();
            setTextBoxesToPlayerData(playerDetails);

        }

        private void setTextBoxesToPlayerData(PlayersDetails playerDetails)
        {
            txtNumOfGames.Text = playerDetails.numOfGames.ToString();
            txtNumOfLoses.Text = playerDetails.numOfLoses.ToString();
            txtNumOfPoints.Text = playerDetails.numOfPoints.ToString();
            txtNumOfWins.Text = playerDetails.numOfWins.ToString();
            if (playerDetails.numOfGames == 0) // to make sure not to divide by 0
            {
                txtWinPercent.Text = "0";
                return;
            }
            txtWinPercent.Text = ((playerDetails.numOfWins / playerDetails.numOfGames) * 100).ToString() + "%";
        }

        //This method set all textboxes to empty
        private void setTextBoxesToEmpty()
        {
            txtNumOfGames.Text = string.Empty;
            txtNumOfLoses.Text = string.Empty;
            txtNumOfPoints.Text = string.Empty;
            txtNumOfWins.Text = string.Empty;
            txtWinPercent.Text = string.Empty;
        }
    }
}

