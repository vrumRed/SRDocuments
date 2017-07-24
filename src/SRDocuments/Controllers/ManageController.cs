using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using SRDocuments.Models;
using SRDocuments.Data;
using FluentEmail.Mailgun;
using FluentEmail.Core;
using SRDocuments.Models.ManageViewModels;

namespace SRDocuments.Controllers
{
    public class ManageController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConnection _conn;

        public ManageController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConnection conn)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _conn = conn;
        }

        public IActionResult Index()
        {
            if(TempData["manageIn"] != null)
            {
                ViewBag.manageIn = TempData["manageIn"];
            }
            return View();
        }

        public async Task<string> RequestBlockAccount()
        {
            //temporario
            var token = Guid.NewGuid().ToString();
            var user = await _userManager.FindByIdAsync(_userManager.GetUserId(User));

            _conn.insertBlockToken(user.Id, token);

            if (await sendBlockEmail(user, token))
            {
                return $"Block request sent to {user.Email}";
            }
            else
            {
                 return $"Failed to send block request";
            }
        }

        public async Task<IActionResult> BlockAccount(string email, string token)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if(user == null)
            {
                return RedirectToAction("Error", "Home", new { statusCode = 404 });
            }

            if (user.BlockToken != token)
            {
                return RedirectToAction("Error", "Home", new { statusCode = -5 });
            }

            _conn.blockUser(user.Id);

            if (_signInManager.IsSignedIn(User))
            {
                await _signInManager.SignOutAsync();
            }

            TempData["resultado"] = $"Usuário {user.Email} bloqueado com sucesso.";
            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        public async Task<IActionResult> RequestUnblockAccount(string email)
        {       
            //temporario
            var token = Guid.NewGuid().ToString();
            var user = await _userManager.FindByEmailAsync(email);
            if(user == null)
            {
                return RedirectToAction("Error", "Home", new { statusCode = 404 });
            }

            _conn.insertUnblockToken(user.Id, token);

            if (await sendUnblockEmail(user, token))
            {
                TempData["resultado"] = $"Unblock Account Email sent to {user.Email}";
            }
            else
            {
                TempData["resultado"] = $"Failed to send Unblock Account Email";
            }
            return RedirectToAction("Login", "Account");
        }

        public async Task<IActionResult> UnblockAccount(string email, string token)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Error", "Home", new { statusCode = 404 });
            }

            if (user == null)
            {
                TempData["resultado"] = $"The user doesn't exist.";
                return RedirectToAction("Login", "Account");
            }

            if (!user.IsBlocked)
            {
                TempData["resultado"] = $"Account not blocked.";
                return RedirectToAction("Login", "Account");
            }

            if (user.UnblockToken != token)
            {
                TempData["resultado"] = $"Incorrect token.";
                return RedirectToAction("Login", "Account");
            }

            _conn.unblockUser(email);

            TempData["resultado"] = $"Account unblocked";
            return RedirectToAction("Login", "Account");
        }

        private async Task<bool> sendBlockEmail(ApplicationUser someUser, string token2)
        {
            var link = Url.Action("BlockAccount",
                          "Manage", new
                          {
                              email = someUser.Email,
                              token = token2
                          },
                           protocol: HttpContext.Request.Scheme);

            var sender = new MailgunSender(
                "sandbox73991a628abf4549a9e0fd6b1ef61415.mailgun.org", // Mailgun Domain
                "key-2218a1c56f1a43865cb11c0bcb51b1a7" // Mailgun API Key
            );
            Email.DefaultSender = sender;

            var email = Email
                .From("valid.neverreply@sandbox73991a628abf4549a9e0fd6b1ef61415.mailgun.org", "Valid SDR")
                .To(someUser.Email)
                .Subject("Block Account")
                .Body($"Link aqui: \n{link}");

            var result = await email.SendAsync();
            return result.Successful;
        }

        private async Task<bool> sendUnblockEmail(ApplicationUser someUser, string token2)
        {
            var link = Url.Action("UnblockAccount",
                          "Manage", new
                          {
                              email = someUser.Email,
                              token = token2
                          },
                           protocol: HttpContext.Request.Scheme);

            var sender = new MailgunSender(
                "sandbox73991a628abf4549a9e0fd6b1ef61415.mailgun.org", // Mailgun Domain
                "key-2218a1c56f1a43865cb11c0bcb51b1a7" // Mailgun API Key
            );
            Email.DefaultSender = sender;

            var email = Email
                .From("valid.neverreply@sandbox73991a628abf4549a9e0fd6b1ef61415.mailgun.org", "Valid SDR")
                .To(someUser.Email)
                .Subject("Unblock Account")
                .Body($"Link aqui: \n{link}");

            var result = await email.SendAsync();
            return result.Successful;
        }
    }
}
