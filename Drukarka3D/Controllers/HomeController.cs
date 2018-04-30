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

namespace Drukarka3D.Controllers
{
    //enum SortingOrder
    //{
    //    Ascending  = 1,
    //    Descending = 2
    //}
    //enum SortingType
    //{
    //    ByDate = 1,
    //    ByStatus = 2
    //}
    public class CanvasScreenshot
    {
        public string File { get; set; }
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

        public void ForwardFiles(string pathForStl)
        {
            var adressFTP = "http://192.168.43.155:5000:23";
            var usrNameFTP = "drukarka";
            var passwordFTP = "ZAQ!2wsx";

            // Get the object used to communicate with the server.  
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(adressFTP);
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

        //public IActionResult Search(string SearchString)
        //{
        //    ICollection<Order> userOrders = context.Order.Where(order => order.User.Id
        //    .Equals(userManager.GetUserId(HttpContext.User))
        //    &&(order.Status.Contains(SearchString)
        //    ||order.UploadDate.ToString().Contains(SearchString)
        //    ||order.Name.Contains(SearchString)))
        //    .OrderByDescending(order => order.UploadDate).ToList();
        //    return RedirectToAction("MyOrders", userOrders);
        //}

        //public IActionResult ProjectView(Order order)
        //{
        //    return View(order);         
        //}

        [HttpPost]
        public IActionResult ProjectView(IFormCollection param)
        {
            try
            {
                string tmp = param.Keys.ElementAt(0).Substring(0, param.Keys.ElementAt(0).Length-2);

                ICollection<Order> order = context.Order.Where(o => o
                .OrderId.Equals(Convert.ToInt32(tmp))).ToList();

                if (order == null) throw new NullReferenceException();

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
            catch(Exception)
            {
                return Error();
            }
}

        //[HttpPost]
        //public IActionResult Test([FromBody]ProjectViewForm projectViewForm)
        //{
        //    //ViewData["PageNumber"] = projectViewForm.PageNumber;
        //    //ViewBag.PageNumber = projectViewForm.PageNumber;

        //    ProjectsGallery(projectViewForm.SearchString, projectViewForm.SortingOrder, projectViewForm.SortingType, Convert.ToInt32(projectViewForm.PageNumber), projectViewForm.NumberOfResolutsInPage);
        //    return Json(projectViewForm);
        //}

        [HttpPost]
        public IActionResult UpdateRating([FromBody]Rating rating)
        {
            Order userOrder = context.Order.Where(order => order.OrderId
            .Equals(Convert.ToInt32(rating.Id))).First();
            //(parseFloat(@Model.ElementAt(j).Rate.ToString().Replace(",", ".")) + parseFloat(@Model.ElementAt(j).RatingsCount)) / Boolean(@Model.ElementAt(j).RatingsCount.Equals(0)) ? 1 : @Model.ElementAt(j).RatingsCount};),
            userOrder.RatingsCount += 1;
            userOrder.RatingsSum += rating.Rate;
            userOrder.Rate = Convert.ToSingle((userOrder.RatingsSum)/(userOrder.RatingsCount));
            context.SaveChanges();

            return RedirectToAction("ProjectsGallery");
        }

        //[HttpPost]
        //public IActionResult ProjectsGallery(IFormCollection param, string SearchString, string SortingOrder, string SortingType, int PageNumber, string NumberOfResolutsInPage)
        //{
        //    if (SearchString == null) SearchString = String.Empty;
        //    if (SortingOrder == null) SortingOrder = "1";
        //    if (SortingType == null) SortingType = "1";
        //    if (NumberOfResolutsInPage == null) NumberOfResolutsInPage = "0";
        //    if (PageNumber == 0) PageNumber = 1;
        //    int elToSkip = PageNumber * Convert.ToInt32(NumberOfResolutsInPage);
        //    int onPage = Convert.ToInt32(NumberOfResolutsInPage);

        //    int max = context.Order.Where(order =>
        //    (order.Status.Contains(SearchString)
        //    || order.UploadDate.ToString().Contains(SearchString)
        //    || order.Name.Contains(SearchString)
        //    || order.User.UserName.Contains(SearchString))).ToList().Count;

        //    ICollection<Order> userOrders = context.Order.Where(order =>
        //    (order.Status.Contains(SearchString)
        //    || order.UploadDate.ToString().Contains(SearchString)
        //    || order.Name.Contains(SearchString)
        //    || order.User.UserName.Contains(SearchString)))
        //    .Skip(elToSkip)
        //    .Take(elToSkip + onPage >= max ? (max - elToSkip) : onPage)
        //    .ToList();

        //    if (SortingOrder.Equals("1"))
        //    {
        //        if(SortingType.Equals("1"))
        //        {
        //            userOrders.OrderBy(order => order.UploadDate).ToList();
        //        }
        //        else
        //        {
        //            userOrders.OrderBy(order => order.Status).ToList();
        //        }
        //    }
        //    else
        //    {
        //        if (SortingType.Equals("1"))
        //        {
        //            userOrders.OrderByDescending(order => order.UploadDate).ToList();
        //        }
        //        else
        //        {
        //            userOrders.OrderByDescending(order => order.Status).ToList();
        //        }
        //    }

        //    return View(new OrderViewModel()
        //    {
        //        NumberOfResolutsInPage = Convert.ToInt32(NumberOfResolutsInPage),
        //        PageNumber = PageNumber,
        //        SortingType = SortingType,
        //        SearchString = SearchString,
        //        SortingOrder = SortingOrder,
        //        Order = userOrders
        //    });
        //}
        public async Task<IActionResult> Index(int page=1)
        {
            var newestOrders = context.Order.Where(order => order.Private.Equals(false))
                .OrderByDescending(order => order.UploadDate);

            var model = await PagingList.CreateAsync(newestOrders, 10, page);

            return View(model);
        }

        public IActionResult MyOrders()
        {

             ICollection<Order> userOrders = context.Order.Where(order => order.User.Id
            .Equals(userManager.GetUserId(HttpContext.User)))
            .OrderByDescending(order => order.UploadDate).ToList();

            return View(userOrders);
        }

        [HttpPost]
        public IActionResult MyOrders(string SearchString)
        {
            if (SearchString == null) SearchString = String.Empty;

            ICollection<Order> userOrders = context.Order.Where(order => 
            (order.Status.Contains(SearchString)
            || order.UploadDate.ToString().Contains(SearchString)
            || order.Name.Contains(SearchString)
            || order.User.UserName.Contains(SearchString)))
            .OrderByDescending(order => order.UploadDate).ToList();

            return View(userOrders);
        }

        //public IActionResult MyOrders()
        //{
        //    ICollection<Order> userOrders = context.Order.Where(order => order.User.Id
        //    .Equals(userManager.GetUserId(HttpContext.User)))
        //    .OrderByDescending(order => order.UploadDate).ToList();
        //    return View(userOrders);
        //}

        //[HttpPost]
        //public IActionResult MyOrders(string SearchString, string UserName)
        //{
        //    ICollection<Order> userOrders = context.Order.Where(order => order.User.Id
        //    .Equals(userManager.GetUserId(HttpContext.User))
        //    && (order.Status.Contains(SearchString)
        //    || order.UploadDate.ToString().Contains(SearchString)
        //    || order.Name.Contains(SearchString)))
        //    .OrderByDescending(order => order.UploadDate).ToList();
        //    return View(userOrders);
        //}

        public IActionResult AllOrders()
        {
            ICollection<Order> latestOrders = context.Order.Select(order => order)
            .OrderByDescending(order => order.UploadDate).ToList();

            return View(latestOrders);
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage([FromBody]CanvasScreenshot screen)
        {
            try
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/");

                string tmp = DateTime.Now.ToString().Replace("/", "-").Replace(" ", "-").Replace(":", "") + ".png";

                UserScreenPath = "/images/" + tmp;

                string fileNameWitPath = "wwwroot/images/" + tmp;
                using (FileStream fs = new FileStream(fileNameWitPath, FileMode.Create))
                {
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {
                        //await Response.WriteAsync("<script>alert('" + screen.File + "');</script>");
                        byte[] data = Convert.FromBase64String(screen.File);
                        bw.Write(data);
                        bw.Close();
                    }
                }

                var usrOrder = new Order
                {
                    Status = "Przyjęto",
                    User = await userManager.GetUserAsync(HttpContext.User),
                    UploadDate = DateTime.Now,
                    UserScreenPath = UserScreenPath
                };

                context.Order.Add(usrOrder);
                context.SaveChanges();

                return RedirectToAction("Loader");
            }
            catch (Exception ex)
            {               
                //await Response.WriteAsync("<script>alert('"+ex.Message+"');</script>");
                return RedirectToAction("Loader");
            }

        }

        [HttpPost]
        [RequestSizeLimit(100_000_000)]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                //await Response.WriteAsync("<script>alert('Nie wybrano pliku!')</script>");
                return RedirectToPagePermanent("Loader");
                //return Content("file not selected");
            }

            var path = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot/DoZatwierdzenia/",
                        file.GetFilename());

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            Order c = context.Order.Where(order => order.User.Id
            .Equals(userManager.GetUserId(HttpContext.User)))
            .OrderByDescending(order => order.UploadDate).First();

