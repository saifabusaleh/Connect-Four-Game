using ConnectFourClient.ConnectFourService;
using System;
using System.Collections.Generic;
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

        public WaitingGameWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // call update clients which iterates through all the online clients
            //and add this user to their list
            Callback.updateUsers += UpdateUsers;

            Callback.sendGameRequestToUserFunc += RecieveGameRequest;
            Callback.sendAcceptRequestToUserFunc += ReceiveAccept;

            Callback.sendRejectRequestToUserFunc += RecieveReject;
            try
            {
                client.Connect(currentUser);

            }
            catch (FaultException<UserAlreadyLoggedInFault> ex)
            {
                MessageBox.Show(ex.Detail.Message);
                this.Close();
                LoginWindow window = new LoginWindow();
                window.Show();
            }
            this.Title = "Waiting window, connected as: " + currentUser;
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

        private void UpdateUsers(string[] users)
        {
            List<String> connectedUsersWithoutCurrent = new List<String>(users);
            //foreach (string user in connectedUsersWithoutCurrent)
            //{
            //    if (user == currentUser)
            //    {
            //        connectedUsersWithoutCurrent.Remove(user);
            //    }
            //}
            lbUsers.ItemsSource = users;
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
