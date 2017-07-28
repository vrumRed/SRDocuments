using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using SRDocuments.Models;
using SRDocuments.Data;

namespace SRDocuments.Controllers
{
    public class ChatController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConnection _conn;

        public ChatController(UserManager<ApplicationUser> userManager, IConnection conn)
        {
            _userManager = userManager;
            _conn = conn;
        }

        public async Task<IActionResult> Index(string email, int documentId)
        {
            var user1 = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            var user2 = await _userManager.FindByEmailAsync(email);
            if(user1 == null || user2 == null)
            {
                return RedirectToAction("Error", "Home", new { statusCode = 404 });
            }
            Chat chat = await _conn.getChat(user1, user2, documentId);

            if(chat == null)
            {
                chat = new Chat
                {
                    DocumentID = documentId,
                    Person1ID = user1.Id,
                    Person2ID = user2.Id
                };

                if (!(await _conn.addChat(chat)))
                {
                    return RedirectToAction("Error", "Home", new { statusCode = 404 });
                }
                chat = await _conn.getChat(user1, user2, documentId);
            }

            return View(chat);
        }

        [HttpPost]
        public async Task<IActionResult> Index(Chat chat, string message)
        {
            if (!(await _conn.chatExists(chat.ChatID, chat.DocumentID, _userManager.GetUserId(User))))
            {
                return RedirectToAction("Error", "Home", new { statusCode = 404 });
            }

            Message mess = new Message
            {
                ChatID = chat.ChatID,
                SentByID = _userManager.GetUserId(User),
                SentDate = getTodayDate(),
                Text = message
            };

            await _conn.sendMessage(mess);

            return View(await _conn.getChat(chat.ChatID));
        }

        private string getTodayDate()
        {
            string day = (DateTime.Today.Day >= 10) ? DateTime.Today.Day.ToString() : "0" + DateTime.Today.Day.ToString();
            string mon = (DateTime.Today.Month >= 10) ? DateTime.Today.Month.ToString() : "0" + DateTime.Today.Month.ToString();
            return day + "/" + mon + "/" + DateTime.Today.Year.ToString();
        }
    }
}
