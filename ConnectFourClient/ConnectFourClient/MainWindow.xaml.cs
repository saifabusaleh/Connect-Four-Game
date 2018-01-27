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
using ConnectFourClient.ConnectFourService;
using System.Threading;
using System.ServiceModel;

namespace ConnectFourClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ClientCallback Callback { get; set; }
        public string currentUser { get; set; }
        public ConnectFourServiceClient client { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void showPlayers_Click(object sender, RoutedEventArgs e)
        {
            ShowPlayersWindow window = new ShowPlayersWindow(this);
            
            window.Show();
        }

        private void showGames_Click(object sender, RoutedEventArgs e)
        {
            ShowGamesWindow window = new ShowGamesWindow(this);
            window.Show();
        }

        private void showCurrentGames_Click(object sender, RoutedEventArgs e)
        {
            ShowCurrentGamesWindow window = new ShowCurrentGamesWindow(this);
            window.Show();
        }

        public PlayersDetails[] getPlayers()
        {
            return client.getPlayers();
        }

        public GameDetails[] getGames()
        {
            return client.getGames();
        }

        public PlayingGames[] getCurrentGames()
        {
            return client.getCurrentGames();
        }


        private void waitingWindow_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                client.Connect(currentUser);

            }
            catch (FaultException<UserAlreadyLoggedInFault> ex)
            {
                MessageBox.Show(ex.Detail.Message);
                return;
            }

            WaitingGameWindow waitingGameWindow = new WaitingGameWindow();
            waitingGameWindow.Callback = Callback;
            waitingGameWindow.currentUser = currentUser;
            waitingGameWindow.client = client;
            waitingGameWindow.Show();
        }
    }
}
