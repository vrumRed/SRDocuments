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
                var query = "UPDATE dbo.AspNetUsers SET IsBlocked = 1, BlockToken = @TOKEN WHERE Id = @ID";
                db.Query(query, new { TOKEN = (string)null, ID = id });
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

        public void unblockUser(string email)
        {
            using (var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "UPDATE dbo.AspNetUsers SET IsBlocked = 0, UnblockToken = @TOKEN WHERE Email = @E";
                db.Query(query, new { TOKEN = (string)null, E = email });
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
                var query = "INSERT INTO dbo.DocumentImages(DocumentID, Locale, Name, Original, DateSent)" +
                    " VALUES (@DID, @L, @N, @O, @DS)";
                db.Query(query, new { DID = n.DocumentID, L = n.Locale, N = n.Name, O = n.Original, DS = n.DateSent });
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
                    query = "SELECT FullName, Email FROM dbo.AspNetUsers WHERE Id = @I";
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
                    query = "SELECT FullName, Email FROM dbo.AspNetUsers WHERE Id = @I";
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

        public bool addChat(Chat chat)
        {
            using(var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "SELECT SentByID, SentToID FROM dbo.Documents WHERE DocumentID = @D";
                var result = db.Query<Document>(query, new { D = chat.DocumentID }).FirstOrDefault();
                if((result.SentByID != chat.Person1ID && result.SentToID != chat.Person2ID) && (result.SentByID != chat.Person2ID && result.SentToID != chat.Person1ID))
                {
                    return false;
                }
                query = "INSERT INTO dbo.Chats(DocumentID, Person1ID, Person2ID) VALUES (@D, @P1, @P2)";
                db.Query(query, new { D = chat.DocumentID, P1 = chat.Person1ID, P2 = chat.Person2ID });
            }
            return true;
        }

        public Chat getChat(ApplicationUser user1, ApplicationUser user2, int documentId)
        {
            Chat result;
            using (var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "SELECT * FROM dbo.Chats WHERE ((Person1ID = @P1 AND Person2ID = @P2) OR (Person1ID = @P2 AND Person2ID = @P1)) AND DocumentID = @D";
                result = db.Query<Chat>(query, new { P1 = user1.Id, P2 = user2.Id, D = documentId }).FirstOrDefault();
                if (result == null)
                {
                    return result;
                }
                query = "SELECT * FROM dbo.Messages WHERE ChatID = @C";
                List<Message> ms = db.Query<Message>(query, new { C = result.ChatID }).ToList();
                result.Messages = new List<Message>();
                foreach (var m in ms)
                {
                    query = "SELECT FullName FROM dbo.AspNetUsers WHERE Id = @S";
                    m.SentBy = db.Query<ApplicationUser>(query, new { S = m.SentByID }).FirstOrDefault();
                    result.Messages.Add(m);
                }
            }
            return result;
        }

        public bool chatExists(int chatId, int documentId, string userId)
        {
            bool b = true;
            using(var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "SELECT ChatID FROM dbo.Chats WHERE ChatID = @C AND DocumentID = @D AND (Person1ID = @P OR Person2ID = @P)";
                var result = db.Query(query, new { C = chatId, D = documentId, P = userId });
                if(result == null)
                {
                    b = false;
                }
            }
            return b;
        }

        public Chat getChat(int chatId)
        {
            Chat result;
            using(var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "SELECT ChatID, DocumentID FROM dbo.Chats WHERE ChatID = @C";
                result = db.Query<Chat>(query, new { C = chatId }).FirstOrDefault();
                if (result == null)
                {
                    return result;
                }
                query = "SELECT * FROM dbo.Messages WHERE ChatID = @C";
                List<Message> ms = db.Query<Message>(query, new { C = result.ChatID }).ToList();
                result.Messages = new List<Message>();
                foreach(var m in ms)
                {
                    query = "SELECT FullName FROM dbo.AspNetUsers WHERE Id = @S";
                    m.SentBy = db.Query<ApplicationUser>(query, new { S = m.SentByID }).FirstOrDefault();
                    result.Messages.Add(m);
                }
                
            }
            return result;
        }

        public void sendMessage(Message message)
        {
            using (var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "INSERT INTO dbo.Messages(ChatID, SentByID, SentDate, Text) VALUES (@C, @SB, @SD, @T)";
                db.Query(query, new { C = message.ChatID, SB = message.SentByID, SD = message.SentDate, T = message.Text });
            }
        }
    }
}
