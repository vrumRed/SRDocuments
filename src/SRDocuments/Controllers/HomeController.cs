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

        public HomeController(IConnection conn, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _conn = conn;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Login", "Account");
            }

            List<Notification> list = _conn.listHomeNotifications(_userManager.GetUserId(User));

            ViewBag.nNotifications = _conn.countNotReadNotifications(_userManager.GetUserId(User));
            ViewBag.nSentDocuments = _conn.nSentDocuments(_userManager.GetUserId(User));
            ViewBag.nReceivedDocuments = _conn.nReceivedDocuments(_userManager.GetUserId(User));
            return View(list);
        }

        [HttpGet("/Error/{statusCode}")]
        public IActionResult Error(int statusCode)
        {
            return View(statusCode);
        }
    }
}
