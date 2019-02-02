using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Law_Hub.Models;
using Law_Hub.Data;

namespace Law_Hub.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }


        public IActionResult Index()
        {
            return View("Test");
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Login()
        {
            ViewBag.Message = TempData["Message"];
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult SearchPage()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SearchPage(SearchViewModel model)
        {
            TempData["Message"] = "You need to login before you can View search results";
            return RedirectToAction("Login");
        }

        public IActionResult Index1()
        {
            return View("Index");
        }

        public IActionResult Contact()
        {
            return View();
        }

        

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