            Order userOrder = (from p in context.Order
                               where p.UploadDate.Equals(c.UploadDate)
                               select p).SingleOrDefault();

            if (userOrder != null)
            {
                userOrder.Name = file.GetFilename();
                userOrder.Path = file.GetFilename();
            }
            context.SaveChanges();

            var pathForStl = Path.Combine(
            Directory.GetCurrentDirectory(), "wwwroot\\DoZatwierdzenia\\");
            DirectoryInfo d = new DirectoryInfo(pathForStl);

            ForwardFiles(pathForStl);

            //var pathForGCode = Path.Combine(
            //Directory.GetCurrentDirectory(), "wwwroot\\Zatwierdzone\\");
            //DirectoryInfo gcode = new DirectoryInfo(pathForGCode);

            //var pathForSlicer = Path.Combine(
            //Directory.GetCurrentDirectory(), "wwwroot\\slic3r\\slic3r\\Slic3r.exe ");
            //DirectoryInfo slicer = new DirectoryInfo(pathForSlicer);

            //foreach (var i in d.GetFiles("*.stl"))
            //{
            //    string strCmdText;
            //    strCmdText = pathForSlicer + i.Directory+ "\\"+i+" --output "+gcode+i+".gcode";

            //    System.Diagnostics.Process process = new System.Diagnostics.Process();
            //    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            //    startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            //    startInfo.FileName = "cmd.exe";
            //    startInfo.Arguments = strCmdText;
            //    process.StartInfo = startInfo;
            //    process.Start();
            //    process.WaitForExit(1000);

            //    //Process.Start("CMD.exe", strCmdText);
            //}

            return RedirectToAction("Loader");
        }

        //[HttpPost]
        //[Authorize]
        //public IActionResult Loader(Order userOrder)
        //{

        //    return View();
        //}


        [HttpPost]
        public async Task<IActionResult> UploadFiles(List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return Content("files not selected");

            foreach (var file in files)
            {
                var path = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot/DoZatwierdzenia/",
                        file.GetFilename());

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
            ViewData["Uploaded"] = "Pomyślnie przesłano plik";
            return RedirectToAction("Loader");
        }



        [HttpPost]
        public async Task<IActionResult> UploadFileViaModel(FileInputModel model)
        {
            if (model == null ||
                model.FileToUpload == null || model.FileToUpload.Length == 0)
                return Content("file not selected");

            var path = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot",
                        model.FileToUpload.GetFilename());

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await model.FileToUpload.CopyToAsync(stream);
            }

            return RedirectToAction("Files");
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
            else return RedirectToAction("Login","Home");

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
                    return RedirectToAction("Loader","Home");
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
                    return RedirectToAction("Loader","Home");
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
