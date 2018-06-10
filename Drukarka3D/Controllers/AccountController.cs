using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Drukarka3D.Services;
using Drukarka3DData;
using Drukarka3DData.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using ReflectionIT.Mvc.Paging;

namespace Drukarka3D.Controllers
{
    public class AccountController : Controller
    {
        public string UserScreenPath { get; set; }
        public string StatusMessage { get; private set; }

        private Drukarka3DContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IFileProvider _fileProvider;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, IEmailSender emailSender,
            IFileProvider fileProvider, Drukarka3DContext context)
        {
            _emailSender = emailSender;
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _fileProvider = fileProvider;
        }//Kamil

     


        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            ViewData["message"] = "Wylogowano pomyślnie";
            return RedirectToAction("Login", "Account", new { message = "Wylogowano pomyślnie" });
        }//Kamil

        [HttpGet]
        public IActionResult Login(string message)
        {
            ViewData["message"] = message;
            return View();
        }//Kamil

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(vm.Login, vm.Password, vm.RememberMe, false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Nieprawidłowy e-mail lub hasło.");
                return View(vm);
            }
            return View(vm);
        }//Dawid

        [HttpGet]
        public IActionResult Register()
        {
            ViewData["Title"] = "Rejestracja";
            return View();
        }//Dawid

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = vm.Login, Email = vm.Email };
                var result = await _userManager.CreateAsync(user, vm.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(vm);
        }//Kamil

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
