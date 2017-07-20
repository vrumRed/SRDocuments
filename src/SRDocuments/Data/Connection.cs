using Dapper;
using Microsoft.Extensions.Options;
using SRDocuments.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace SRDocuments.Data
{
    public class Connection : IConnection
    {
        private readonly CustomSettings _settings;

        public Connection(IOptions<CustomSettings> settings)
        {
            _settings = settings.Value;
        }

        public List<Notification> listHomeNotifications(string id)
        {
            List<Notification> list;
            int n = 0;
            using (var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "SELECT * FROM dbo.Notifications WHERE NotificationUserId = @ID AND wasRead = 'false'";
                list = db.Query<Notification>(query, new { ID = id }).ToList();
                query = "SELECT COUNT(NotificationID) FROM dbo.Notifications WHERE NotificationUserID = @ID";
                n = db.Query<int>(query, new { ID = id }).FirstOrDefault();
            }

            list.Reverse();
            foreach (var l in list)
            {
                l.Number = n--;
            }

            List<Notification> list2 = new List<Notification>();
            n = list.Count;
            int i = 0;
            foreach (var l in list)
            {
                if (i == n || i == 3) break;
                list2.Add(l);
                i++;
            }

            return list2;
        }

        public int countNotReadNotifications(string id)
        {
            int i = 0;
            using(var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "SELECT COUNT(NotificationID) FROM dbo.Notifications WHERE NotificationUserID = @NUI AND wasRead = @WR";
                i = db.Query<int>(query, new { NUI = id, WR = false }).FirstOrDefault();
            }
            return i;
        }

        public int nReceivedDocuments(string id)
        {
            int n;
            using (var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "SELECT COUNT(DocumentID) FROM dbo.Documents WHERE SentToID = @ID";
                var result = db.Query<int>(query, new { ID = id });

                n = result.FirstOrDefault();
            }
            return n;
        }

        public int nSentDocuments(string id)
        {
            int n;
            using (var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "SELECT COUNT(DocumentID) FROM dbo.Documents WHERE SentByID = @ID";
                var result = db.Query<int>(query, new { ID = id });

                n = result.FirstOrDefault();
            }
            return n;
        }

        public void insertBlockToken(string id, string token)
        {
            using (var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "UPDATE dbo.AspNetUsers SET BlockToken = @TOKEN WHERE Id = @ID";
                db.Query(query, new { TOKEN = token, ID = id });
            }
        }

        public void blockUser(string id)
        {
            using (var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "UPDATE dbo.AspNetUsers SET IsBlocked = 1 WHERE Id = @ID";
                db.Query(query, new { ID = id });
            }
        }

        public void insertUnblockToken(string id, string token)
        {
            using (var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "UPDATE dbo.AspNetUsers SET UnblockToken = @TOKEN WHERE Id = @ID";
                db.Query(query, new { TOKEN = token, ID = id });
            }
        }

        public void unblockUser(string id)
        {
            using (var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "UPDATE dbo.AspNetUsers SET IsBlocked = 0 WHERE Id = @ID";
                db.Query(query, new { ID = id });
            }
        }

        public int addDocument(Document n)
        {
            int id;
            using (var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "INSERT INTO dbo.Documents(Name, Description, SentToID, SentByID, SentDate, SentImagesRarLocale, RequiredDate)" +
                    " VALUES (@N, @D, @STI, @SBI, @SD, @SIRL, @RD)";
                db.Query(query, new { N = n.Name, D = n.Description, STI = n.SentToID, SBI = n.SentByID, SD = n.SentDate, SIRL = n.SentImagesRarLocale, RD = n.RequiredDate });
                query = "SELECT DocumentID FROM dbo.Documents";
                var result = db.Query<int>(query);
                id = result.Last();
            }
            return id;
        }

        public void addDocumentImage(DocumentImage n)
        {
            using (var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "INSERT INTO dbo.DocumentImages(DocumentID, Locale, Name, Original)" +
                    " VALUES (@DID, @L, @N, @O)";
                db.Query(query, new { DID = n.DocumentID, L = n.Locale, N = n.Name, O = n.Original });
            }
        }

        public void addNotification(Notification n)
        {
            using(var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "INSERT INTO dbo.Notifications(NotificationUserID, Message, wasRead) VALUES (@NUID, @M, 0)";
                db.Query(query, new { NUID = n.NotificationUserID, M = n.Message});
            }
        }

        public List<Document> getAllSentDocuments(string id)
        {
            List<Document> final = new List<Document>();
            using (var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "SELECT DocumentID, Name, SentDate, SentToID, Finished, NotAccepted, AnswerDate, VisualizationDate FROM dbo.Documents WHERE SentByID = @SBID";
                var result = db.Query<Document>(query, new { SBID = id });
                List<Document> ds = result.ToList();
                
                foreach (Document d in ds)
                {
                    query = "SELECT FullName FROM dbo.AspNetUsers WHERE Id = @I";
                    d.SentTo = db.Query<ApplicationUser>(query, new { I = d.SentToID }).First();
                    final.Add(d);
                }
            }
            return final;
        }

        public List<Document> getAllReceivedDocuments(string id)
        {
            List<Document> final = new List<Document>();
            using (var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "SELECT DocumentID, Name, SentDate, SentByID, Finished, NotAccepted, AnswerDate FROM dbo.Documents WHERE SentToID = @STID";
                var result = db.Query<Document>(query, new { STID = id });
                List<Document> ds = result.ToList();
                foreach (Document d in ds)
                {
                    query = "SELECT FullName FROM dbo.AspNetUsers WHERE Id = @I";
                    d.SentBy = db.Query<ApplicationUser>(query, new { I = d.SentByID }).First();
                    final.Add(d);
                }
            }
            return final;
        }

        public Document getDocumentDetails(int id)
        {
            Document result;
            using(var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "SELECT Name, Description, SentDate, RequiredDate, AnswerDate, ConclusionDate, SentImagesRarLocale, ReceivedImagesRarLocale, SentByID, SentToID, NotAccepted, Finished FROM dbo.Documents WHERE DocumentID = @I";
                result = db.Query<Document>(query, new { I = id }).SingleOrDefault();
                
                try
                {
                    query = "SELECT Original, Name, Locale FROM dbo.DocumentImages WHERE DocumentID = @DI";
                    result.DocumentImages = db.Query<DocumentImage>(query, new { DI = id }).ToList();
                }
                catch (NullReferenceException)
                {
                    
                }
            }
            return result;
        }

        public Document getDocumentToDelete(int id)
        {
            Document result;
            using(var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "SELECT Finished, NotAccepted, AnswerDate, VisualizationDate, SentByID, SentToID, SentDate FROM dbo.Documents WHERE DocumentID = @I";
                result = db.Query<Document>(query, new { I = id }).FirstOrDefault();
            }
            return result;
        }

        public Document getDocumentToReply(int id)
        {
            Document result;
            using (var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "SELECT DocumentID, Finished, NotAccepted, AnswerDate, SentByID, SentToID, SentDate, Description, Name, ReceivedImagesRarLocale FROM dbo.Documents WHERE DocumentID = @I";
                result = db.Query<Document>(query, new { I = id }).FirstOrDefault();
                try
                {
                    query = "SELECT FullName FROM dbo.AspNetUsers WHERE Id = @I";
                    result.SentBy = db.Query<ApplicationUser>(query, new { I = result.SentByID }).FirstOrDefault();
                }
                catch (NullReferenceException)
                {

                }
            }
            return result;
        }

        public void deleteDocument(int id)
        {
            using(var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "SELECT Locale FROM dbo.DocumentImages WHERE DocumentID = @I";
                var result = db.Query<string>(query, new { I = id }).ToList();
                foreach(var s in result)
                {
                    System.IO.File.Delete($"wwwroot\\{s}");
                }
                query = "SELECT SentImagesRarLocale FROM dbo.Documents WHERE DocumentID = @I";
                var result2 = db.Query<string>(query, new { I = id }).First();
                System.IO.File.Delete($"wwwroot\\{result2}");
                query = "DELETE FROM dbo.DocumentImages WHERE DocumentID = @I; DELETE FROM dbo.Documents WHERE DocumentID = @I";
                db.Query(query, new { I = id });

            }
        }

        public int getNumberOfLastImage(int id)
        {
            int number = 1;

            using(var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "SELECT Locale FROM dbo.DocumentImages WHERE DocumentID = @DI AND Original = @O";
                var result = db.Query<string>(query, new { DI = id, O = false }).Last().Substring(65);
                int i = 0;
                while (result.ElementAt(i) != '.') i++;
                result = result.Substring(0, i);
                number = Convert.ToInt32(result);
            }

            return number;
        }

        public void AddDocumentRepliedInfo(Document document)
        {
            using(var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "UPDATE dbo.Documents SET AnswerDate = @AD, ReceivedImagesRarLocale = @RIRL, NotAccepted = @NA WHERE DocumentID = @DI";
                db.Query(query, new { AD = document.AnswerDate, RIRL = document.ReceivedImagesRarLocale, DI = document.DocumentID, NA = document.NotAccepted });
            }
        }

        public Document getDocumentToDA(int id)
        {
            Document result;
            using(var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "SELECT AnswerDate, Finished, NotAccepted, SentByID, SentToID, DocumentID, ConclusionDate FROM dbo.Documents WHERE DocumentID = @DI";
                result = db.Query<Document>(query, new { DI = id }).FirstOrDefault();
            }
            return result;
        }

        public void updateDocumentDA(Document document)
        {
            using(var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "UPDATE dbo.Documents SET NotAccepted = @NA, Finished = @F, ConclusionDate = @CD WHERE DocumentID = @DI";
                db.Query(query, new { NA = document.NotAccepted, F = document.Finished, CD = document.ConclusionDate, DI = document.DocumentID });
            }
        }

        public List<ApplicationUser> getOtherUsersInfo(string id)
        {
            List<ApplicationUser> users = new List<ApplicationUser>();

            using(var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "SELECT Id, Info FROM dbo.AspNetUsers WHERE Id != @I AND IsBlocked = @IB AND EmailConfirmed = @EC";
                users = db.Query<ApplicationUser>(query, new { I = id, IB = false, EC = true }).ToList();
            }

            return users;
        }

        public List<Notification> listNotifications(string id)
        {
            List<Notification> last = new List<Notification>();
            using (var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "SELECT wasRead, Message FROM dbo.Notifications WHERE NotificationUserID = @NUI";
                var result = db.Query<Notification>(query, new { NUI = id }).ToList();
                var i = 1;
                foreach (var l in result)
                {
                    l.Number = i++;
                    last.Add(l);
                }
                query = "UPDATE dbo.Notifications SET wasRead = @WR WHERE NotificationUserID = @NUI";
                db.Query(query, new { WR = true, NUI = id });
            }            
            last.Reverse();
            return last;
        }
    }
}
