﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SRDocuments.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using SRDocuments.Models;
using SRDocuments.Models.ItemViewModels;
using System.IO;
using Microsoft.AspNetCore.Identity;
using System.IO.Compression;
using Microsoft.AspNetCore.Http;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace SRDocuments.Controllers
{
    public class ItemController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ItemController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
            _context = context;
            _userManager = userManager;
        }
        
        [HttpGet]
        public async Task<IActionResult> Send()
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Error", "Home", new { statusCode = -1 });
            }
            ViewData["ReceiverID"] = new SelectList(await getOtherUsersInfo(), "Id", "Info");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Send(SendViewModel model)
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Error", "Home", new { statusCode = -1 });
            }

            ViewData["ReceiverID"] = new SelectList(await getOtherUsersInfo(), "Id", "Info");
            var imageName = getImageName();

            var newDocument = new Document
            {
                Name = model.Name,
                Description = model.Description,
                SentToId = model.ReceiverID,
                SentById = _userManager.GetUserId(User),
                SentDate = getTodayDate(),
                SentImagesRarLocale = "uploads\\original\\" + imageName + ".rar"
            };

            if (model.CheckRequiredDate)
            {
                if (model.RequiredDate == null)
                {
                    ViewBag.rDateError = "Required Date is required";
                    return View(model);
                }
                else if (DateTime.Compare(DateTime.Today, DateTime.Parse(model.RequiredDate)) >= 0)
                {
                    ViewBag.rDateError = "Required Date can't be today or before than today";
                    return View(model);
                }

                newDocument.RequiredDate = model.RequiredDate.Substring(8, 2) + "/" +
                                            model.RequiredDate.Substring(5, 2) + "/" +
                                            model.RequiredDate.Substring(0, 4);
            }

            if (ModelState.IsValid)
            {
                _context.Add(newDocument);
                await _context.SaveChangesAsync();

                var tempDocumentId = _context.Documents.Last(d => d.SentById == _userManager.GetUserId(User)).Id;
                
                var pictureNumber = 1;
                Directory.CreateDirectory("wwwroot\\uploads\\original\\"+imageName);
                foreach (var img in model.Files)
                {
                    var pictureLocale = $"uploads\\original\\{imageName}\\{pictureNumber}{Path.GetExtension(img.FileName)}";
                    using (var fs = new FileStream($"wwwroot\\{pictureLocale}", FileMode.CreateNew))
                    {
                        DocumentImage newDocumentImage = new DocumentImage
                        {
                            Name = $"Document {pictureNumber}",
                            DocumentId = tempDocumentId,
                            Locale = pictureLocale,
                            Original = true
                        };
                        _context.Add(newDocumentImage);
                        img.CopyTo(fs);
                        fs.Flush();
                    }
                    pictureNumber++;
                }
                ZipFile.CreateFromDirectory("wwwroot\\uploads\\original\\" + imageName, "wwwroot\\uploads\\original\\" + imageName +".rar");
                Notification notification = new Notification
                {
                    NotificationUserId = newDocument.SentToId,
                    Message = $"Document sent by {(await _userManager.FindByIdAsync(newDocument.SentById)).FullName} on {newDocument.SentDate}"
                };
                _context.Add(notification);
                await _context.SaveChangesAsync();
                return RedirectToAction("SentList");
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> SentList()
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Error", "Home", new { statusCode = -1 });
            }

            var items = _context.Documents.ToList();
            items.RemoveAll(a => a.SentById != _userManager.GetUserId(User));
            List<Document> itemsFinal = new List<Document>();
            foreach(var item in items)
            {
                item.SentTo = await _userManager.FindByIdAsync(item.SentToId);
                itemsFinal.Add(item);
            }

            return View(itemsFinal);
        }
        
        [HttpGet]
        public async Task<IActionResult> ReceivedList()
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Error", "Home", new { statusCode = -1 });
            }

            var items = _context.Documents.ToList();
            items.RemoveAll(a => a.SentToId != _userManager.GetUserId(User));
            List<Document> itemsFinal = new List<Document>();
            foreach (var item in items)
            {
                item.VisualizationDate = (item.VisualizationDate!=null)?item.VisualizationDate:getTodayDate();
                _context.Update(item);
                item.SentBy = await _userManager.FindByIdAsync(item.SentById);

                itemsFinal.Add(item);
            }
            await _context.SaveChangesAsync();
            return View(itemsFinal);
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            if (!_signInManager.IsSignedIn(User) )
            {
                return RedirectToAction("Error", "Home", new { statusCode = -1 });
            }

            var item = _context.Documents.SingleOrDefault(d => d.Id == id);

            if(item == null || (_userManager.GetUserId(User) != item.SentById && _userManager.GetUserId(User) != item.SentToId))
            {
                return RedirectToAction("Error", "Home", new { statusCode = 404 });
            }

            item.DocumentImages = _context.DocumentImages.ToList();
            item.DocumentImages.RemoveAll(dI => dI.DocumentId != id);
            return View(item);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int deleteValueInput)
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Error", "Home", new { statusCode = -1 });
            }
            var item = _context.Documents.FirstOrDefault(d => d.Id == deleteValueInput);
            if(item == null || item.Finished || item.NotAccepted || item.AnswerDate != null || item.VisualizationDate != null ||
                item.SentById != _userManager.GetUserId(User))
            {
                return RedirectToAction("Error", "Home", new { statusCode = 404 });
            }
            var documentImages = _context.DocumentImages.ToList();
            documentImages.RemoveAll(dI => dI.DocumentId != deleteValueInput);
            foreach(var dI in documentImages)
            {
                System.IO.File.Delete($"wwwroot\\{dI.Locale}");
                _context.Remove(dI);
            }
            System.IO.File.Delete($"wwwroot\\{item.SentImagesRarLocale}");
            _context.Remove(item);
            Notification notification = new Notification
            {
                NotificationUserId = item.SentToId,
                Message = $"Document sent by {(await _userManager.FindByIdAsync(item.SentById)).FullName} on {item.SentDate} was deleted on {getTodayDate()}"
            };
            _context.Add(notification);
            await _context.SaveChangesAsync();
            return RedirectToAction("SentList");
        }

        [HttpGet]
        public IActionResult Reply(int id)
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Error", "Home", new { statusCode = -1 });
            }
            var document = _context.Documents.FirstOrDefault(d => d.Id == id);

            if (document == null || document.Finished || (!document.Finished && !document.NotAccepted && document.AnswerDate != null) || _userManager.GetUserId(User) != document.SentToId)
            {
                return RedirectToAction("Error", "Home", new { statusCode = 404 });
            }

            document.SentBy = _context.Users.FirstOrDefault(x => x.Id == document.SentById);
            return View(document);
        }

        [HttpPost]
        public async Task<IActionResult> Reply(int documentId, List<IFormFile> files)
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Error", "Home", new { statusCode = -1 });
            }

            var document = _context.Documents.FirstOrDefault(d => d.Id == documentId);

            if (document == null || document.Finished || (!document.Finished && !document.NotAccepted && document.AnswerDate != null) || _userManager.GetUserId(User) != document.SentToId)
            {
                return RedirectToAction("Error", "Home", new { statusCode = 404 });
            }

            if (files.Count == 0)
            {
                ViewBag.error = "Necessário pelo menos um arquivo";
                document.SentBy = _context.Users.FirstOrDefault(x => x.Id == document.SentById);
                return View(document);
            }

            if (document.NotAccepted)
            {
                var documentImages = _context.DocumentImages.ToList();
                documentImages.RemoveAll(dI => dI.DocumentId != documentId && dI.Original);
                foreach (var dI in documentImages)
                {
                    System.IO.File.Delete($"wwwroot\\{dI.Locale}");
                    _context.Remove(dI);
                }
                System.IO.File.Delete($"wwwroot\\{document.ReceivedImagesRarLocale}");
                document.NotAccepted = false;
            }

            var imageName = getImageName();
            var pictureNumber = 1;

            document.AnswerDate = getTodayDate();
            document.ReceivedImagesRarLocale = "uploads\\notoriginal\\" + imageName + ".rar";
            _context.Update(document);
            Directory.CreateDirectory("wwwroot\\uploads\\notoriginal\\" + imageName);
            foreach (var img in files)
            {
                var pictureLocale = $"uploads\\notoriginal\\{imageName}\\{pictureNumber}{Path.GetExtension(img.FileName)}";
                using (var fs = new FileStream($"wwwroot\\{pictureLocale}", FileMode.CreateNew))
                {
                    DocumentImage newDocumentImage = new DocumentImage
                    {
                        Name = $"Document {pictureNumber}",
                        DocumentId = documentId,
                        Locale = pictureLocale,
                        Original = false
                    };
                    _context.Add(newDocumentImage);
                    img.CopyTo(fs);
                    fs.Flush();
                }
                pictureNumber++;
            }
            ZipFile.CreateFromDirectory("wwwroot\\uploads\\notoriginal\\" + imageName, "wwwroot\\uploads\\notoriginal\\" + imageName + ".rar");

            Notification notification = new Notification
            {
                NotificationUserId = document.SentById,
                Message = $"Document n°{document.Id} sent to {(await _userManager.FindByIdAsync(document.SentToId)).FullName} on {document.SentDate} was replied on {document.AnswerDate}"
            };
            _context.Add(notification);

            await _context.SaveChangesAsync();
            return RedirectToAction("ReceivedList");
        }

        [HttpPost]
        public async Task<IActionResult> Deny(int denyValueInput)
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Error", "Home", new { statusCode = -1 });
            }
            
            var document = _context.Documents.FirstOrDefault(d => d.Id == denyValueInput);

            if (document == null || document.AnswerDate == null || document.Finished || document.NotAccepted || document.SentById != _userManager.GetUserId(User))
            {
                return RedirectToAction("Error", "Home", new { statusCode = 404 });
            }
            
            Notification notification = new Notification
            {
                NotificationUserId = document.SentToId,
                Message = $"Document n°{document.Id} replied to {(await _userManager.FindByIdAsync(document.SentById)).FullName} on {document.AnswerDate} was denied on {getTodayDate()}"
            };

            document.NotAccepted = true;
            _context.Update(document);
            _context.Add(notification);

            await _context.SaveChangesAsync();
            return RedirectToAction("SentList");
        }

        [HttpPost]
        public async Task<IActionResult> Accept(int acceptValueInput)
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Error", "Home", new { statusCode = -1 });
            }

            var document = _context.Documents.FirstOrDefault(d => d.Id == acceptValueInput);

            if (document == null || document.AnswerDate == null || document.Finished || document.NotAccepted || document.SentById != _userManager.GetUserId(User))
            {
                return RedirectToAction("Error", "Home", new { statusCode = 404 });
            }


            document.NotAccepted = false;
            document.Finished = true;
            document.ConclusionDate = getTodayDate();
            Notification notification = new Notification
            {
                NotificationUserId = document.SentToId,
                Message = $"Document n°{document.Id} replied to {(await _userManager.FindByIdAsync(document.SentById)).FullName} on {document.AnswerDate} was concluded on {document.ConclusionDate}"
            };
            _context.Update(document);
            _context.Add(notification);

            await _context.SaveChangesAsync();
            return RedirectToAction("SentList");
        }

        [HttpGet]
        public IActionResult History()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Notifications()
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Error", "Home", new { statusCode = -1 });
            }
            var list = _context.Notifications.ToList();
            list.RemoveAll(l => l.NotificationUserId != _userManager.GetUserId(User));
            List<Notification> model = new List<Notification>();
            var i = 1;
            foreach (var l in list)
            {
                l.Number = i++;
                l.wasRead = true;
                _context.Update(l);
                model.Add(l);
            }
            model.Reverse();
            await _context.SaveChangesAsync();
            return View(model);
        }

        private async Task<List<ApplicationUser>> getOtherUsersInfo()
        {
            List<ApplicationUser> userList = _context.Users.ToList();
            ApplicationUser user = await _userManager.GetUserAsync(User);
            userList.Remove(user);
            userList.RemoveAll(u => u.IsBlocked || !u.EmailConfirmed);
            return userList;
        }

        private string getTodayDate()
        {
            string day = (DateTime.Today.Day >= 10) ? DateTime.Today.Day.ToString() : "0" + DateTime.Today.Day.ToString();
            string mon = (DateTime.Today.Month >= 10) ? DateTime.Today.Month.ToString() : "0" + DateTime.Today.Month.ToString();
            return day+"/"+mon+"/"+DateTime.Today.Year.ToString();
        }

        private string getImageName()
        {
            string day = (DateTime.Today.Day >= 10) ? DateTime.Today.Day.ToString() : "0" + DateTime.Today.Day.ToString();
            string mon = (DateTime.Today.Month >= 10) ? DateTime.Today.Month.ToString() : "0" + DateTime.Today.Month.ToString();
            string year = DateTime.Today.Year.ToString();
            return year + mon + day + Guid.NewGuid();
        }
        
    }

}
