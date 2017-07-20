using SRDocuments.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SRDocuments.Data
{
    public interface IConnection
    {
        List<Notification> listHomeNotifications(string id);
        int countNotReadNotifications(string id);
        int nSentDocuments(string id);
        int nReceivedDocuments(string id);
        void insertBlockToken(string id, string token);
        void blockUser(string id);
        void insertUnblockToken(string id, string token);
        void unblockUser(string id);
        int addDocument(Document n);
        void addDocumentImage(DocumentImage n);
        void addNotification(Notification n);
        List<Document> getAllSentDocuments(string id);
        List<Document> getAllReceivedDocuments(string id);
        Document getDocumentDetails(int id);
        Document getDocumentToDelete(int id);
        Document getDocumentToReply(int id);
        void deleteDocument(int id);
        int getNumberOfLastImage(int id);
        void AddDocumentRepliedInfo(Document document);
        Document getDocumentToDA(int id);
        void updateDocumentDA(Document document);
        List<ApplicationUser> getOtherUsersInfo(string id);
        List<Notification> listNotifications(string id);
    }
}
