using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Law_Hub.Models;
using Law_Hub.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Law_Hub.Models.ManageViewModels;

namespace Law_Hub.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;


        private ApplicationDbContext _db;

        public AdminController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public IActionResult Index()
        {            //Get the User Details using the User Id
            var search = _db.Admin_Profile.Find(_userManager.GetUserId(User));

            var _user = _db.User_Profile_Controller.Find(_userManager.GetUserId(User));

            if (_user.Profile_Activated == "0")
            {
                return RedirectToAction("ChangePassword");
            }
            else
            {

                return View(search);
            }
        }

        public IActionResult CreateProfile()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateProfile(Admin_Profile admin)
        {
            var check = _db.Admin_Profile.Find(_userManager.GetUserId(User));

            if (_signInManager.IsSignedIn(User))
            {
                if (check != null)
                {
                    //admin.Id = _userManager.GetUserId(User);
                    //admin.CreatedBy = "";
                    //_db.SaveChanges();

                    //Search for an existing profile and update it
                    var search = _db.User_Profile_Controller.Find(_userManager.GetUserId(User));
                    var search1 = _db.Admin_Profile.Find(_userManager.GetUserId(User));
                    if (search != null && search1 != null)
                    {
                        search.Profile_Activated = "1"; //Profile Activated codes 1(Fully Activated) 2(Pending to be Activated by Admin) 0(Disactivated)
                        search.Account_Activated = "1"; //Account_Activated codes 1(Fully Activated) 0(Disactivated)
                        search1.Email = admin.Email;
                        search1.FirstName = admin.FirstName;
                        search1.LastName = admin.LastName;
                        search1.OtherNames = admin.OtherNames;
                        search1.PhoneNumber = admin.PhoneNumber;
                        search1.TItle = admin.TItle;
                        _db.SaveChanges();
                    }
                    return RedirectToAction("Index", "Admin");
                }

            }
            else
            {
                return RedirectToAction("Index", "Admin");
            }
            return View();
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
            var _user = _db.User_Profile_Controller.Find(_userManager.GetUserId(User));

            return View(model);

        }

        public IActionResult ActivateAccounts()
        {

            IQueryable<ActivateAccounts> list =
                                          from lawyerProfile in _db.Lawyers_Profile
                                          join userProfileController in _db.User_Profile_Controller on lawyerProfile.Id equals userProfileController.Id
                                          select new ActivateAccounts
                                          {
                                              Id = lawyerProfile.Id,
                                              First_Name = lawyerProfile.First_Name,
                                              Last_Name = lawyerProfile.Last_Name,
                                              Sex = lawyerProfile.Sex,
                                              Phone_Number = lawyerProfile.Phone_Number,
                                              Profile_Activated = userProfileController.Profile_Activated,
                                              Account_Activated = userProfileController.Account_Activated

                                          };


            return View(list);
        }

        [HttpPost]
        public IActionResult ActivateProfile(string data)
        {
            try
            {
                var a = data.Substring(0, 36);
                if (data.Contains("&dis"))
                {
                    var _User = _db.User_Profile_Controller.Find(a);
                    _User.Profile_Activated = "0";
                    _db.SaveChanges();
                }
                else
                {
                    var _User = _db.User_Profile_Controller.Find(a);
                    _User.Profile_Activated = "1";
                    _db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return View(data);

        }

        [HttpPost]
        public IActionResult ActivateAccount(string data)
        {
            try
            {
                var a = data.Substring(0, 36);
                if (data.Contains("&dis"))
                {

                    var _User = _db.User_Profile_Controller.Find(a);
                    _User.Account_Activated = "0";
                    _db.SaveChanges();
                }
                else
                {
                    var _User = _db.User_Profile_Controller.Find(a);
                    _User.Account_Activated = "1";
                    _db.SaveChanges();
                }

            }
            catch (Exception e)
            {
                throw e;
            }
            return View(data);

        }

        public IActionResult ActivateProfile()
        {
            IQueryable<ActivateAccounts> list =
                                          from lawyerProfile in _db.Lawyers_Profile
                                          join userProfileController in _db.User_Profile_Controller on lawyerProfile.Id equals userProfileController.Id
                                          select new ActivateAccounts
                                          {
                                              Id = lawyerProfile.Id,
                                              First_Name = lawyerProfile.First_Name,
                                              Last_Name = lawyerProfile.Last_Name,
                                              Sex = lawyerProfile.Sex,
                                              Phone_Number = lawyerProfile.Phone_Number,
                                              Certificate_Paths = lawyerProfile.Certificate_path,
                                              Profile_Activated = userProfileController.Profile_Activated,
                                              Account_Activated = userProfileController.Account_Activated
                                          };


            return View(list);
        }

        public IActionResult AdminSetup()
        {
            return View();
        }

        public IActionResult ViewUsers()
        {
            var LawyerSearch = _db.Lawyers_Profile.ToList();
            var ClientSearch = _db.Clients_Profile.ToList();

            ViewBag.LawyerSearch = LawyerSearch;
            ViewBag.ClientSearch = ClientSearch;
            return View();
        }

        public IActionResult Reports()
        {
            return View();
        }

        public IActionResult ResetPasswords()
        {
            return View();
        }

        public IActionResult ViewCases()
        {
            return View();
        }

        public IActionResult Payments()
        {
            return View();
        }
    }
}