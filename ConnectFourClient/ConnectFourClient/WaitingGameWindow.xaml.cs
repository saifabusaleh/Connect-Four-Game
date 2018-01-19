using ConnectFourClient.ConnectFourService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
             Callback.updateUsers += UpdateUsers;
            // call update clients which iterates through all the online clients
            //and add this user to their list
            client.updateClients(currentUser);
            this.Title = "Waiting window, connected as: " + currentUser;
        }

        private void UpdateUsers(string[] users)
        {
            List<String> connectedUsersWithoutCurrent = new List<String>(users);
            //foreach( string user in connectedUsersWithoutCurrent)
            //{
            //    if(user == currentUser)
            //    {
            //        connectedUsersWithoutCurrent.Remove(user);
            //    }
            //}
            lbUsers.ItemsSource = connectedUsersWithoutCurrent;
        }

        private void btnPick_Click(object sender, RoutedEventArgs e)
        {
            var selectedOponent = lbUsers.SelectedItem;
            if (selectedOponent == null)
            {
                MessageBox.Show("you need to pick opponent to play with!");
                return;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            client.Disconnect(currentUser);
            System.Environment.Exit(System.Environment.ExitCode);
        }
    }
}
