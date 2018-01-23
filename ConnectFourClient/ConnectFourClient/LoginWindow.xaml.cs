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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ConnectFourClient.ConnectFourService;
using System.ServiceModel;

namespace ConnectFourClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public ConnectFourServiceClient client { get; set; }
        public LoginWindow()
        {
            InitializeComponent();

        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = tbUsername.Text;
            string password = tbPassword.Password;
            if(string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("username and password cant be empty!");
                return;
            }
            ClientCallback callback = new ClientCallback();

            client = new ConnectFourServiceClient(new InstanceContext(callback));
            bool loginResult = client.login(username, password);
            if(loginResult == false)
            {
                MessageBox.Show("username or password is incorrect!");
                return;
            }

            try
            {
                client.Connect(username);

            }
            catch (FaultException<UserAlreadyLoggedInFault> ex)
            {
                MessageBox.Show(ex.Detail.Message);
                return;
            }

            WaitingGameWindow waitingGameWindow = new WaitingGameWindow();
            waitingGameWindow.Callback = callback;
            waitingGameWindow.currentUser = username;
            waitingGameWindow.client = client;
            waitingGameWindow.Show();
            this.Close();
        }

        private void Signup_Click(object sender, RoutedEventArgs e)
        {
            SignupWindow window = new SignupWindow();
            window.Show();
        }
    }
}
