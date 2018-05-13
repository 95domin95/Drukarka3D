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
    public class CanvasScreenshot
    {
        public string File { get; set; }
        public string ProjectName { get; set; }
    }
    public class Like
    {
        public int OrderId { get; set; }
    }

    public class Rating
    {
        public double Rate { get; set; }

        public string Id { get; set; }
    }

    public class MyProjectsForm
    {
        public string UserName { get; set; }
        public string SearchString { get; set; }
    }

    public class FileToPrint
    {
        public string FilePath { get; set; }
    }

    public class ProjectViewForm
    {
        public string PageNumber { get; set; }
        public string SearchString { get; set; }
        public string NumberOfResolutsInPage { get; set; }
        public string SortingType { get; set; }
        public string SortingOrder { get; set; }
    }

    public class HomeController : Controller
    {
        public string UserScreenPath { get; set; }
        private Drukarka3DContext context;
        private readonly IEmailSender emailSender;
        private readonly IFileProvider fileProvider;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        [TempData]
        public string StatusMessage { get; set; }

        public HomeController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, IEmailSender emailSender,
            IFileProvider fileProvider, Drukarka3DContext context)
        {
            this.emailSender = emailSender;
            this.context = context;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.fileProvider = fileProvider;
        }

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
            sourceStream.Close();
            request.ContentLength = fileContents.Length;

            Stream requestStream = request.GetRequestStream();
            requestStream.Write(fileContents, 0, fileContents.Length);
            requestStream.Close();

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);

            response.Close();
        }

        [HttpPost]
        public IActionResult ProjectView(IFormCollection param)
        {
            try
            {
                bool isRated = false;
                bool isSignedIn = true;
                bool isProjectOwner = false;
                bool isLiked = false;

                UserFavoriteProject project = default(UserFavoriteProject);

                int tmp = Convert.ToInt32(param.Keys.ElementAt(0)
                    .Substring(0, param.Keys.ElementAt(0).Length-2));

                ICollection<Order> order = context.Order.Where(o => o
                .OrderId.Equals(tmp)).ToList();

                if (!signInManager.IsSignedIn(User)) isSignedIn = false;
                else
                {
                    project = context.UserFavouriteProject
                    .Where(p => p.OrderId.Equals(tmp))
                    .FirstOrDefault();

                    if (project != default(UserFavoriteProject))
                    {
                        if (project.IsRated.Equals(true)) isRated = true;

                        project = context.UserFavouriteProject
                        .Where(p => p.Order.Equals(tmp)
                        && p.UserId.Equals(userManager.GetUserId(HttpContext.User)))
                        .FirstOrDefault();

                        if (project != default(UserFavoriteProject))
                        {
                            isProjectOwner = true;

                            if (project.IsFavourite.Equals(true))
                            {
                                isLiked = true;
                            }
                        }                        
                    }
                }

                //if (order == null) throw new NullReferenceException();

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
                    IsLiked = isLiked,
                    LikesCount = order.FirstOrDefault().Likes
                    
                });
            }
            catch(Exception)
            {
                return Loader();
            }
        }
        [HttpPost]
        public async Task<IActionResult> AddToFavourites([FromBody]Like like)
        {
            if (signInManager.IsSignedIn(User))
            {
                var userOrder = context.Order.Where(order => order.OrderId
                .Equals(Convert.ToInt32(like.OrderId))).FirstOrDefault();

                var favoriteProject = context.UserFavouriteProject
                .Where(u => u.UserId.Equals(userManager.GetUserId(HttpContext.User))
                && u.Order.Equals(userOrder)).FirstOrDefault();

                if (favoriteProject == default(UserFavoriteProject))
                {
                    context.UserFavouriteProject.Add(new UserFavoriteProject()
                    {
                        User = await userManager.GetUserAsync(HttpContext.User),
                        Order = userOrder,
                        IsFavourite = true
                    });

                    userOrder.Likes += 1;

                    context.SaveChanges();
                }
                else
                {
                    if (!favoriteProject.IsFavourite)
                    {
                        favoriteProject.IsFavourite = true;

                        userOrder.Likes += 1;

                        context.SaveChanges();
                    }

                }
            }

            return Json(like);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRating([FromBody]Rating rating)
        {
            if(signInManager.IsSignedIn(User))
            {
                var userOrder = context.Order.Where(order => order.OrderId
                .Equals(Convert.ToInt32(rating.Id))).FirstOrDefault();

                var favoriteProject = context.UserFavouriteProject
                    .Where(u => u.UserId.Equals(userManager.GetUserId(HttpContext.User))
                    && u.Order.Equals(userOrder)).FirstOrDefault();

                if (favoriteProject == default(UserFavoriteProject))
                {
                    context.UserFavouriteProject.Add(new UserFavoriteProject()
                    {
                        User = await userManager.GetUserAsync(HttpContext.User),
                        Order = userOrder,
                        IsRated = true
                    });

                    userOrder.RatingsCount += 1;
                    userOrder.RatingsSum += rating.Rate;
                    userOrder.Rate = Convert.ToSingle((userOrder.RatingsSum) / (userOrder.RatingsCount));

                    context.SaveChanges();
                }
                else
                {
                    if (!favoriteProject.IsRated)
                    {
                        favoriteProject.IsRated = true;

                        userOrder.RatingsCount += 1;
                        userOrder.RatingsSum += rating.Rate;
                        userOrder.Rate = Convert.ToSingle((userOrder.RatingsSum) / (userOrder.RatingsCount));

                        context.SaveChanges();
                    }
                }
            }

            return RedirectToAction("ProjectsGallery");
        }

        public async Task<IActionResult> Index(string filter = "", int page=1, string sortExpression = "Status", int onPage=20)
        {
            var newestOrders = context.Order.AsNoTracking().Where(order => order.Private.Equals(false))
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

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> UploadImage([FromBody]CanvasScreenshot screen)
        {
            try
            {
                var user = await userManager.GetUserAsync(HttpContext.User);
                if (CheckProjectNameAvailability(screen.ProjectName, user))
                {
                    return Json(screen);
                }

                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/");

                var fileName = user.Id + screen.ProjectName + "thumb.png";

                string fileNameWithPath = "wwwroot/images/" + fileName;
                using (FileStream fs = new FileStream(fileNameWithPath, FileMode.Create))
                {
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {                      
                        byte[] data = Convert.FromBase64String(screen.File);
                        bw.Write(data);
                        bw.Close();
                    }
                }

                return Json(screen);
            }
            catch (Exception ex)
            {               
                await Response.WriteAsync("<script>alert('"+ex.Message+"');</script>");
                return Json(screen);
            }

        }

        public bool CheckProjectNameAvailability(string projectName, ApplicationUser user)
        {
            var allUsrProjects = context.Order.Where(o => o.Path.Equals(user.Id + projectName)).ToList();

            return allUsrProjects.Count() != 0;
        }

        [HttpPost]
        [RequestSizeLimit(100_000_000)]
        public async Task<IActionResult> UploadFile(string projectName, string isPrivate, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                await Response.WriteAsync("<script>alert('Nie wybrano pliku!')</script>");
                return RedirectToAction("Index");
            }

            var user = await userManager.GetUserAsync(HttpContext.User);

            if (CheckProjectNameAvailability(projectName, user))
            {
                await Response.WriteAsync("<script>alert('Już stworzyłeś projekt o takiej samej nazwie!')</script>");
                return RedirectToAction("Index");
            }

            if (projectName == null||projectName == "") projectName = file.GetFilename().Substring(0, file.GetFilename().Length - 4);

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/DoZatwierdzenia/", user.Id + projectName + ".stl");
                        
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            if (isPrivate == null) isPrivate = "off";

            context.Order.Add(new Order
            {
                Status = "Przyjęto",
                User = user,
                UserId = user.Id,
                UploadDate = DateTime.Now,
                Private = isPrivate.Equals("on") ? false : true,
                Name = projectName,
                Path = user.Id + projectName + ".stl",
                UserScreenPath = "/images/" + user.Id + projectName + "thumb.png"
            });

            context.SaveChanges();
            
            var pathForStl = Path.Combine(
            Directory.GetCurrentDirectory(), "wwwroot\\DoZatwierdzenia\\", projectName);
            DirectoryInfo d = new DirectoryInfo(pathForStl);
            
            //ForwardFiles(pathForStl, userOrder.Path);

            return RedirectToAction("Index");
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

        public IActionResult Files()
        {
            var model = new FilesViewModel();
            foreach (var item in this.fileProvider.GetDirectoryContents(""))
            {
                model.Files.Add(
                    new FileDetails { Name = item.Name, Path = item.PhysicalPath });
            }
            return View(model);
        }

        public async Task<IActionResult> Download(string filename)
        {
            try
            {
                if (filename == null)
                    return Content("filename not present");

                var path = Path.Combine(
                               Directory.GetCurrentDirectory(),
                               "wwwroot/DoZatwierdzenia/", filename);

                var memory = new MemoryStream();
                using (var stream = new FileStream(path, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;
                return File(memory, GetContentType(path), Path.GetFileName(path));
            }
            catch(FileNotFoundException)
            {
                await Response.WriteAsync("<script>alert('Blad: Nie znaleziono pliku!')</script>");
                return RedirectToAction("Loader");
            }

        }

        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }

        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.ms-word"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"},
                {".stl", "application/vnd.ms-pki.stl"}
            };
        }

        [Authorize]
        public IActionResult Loader()
        {
            if (User.Identity.IsAuthenticated)
            {
                ViewData["Title"] = "Strona Główna";
                return View();
            }
            else return RedirectToAction("Index","Home");

        }

        public async Task<IActionResult> LogOut()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login", "Home");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if(ModelState.IsValid)
            {
                var result = await signInManager.PasswordSignInAsync(vm.Login, vm.Password, vm.RememberMe, false);
                if(result.Succeeded)
                {
                    return RedirectToAction("Index","Home");
                }
                ModelState.AddModelError("", "Invalid Login Attempt.");
                return View(vm);
            }
            return View(vm);
        }

        [HttpGet]
        public IActionResult Register()
        {
            ViewData["Title"] = "Rejestracja";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = vm.Login, Email = vm.Email };
                var result = await userManager.CreateAsync(user, vm.Password);

                if(result.Succeeded)
                {
                    await signInManager.SignInAsync(user, false);
                    return RedirectToAction("Index","Home");
                }
                else
                {
                    foreach(var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(vm);
        }

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
        }

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
        }

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
        }

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
        }

        [HttpGet]
        public async Task<IActionResult> ManageAccount()
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

            return View(model);
        }

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
            return RedirectToAction(nameof(ManageAccount));
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> SendVerificationEmail(IndexViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    var user = await userManager.GetUserAsync(User);
        //    if (user == null)
        //    {
        //        throw new ApplicationException($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
        //    }

        //    var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        //    var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
        //    var email = user.Email;
        //    await emailSender.SendEmailConfirmationAsync(email, callbackUrl);

        //    StatusMessage = "Verification email sent. Please check your email.";
        //    return RedirectToAction(nameof(Index));
        //}

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
