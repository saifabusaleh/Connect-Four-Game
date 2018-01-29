using ConnectFourClient.ConnectFourService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for SignupWindow.xaml
    /// </summary>
    public partial class SignupWindow : Window
    {
        private ConnectFourServiceClient client;
        public SignupWindow()
        {
            InitializeComponent();
            ClientCallback callback = new ClientCallback();

            client = new ConnectFourServiceClient(new InstanceContext(callback));
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

        private void Signup_Click(object sender, RoutedEventArgs e)
        {
            string username = tbUsername.Text;
            string password = tbPassword.Password;
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("username and password cant be empty!");
                return;
            }
            var hasMinimum8Chars = new Regex(@".{6,}");
            if(!hasMinimum8Chars.IsMatch(password))
            {
                MessageBox.Show("Password is too weak, it must have minimum 6 characters");
                return;
            }
            // Encrypt the password
            string passwordEncrypted = GetSHA1HashData(password);
            try
            {
                client.register(username, passwordEncrypted);
                MessageBox.Show("Register done successfully!");
                this.Close();
            }
            catch (FaultException<UserExistsFault> ex)
            {

                MessageBox.Show(ex.Detail.Message);
            }
        }
    }
}
