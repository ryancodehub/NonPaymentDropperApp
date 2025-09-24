using SmallApp.Library.GlobalFeatures.EmailTemplate;

namespace NonPaymentDropper.Library.DataAccess;

public interface IInHouseDataAccess
{
    string ProcessDropList(string filepath, string year, string term, string sessions, bool testing, Guid emailId, string textMessage);
    string ProcessReAddList(string filepath, string year, string term, string sessions, bool testing, Guid emailId, string textMessage);
    string SendEmailNotification(string studentId, Guid emailId);
    List<EmailTemplateModel> GetEmailTemplates();
    string SendTextMessage(string studentId, string message);
}