using Microsoft.Extensions.DependencyInjection;
using SmallApp.Library.DataAccess;
using SmallApp.Library.Models;
using System.Windows;

namespace NonPaymentDropper.UserWindows
{
    public partial class UserAppControl : Window
    {
        private readonly IApplicationUserDataAccess _db;

        public UserAppControl(IApplicationUserDataAccess db)
        {
            _db = db;
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            var userList = _db.GetAllUsersForApp();
            DgUsers.ItemsSource = userList;
        }

        private void UserAppControl_OnClosed(object? sender, EventArgs e)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(AddUserWindow))
                {
                    window.Close();
                }
            }
        }

        private void ToggleUserStatusBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var user = DgUsers.SelectedItem as ApplicationUserModel;
            var action = user.IsActive ? "Deactivate" : "Activate";
            var isSure = MessageBox.Show($"Are you sure you want to {action} {user.Username}?", "Confirm",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (isSure == MessageBoxResult.No) return;

            var message = _db.ToggleUserActive(user.Username);
            MessageBox.Show(message);
            LoadData();
        }

        private void AddUserBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var addWindow = App.serviceProvider.GetRequiredService<AddUserWindow>();
            addWindow.Owner = this;

            addWindow.UserSaved += (s, args) =>
            {
                LoadData();
            };

            addWindow.Show();
        }

        private void SearchUserBtn_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SearchTxt.Text))
                LoadData();

            DgUsers.ItemsSource = DgUsers.ItemsSource.Cast<ApplicationUserModel>()
                .Where(x => x.Username.ToLower().Contains(SearchTxt.Text.ToLower()))
                .OrderBy(x => x.Username);
        }
    }
}
