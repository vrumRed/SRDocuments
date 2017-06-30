using SRDocuments.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SRDocuments.Data
{
    public interface IConnection
    {
        List<Notification> listNotifications(string id);
    }
}
