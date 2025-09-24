using Microsoft.Extensions.DependencyInjection;
using NonPaymentDropper.FunctionWindows;
using NonPaymentDropper.UserWindows;
using SmallApp.Library.Authentication;
using System.Windows;

namespace NonPaymentDropper
{
    public partial class MainMenu : Window
    {
        private readonly IUserSession _userSession;

        public MainMenu(IUserSession userSession)
        {
            _userSession = userSession;
            InitializeComponent();

            if (App.isDevMode)
                DevModeLabel.Visibility = Visibility.Visible;

            if (_userSession.GetSqlSysAdmin())
            {
                UserControlBtn.Visibility = Visibility.Visible;
                UserControlBtn.IsEnabled = true;
            }
        }

        private void DropBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var dropWindow = App.serviceProvider.GetRequiredService<DropNonPayment>();
            this.Hide();
            dropWindow.ShowDialog();
            this.Show();
        }

        private void UserControlBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var userControl = App.serviceProvider.GetRequiredService<UserAppControl>();
            this.Hide();
            userControl.ShowDialog();
            this.Show();
        }

        private void ReaddBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var readdWindow = App.serviceProvider.GetRequiredService<DropNonPaymentAddBack>();
            this.Hide();
            readdWindow.ShowDialog();
            this.Show();
        }

        private void EditEmailBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var emailEditor = App.serviceProvider.GetRequiredService<EmailTemplateEditor>();
            this.Hide();
            emailEditor.ShowDialog();
            this.Show();
        }
    }
}
