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
using System.Security.Cryptography;
using System.Threading;

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

        private string GetSHA1HashData(string data)
        {
            //create new instance of md5
            SHA1 sha1 = SHA1.Create();

            //convert the input text to array of bytes
            byte[] hashData = sha1.ComputeHash(Encoding.Default.GetBytes(data));

            //create new instance of StringBuilder to save hashed data
            StringBuilder returnValue = new StringBuilder();

            //loop for each byte and add it to StringBuilder
            for (int i = 0; i < hashData.Length; i++)
            {
                returnValue.Append(hashData[i].ToString());
            }

            // return hexadecimal string
            return returnValue.ToString();
        }


        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = tbUsername.Text;
            string password = tbPassword.Password;
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("username and password cant be empty!");
                return;
            }
            string passwordEncrypted = GetSHA1HashData(password);
            ClientCallback callback = new ClientCallback();
            client = new ConnectFourServiceClient(new InstanceContext(callback));
            bool loginResult = false;
            Thread t = new Thread(() => { loginResult = client.login(username, passwordEncrypted); });
            t.Start();
            t.Join();
            if (loginResult == false)
            {
                MessageBox.Show("username or password is incorrect!");
                return;
            }

            OpenMainWindow(username, callback);
            this.Close();
        }

        private void OpenMainWindow(string username, ClientCallback callback)
        {
            MainWindow window = new MainWindow();
            window.Callback = callback;
            window.currentUser = username;
            window.client = client;

            window.Show();
        }

        private void Signup_Click(object sender, RoutedEventArgs e)
        {
            SignupWindow window = new SignupWindow();
            window.Show();
        }
    }
}
