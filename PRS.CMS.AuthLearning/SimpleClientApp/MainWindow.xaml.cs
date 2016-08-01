using System.Windows;
using IdentityModel.Client;

namespace SimpleClientApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var client = new TokenClient("http://localhost:44300/AuthServer/identity/connect/token",
                "SimpleClientApp",
                "secret");

            var userName = UserNameTextBox.Text;
            var password = PasswordTextBox.Text;

            var result = client.RequestResourceOwnerPasswordAsync(userName, password, "openid profile email").Result;
            tokenTextBlock.Text = result.AccessToken;
        }
    }
}
