﻿using System;
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

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace SRDocuments.Controllers
{
    public class ManageController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public ManageController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
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
            user.BlockToken = token;
            _context.Update(user);
            await _context.SaveChangesAsync();
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
            if (user.BlockToken != token)
            {
                return RedirectToAction("Error", "Home", new { statusCode = 404 });
            }
            user.IsBlocked = true;
            _context.Update(user);
            await _context.SaveChangesAsync();
            if (_signInManager.IsSignedIn(User))
            {
                await _signInManager.SignOutAsync();
            }

            return RedirectToAction("Index", "Home");
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
            user.UnblockToken = token;
            _context.Update(user);
            await _context.SaveChangesAsync();
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
            if (user == null || user.UnblockToken != token)
            {
                return RedirectToAction("Error", "Home", new { statusCode = 404 });
            }
            user.IsBlocked = false;
            _context.Update(user);
            await _context.SaveChangesAsync();
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
