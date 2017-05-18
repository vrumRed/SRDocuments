using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using SRDocuments.Models;
using SRDocuments.Data;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace SRDocuments.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public HomeController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Login", "Account");
            }
            var list = _context.Notifications.ToList();
            list.RemoveAll(l => l.NotificationUserId != _userManager.GetUserId(User));
            list.Reverse();
            List<Notification> model = new List<Notification>();
            int i = 0;
            int n = list.ToArray().Length;
            foreach(var l in list)
            {
                l.Number = n--;
            }
            
            list.RemoveAll(l => l.wasRead);
            n = list.ToArray().Length;
            i = 0;
            foreach (var l in list)
            {
                if (i == n || i == 3) break;
                model.Add(l);
                i++;
            }
            ViewBag.nNotifications = list.ToArray().Length;
            ViewBag.nSentDocuments = _context.Documents.Count(d => d.SentById == _userManager.GetUserId(User));
            ViewBag.nReceivedDocuments = _context.Documents.Count(d => d.SentToId == _userManager.GetUserId(User));
            return View(model);
        }

        [HttpGet("/Error/{statusCode}")]
        public IActionResult Error(int statusCode)
        {
            return View(statusCode);
        }
    }
}
