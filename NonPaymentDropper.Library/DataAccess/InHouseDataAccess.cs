using Microsoft.Extensions.Configuration;
using SmallApp.Library;
using SmallApp.Library.Authentication;
using SmallApp.Library.GlobalFeatures.Audit;
using SmallApp.Library.GlobalFeatures.EmailTemplate;
using SmallApp.Library.Helper;

namespace NonPaymentDropper.Library.DataAccess
{
    public class InHouseDataAccess : IInHouseDataAccess
    {
        private readonly ISqlDataAccess _db;
        private readonly IUserSession _userSession;
        private readonly IAuditLogDataAccess _audit;
        private readonly IConfiguration _config;
        private readonly string InHouse = "InHouse";

        public InHouseDataAccess(ISqlDataAccess db, IUserSession userSession, IAuditLogDataAccess audit, IConfiguration config)
        {
            _db = db;
            _userSession = userSession;
            _audit = audit;
            _config = config;
        }

        public string SendTextMessage(string studentId, string message)
        {
            var username = _db.LoadData<string, dynamic>("smallApp.spGetUsername", new { StudentId = studentId },
                InHouse, true).FirstOrDefault();
            var parameters = new
            {
                Username = username,
                Message = message
            };

            try
            {
                _db.SaveData("smallApp.spSendTextMessage", parameters, InHouse, true);
            }
            catch (Exception e)
            {
                return "Error: " + e.Message;
            }

            return "Successfully sent Text Message";
        }

        public List<EmailTemplateModel> GetEmailTemplates()
        {
            var parameters = new
            {
                CategoryId = _config["AppSettings:EmailCategoryId"] ??
                             throw new InvalidOperationException("Missing EmailCategoryId in config")
            };
            return _db.LoadData<EmailTemplateModel, dynamic>(
                "Select * from smallApp.EmailTemplate where CategoryId = @CategoryId",
                parameters, InHouse);
        }

        #region DropProcess
        public string ProcessDropList(string filepath, string year, string term, string sessions, bool testing, Guid emailId, string textMessage)
        {
            var studentIds = new List<string>();
            var csvRows = CsvActions.ReadCSV(filepath);
            foreach (var stu in csvRows)
            {
                var studentId = stu[0].Trim();
                if (!string.IsNullOrEmpty(studentId))
                {
                    try
                    {
                        DropStudentClasses(studentId, year, term, sessions, testing);
                        if (emailId != Guid.Empty && !testing)
                            SendEmailNotification(studentId, emailId);
                        if (!string.IsNullOrEmpty(textMessage) && !testing)
                            SendTextMessage(studentId, textMessage);
                    }
                    catch (Exception e)
                    {
                        return "Error: " + e.Message;
                    }
                    studentIds.Add(studentId);
                }
            }

            if (!testing)
                CreateDropAudit(studentIds, year, term, sessions, emailId);

            return "Success";
        }

        private void DropStudentClasses(string studentId, string year, string term, string sessions, bool testing)
        {
            var parameters = new
            {
                StudentId = studentId,
                AcademicYear = year,
                AcademicTerm = term,
                AcademicSessionList = sessions,
                Testing = testing,
                Opid = _userSession.GetCreateOPID()
            };

            _db.SaveData("smallApp.spDropForNonPayment", parameters, InHouse, true);
        }

        private void CreateDropAudit(List<string> studentIds, string year, string term, string sessions, Guid emailId)
        {
            var sendEmailText = emailId != Guid.Empty ? $"with email notification {emailId} " : "without email notification ";
            var log = "Dropped for non payment " + sendEmailText + $"on students for {term} {year}: {sessions}";
            var logHtml = $"<p>{log}</p><ul>";
            logHtml = studentIds.Aggregate(logHtml, (current, id) => current + $"<li>{id}</li>");
            logHtml += "</ul>";

            _audit.LogAction(log, logHtml);
        }
        #endregion

        #region ReAddProcess
        public string ProcessReAddList(string filepath, string year, string term, string sessions, bool testing, Guid emailId, string textMessage)
        {
            var studentIds = new List<string>();
            var csvRows = CsvActions.ReadCSV(filepath);
            foreach (var stu in csvRows)
            {
                var studentId = stu[0].Trim();
                if (!string.IsNullOrEmpty(studentId))
                {
                    try
                    {
                        ReAddStudentClasses(studentId, year, term, sessions, testing);
                        if (emailId != Guid.Empty && !testing)
                            SendEmailNotification(studentId, emailId);
                        if (!string.IsNullOrEmpty(textMessage) && !testing)
                            SendTextMessage(studentId, textMessage);
                    }
                    catch (Exception e)
                    {
                        return "Error: " + e.Message;
                    }
                    studentIds.Add(studentId);
                }
            }

            if (!testing)
                CreateReAddAudit(studentIds, year, term, sessions, emailId);

            return "Success";
        }
        
        private void ReAddStudentClasses(string studentId, string year, string term, string sessions, bool testing)
        {
            var parameters = new
            {
                StudentId = studentId,
                AcademicYear = year,
                AcademicTerm = term,
                AcademicSessionList = sessions,
                Testing = testing,
                Opid = _userSession.GetCreateOPID()
            };

            _db.SaveData("smallApp.spReAddDroppedForNonPayment", parameters, InHouse, true);
        }

        private void CreateReAddAudit(List<string> studentIds, string year, string term, string sessions, Guid emailId)
        {
            var sendEmailText = emailId != Guid.Empty ? $"with email notification {emailId} " : "without email notification ";
            var log = "ReAdded from non payment drop " + sendEmailText + $"on students for {term} {year}: {sessions}";
            var logHtml = $"<p>{log}</p><ul>";
            logHtml = studentIds.Aggregate(logHtml, (current, id) => current + $"<li>{id}</li>");
            logHtml += "</ul>";

            _audit.LogAction(log, logHtml);
        }
        #endregion

        public string SendEmailNotification(string studentId, Guid emailId)
        {
            var username = _db.LoadData<string, dynamic>("smallApp.spGetUsername", new { StudentId = studentId },
                InHouse, true).FirstOrDefault();
            var parameters = new
            {
                Username = username,
                EmailId = emailId
            };

            try
            {
                _db.SaveData("smallApp.spSendEmailTemplate", parameters, InHouse, true);
            }
            catch (Exception e)
            {
                return "Error: " + e.Message;
            }

            return "Successfully sent Email";
        }
    }
}
