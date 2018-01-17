using ConnectFourClient.ConnectFourService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
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
    /// Interaction logic for SignupWindow.xaml
    /// </summary>
    public partial class SignupWindow : Window
    {
        ConnectFourServiceClient connection;

        public SignupWindow()
        {
            InitializeComponent();
            connection = new ConnectFourServiceClient();
        }

        private void Signup_Click(object sender, RoutedEventArgs e)
        {
            string username = tbUsername.Text;
            string password = tbPassword.Text;
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("username and password cant be empty!");
                return;
            }
            try
            {
                connection.register(username, password);
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
