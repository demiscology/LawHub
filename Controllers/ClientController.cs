using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Law_Hub.Models;
using Law_Hub.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Law_Hub.Models.ManageViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace Law_Hub.Controllers
{
    [Authorize(Roles ="Clients")]
    public class ClientController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IHostingEnvironment _ihostingEnviroment;
        private readonly ILogger _logger;


        private ApplicationDbContext _db;

        public ClientController(ApplicationDbContext db,
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
            //Get the User Details using the User Id
            var search = _db.Clients_Profile.Find(_userManager.GetUserId(User));

            return View(search);
        }

        public IActionResult Cases()
        {
            return View();
        }

        public IActionResult CreateProfile()
        {
            return View();
        }

        public IActionResult Search()
        {
            var list = _db.Lawyers_Profile.ToList();
            return View(list);
        }

        [HttpPost]
        public IActionResult Search(SearchViewModel searchViewModel)
        {
            var search_string = searchViewModel.search;

            var split = search_string.Split(',', ' ');

            //Compare each word in split with each word in the Stop Word, If it's in the stop word list , check the next split else add it to a list which we would be iterating from
            List<String> list = new List<String>();

            string[] file = System.IO.File.ReadAllLines("wwwroot/Stop Words.txt");

            foreach (var word in split)
            {
                if (file.Contains(word) || word.Length <= 2)
                {
                    continue;
                }
                else
                {
                    list.Add(word);
                }
            }

            //Fetch all the data from the database

            var result = from lawyerProfile in _db.Lawyers_Profile
                         join userProfileController in _db.User_Profile_Controller on lawyerProfile.Id equals userProfileController.Id
                         where userProfileController.Profile_Activated == "1" && userProfileController.Account_Activated == "1"
                         select lawyerProfile;


            //Create a variable totalresult of type IQueryable<Lawyers_Profile> to store the totalsearch result
            IQueryable<Lawyers_Profile> totalresult = Enumerable.Empty<Lawyers_Profile>().AsQueryable();

            foreach (var substring in list)
            {
                var newresult = result.Where(s => s.Experiences.Contains(substring) ||
                                         s.First_Name.Contains(substring) ||
                                         s.Last_Name.Contains(substring) ||
                                        s.Maiden_Name.Contains(substring) ||
                                        s.Other_Titles.Contains(substring)
                                        );
                totalresult = totalresult.Concat(newresult);
            }
            var count2 = totalresult.Distinct().Count();
            if (count2 > 0)
            {
                return View("SearchResults", totalresult.Distinct());
            }
            else
            {//Error message for empty result 
                ViewBag.error = "Sorry, no results found. Trying searching with exact keyword, E.G Lagos, SAN, FIrst Name, Experience etc. You can also view all Lawyers from the search page";
                return View("EmptySearchResult");
            }
        }

        [HttpPost]
        public IActionResult CreateProfile(Clients_Profile clients_Profile)
        {
            var check = _db.Clients_Profile.Find(_userManager.GetUserId(User));

            if (_signInManager.IsSignedIn(User))
            {
                if (check == null)
                {
                    clients_Profile.Id = _userManager.GetUserId(User);
                    _db.Clients_Profile.Add(clients_Profile);
                    _db.SaveChanges();

                    //Search for an existing profile and update it
                    var search = _db.User_Profile_Controller.Find(_userManager.GetUserId(User));
                    if (search != null)
                    {
                        search.Profile_Activated = "2"; //Profile Activated codes 1(Fully Activated) 2(Pending to be Activated by Admin) 0(Disactivated)
                        search.Account_Activated = "1"; //Account_Activated codes 1(Fully Activated) 0(Disactivated)
                        _db.SaveChanges();
                    }
                    return RedirectToAction("Index", "Client");
                }
                else
                {
                    return RedirectToAction("Index", "Client");
                }
            }
            else
            {
                return RedirectToAction("Index", "Client");
            }
        }

        public IActionResult Lawyer()
        {
            return View();
        }

        public IActionResult ManageProfile()
        {
            var _user = _db.Clients_Profile.Find(_userManager.GetUserId(User));

            return View(_user);
        }

        public IActionResult ManageProfile_ProfilePicture()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ManageProfile_ProfilePicture(ProfilePictureViewModel model)
        {
            var check = _db.Clients_Profile.Find(_userManager.GetUserId(User));

            var image = model.ProfilePicture;

            if (image != null && check != null)
            {
                var uniqueFileName = GetUniqueFileName(image.FileName);
                var uploads = Path.Combine(_ihostingEnviroment.WebRootPath, "ProfilePictures");
                var Filepath = Path.Combine(uploads, uniqueFileName);
                using (var fileStream = new FileStream(Filepath, FileMode.Create))
                {
                     image.CopyToAsync(fileStream);
                }
                check.ProfilePicture = uniqueFileName;
                _db.SaveChanges();
            }
            return RedirectToAction("Index");
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

        public IActionResult UpdateProfile()
        {
            var search = _db.Clients_Profile.Find(_userManager.GetUserId(User));
            return View(search);
        }

        [HttpPost]
        public IActionResult UpdateProfile(Clients_Profile model)
        {
            var search = _db.Clients_Profile.Find(_userManager.GetUserId(User));
            if (search != null)
            {
                search.Email = model.Email;
                search.Phone_Number = model.Phone_Number;
                _db.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        //Function for getting unique file name for Profile Picture upload
        private string GetUniqueFileName(string FileName)
        {
            FileName = Path.GetFileName(FileName);
            FileName = string.Join("", FileName.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
            return Path.GetFileNameWithoutExtension(FileName) + "_"
                + Guid.NewGuid().ToString().Substring(0, 4)
                + Path.GetExtension(FileName);
        }

        //[HttpPost]
        //public async Task<IActionResult> ManageProfile_ChangePasswordAsync(ChangePasswordViewModel model)
        //{

        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    var user = await _userManager.GetUserAsync(User);
        //    if (user == null)
        //    {
        //        throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        //    }

        //    var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
        //    if (!changePasswordResult.Succeeded)
        //    {
        //        AddErrors(changePasswordResult);
        //        return View(model);
        //    }

        //    await _signInManager.SignInAsync(user, isPersistent: false);
        //    _logger.LogInformation("User changed their password successfully.");
        //    StatusMessage = "Your password has been changed.";

        //    return RedirectToAction(nameof(ChangePassword));

        //}


        public IActionResult Report()
        {
            return View();
        }

        public IActionResult SearchPage()
        {
            return View();
        }

    }
}