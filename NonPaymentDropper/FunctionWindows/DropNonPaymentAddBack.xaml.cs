using System.Reflection.Emit;
using System.Windows;
using NonPaymentDropper.Library.DataAccess;
using SmallApp.Library.Authentication;
using SmallApp.Library.DataAccess;
using SmallApp.Library.Models;

namespace NonPaymentDropper.FunctionWindows
{
    public partial class DropNonPaymentAddBack : Window
    {
        private readonly IInHouseDataAccess _db;
        private readonly IUserSession _userSession;
        private readonly IGeneralDataPulls _codes;

        public DropNonPaymentAddBack(IInHouseDataAccess db, IUserSession userSession, IGeneralDataPulls codes)
        {
            _db = db;
            _userSession = userSession;
            _codes = codes;
            InitializeComponent();
            LoadData();
            // Run In Test is checked by default
            RunInTestCb.IsChecked = true;
        }

        private void LoadData()
        {
            EmailTemplateSelectCmb.ItemsSource = _db.GetEmailTemplates();
            EmailTemplateSelectCmb.DisplayMemberPath = "TemplateName";
            EmailTemplateSelectCmb.SelectedValuePath = "Id";

            var options = _codes.GetAcademicSessions();
            SessionsLb.ItemsSource = options;
            SessionsLb.DisplayMemberPath = "LONG_DESC";
            SessionsLb.SelectedValuePath = "CODE_VALUE_KEY";

            var years = _codes.GetAcademicYears();
            YearCmb.ItemsSource = years;

            var terms = _codes.GetAcademicTerms();
            TermCmb.ItemsSource = terms;
            TermCmb.DisplayMemberPath = "LONG_DESC";
            TermCmb.SelectedValuePath = "CODE_VALUE_KEY";
        }

        private void BrowseBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*"
            };
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                FilePathTb.Text = openFileDialog.FileName;
            }
        }

        private void RunInTestCb_OnChecked(object sender, RoutedEventArgs e)
        {
            EmailTemplateSelectCmb.Visibility = Visibility.Collapsed;
            EmailTemplateSelectLbl.Visibility = Visibility.Collapsed;
            SendTestEmailNowBtn.Visibility = Visibility.Collapsed;
            SendEmailSelfLbl.Visibility = Visibility.Collapsed;
            SendEmailToSelfCb.Visibility = Visibility.Collapsed;
            SendEmailLbl.Visibility = Visibility.Collapsed;
            SendEmailCb.Visibility = Visibility.Collapsed;
            TextMessageTb.Visibility = Visibility.Collapsed;
            TextTextingBtn.Visibility = Visibility.Collapsed;
            TextMessageLbl.Visibility = Visibility.Collapsed;

            SendEmailToSelfCb.IsChecked = false;
            SendEmailCb.IsChecked = false;
            SendEmailToSelfCb.IsChecked = false;
            EmailTemplateSelectCmb.SelectedValue = null;
            TextMessageTb.Text = string.Empty;
        }

        private void RunInTestCb_OnUnchecked(object sender, RoutedEventArgs e)
        {
            EmailTemplateSelectCmb.Visibility = Visibility.Visible;
            EmailTemplateSelectLbl.Visibility = Visibility.Visible;
            SendTestEmailNowBtn.Visibility = Visibility.Visible;
            SendEmailSelfLbl.Visibility = Visibility.Visible;
            SendEmailToSelfCb.Visibility = Visibility.Visible;
            SendEmailLbl.Visibility = Visibility.Visible;
            SendEmailCb.Visibility = Visibility.Visible;
            TextMessageTb.Visibility = Visibility.Visible;
            TextTextingBtn.Visibility = Visibility.Visible;
            TextMessageLbl.Visibility = Visibility.Visible;
        }

        private void SendTestEmailNowBtn_OnClick(object sender, RoutedEventArgs e)
        {
            if (EmailTemplateSelectCmb.SelectedValue == null)
            {
                MessageBox.Show("Please select an email template.", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            SendSelfEmail();
        }

        private void SendSelfEmail()
        {
            var userPcid = _userSession.GetPowerCampusId();
            var emailId = (Guid)EmailTemplateSelectCmb.SelectedValue;

            var message = _db.SendEmailNotification(userPcid, emailId);
            MessageBox.Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UploadBtn_OnClick(object sender, RoutedEventArgs e)
        {
            if (!AllRequiredFieldsFilled()) return;

            var emailId = EmailTemplateSelectCmb.SelectedValue != null && SendEmailCb.IsChecked == true
                ? (Guid)EmailTemplateSelectCmb.SelectedValue
                : Guid.Empty;

            // join all selected sessions into a comma-separated string
            var sessions = string.Join(",",
                SessionsLb.SelectedItems.Cast<GenericCodeModel>().Select(x => x.CODE_VALUE_KEY));

            var parameters = new
            {
                FilePath = FilePathTb.Text,
                AcademicYear = YearCmb.SelectedValue.ToString(),
                AcademicTerm = TermCmb.SelectedValue.ToString(),
                AcademicSessions = sessions,
                Testing = RunInTestCb.IsChecked == true,
                EmailId = emailId,
                TextMessage = TextMessageTb.Text
            };

            var message = _db.ProcessReAddList(parameters.FilePath, parameters.AcademicYear, parameters.AcademicTerm,
                parameters.AcademicSessions, parameters.Testing, parameters.EmailId, parameters.TextMessage);

            if (SendEmailToSelfCb.IsChecked == true)
                SendSelfEmail();

            MessageBox.Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool AllRequiredFieldsFilled()
        {
            if (string.IsNullOrEmpty(FilePathTb.Text))
            {
                MessageBox.Show("Please select a file.", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }

            if (YearCmb.SelectedValue == null)
            {
                MessageBox.Show("Please select an academic year.", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }

            if (TermCmb.SelectedValue == null)
            {
                MessageBox.Show("Please select an academic term.", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }

            if (SessionsLb.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select at least one academic session.", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }

            if (EmailTemplateSelectCmb.SelectedValue == null && SendEmailCb.IsChecked == true)
            {
                MessageBox.Show("Please select an email template.", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private void TextTextingBtn_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextMessageTb.Text))
            {
                MessageBox.Show("Please enter a text message to send.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SendSelfTextMessage();
        }

        private void SendSelfTextMessage()
        {
            var userPcid = _userSession.GetPowerCampusId();
            var textmsg = TextMessageTb.Text;

            var message = _db.SendTextMessage(userPcid, textmsg);
            MessageBox.Show(message, "Text Message Status", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
