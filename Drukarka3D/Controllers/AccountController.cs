﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Drukarka3D.Services;
using Drukarka3DData;
using Drukarka3DData.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using ReflectionIT.Mvc.Paging;

namespace Drukarka3D.Controllers
{
    public class ProjectPrivacy
    {
        public string Id { get; set; }
        public string IsPrivate { get; set; }
    }
    public class AccountController : Controller
    {
        public string UserScreenPath { get; set; }
        private Drukarka3DContext context;
        private readonly IEmailSender emailSender;
        private readonly IFileProvider fileProvider;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, IEmailSender emailSender,
            IFileProvider fileProvider, Drukarka3DContext context)
        {
            this.emailSender = emailSender;
            this.context = context;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.fileProvider = fileProvider;
        }

        [HttpPost]
        public IActionResult Print([FromBody]FileToPrint data)
        {
            var path1 = Path.Combine(
            Directory.GetCurrentDirectory(), "wwwroot/DoZatwierdzenia/", data.FilePath);

            string currentDate = DateTime.Now.ToString();
            currentDate = currentDate.Replace(':', '_');
            string fileName = currentDate + userManager.GetUserId(HttpContext.User) + ".stl";

            var path2 = Path.Combine(
            Directory.GetCurrentDirectory(), "wwwroot/DoZatwierdzenia/", fileName);

            System.IO.File.Copy(path1, path2);

            return Json(new JsonResult(data));
        }
        public async Task<IActionResult> Index(string filter = "", int page = 1, string sortExpression = "Status")
        {
            var newestOrders = context.Order.AsNoTracking().Where(order => order.User.Id
            .Equals(userManager.GetUserId(HttpContext.User)))
            .OrderByDescending(order => order.UploadDate).AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter))
            {
                newestOrders = newestOrders.Where(order => order.Name.Contains(filter));
            }

            var model = await PagingList.CreateAsync(newestOrders, 16, page, sortExpression, "Status");

            model.RouteValue = new RouteValueDictionary
            {
                { "filter", filter}
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult ProjectShare([FromBody]ProjectPrivacy privacy)
        {
            var result = context.Order.Where(o => o.OrderId
            .Equals(Convert.ToInt32(privacy.Id))).FirstOrDefault();

            if (privacy.IsPrivate == null) privacy.IsPrivate = "off";

            result.Private = privacy.IsPrivate.Equals("true") ? false : true;
            context.SaveChanges();

            return Ok(result);
        }

        [HttpPost]
        public IActionResult ProjectView(IFormCollection param)
        {
            try
            {
                string tmp = param.Keys.ElementAt(0).Substring(0, param.Keys.ElementAt(0).Length - 2);

                ICollection<Order> order = context.Order.Where(o => o
                .OrderId.Equals(Convert.ToInt32(tmp))).ToList();

                if (order == null) throw new NullReferenceException();

                order.First().ViewsCount += 1;
                context.SaveChanges();

                return View(new OrderViewModel()
                {
                    NumberOfResolutsInPage = 0,
                    PageNumber = 1,
                    SortingType = String.Empty,
                    SearchString = String.Empty,
                    SortingOrder = String.Empty,
                    Order = order
                });
            }
            catch (Exception)
            {
                return View();
            }
        }
    }
}