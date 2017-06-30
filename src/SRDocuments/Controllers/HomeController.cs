using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using SRDocuments.Models;
using SRDocuments.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data.SqlClient;
using Dapper;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace SRDocuments.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConnection _conn;
        private readonly CustomSettings _settings;
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context, IConnection conn, IOptions<CustomSettings> settings, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _settings = settings.Value;
            _conn = conn;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Login", "Account");
            }

            List<Notification> list = _conn.listNotifications(_userManager.GetUserId(User));
            /*using(var db = new SqlConnection(_settings.ConnectionString))
            {
                var query = "SELECT * FROM dbo.Notifications WHERE NotificationUserId = @ID";
                var result = db.Query<Notification>(query, new { ID = _userManager.GetUserId(User) });

                list = result.ToList();
            }*/
            
            list.Reverse();
            List<Notification> model = new List<Notification>();
            int i = 0;
            int n = list.ToArray().Length;
            foreach(var l in list)
            {
                l.Number = n--;
            }
            
            n = list.ToArray().Length;
            i = 0;
            foreach (var l in list)
            {
                if (i == n || i == 3) break;
                model.Add(l);
                i++;
            }
            ViewBag.nNotifications = list.ToArray().Length;
            ViewBag.nSentDocuments = _context.Documents.Count(d => d.SentByID == _userManager.GetUserId(User));
            ViewBag.nReceivedDocuments = _context.Documents.Count(d => d.SentToID == _userManager.GetUserId(User));
            return View(model);
        }

        [HttpGet("/Error/{statusCode}")]
        public IActionResult Error(int statusCode)
        {
            return View(statusCode);
        }
    }
}
