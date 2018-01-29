using ConnectFourClient.ConnectFourService;
using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Interaction logic for ShowPlayersWindow.xaml
    /// </summary>
    public partial class ShowPlayersWindow : Window
    {

        public delegate PlayersDetails[] getPlayersDelegate();
        public event getPlayersDelegate getPlayers;
        public ShowPlayersWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            this.getPlayers += mainWindow.getPlayers;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PlayersDetails[] users = null;
            Thread t = new Thread(() => { users = getPlayers(); });
            t.Start();
            t.Join();
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                PlayersDG.ItemsSource = from user in users
                                        select new
                                        {
                                            Username = user.username,
                                            Number_Of_Games = user.numOfGames,
                                            Number_Of_Wins = user.numOfWins,
                                            Number_Of_Loses = user.numOfLoses,
                                            Number_Of_Points = user.numOfPoints
                                        };
            }));
        }
    }
}
