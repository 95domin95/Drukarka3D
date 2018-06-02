using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Drukarka3DData;
using Drukarka3DData.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Drukarka3D.Controllers
{
    public class ManageController : Controller
    {
        [TempData]
        public string StatusMessage { get; set; }
        private Drukarka3DContext _context;
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signInManager;

        public ManageController(Drukarka3DContext context, 
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var hasPassword = await _userManager.HasPasswordAsync(user);
            if (!hasPassword)
            {
                return RedirectToAction(nameof(SetPassword));
            }

            var model = new ChangePasswordViewModel { StatusMessage = StatusMessage };
            return View(model);
        }//Dominik

        [Authorize]
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
                return View(model);
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            StatusMessage = "Your password has been changed.";

            return RedirectToAction(nameof(ChangePassword));
        }//Dominik

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> SetPassword()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var hasPassword = await _userManager.HasPasswordAsync(user);

            if (hasPassword)
            {
                return RedirectToAction(nameof(ChangePassword));
            }

            var model = new SetPasswordViewModel { StatusMessage = StatusMessage };
            return View(model);
        }//Kamil


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
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

            var addPasswordResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
            if (!addPasswordResult.Succeeded)
            {
                AddErrors(addPasswordResult);
                return View(model);
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            StatusMessage = "Your password has been set.";

            return RedirectToAction(nameof(SetPassword));
        }//Dawid

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ManageAccount(string message = "")
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var model = new IndexViewModel
            {
                Username = user.UserName,
                Email = user.Email,
                Name = user.Name,
                Surname = user.Surname,
                City = user.City,
                PostCode = user.PostCode,
                Street = user.Street,
                ApartmentNumber = user.ApartmentNumber,
                PhoneNumber = user.PhoneNumber,
                IsEmailConfirmed = user.EmailConfirmed,
                StatusMessage = StatusMessage
            };

            ViewData["message"] = message;

            return View(model);
        }//Kamil

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageAccount(IndexViewModel model)
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

            var email = user.Email;
            if (model.Email != email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
                if (!setEmailResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred setting email for user with ID '{user.Id}'.");
                }
            }

            var name = user.Name;
            if (model.Name != name)
            {
                var usr = await _userManager.GetUserAsync(HttpContext.User);
                usr.Name = model.Name;
                _context.SaveChanges();
            }

            var surname = user.Surname;
            if (model.Surname != surname)
            {
                var usr = await _userManager.GetUserAsync(HttpContext.User);
                usr.Surname = model.Surname;
                _context.SaveChanges();
            }

            var city = user.City;
            if (model.City != city)
            {
                var usr = await _userManager.GetUserAsync(HttpContext.User);
                usr.City = model.City;
                _context.SaveChanges();
            }

            var postCode = user.PostCode;
            if (model.PostCode != postCode)
            {
                var usr = await _userManager.GetUserAsync(HttpContext.User);
                usr.PostCode = model.PostCode;
                _context.SaveChanges();
            }

            var street = user.Street;
            if (model.Street != street)
            {
                var usr = await _userManager.GetUserAsync(HttpContext.User);
                usr.Street = model.Street;
                _context.SaveChanges();
            }

            var apartmentNumber = user.ApartmentNumber;
            if (model.ApartmentNumber != apartmentNumber)
            {
                var usr = await _userManager.GetUserAsync(HttpContext.User);
                usr.ApartmentNumber = model.ApartmentNumber;
                _context.SaveChanges();
            }

            var phoneNumber = user.PhoneNumber;
            if (model.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred setting phone number for user with ID '{user.Id}'.");
                }
            }

            StatusMessage = "Your profile has been updated";
            return RedirectToAction("ManageAccount", "Manage", new { message = "Twój profil został zaktualizowany" });
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