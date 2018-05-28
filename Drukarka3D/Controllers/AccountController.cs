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
    public class ProjectPrivacy
    {
        public string Id { get; set; }
        public string IsPrivate { get; set; }
    }//Dawid
    public class AccountController : Controller
    {
        public string UserScreenPath { get; set; }
        public string StatusMessage { get; private set; }

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
        }//Kamil

        [Authorize]
        [HttpPost]
        public IActionResult Print([FromBody]FileToPrint data)
        {
            //var path1 = Path.Combine(
            //Directory.GetCurrentDirectory(), "wwwroot/DoZatwierdzenia/", data.FilePath);

            var path1 = Path.Combine(
            Directory.GetCurrentDirectory(), "wwwroot/DoZatwierdzenia/", "kuter.stl");

            string currentDate = DateTime.Now.ToString();
            currentDate = currentDate.Replace(':', '_');
            string fileName = currentDate + userManager.GetUserId(HttpContext.User) + ".stl";

            var path2 = Path.Combine(
            Directory.GetCurrentDirectory(), "wwwroot/DoZatwierdzenia/", fileName);

            System.IO.File.Copy(path1, path2);



            ForwardFiles(path2, fileName);

            return Json(new JsonResult(data));
        }//Dominik

        [Authorize]
        public async Task<IActionResult> MyProjectsAsync(string filter = "", int page = 1,
    string sortExpression = "Status", string sortOrder = "Ascending",
    int elementsOnPage = 20, int onPage = 10, string message="")
        {
            var user = await userManager.GetUserAsync(HttpContext.User);
            
            var projects = context.Order.AsNoTracking().Where(order => order.UserId
                .Equals(userManager.GetUserId(HttpContext.User)))
                .OrderBy(order => order.UploadDate).AsQueryable();

            projects = projects.Where(p => p.Name.Contains(filter));

            ViewData["Title"] = "Moje projekty";

            switch (sortExpression)
            {
                case "Liczba polubień":
                    projects = projects.OrderBy(p => p.Likes);
                    break;
                case "Alfabetycznie":
                    projects = projects.OrderBy(p => p.Name);
                    break;
                case "Najnowsze":
                    projects = projects.OrderBy(p => p.UploadDate);
                    break;
                case "Liczba wyświetleń":
                    projects = projects.OrderBy(p => p.RatingsCount);
                    break;
                default:
                    break;
            }

            var numberOfElements = projects.Count();

            var numberOfPages = (int)(projects.Count() / onPage);

            var elToTake = onPage;

            if ((projects.Count() % onPage) != 0) numberOfPages++;

            if (page < 1) page = 1;
            if (page > numberOfPages) page = numberOfPages;

            if (page.Equals(numberOfPages)) elToTake = projects.Count() - ((numberOfPages - 1) * onPage);

            if (numberOfElements > 0)
            {
                projects = projects.Skip((page - 1) * onPage).Take(elToTake);
            }

            ViewData["message"] = message;

            return View(new FavouriteProjectsViewModel()
            {
                NumberOfPages = numberOfPages,
                NumberOfElements = numberOfElements,
                Orders = projects,
                OnPage = onPage,
                Filter = filter,
                Page = page,
                SortExpression = sortExpression,
                SortingOrder = sortOrder
            });
        }//Dawid

        [Authorize]
        [HttpPost]
        public IActionResult AdminPanel(string FileName="", string Message="")
        {
            if(!FileName.Equals("")&&!Message.Equals(""))
            {
                var order = context.Order.Where(o => o.Path.Equals(FileName+".stl")).FirstOrDefault();
                if(order!=default(Order))
                {
                    order.Status = Message;
                    context.SaveChanges();
                }
            }

            return View();
        }

        [Authorize]
        public IActionResult AdminPanel()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> FavouriteProjectsAsync(string filter = "", int page = 1,
            string sortExpression = "Status", string sortOrder = "Ascending", int elementsOnPage = 20, int onPage=10)
        {
            var user = await userManager.GetUserAsync(HttpContext.User);

            var projects = from p in context.UserFavouriteProject
                         join o in context.Order on p.OrderId equals o.OrderId
                         where p.UserId.Equals(user.Id)
                         select o ;

            projects = projects.Where(p => p.Name.Contains(filter));

            ViewData["Title"] = "Ulubione";

            switch (sortExpression)
            {
                case "Liczba polubień":
                    projects = projects.OrderBy(p => p.Likes);
                    break;
                case "Alfabetycznie":
                    projects = projects.OrderBy(p => p.Name);                  
                    break;
                case "Najnowsze":
                    projects = projects.OrderBy(p => p.UploadDate);
                    break;
                case "Liczba wyświetleń":
                    projects = projects.OrderBy(p => p.RatingsCount);                    
                    break;
                default:
                    break;
            }

            var favourites = context.UserFavouriteProject.Where(f => f.User.Equals(user)&&f.IsFavourite);

            List<Order> tmp = new List<Order>();

            var numberOfElements = projects.Count();

            var numberOfPages = (int)(projects.Count() / onPage);

            var elToTake = onPage;

            if ((projects.Count() % onPage) != 0) numberOfPages++;

            if (page < 1) page = 1;
            if (page > numberOfPages) page = numberOfPages;

            if (page.Equals(numberOfPages)) elToTake = projects.Count() - ((numberOfPages - 1) * onPage);

            if (numberOfElements > 0)
            {
                projects = projects.Skip((page - 1) * onPage).Take(elToTake);

                if (user != null && favourites.Count() > 0)
                {
                    foreach (var i in projects)
                    {
                        foreach (var j in favourites)
                        {
                            if (i.Equals(j.Order) && j.IsFavourite) tmp.Add(i);
                        }
                    }
                }
            }

            return View(new FavouriteProjectsViewModel()
            {
                NumberOfPages = numberOfPages,
                NumberOfElements = numberOfElements,
                Orders = projects,
                OnPage = onPage,
                Filter = filter,
                Page = page,
                SortExpression = sortExpression,
                SortingOrder = sortOrder
            });
        }//Dawid

        [Authorize]
        public async Task<IActionResult> Index(string filter = "", int page = 1, 
            string sortExpression = "Status", int onPage = 20, string viewType= "Moje Projekty")
        {
            IQueryable<Order> newestOrders = null;
            if (viewType.Equals("Moje Projekty"))
            {
                newestOrders = context.Order.AsNoTracking().Where(order => order.UserId
                .Equals(userManager.GetUserId(HttpContext.User)))
                .OrderBy(order => order.UploadDate).AsQueryable();

                ViewData["Title"] = "Moje Projekty";
                viewType = "Moje Projekty";
            }
            else if(viewType.Equals("Ulubione"))
            {
                var user = await userManager.GetUserAsync(HttpContext.User);

                //IEnumerable<UserFavoriteProject> projects = context.UserFavouriteProject.Where(p => p.UserId.Equals(user.Id));

                newestOrders = from p in context.UserFavouriteProject
                             join o in context.Order on p.OrderId equals o.OrderId
                             select o;

                if(newestOrders.Count()>0) newestOrders = newestOrders.OrderByDescending(order => order.UploadDate).AsQueryable();

                ViewData["Title"] = "Ulubione";
                viewType = "Ulubione";
            }


            if (!string.IsNullOrWhiteSpace(filter))
            {
                newestOrders = newestOrders.Where(order => order.Name.Contains(filter));
            }

            var model = await PagingList.CreateAsync(newestOrders, onPage, page, sortExpression, "Status");

            model.RouteValue = new RouteValueDictionary
            {
                { "filter", filter},
                { "onPage", onPage},
                { "viewType", viewType}
            };
            return View(model);
        }//Kamil

        [HttpPost]
        [Authorize]
        public IActionResult ProjectShare([FromBody]ProjectPrivacy privacy)
        {
            var result = context.Order.Where(o => o.OrderId
            .Equals(Convert.ToInt32(privacy.Id))).FirstOrDefault();

            if (privacy.IsPrivate == null) privacy.IsPrivate = "off";

            result.Private = privacy.IsPrivate.Equals("true") ? false : true;
            context.SaveChanges();

            return Ok(result);
        }//Dominik

        public void ForwardFiles(string pathForStl, string fileName)
        {
            var adressFTP = "ftp://192.168.43.155:21";
            var usrNameFTP = "drukarka";
            var passwordFTP = "ZAQ!2wsx";

            // Get the object used to communicate with the server.  
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(adressFTP + @"/" + fileName);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            // This example assumes the FTP site uses anonymous logon.  
            request.Credentials = new NetworkCredential(usrNameFTP, passwordFTP);

            // Copy the contents of the file to the request stream.  
            StreamReader sourceStream = new StreamReader(pathForStl);
            byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
            string temp = String.Empty;
            foreach(var i in fileContents)
            {
                temp += Convert.ToChar(i);
            }
            sourceStream.Close();
            request.ContentLength = fileContents.Length;

            Stream requestStream = request.GetRequestStream();
            requestStream.Write(fileContents, 0, fileContents.Length);
            requestStream.Close();

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);

            response.Close();
        }//Dominik

        [Authorize]
        [HttpPost]
        public IActionResult ProjectView(IFormCollection param)
        {
            try
            {
                bool isRated = false;
                bool isSignedIn = true;
                bool isProjectOwner = true;

                string tmp = param.Keys.ElementAt(0).Substring(0, param.Keys.ElementAt(0).Length - 2);

                ICollection<Order> order = context.Order.Where(o => o
                .OrderId.Equals(Convert.ToInt32(tmp))).ToList();


                order.First().ViewsCount += 1;
                context.SaveChanges();

                return View(new OrderViewModel()
                {
                    NumberOfResolutsInPage = 0,
                    PageNumber = 1,
                    SortingType = String.Empty,
                    SearchString = String.Empty,
                    SortingOrder = String.Empty,
                    Order = order,
                    IsRated = isRated,
                    IsProjectOwner = isProjectOwner,
                    IsSignedIn = isSignedIn,
                    LikesCount = order.FirstOrDefault().Likes

                });
            }
            catch (Exception)
            {
                return View();
            }
        }//Dawid


        public async Task<IActionResult> LogOut()
        {
            await signInManager.SignOutAsync();
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
                var result = await signInManager.PasswordSignInAsync(vm.Login, vm.Password, vm.RememberMe, false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Invalid Login Attempt.");
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
                var result = await userManager.CreateAsync(user, vm.Password);

                if (result.Succeeded)
                {
                    await signInManager.SignInAsync(user, false);
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

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
            }

            var hasPassword = await userManager.HasPasswordAsync(user);
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

            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
            }

            var changePasswordResult = await userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                AddErrors(changePasswordResult);
                return View(model);
            }

            await signInManager.SignInAsync(user, isPersistent: false);
            StatusMessage = "Your password has been changed.";

            return RedirectToAction(nameof(ChangePassword));
        }//Dominik

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> SetPassword()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
            }

            var hasPassword = await userManager.HasPasswordAsync(user);

            if (hasPassword)
            {
                return RedirectToAction(nameof(ChangePassword));
            }

            var model = new SetPasswordViewModel { StatusMessage = StatusMessage };
            return View(model);
        }//Kamil

        [Authorize]
        public IActionResult RemoveProject(string OrderId)
        {
            var order = context.Order.Where(o => o.OrderId.Equals(Convert.ToInt32(OrderId))).FirstOrDefault();

            if(order!=default(Order))
            {
                var favourites = context.UserFavouriteProject.Where(f => f.OrderId.Equals(OrderId));

                if (favourites.Count() > 0)
                {
                    foreach(var i in favourites)
                    {
                        context.UserFavouriteProject.Remove(i);
                        context.SaveChanges();
                    }
                }

                context.Order.Remove(order);
                context.SaveChanges();
            }

            return RedirectToAction("MyProjectsAsync","Account", new { message="Usunięto projekt."});
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
            }

            var addPasswordResult = await userManager.AddPasswordAsync(user, model.NewPassword);
            if (!addPasswordResult.Succeeded)
            {
                AddErrors(addPasswordResult);
                return View(model);
            }

            await signInManager.SignInAsync(user, isPersistent: false);
            StatusMessage = "Your password has been set.";

            return RedirectToAction(nameof(SetPassword));
        }//Dawid

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ManageAccount(string message="")
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
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

            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
            }

            var email = user.Email;
            if (model.Email != email)
            {
                var setEmailResult = await userManager.SetEmailAsync(user, model.Email);
                if (!setEmailResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred setting email for user with ID '{user.Id}'.");
                }
            }

            var name = user.Name;
            if (model.Name != name)
            {
                var usr = await userManager.GetUserAsync(HttpContext.User);
                usr.Name = model.Name;
                context.SaveChanges();
            }

            var surname = user.Surname;
            if (model.Surname != surname)
            {
                var usr = await userManager.GetUserAsync(HttpContext.User);
                usr.Surname = model.Surname;
                context.SaveChanges();
            }

            var city = user.City;
            if (model.City != city)
            {
                var usr = await userManager.GetUserAsync(HttpContext.User);
                usr.City = model.City;
                context.SaveChanges();
            }

            var postCode = user.PostCode;
            if (model.PostCode != postCode)
            {
                var usr = await userManager.GetUserAsync(HttpContext.User);
                usr.PostCode = model.PostCode;
                context.SaveChanges();
            }

            var street = user.Street;
            if (model.Street != street)
            {
                var usr = await userManager.GetUserAsync(HttpContext.User);
                usr.Street = model.Street;
                context.SaveChanges();
            }

            var apartmentNumber = user.ApartmentNumber;
            if (model.ApartmentNumber != apartmentNumber)
            {
                var usr = await userManager.GetUserAsync(HttpContext.User);
                usr.ApartmentNumber = model.ApartmentNumber;
                context.SaveChanges();
            }

            var phoneNumber = user.PhoneNumber;
            if (model.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred setting phone number for user with ID '{user.Id}'.");
                }
            }

            StatusMessage = "Your profile has been updated";
            return RedirectToAction("ManageAccount", "Account", new { message="Twój profil został zaktualizowany"});
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
