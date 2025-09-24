using Microsoft.Extensions.DependencyInjection;
using SmallApp.Library.Authentication;
using System.Windows;
using SmallApp.Library.DataAccess;

namespace NonPaymentDropper
{
    public partial class Login : Window
    {
        private readonly IAuthentication _auth;
        private readonly IUserSession _userSession;
        private readonly IApplicationSecurityDataAccess _sec;

        public Login(IAuthentication auth, IUserSession userSession, IApplicationSecurityDataAccess sec)
        {
            _auth = auth;
            _userSession = userSession;
            _sec = sec;
            InitializeComponent();
        }

        private void AuthenticateUser_OnClick(object sender, RoutedEventArgs e)
        {
            if (!_sec.IsApplicationActive())
            {
                MessageBox.Show("This application is currently no longer available. Please contact the administrator.",
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }

            var isAuthenticated = _auth.ValidateCredentials(usernameText.Text, passwordText.Password);
            if (isAuthenticated)
            {
                _userSession.SetAppUserInfo(usernameText.Text);
                _userSession.SetCreateOPID();
                var hasOpid = _userSession.GetCreateOPID() != null;

                if (hasOpid)
                {
                    _userSession.SetPCSysAdmin();
                    _userSession.SetSqlSysAdmin();
                    if (_userSession.IsActiveForApp())
                    {
                        var mainMenu = App.serviceProvider.GetRequiredService<MainMenu>();
                        mainMenu.Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("User is not active for this app. Please contact the administrator.");
                    }
                }
                else
                {
                    MessageBox.Show("User does not have access to PowerCampus. Pleace contact the administrator.");
                }
            }
            else
            {
                MessageBox.Show("Invalid credentials. Please try again.");
            }
        }

        private void ExitApplication_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
