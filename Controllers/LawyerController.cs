using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Law_Hub.Models;
using Law_Hub.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Law_Hub.Models.ManageViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace Law_Hub.Controllers
{
    [Authorize(Roles = "Lawyers")]
    public class LawyerController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IHostingEnvironment _ihostingEnviroment;
        private readonly ILogger _logger;

        private ApplicationDbContext _db;

        public LawyerController(ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IHostingEnvironment hostingEnvironment,
            ILogger<ManageController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _ihostingEnviroment = hostingEnvironment;
            _db = db;
            _logger = logger;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public IActionResult Index()
        {
            var search = _db.Lawyers_Profile.Find(_userManager.GetUserId(User));

            return View(search);
        }

        public IActionResult ManageProfile()
        {
            return View();
        }

        public IActionResult Create_Profile()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateProfile(Lawyers_Profile param)
        {
            var check = _db.Lawyers_Profile.Find(_userManager.GetUserId(User));
            if (_signInManager.IsSignedIn(User))
            {
                if (check == null)
                {
                    //uploading the Certificates Paths to the database
                    if (param.Certificates_Paths != null)
                    {
                        var totalfile = "";
                        param.Id = _userManager.GetUserId(User);
                        var files = HttpContext.Request.Form.Files;
                        foreach (var f in files)
                        {
                            var uniqueFileName = GetUniqueFileName(f.FileName);
                            var uploads = Path.Combine(_ihostingEnviroment.WebRootPath, "Certificates");
                            var Filepath = Path.Combine(uploads, uniqueFileName);
                            using (var filestream = new FileStream(Filepath, FileMode.Create))
                            {
                                f.CopyTo(filestream);
                            }
                            //f.CopyTo(new FileStream(Filepath, FileMode.Create));

                            totalfile = totalfile + uniqueFileName + " ,";
                        }
                        param.Certificate_path = totalfile;
                        _db.Lawyers_Profile.Add(param);
                        _db.SaveChanges();

                        //Creating the User_Profile_Controller
                        //Search for the existing profile and update it
                        var search = _db.User_Profile_Controller.Find(_userManager.GetUserId(User));
                        if (search != null)
                        {
                            search.Profile_Activated = "2"; //Profile Activated codes 1(Fully Activated) 2(Pending to be Activated by Admin) 0(Disactivated)
                            search.Account_Activated = "1"; //Account_Activated codes 1(Fully Activated) 0(Disactivated)
                            _db.SaveChanges();
                        }
                        return RedirectToAction("Index", "Lawyer");
                    }

                }
                else
                {
                    return RedirectToAction("Index", "Lawyer");
                }
            }
            //_db.LawyerProfile.Add(param);
            //_db.SaveChanges();
            return View();

        }


        private string GetUniqueFileName(string FileName)
        {
            FileName = Path.GetFileName(FileName);
            FileName = string.Join("", FileName.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
            return Path.GetFileNameWithoutExtension(FileName) + "_"
                + Guid.NewGuid().ToString().Substring(0, 4)
                + Path.GetExtension(FileName);
        }

        public IActionResult UploadProfilePicture()
        {
            return View();
        }

        [HttpPost]
        public IActionResult UploadProfilePicture(ProfilePictureViewModel model)
        {
            var check = _db.Lawyers_Profile.Find(_userManager.GetUserId(User));

            var image = model.ProfilePicture;

            if (image != null && check != null)
            {
                var uniqueFileName = GetUniqueFileName(image.FileName);
                var uploads = Path.Combine(_ihostingEnviroment.WebRootPath, "ProfilePictures");
                var Filepath = Path.Combine(uploads, uniqueFileName);
                using (var filestream = new FileStream(Filepath, FileMode.Create))
                {
                    image.CopyTo(filestream);
                }
                check.ProfilePicture = uniqueFileName;
                _db.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var hasPassword = await _userManager.HasPasswordAsync(user);

            var model = new ChangePasswordViewModel { StatusMessage = StatusMessage };
            return View("ChangePassword", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                AddErrors(changePasswordResult);
                return View("ChangePassword", model);
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            _logger.LogInformation("User changed their password successfully.");
            StatusMessage = "Your password has been changed.";

            return RedirectToAction(nameof(ChangePassword));
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        public IActionResult UpdateProfileDetails()
        {
            var user = _db.Lawyers_Profile.Find(_userManager.GetUserId(User));
            return View(user);
        }

        [HttpPost]
        public IActionResult UpdateProfileDetails(Lawyers_Profile model)
        {
            var _user = _db.Lawyers_Profile.Find(_userManager.GetUserId(User));

            if (_user != null && model != null)
            {
                _user.Email = model.Email;
                _user.Experiences = model.Experiences;
                if (model.Certificates_Paths != null)
                {
                    var images = HttpContext.Request.Form.Files;
                    var Total = _user.Certificate_path;
                    foreach (var image in images)
                    {
                        var uniqueFileName = GetUniqueFileName(image.FileName);
                        var uploads = Path.Combine(_ihostingEnviroment.WebRootPath, "Certificates");
                        var Filepath = Path.Combine(uploads, uniqueFileName);
                        image.CopyTo(new FileStream(Filepath, FileMode.Create));
                        Total = Total + uniqueFileName + ",";
                    }

                    _user.Certificate_path = Total;
                }
                var _UserController = _db.User_Profile_Controller.Find(_userManager.GetUserId(User));
                _UserController.Profile_Activated = "3";

                _db.SaveChanges();

            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Cases()
        {
            return View();
        }

        public IActionResult Clients()
        {
            return View();
        }

        public IActionResult History()
        {
            return View();
        }

        public IActionResult CreateProfile()
        {
            return View();
        }

        //[HttpPost]
        //public IActionResult CreateProfile()
        //{

        //}
    }
}