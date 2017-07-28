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

        public async Task<List<Notification>> listHomeNotifications(string id)
        {
            List<Notification> list;
            int n = 0;
            using (var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "SELECT * FROM dbo.Notifications WHERE NotificationUserId = @ID AND wasRead = 'false'";
                list = (await db.QueryAsync<Notification>(query, new { ID = id })).ToList();
                query = "SELECT COUNT(NotificationID) FROM dbo.Notifications WHERE NotificationUserID = @ID";
                n = (await db.QueryAsync<int>(query, new { ID = id })).FirstOrDefault();
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

        public async Task<int> countNotReadNotifications(string id)
        {
            int i = 0;
            using(var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "SELECT COUNT(NotificationID) FROM dbo.Notifications WHERE NotificationUserID = @NotificationUserID AND wasRead = @wasRead";
                i = (await db.QueryAsync<int>(query, new { NotificationUserID = id, wasRead = false })).FirstOrDefault();
            }
            return i;
        }

        public async Task<int> nReceivedDocuments(string id)
        {
            int n;
            using (var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "SELECT COUNT(DocumentID) FROM dbo.Documents WHERE SentToID = @ID";
                n = (await db.QueryAsync<int>(query, new { ID = id })).FirstOrDefault();
            }
            return n;
        }

        public async Task<int> nSentDocuments(string id)
        {
            int n;
            using (var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "SELECT COUNT(DocumentID) FROM dbo.Documents WHERE SentByID = @ID";
                n = (await db.QueryAsync<int>(query, new { ID = id })).FirstOrDefault();
            }
            return n;
        }

        public async Task insertBlockToken(string id, string token)
        {
            using (var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "UPDATE dbo.AspNetUsers SET BlockToken = @BlockToken WHERE Id = @Id";
                await db.ExecuteAsync(query, new { BlockToken = token, Id = id });
            }
        }

        public async Task blockUser(string id)
        {
            using (var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "UPDATE dbo.AspNetUsers SET IsBlocked = @IsBlocked, BlockToken = @BlockToken WHERE Id = @Id";
                await db.ExecuteAsync(query, new { IsBlocked = true,BlockToken = (string)null, Id = id });
            }
        }

        public async Task insertUnblockToken(string id, string token)
        {
            using (var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "UPDATE dbo.AspNetUsers SET UnblockToken = @UnblockToken WHERE Id = @Id";
                await db.ExecuteAsync(query, new { UnblockToken = token, Id = id });
            }
        }

        public async Task unblockUser(string email)
        {
            using (var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "UPDATE dbo.AspNetUsers SET IsBlocked = @IsBlocked, UnblockToken = @UnblockToken WHERE Email = @Email";
                await db.ExecuteAsync(query, new { IsBlocked = false, UnblockToken = (string)null, Email = email });
            }
        }

        public async Task<int> addDocument(Document n)
        {
            int id;
            using (var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "INSERT INTO dbo.Documents(Name, Description, SentToID, SentByID, SentDate, SentImagesRarLocale, RequiredDate)" +
                    " VALUES (@Name, @Description, @SentToID, @SentByID, @SentDate, @SentImagesRarLocale, @RequiredDate)";
                await db.ExecuteAsync(query, n);
                query = "SELECT DocumentID FROM dbo.Documents WHERE SentImagesRarLocale = @SentImagesRarLocale";
                id = (await db.QueryAsync<int>(query, n)).FirstOrDefault();
            }
            return id;
        }

        public async Task addDocumentImage(DocumentImage n)
        {
            using (var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "INSERT INTO dbo.DocumentImages(DocumentID, Locale, Name, Original, DateSent)" +
                    " VALUES (@DocumentID, @Locale, @Name, @Original, @DateSent)";
                await db.ExecuteAsync(query, n);
            }
        }

        public async Task addNotification(Notification n)
        {
            using(var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "INSERT INTO dbo.Notifications(NotificationUserID, Message, wasRead) VALUES (@NotificationUserID, @Message, 0)";
                await db.ExecuteAsync(query, n);
            }
        }

        public async Task<List<Document>> getAllSentDocuments(string id)
        {
            List<Document> final = new List<Document>();
            using (var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "SELECT DocumentID, Name, SentDate, SentToID, Finished, NotAccepted, AnswerDate, VisualizationDate FROM dbo.Documents WHERE SentByID = @SentByID";
                List<Document> ds = (await db.QueryAsync<Document>(query, new { SentByID = id })).ToList();
                
                foreach (Document d in ds)
                {
                    query = "SELECT FullName, Email FROM dbo.AspNetUsers WHERE Id = @SentToID";
                    d.SentTo = (await db.QueryAsync<ApplicationUser>(query, d)).First();
                    final.Add(d);
                }
            }
            return final;
        }

        public async Task<List<Document>> getAllReceivedDocuments(string id)
        {
            List<Document> final = new List<Document>();
            using (var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "SELECT DocumentID, Name, SentDate, SentByID, Finished, NotAccepted, AnswerDate FROM dbo.Documents WHERE SentToID = @SentToID";
                List<Document> ds = (await db.QueryAsync<Document>(query, new { SentToID = id })).ToList();

                foreach (Document d in ds)
                {
                    query = "SELECT FullName, Email FROM dbo.AspNetUsers WHERE Id = @SentByID";
                    d.SentBy = (await db.QueryAsync<ApplicationUser>(query, d)).First();
                    final.Add(d);
                }
            }
            return final;
        }

        public async Task<Document> getDocumentDetails(int id)
        {
            Document result;
            using(var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "SELECT Name, Description, SentDate, RequiredDate, AnswerDate, ConclusionDate, SentImagesRarLocale, ReceivedImagesRarLocale, SentByID, SentToID, NotAccepted, Finished FROM dbo.Documents WHERE DocumentID = @DocumentID";
                result = (await db.QueryAsync<Document>(query, new { DocumentID = id })).SingleOrDefault();
                
                try
                {
                    query = "SELECT Original, Name, Locale FROM dbo.DocumentImages WHERE DocumentID = @DocumentID";
                    result.DocumentImages = (await db.QueryAsync<DocumentImage>(query, new { DocumentID = id })).ToList();
                }
                catch (NullReferenceException)
                {
                    
                }
            }
            return result;
        }

        public async Task<Document> getDocumentToDelete(int id)
        {
            Document result;
            using(var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "SELECT Finished, NotAccepted, AnswerDate, VisualizationDate, SentByID, SentToID, SentDate FROM dbo.Documents WHERE DocumentID = @DocumentID";
                result = (await db.QueryAsync<Document>(query, new { DocumentID = id })).FirstOrDefault();
            }
            return result;
        }

        public async Task<Document> getDocumentToReply(int id)
        {
            Document result;
            using (var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "SELECT DocumentID, Finished, NotAccepted, AnswerDate, SentByID, SentToID, SentDate, Description, Name, ReceivedImagesRarLocale FROM dbo.Documents WHERE DocumentID = @DocumentID";
                result = (await db.QueryAsync<Document>(query, new { DocumentID = id })).FirstOrDefault();
                try
                {
                    query = "SELECT FullName FROM dbo.AspNetUsers WHERE Id = @SentByID";
                    result.SentBy = (await db.QueryAsync<ApplicationUser>(query, result)).FirstOrDefault();
                }
                catch (NullReferenceException)
                {

                }
            }
            return result;
        }

        public async Task deleteDocument(int id)
        {
            using(var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "SELECT Locale FROM dbo.DocumentImages WHERE DocumentID = @DocumentID";
                var result = (await db.QueryAsync<string>(query, new { DocumentID = id })).ToList();

                foreach(var s in result)
                {
                    System.IO.File.Delete($"wwwroot\\{s}");
                }

                query = "SELECT SentImagesRarLocale FROM dbo.Documents WHERE DocumentID = @DocumentID";
                var result2 = (await db.QueryAsync<string>(query, new { DocumentID = id })).FirstOrDefault();
                System.IO.File.Delete($"wwwroot\\{result2}");
                query = "DELETE FROM dbo.DocumentImages WHERE DocumentID = @DocumentID; DELETE FROM dbo.Documents WHERE DocumentID = @DocumentID";
                await db.ExecuteAsync(query, new { DocumentID = id });

            }
        }

        public async Task<int> getNumberOfLastImage(int id)
        {
            int number = 1;

            using(var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "SELECT Locale FROM dbo.DocumentImages WHERE DocumentID = @DI AND Original = @O";
                var result = (await db.QueryAsync<string>(query, new { DI = id, O = false })).Last().Substring(65);
                int i = 0;
                while (result.ElementAt(i) != '.') i++;
                result = result.Substring(0, i);
                number = Convert.ToInt32(result);
            }

            return number;
        }

        public async Task AddDocumentRepliedInfo(Document document)
        {
            using(var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "UPDATE dbo.Documents SET AnswerDate = @AnswerDate, ReceivedImagesRarLocale = @ReceivedImagesRarLocale, NotAccepted = @NotAccepted WHERE DocumentID = @DocumentID";
                await db.ExecuteAsync(query, document);
            }
        }

        public async Task<Document> getDocumentToDA(int id)
        {
            Document result;
            using(var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "SELECT AnswerDate, Finished, NotAccepted, SentByID, SentToID, DocumentID, ConclusionDate FROM dbo.Documents WHERE DocumentID = @DocumentID";
                result = (await db.QueryAsync<Document>(query, new { DocumentID = id })).FirstOrDefault();
            }
            return result;
        }

        public async Task updateDocumentDA(Document document)
        {
            using(var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "UPDATE dbo.Documents SET NotAccepted = @NotAccepted, Finished = @Finished, ConclusionDate = @ConclusionDate WHERE DocumentID = @DocumentID";
                await db.QueryAsync(query, document);
            }
        }

        public async Task<List<ApplicationUser>> getOtherUsersInfo(string id)
        {
            List<ApplicationUser> users = new List<ApplicationUser>();

            using(var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "SELECT Id, Info FROM dbo.AspNetUsers WHERE Id != @Id AND IsBlocked = @IsBlocked AND EmailConfirmed = @EmailConfirmed";
                users = (await db.QueryAsync<ApplicationUser>(query, new { Id = id, IsBlocked = false, EmailConfirmed = true })).ToList();
            }

            return users;
        }

        public async Task<List<Notification>> listNotifications(string id)
        {
            List<Notification> last = new List<Notification>();
            using (var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "SELECT wasRead, Message FROM dbo.Notifications WHERE NotificationUserID = @NotificationUserID";
                var result = (await db.QueryAsync<Notification>(query, new { NotificationUserID = id })).ToList();
                var i = 1;
                foreach (var l in result)
                {
                    l.Number = i++;
                    last.Add(l);
                }
                query = "UPDATE dbo.Notifications SET wasRead = @wasRead WHERE NotificationUserID = @NotificationUserID";
                await db.ExecuteAsync(query, new { wasRead = true, NotificationUserID = id });
            }            
            last.Reverse();
            return last;
        }

        public async Task<bool> addChat(Chat chat)
        {
            using(var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "SELECT SentByID, SentToID FROM dbo.Documents WHERE DocumentID = @DocumentID";
                var result = (await db.QueryAsync<Document>(query, chat)).FirstOrDefault();

                if((result.SentByID != chat.Person1ID && result.SentToID != chat.Person2ID) && (result.SentByID != chat.Person2ID && result.SentToID != chat.Person1ID))
                {
                    return false;
                }

                query = "INSERT INTO dbo.Chats(DocumentID, Person1ID, Person2ID) VALUES (@DocumentID, @Person1ID, @Person2ID)";
                await db.ExecuteAsync(query, chat);
            }
            return true;
        }

        public async Task<Chat> getChat(ApplicationUser user1, ApplicationUser user2, int documentId)
        {
            Chat result;
            using (var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "SELECT * FROM dbo.Chats WHERE ((Person1ID = @Person1ID AND Person2ID = @Person2ID) OR (Person1ID = @Person2ID AND Person2ID = @Person1ID)) AND DocumentID = @DocumentID";
                result = (await db.QueryAsync<Chat>(query, new { Person1ID = user1.Id, Person2ID = user2.Id, DocumentID = documentId })).FirstOrDefault();
                if (result == null)
                {
                    return result;
                }
                query = "SELECT * FROM dbo.Messages WHERE ChatID = @ChatID";
                List<Message> ms = (await db.QueryAsync<Message>(query, result)).ToList();

                result.Messages = new List<Message>();
                foreach (var m in ms)
                {
                    query = "SELECT FullName FROM dbo.AspNetUsers WHERE Id = @SentByID";
                    m.SentBy = (await db.QueryAsync<ApplicationUser>(query, m)).FirstOrDefault();
                    result.Messages.Add(m);
                }
            }
            return result;
        }

        public async Task<bool> chatExists(int chatId, int documentId, string userId)
        {
            bool b = true;
            using(var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "SELECT ChatID FROM dbo.Chats WHERE ChatID = @ChatID AND DocumentID = @DocumentID AND (Person1ID = @PersonID OR Person2ID = @PersonID)";
                var result = await db.QueryAsync(query, new { ChatID = chatId, DocumentID = documentId, PersonID = userId });
                if(result == null)
                {
                    b = false;
                }
            }
            return b;
        }

        public async Task<Chat> getChat(int chatId)
        {
            Chat result;
            using(var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "SELECT ChatID, DocumentID FROM dbo.Chats WHERE ChatID = @ChatID";
                result = (await db.QueryAsync<Chat>(query, new { ChatID = chatId })).FirstOrDefault();
                if (result == null)
                {
                    return result;
                }
                query = "SELECT * FROM dbo.Messages WHERE ChatID = @ChatID";
                List<Message> ms = (await db.QueryAsync<Message>(query, result)).ToList();
                result.Messages = new List<Message>();
                foreach(var m in ms)
                {
                    query = "SELECT FullName FROM dbo.AspNetUsers WHERE Id = @SentByID";
                    m.SentBy = (await db.QueryAsync<ApplicationUser>(query, m)).FirstOrDefault();
                    result.Messages.Add(m);
                }
                query = "UPDATE dbo.Messages SET VisualizationDate = @VisualizationDate";
                await db.ExecuteAsync(query, new { VisualizationDate = getTodayDate() });
                
            }
            return result;
        }

        public async Task sendMessage(Message message)
        {
            using (var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "INSERT INTO dbo.Messages(ChatID, SentByID, SentDate, Text) VALUES (@ChatID, @SentByID, @SentDate, @Text)";
                await db.ExecuteAsync(query, message);
            }
        }

        private string getTodayDate()
        {
            string day = (DateTime.Today.Day >= 10) ? DateTime.Today.Day.ToString() : "0" + DateTime.Today.Day.ToString();
            string mon = (DateTime.Today.Month >= 10) ? DateTime.Today.Month.ToString() : "0" + DateTime.Today.Month.ToString();
            return day + "/" + mon + "/" + DateTime.Today.Year.ToString();
        }
    }
}
