using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Drukarka3DData;
using Drukarka3DData.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drukarka3D.Controllers
{
    public class AdminController : Controller
    {
        private Drukarka3DContext _context;
        public AdminController(Drukarka3DContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public IActionResult AdminPanel(string FileName = "", string Message = "")
        {
            if (!FileName.Equals("") && !Message.Equals(""))
            {
                var order = _context.Order.Where(o => o.Path.Equals(FileName + ".stl")).FirstOrDefault();
                if (order != default(Order))
                {
                    order.Status = Message;
                    _context.SaveChanges();
                }
            }

            ViewData["message"] = "Zmieniono status zamówienia";

            return View();
        }

        [Authorize]
        public IActionResult AdminPanel()
        {
            return View();
        }
    }

}