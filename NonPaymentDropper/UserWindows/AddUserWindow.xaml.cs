using System.Windows;
using NonPaymentDropper.Library.DataAccess;
using SmallApp.Library.DataAccess;

namespace NonPaymentDropper.UserWindows
{
    public partial class AddUserWindow : Window
    {
        private readonly IApplicationUserDataAccess _db;
        public event EventHandler? UserSaved;

        public AddUserWindow(IApplicationUserDataAccess db)
        {
            _db = db;
            InitializeComponent();
        }

        private void SaveBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var areYouSure = MessageBox.Show("Are you sure you want to save this user?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (areYouSure == MessageBoxResult.No) return;

            var message = _db.AddUser(UsernameTxt.Text);
            if (message.StartsWith("Error"))
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else
                MessageBox.Show(message, "Succes", MessageBoxButton.OK, MessageBoxImage.Information);

            UserSaved?.Invoke(this, EventArgs.Empty);
            this.Close();
        }
    }
}
