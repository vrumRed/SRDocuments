using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using SRDocuments.Models;
using SRDocuments.Models.AccountViewModels;
using FluentEmail.Mailgun;
using FluentEmail.Core;
using Microsoft.AspNetCore.Http;
using System;

namespace SRDocuments.Controllers
{

    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        // GET: /<controller>/
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        
        [HttpGet]
        public IActionResult Login()
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Error", "Home", new { statusCode = -1 });
            }

            if(TempData["resultado"] != null)
            {
                ViewBag.Result = TempData["resultado"].ToString();
                TempData.Clear();
            }
            else
            {
                ViewBag.Result = "";
            }
            
            ViewBag.error = "";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Error", "Home", new { statusCode = -1 });
            }
            if (ModelState.IsValid)
            {
                var loginResult = await _signInManager.PasswordSignInAsync(model.Email, model.Password,
                    false, lockoutOnFailure: false);

                if (loginResult.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user.EmailConfirmed)
                    {
                        if (user.IsBlocked)
                        {
                            await _signInManager.SignOutAsync();
                            ViewBag.Result = $"This user is blocked. Request to unblock <form method='post' action='/Manage/RequestUnblockAccount' novalidate='novalidate'><input type='hidden' name='email' id='email' value='{user.Email}'><button type='submit' class='btn btn-link' style='cursor: pointer; cursor: hand;'>Here</button></form>";
                            return View(model);
                        }
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        await _signInManager.SignOutAsync();
                        ViewBag.Result = "Email is not confirmed. Send Confirmation Email <form method='post' action='/Account/ReSendConfirmation' novalidate='novalidate'><input type='hidden' name='email' id='email' value='{user.Email}'><button type='submit' class='btn btn-link' style='cursor: pointer; cursor: hand;'>Here</button></form>";
                        return View(model);
                    }
                }
                else if(model.Password != null && model.Email != null)
                {
                    ViewBag.error = "Invalid Login Information";
                    return View(model);
                }
            }
            ViewBag.error = "";
            return View(model);
        }

        public IActionResult Register()
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Error", "Home", new { statusCode = -1 });
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Error", "Home", new { statusCode = -1 });
            }            

            if (ModelState.IsValid)
            {

                if (await _userManager.FindByEmailAsync(model.Email) != null)
                {
                    ViewBag.EmailError = "Email already in use.";
                    return View(model);
                }

                var identityUser = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    CPF = model.CPF,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    IsBlocked = false
                };

                IdentityResult identityResult;
                try
                {
                    identityResult = await _userManager.CreateAsync(identityUser, model.Password);
                }
                catch (Exception)
                {
                    
                    ViewBag.CPFError = "CPF already in use.";
                    return View(model);
                }
                

                if (identityResult.Succeeded)
                {
                    if (await sendConfirmationEmail(identityUser)) {
                        TempData["resultado"] = $"Confirmation email sent to {model.Email}";
                    }
                    else
                    {
                        TempData["resultado"] = $"Failed to send email to {model.Email}";
                    }                    
                    return RedirectToAction("Login");
                }
                AddErrors(identityResult);

            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOut()
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Error", "Home", new { statusCode = -1 });
            }

            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string email, string token)
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Error", "Home", new { statusCode = -1 });
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                TempData["resultado"] = "This email has already been confirmed yet";
            }
            else
            {
                var isOkay = await _userManager.ConfirmEmailAsync(user, token);
                if (isOkay.Succeeded)
                {
                    TempData["resultado"] = "Email confirmed";
                }
                else
                {
                    TempData["resultado"] = "Email confirmation failed, please try again";
                }
            }
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ViewBag.Error = "Email not found";
                    return View(model);
                }
                if (!(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    ViewBag.Error = $"This email hasn't been confirmed yet";
                    return View(model);
                }
                if (await sendPasswordResetEmail(user))
                {
                    TempData["resultado"] = $"Reset Password request sent to {model.Email}";
                }
                else
                {
                    TempData["resultado"] = $"Failed to send email to {model.Email}";
                }
                return RedirectToAction("Login");
            }
            return View(model);
            
        }

        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            ResetPasswordViewModel model = new ResetPasswordViewModel
            {
                Email = email,
                Token = token
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
                if (result.Succeeded)
                {
                    TempData["resultado"] = "Password has been successfully changed";
                    return RedirectToAction("Login");
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ReSendConfirmation(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData["resultado"] = "Email not found";
            }
            else if (await _userManager.IsEmailConfirmedAsync(user))
            {
                TempData["resultado"] = $"This email has already been confirmed";
            }
            else if (await sendConfirmationEmail(user))
            {
                TempData["resultado"] = $"Confirmation email sent to {email}";
            }
            else
            {
                TempData["resultado"] = $"Failed to send email to {email}";
            }
            return RedirectToAction("Login");
        }

        private void AddErrors(IdentityResult result)
        {
            foreach(var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private async Task<bool> sendConfirmationEmail(ApplicationUser someUser)
        {
            var token2 = await _userManager.GenerateEmailConfirmationTokenAsync(someUser);
            var link = Url.Action("ConfirmEmail",
                          "Account", new
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
                .Subject("Email Confirmation")
                .Body($"Link aqui: \n <br/> {link}");

            var response = await email.SendAsync();
            return response.Successful;
        }

        private async Task<bool> sendPasswordResetEmail(ApplicationUser someUser)
        {
            var token2 = await _userManager.GeneratePasswordResetTokenAsync(someUser);
            var link = Url.Action("ResetPassword",
                          "Account", new
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
                .Subject("Reset Password")
                .Body($"Link aqui: \n <br/>{link}");

            var result =  await email.SendAsync();
            return result.Successful;
        }
        
    }
}
