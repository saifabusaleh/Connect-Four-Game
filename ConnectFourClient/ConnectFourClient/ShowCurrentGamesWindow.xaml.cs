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
    /// Interaction logic for ShowCurrentGamesWindow.xaml
    /// </summary>
    public partial class ShowCurrentGamesWindow : Window
    {
        public delegate PlayingGames[] getCurrentGamesDelegate();
        public event getCurrentGamesDelegate getCurrentGames;
        public ShowCurrentGamesWindow(MainWindow window)
        {
            InitializeComponent();
            this.getCurrentGames += window.getCurrentGames;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PlayingGames[] currentGames = null;
            Thread t = new Thread(() => { currentGames = getCurrentGames(); });
            t.Start();
            t.Join();
            Application.Current.Dispatcher.Invoke(new Action(() => { CurrentGamesDG.ItemsSource = currentGames; }));
        }
    }
}
