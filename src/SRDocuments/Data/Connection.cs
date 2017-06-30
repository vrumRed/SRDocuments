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
    public class Connection
    {
        private readonly CustomSettings _settings;

        public Connection(IOptions<CustomSettings> settings)
        {
            _settings = settings.Value;
        }

        public List<Notification> getNotifications(string id)
        {
            List<Notification> list;
            using (var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "SELECT * FROM dbo.Notifications WHERE NotificationUserId = @ID";
                var result = db.Query<Notification>(query, new { ID = id });

                list = result.ToList();
            }
            return list;
        }
    }
}
