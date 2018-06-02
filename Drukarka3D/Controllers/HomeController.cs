using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Drukarka3DData.Models;
using Drukarka3DData;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Drukarka3D.Services;
using System.Net;
using System.Text;
using ReflectionIT.Mvc.Paging;
using Microsoft.AspNetCore.Routing;

namespace Drukarka3D.Controllers
{  
    public class HomeController : Controller
    {
        public string UserScreenPath { get; set; }
        private Drukarka3DContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IFileProvider _fileProvider;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        //Dawid

        [TempData]
        public string StatusMessage { get; set; }

        public HomeController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, IEmailSender emailSender,
            IFileProvider fileProvider, Drukarka3DContext context)
        {
            _emailSender = emailSender;
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _fileProvider = fileProvider;
        }//Dominik

        public async Task<IActionResult> Index(string filter = "", int page = 1, string sortExpression = "Status", int onPage = 20, string message="")
        {
            var newestOrders = _context.Order.AsNoTracking().Where(order => order.Private.Equals(false))
            .OrderBy(order => order.UploadDate).AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter))
            {
                newestOrders = newestOrders.Where(order => order.Name.Contains(filter));
            }

            ViewData["onPage"] = onPage;

            var model = await PagingList.CreateAsync(newestOrders, onPage, page, sortExpression, "Status");


            model.RouteValue = new RouteValueDictionary
            {
                { "filter", filter},
                { "onPage", onPage}
            };

            if (newestOrders.Count().Equals(0))
            {
                message = "Brak elementów do wyśtlenia";
            }

            ViewData["message"] = message;

            return View(model);
        }//Dominik

        [Authorize]
        public IActionResult Loader(string message)
        {
            if (User.Identity.IsAuthenticated)
            {
                ViewData["Title"] = "Strona Główna";
                ViewData["message"] = message;
                return View();
            }
            else return RedirectToAction("Index","Home");

        }//Dominik

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }//Dominik

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }//Dawid
    }
}
