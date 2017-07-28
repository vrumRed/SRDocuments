using SRDocuments.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SRDocuments.Data
{
    public interface IConnection
    {
        Task<List<Notification>> listHomeNotifications(string id);
        Task<int> countNotReadNotifications(string id);
        Task<int> nSentDocuments(string id);
        Task<int> nReceivedDocuments(string id);
        Task insertBlockToken(string id, string token);
        Task blockUser(string id);
        Task insertUnblockToken(string id, string token);
        Task unblockUser(string email);
        Task<int> addDocument(Document n);
        Task addDocumentImage(DocumentImage n);
        Task addNotification(Notification n);
        Task<List<Document>> getAllSentDocuments(string id);
        Task<List<Document>> getAllReceivedDocuments(string id);
        Task<Document> getDocumentDetails(int id);
        Task<Document> getDocumentToDelete(int id);
        Task<Document> getDocumentToReply(int id);
        Task deleteDocument(int id);
        Task<int> getNumberOfLastImage(int id);
        Task AddDocumentRepliedInfo(Document document);
        Task<Document> getDocumentToDA(int id);
        Task updateDocumentDA(Document document);
        Task<List<ApplicationUser>> getOtherUsersInfo(string id);
        Task<List<Notification>> listNotifications(string id);
        Task<bool> addChat(Chat chat);
        Task<Chat> getChat(ApplicationUser user1, ApplicationUser user2, int documentId);
        Task<Chat> getChat(int chatId);
        Task<bool> chatExists(int chatId, int documentId, string userId);
        Task sendMessage(Message message);
    }
}
