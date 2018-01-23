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
            Callback.sendAcceptRequestToUserFunc += ReceiveAccept;

            Callback.sendRejectRequestToUserFunc += RecieveReject;
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

        private void ReceiveAccept()
        {
            //Init With Red because its Player1
            initGameWindow(GameWindow.Side.Red);
        }

        private void initGameWindow(GameWindow.Side Side)
        {
            GameWindow gWindow = new GameWindow(Side);
            gWindow.client = this.client;
            gWindow.Callback = this.Callback;
            gWindow.currentUser = this.currentUser;
            gWindow.Show();
            this.Close();
        }

        private void RecieveReject()
        {
            string selectedOponent = (string)lbUsers.SelectedItem;
            MessageBox.Show("user: " + selectedOponent + " has reject request for game...");
            lbUsers.IsEnabled = true;
            btnPick.IsEnabled = true;
        }

        private void RecieveGameRequest(string user)
        {
            MessageBoxResult dialogResult = MessageBox.Show("user: " + user + " want to play game with you, do you want to play?", "Game request", MessageBoxButton.YesNo);
            switch (dialogResult)
            {
                case MessageBoxResult.Yes:
                    try
                    {
                        Thread t = new Thread(() => client.SendAcceptForGameToUser(user));
                        t.Start();
                        //Init with Black because its Player2
                        Thread t1 = new Thread(() => initGameThread(user, currentUser));
                        t1.Start();
                        initGameWindow(GameWindow.Side.Black);
                    }
                    catch (FaultException<UserNotFoundFault> ex)
                    {

                        MessageBox.Show(ex.Detail.Message);
                    }
                    break;
                case MessageBoxResult.No:
                    try
                    {
                        Thread t = new Thread(() => client.SendRejectForGameToUser(user));
                        t.Start();
                    }
                    catch (FaultException<UserNotFoundFault> ex)
                    {

                        MessageBox.Show(ex.Detail.Message);
                    }
                    break;
            }
        }
        private void initGameThread(string player1, string player2)
        {
            client.InitGame(player1, player2);
        }
        private void AddUsers(string[] users)
        {
            for(int i=0;i<users.Length;i++)
            {
                connectedUsers.Add(users[i]);
            }
            // lbUsers.ItemsSource = users;
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
                client.SendRequestForGameToUser(selectedOponentString, currentUser);
                lbUsers.IsEnabled = false;
                btnPick.IsEnabled = false;
            }
            catch (FaultException<UserNotFoundFault> ex)
            {

                MessageBox.Show(ex.Detail.Message);
            }

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Temporary disable because of problems with init game
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
}
