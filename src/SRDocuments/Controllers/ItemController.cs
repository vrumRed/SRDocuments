using System;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConnection _conn;

        public ItemController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConnection conn)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _conn = conn;
        }
        
        [HttpGet]
        public async Task<IActionResult> Send()
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Error", "Home", new { statusCode = -1 });
            }
            ViewData["ReceiverID"] = new SelectList(await _conn.getOtherUsersInfo(_userManager.GetUserId(User)), "Id", "Info");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Send(SendViewModel model)
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Error", "Home", new { statusCode = -1 });
            }

            ViewData["ReceiverID"] = new SelectList(await _conn.getOtherUsersInfo(_userManager.GetUserId(User)), "Id", "Info");
            var imageName = getImageName();

            var newDocument = new Document
            {
                Name = model.Name,
                Description = model.Description,
                SentToID = model.ReceiverID,
                SentByID = _userManager.GetUserId(User),
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
                    ViewBag.rDateError = "Required Date can't be today or earlier";
                    return View(model);
                }

                newDocument.RequiredDate = model.RequiredDate.Substring(8, 2) + "/" +
                                            model.RequiredDate.Substring(5, 2) + "/" +
                                            model.RequiredDate.Substring(0, 4);
            }
            /*if(model.Files != null)
            {
                foreach (var img in model.Files)
                {
                    if (Path.GetExtension(img.FileName) != "pdf" || img.Length > 10485760)
                    {
                        ViewBag.fError = "The file to be sent has to be a 'pdf' and has to have a size that is less than 10MB.";
                        return View(model);
                    }
                }
            }*/

            if (ModelState.IsValid)
            {
                var tempDocumentId = await _conn.addDocument(newDocument);

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
                            DocumentID = tempDocumentId,
                            Locale = pictureLocale,
                            Original = true,
                            DateSent = getTodayDate()
                        };
                        await _conn.addDocumentImage(newDocumentImage);
                        img.CopyTo(fs);
                        fs.Flush();
                    }
                    pictureNumber++;
                }
                ZipFile.CreateFromDirectory("wwwroot\\uploads\\original\\" + imageName, "wwwroot\\uploads\\original\\" + imageName +".rar");
                Notification newNotification = new Notification
                {
                    NotificationUserID = newDocument.SentToID,
                    Message = $"Document sent by {(await _userManager.FindByIdAsync(newDocument.SentByID)).FullName} on {newDocument.SentDate}"
                };
                await _conn.addNotification(newNotification);
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

            return View(await _conn.getAllSentDocuments(_userManager.GetUserId(User)));
        }
        
        [HttpGet]
        public async Task<IActionResult> ReceivedList()
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Error", "Home", new { statusCode = -1 });
            }

            return View(await _conn.getAllReceivedDocuments(_userManager.GetUserId(User)));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (!_signInManager.IsSignedIn(User) )
            {
                return RedirectToAction("Error", "Home", new { statusCode = -1 });
            }
            
            var item = await _conn.getDocumentDetails(id);

            if(item == null || (_userManager.GetUserId(User) != item.SentByID && _userManager.GetUserId(User) != item.SentToID))
            {
                return RedirectToAction("Error", "Home", new { statusCode = 404 });
            }
            
            return View(item);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int deleteValueInput)
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Error", "Home", new { statusCode = -1 });
            }

            var item = await _conn.getDocumentToDelete(deleteValueInput);
            if(item == null || item.Finished || item.NotAccepted || item.AnswerDate != null || item.VisualizationDate != null ||
                item.SentByID != _userManager.GetUserId(User))
            {
                return RedirectToAction("Error", "Home", new { statusCode = 404 });
            }

            await _conn.deleteDocument(deleteValueInput);

            Notification notification = new Notification
            {
                NotificationUserID = item.SentToID,
                Message = $"Document sent by {(await _userManager.FindByIdAsync(item.SentByID)).FullName} on {item.SentDate} was deleted on {getTodayDate()}"
            };
            await _conn.addNotification(notification);
            return RedirectToAction("SentList");
        }

        [HttpGet]
        public async Task<IActionResult> Reply(int id)
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Error", "Home", new { statusCode = -1 });
            }
            var document = await _conn.getDocumentToReply(id);

            if (document == null || document.Finished || (!document.Finished && !document.NotAccepted && document.AnswerDate != null) || _userManager.GetUserId(User) != document.SentToID)
            {
                return RedirectToAction("Error", "Home", new { statusCode = 404 });
            }
            
            return View(document);
        }

        [HttpPost]
        public async Task<IActionResult> Reply(int documentId, List<IFormFile> files)
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Error", "Home", new { statusCode = -1 });
            }

            var document = await _conn.getDocumentToReply(documentId);

            if (document == null || document.Finished || (!document.Finished && !document.NotAccepted && document.AnswerDate != null) || _userManager.GetUserId(User) != document.SentToID)
            {
                return RedirectToAction("Error", "Home", new { statusCode = 404 });
            }

            if (files.Count == 0)
            {
                ViewBag.error = "Necessário pelo menos um arquivo";
                return View(document);
            }

            var imageName = getImageName();
            var pictureNumber = 1;

            if (document.NotAccepted)
            {
                imageName = document.ReceivedImagesRarLocale.Substring(20, 44);
                pictureNumber = await _conn.getNumberOfLastImage(documentId)+1;
                System.IO.File.Delete($"wwwroot\\{document.ReceivedImagesRarLocale}");
                document.NotAccepted = false;
            }

            document.AnswerDate = getTodayDate();
            document.ReceivedImagesRarLocale = "uploads\\notoriginal\\" + imageName + ".rar";
            Directory.CreateDirectory("wwwroot\\uploads\\notoriginal\\" + imageName);
            foreach (var img in files)
            {
                var pictureLocale = $"uploads\\notoriginal\\{imageName}\\{pictureNumber}{Path.GetExtension(img.FileName)}";
                using (var fs = new FileStream($"wwwroot\\{pictureLocale}", FileMode.CreateNew))
                {
                    DocumentImage newDocumentImage = new DocumentImage
                    {
                        Name = $"Document {pictureNumber}",
                        DocumentID = documentId,
                        Locale = pictureLocale,
                        Original = false,
                        DateSent = getTodayDate()
                    };
                    await _conn.addDocumentImage(newDocumentImage);
                    img.CopyTo(fs);
                    fs.Flush();
                }
                pictureNumber++;
            }
            ZipFile.CreateFromDirectory("wwwroot\\uploads\\notoriginal\\" + imageName, "wwwroot\\uploads\\notoriginal\\" + imageName + ".rar");

            await _conn.AddDocumentRepliedInfo(document);

            Notification notification = new Notification
            {
                NotificationUserID = document.SentByID,
                Message = $"Document n°{document.DocumentID} sent to {(await _userManager.FindByIdAsync(document.SentToID)).FullName} on {document.SentDate} was replied on {document.AnswerDate}"
            };

            await _conn.addNotification(notification);
            return RedirectToAction("ReceivedList");
        }

        [HttpPost]
        public async Task<IActionResult> Deny(int denyValueInput)
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Error", "Home", new { statusCode = -1 });
            }

            var document = await _conn.getDocumentToDA(denyValueInput);

            if (document == null || document.AnswerDate == null || document.Finished || document.NotAccepted || document.SentByID != _userManager.GetUserId(User))
            {
                return RedirectToAction("Error", "Home", new { statusCode = 404 });
            }
            
            Notification notification = new Notification
            {
                NotificationUserID = document.SentToID,
                Message = $"Document n°{document.DocumentID} replied to {(await _userManager.FindByIdAsync(document.SentByID)).FullName} on {document.AnswerDate} was denied on {getTodayDate()}"
            };

            document.NotAccepted = true;
            await _conn.updateDocumentDA(document);
            await _conn.addNotification(notification);
            
            return RedirectToAction("SentList");
        }

        [HttpPost]
        public async Task<IActionResult> Accept(int acceptValueInput)
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Error", "Home", new { statusCode = -1 });
            }

            var document = await _conn.getDocumentToDA(acceptValueInput);

            if (document == null || document.AnswerDate == null || document.Finished || document.NotAccepted || document.SentByID != _userManager.GetUserId(User))
            {
                return RedirectToAction("Error", "Home", new { statusCode = 404 });
            }
            
            document.NotAccepted = false;
            document.Finished = true;
            document.ConclusionDate = getTodayDate();

            Notification notification = new Notification
            {
                NotificationUserID = document.SentToID,
                Message = $"Document n°{document.DocumentID} replied to {(await _userManager.FindByIdAsync(document.SentByID)).FullName} on {document.AnswerDate} was concluded on {document.ConclusionDate}"
            };

            await _conn.updateDocumentDA(document);
            await _conn.addNotification(notification);

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
            return View(await _conn.listNotifications(_userManager.GetUserId(User)));
        }

        private string getTodayDate()
        {
            string day = (DateTime.Today.Day >= 10) ? DateTime.Today.Day.ToString() : "0" + DateTime.Today.Day.ToString();
            string mon = (DateTime.Today.Month >= 10) ? DateTime.Today.Month.ToString() : "0" + DateTime.Today.Month.ToString();
            return day + "/" + mon + "/" + DateTime.Today.Year.ToString() + DateTime.Today.Hour.ToString() + DateTime.Today.Minute.ToString() + DateTime.Today.Second.ToString();
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
