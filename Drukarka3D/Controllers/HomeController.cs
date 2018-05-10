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

        public void ForwardFiles(string pathForStl)
        {
            var adressFTP = "http://192.168.43.155:5000:23";//Denis
            var usrNameFTP = "drukarka";//Denis
            var passwordFTP = "ZAQ!2wsx";//Denis

            // Get the object used to communicate with the server.  
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(adressFTP);//Kamil
            request.Method = WebRequestMethods.Ftp.UploadFile;//Kamil

            // This example assumes the FTP site uses anonymous logon.  
            request.Credentials = new NetworkCredential(usrNameFTP, passwordFTP);//Kamil

            // Copy the contents of the file to the request stream.  
            StreamReader sourceStream = new StreamReader(pathForStl);//Kamil
            byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());//Kamil
            sourceStream.Close();//Kamil
            request.ContentLength = fileContents.Length;//Kamil

            Stream requestStream = request.GetRequestStream();//Dominik
            requestStream.Write(fileContents, 0, fileContents.Length);//Dominik
            requestStream.Close();//Dominik

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();//Dominik

            Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);//Kamil

            response.Close();//Dominik
        }

        [HttpPost]
        public IActionResult ProjectView(IFormCollection param)
        {
            try
            {
                string tmp = param.Keys.ElementAt(0).Substring(0, param.Keys.ElementAt(0).Length-2);//Dominik

                ICollection<Order> order = context.Order.Where(o => o
                .OrderId.Equals(Convert.ToInt32(tmp))).ToList();//Dominik

                if (order == null) throw new NullReferenceException();//Kamil

                order.First().ViewsCount += 1;
                context.SaveChanges();//Dominik

                return View(new OrderViewModel()
                {
                    NumberOfResolutsInPage = 0,
                    PageNumber = 1,
                    SortingType = String.Empty,
                    SearchString = String.Empty,
                    SortingOrder = String.Empty,
                    Order = order
                    
                });//Dominik
            }
            catch(Exception)
            {
                return Error();
            }
        }


        [HttpPost]
        public IActionResult UpdateRating([FromBody]Rating rating)
        {
            Order userOrder = context.Order.Where(order => order.OrderId
            .Equals(Convert.ToInt32(rating.Id))).First();//Dominik
            //(parseFloat(@Model.ElementAt(j).Rate.ToString().Replace(",", ".")) + parseFloat(@Model.ElementAt(j).RatingsCount)) / Boolean(@Model.ElementAt(j).RatingsCount.Equals(0)) ? 1 : @Model.ElementAt(j).RatingsCount};),
            userOrder.RatingsCount += 1;//Dominik
            userOrder.RatingsSum += rating.Rate;//Dominik
            userOrder.Rate = Convert.ToSingle((userOrder.RatingsSum)/(userOrder.RatingsCount));//Dominik
            context.SaveChanges();//Kamil

            return RedirectToAction("ProjectsGallery");//Kamil
        }

        public async Task<IActionResult> Index(string filter = "", int page=1, string sortExpression = "Status")
        {
            var newestOrders = context.Order.AsNoTracking().Where(order => order.Private.Equals(false))
            .OrderByDescending(order => order.UploadDate).AsQueryable();//Dominik

            if (!string.IsNullOrWhiteSpace(filter))
            {
                newestOrders = newestOrders.Where(order => order.Name.Contains(filter));//Dominik
            }

            var model = await PagingList.CreateAsync(newestOrders, 16, page, sortExpression, "Status");//Dominik

            model.RouteValue = new RouteValueDictionary
            {
                { "filter", filter}
            };//Dominik

            return View(model);//Kamil
        }

        public IActionResult MyOrders()
        {

             ICollection<Order> userOrders = context.Order.Where(order => order.User.Id
            .Equals(userManager.GetUserId(HttpContext.User)))
            .OrderByDescending(order => order.UploadDate).ToList();//Dominik

            return View(userOrders);//Dominik
        }

        [HttpPost]
        public IActionResult MyOrders(string SearchString)
        {
            if (SearchString == null) SearchString = String.Empty; //Kamil

            ICollection<Order> userOrders = context.Order.Where(order => 
            (order.Status.Contains(SearchString)
            || order.UploadDate.ToString().Contains(SearchString)
            || order.Name.Contains(SearchString)
            || order.User.UserName.Contains(SearchString)))
            .OrderByDescending(order => order.UploadDate).ToList();//Dominik

            return View(userOrders);//Kamil
        }

        public IActionResult AllOrders()
        {
            ICollection<Order> latestOrders = context.Order.Select(order => order)
            .OrderByDescending(order => order.UploadDate).ToList();//Dominik

            return View(latestOrders);//Dominik
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage([FromBody]CanvasScreenshot screen)
        {
            try
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/");//Kamil

                string tmp = DateTime.Now.ToString().Replace("/", "-").Replace(" ", "-").Replace(":", "") + ".png";//Kamil

                UserScreenPath = "/images/" + tmp;//Kamil

                string fileNameWitPath = "wwwroot/images/" + tmp;//Kamil
                using (FileStream fs = new FileStream(fileNameWitPath, FileMode.Create))
                {
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {
                        //await Response.WriteAsync("<script>alert('" + screen.File + "');</script>");
                        byte[] data = Convert.FromBase64String(screen.File);//Dawid
                        bw.Write(data);//Dawid
                        bw.Close();//Dawid
                    }
                }

                var usrOrder = new Order
                {
                    Status = "Przyjęto",//Kamil
                    User = await userManager.GetUserAsync(HttpContext.User),//Dominik
                    UploadDate = DateTime.Now,//Kamil   
                    UserScreenPath = UserScreenPath //Kamil
                };

                context.Order.Add(usrOrder);//Dominik
                context.SaveChanges();//Kamil

                return RedirectToAction("Loader");//Kamil
            }
            catch (Exception ex)
            {               
                //await Response.WriteAsync("<script>alert('"+ex.Message+"');</script>");
                return RedirectToAction("Loader");//Dawid
            }

        }

        [HttpPost]
        public IActionResult Print([FromBody]FileToPrint data)
        {
            var path1 = Path.Combine(
            Directory.GetCurrentDirectory(), "wwwroot/DoZatwierdzenia/", data.FilePath);//Kamil

            string currentDate = DateTime.Now.ToString();//Kamil
            currentDate = currentDate.Replace(':', '_');//Dominik
            string fileName = currentDate + userManager.GetUserId(HttpContext.User) + ".stl";//Kamil

            var path2 = Path.Combine(
            Directory.GetCurrentDirectory(), "wwwroot/DoZatwierdzenia/", fileName);//Dominik

            System.IO.File.Copy(path1, path2);//Dominik

            return Json(new JsonResult(data));//Dominik
        }

        [HttpPost]
        [RequestSizeLimit(100_000_000)]
        public async Task<IActionResult> UploadFile(string isPrivate, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                //await Response.WriteAsync("<script>alert('Nie wybrano pliku!')</script>");
                return RedirectToPagePermanent("Loader");//Dominik
                //return Content("file not selected");
            }

            string currentDate = DateTime.Now.ToString();//Kamil
            currentDate = currentDate.Replace(':', '_');//Kamil
            string fileName = currentDate + userManager.GetUserId(HttpContext.User) + file.GetFilename();//Dominik

            var path = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot/DoZatwierdzenia/", fileName);//Kamil
                        

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);//Dominik
            }

            Order c = context.Order.Where(order => order.User.Id
            .Equals(userManager.GetUserId(HttpContext.User)))
            .OrderByDescending(order => order.UploadDate).First();//Dominik

            Order userOrder = (from p in context.Order
                               where p.UploadDate.Equals(c.UploadDate)
                               select p).SingleOrDefault();//Dominik

            if (isPrivate == null) isPrivate = "off";//Kamil

            if (userOrder != null)
            {
                userOrder.Private = isPrivate.Equals("on") ? false : true;//Dawid
                userOrder.Name = file.GetFilename().Substring(0, file.GetFilename().Length - 4);//Dominik
                userOrder.Path = fileName;//Dawid
            }
            context.SaveChanges();//Kamil

            var pathForStl = Path.Combine(
            Directory.GetCurrentDirectory(), "wwwroot\\DoZatwierdzenia\\");//Kamil
            DirectoryInfo d = new DirectoryInfo(pathForStl);//Kamil

            //ForwardFiles(pathForStl);

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

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> UploadFiles(List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return Content("files not selected");//Dawid

            foreach (var file in files)
            {
                var path = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot/DoZatwierdzenia/",
                        file.GetFilename());//Kamil

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);//Dominik
                }
            }
            ViewData["Uploaded"] = "Pomyślnie przesłano plik";//Dominik
            return RedirectToAction("Loader");//Dawid
        }



        [HttpPost]
        public async Task<IActionResult> UploadFileViaModel(FileInputModel model)
        {
            if (model == null ||
                model.FileToUpload == null || model.FileToUpload.Length == 0)
                return Content("file not selected");//Dominik

            var path = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot",
                        model.FileToUpload.GetFilename());//Kamil

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await model.FileToUpload.CopyToAsync(stream);//Dominik
            }

            return RedirectToAction("Files");//Dominik
        }

        public IActionResult Files()
        {
            var model = new FilesViewModel();//Dominik
            foreach (var item in this.fileProvider.GetDirectoryContents(""))
            {
                model.Files.Add(
                    new FileDetails { Name = item.Name, Path = item.PhysicalPath });//Dominik
            }
            return View(model);//Dominik
        }

        public async Task<IActionResult> Download(string filename)
        {
            try
            {
                if (filename == null)
                    return Content("filename not present");//Kamil

                var path = Path.Combine(
                               Directory.GetCurrentDirectory(),
                               "wwwroot/DoZatwierdzenia/", filename);//Kamil

                var memory = new MemoryStream();//Dominik
                using (var stream = new FileStream(path, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);//Dominik
                }
                memory.Position = 0;//Dominik
                return File(memory, GetContentType(path), Path.GetFileName(path));//Dominik
            }
            catch(FileNotFoundException)
            {
                await Response.WriteAsync("<script>alert('Blad: Nie znaleziono pliku!')</script>");//Dominik
                return RedirectToAction("Loader");//Dominik
            }

        }

        private string GetContentType(string path)
        {
            var types = GetMimeTypes();//Dominik
            var ext = Path.GetExtension(path).ToLowerInvariant();//Dominik
            return types[ext];//Dominik
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
            };//Dawid
        }

        [Authorize]
        public IActionResult Loader()
        {
            if (User.Identity.IsAuthenticated)
            {
                ViewData["Title"] = "Strona Główna";//Dawid
                return View();//Dawid
            }
            else return RedirectToAction("Index","Home");//Dawid

        }

        public async Task<IActionResult> LogOut()
        {
            await signInManager.SignOutAsync();//Dominik
            return RedirectToAction("Login", "Home");//Dominik
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
                var result = await signInManager.PasswordSignInAsync(vm.Login, vm.Password, vm.RememberMe, false);//Dominik
                if (result.Succeeded)
                {
                    return RedirectToAction("Index","Home");//Dominik
                }
                ModelState.AddModelError("", "Invalid Login Attempt.");//Dominik
                return View(vm);//Dominik
            }
            return View(vm);//Dominik
        }

        [HttpGet]
        public IActionResult Register()
        {
            ViewData["Title"] = "Rejestracja";//Dawid
            return View();//Dawid
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = vm.Login, Email = vm.Email };//Dominik
                var result = await userManager.CreateAsync(user, vm.Password);//Dominik

                if (result.Succeeded)
                {
                    await signInManager.SignInAsync(user, false);//Dominik
                    return RedirectToAction("Index","Home");//Dominik
                }
                else
                {
                    foreach(var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);//Dominik
                    }
                }
            }
            return View(vm);//Dominik
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await userManager.GetUserAsync(User);//Dominik
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{userManager.GetUserId(User)}'.");//Dawid
            }

            var hasPassword = await userManager.HasPasswordAsync(user);//Dominik
            if (!hasPassword)
            {
                return RedirectToAction(nameof(SetPassword));//Dominik
            }

            var model = new ChangePasswordViewModel { StatusMessage = StatusMessage };//Dominik
            return View(model);//Dominik
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);//Dominik
            }

            var user = await userManager.GetUserAsync(User);//Dominik
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{userManager.GetUserId(User)}'.");//Dominik
            }

            var changePasswordResult = await userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);//Dominik
            if (!changePasswordResult.Succeeded)
            {
                AddErrors(changePasswordResult);//Dominik
                return View(model);//Dominik
            }

            await signInManager.SignInAsync(user, isPersistent: false);//Dominik
            StatusMessage = "Your password has been changed.";//Dominik

            return RedirectToAction(nameof(ChangePassword));//Dominik
        }

        [HttpGet]
        public async Task<IActionResult> SetPassword()
        {
            var user = await userManager.GetUserAsync(User);//Dominik
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{userManager.GetUserId(User)}'.");//Dominik
            }

            var hasPassword = await userManager.HasPasswordAsync(user);//Dominik

            if (hasPassword)
            {
                return RedirectToAction(nameof(ChangePassword));//Dominik
            }

            var model = new SetPasswordViewModel { StatusMessage = StatusMessage };//Dominik
            return View(model);//Dominik
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);//Kamil
            }

            var user = await userManager.GetUserAsync(User);//Dominik
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{userManager.GetUserId(User)}'.");//Kamil
            }

            var addPasswordResult = await userManager.AddPasswordAsync(user, model.NewPassword);//Dominik
            if (!addPasswordResult.Succeeded)
            {
                AddErrors(addPasswordResult);//Kamil
                return View(model);//Kamil
            }

            await signInManager.SignInAsync(user, isPersistent: false);//Dominik
            StatusMessage = "Your password has been set.";//Kamil

            return RedirectToAction(nameof(SetPassword));//Kamil
        }

        [HttpGet]
        public async Task<IActionResult> ManageAccount()
        {
            var user = await userManager.GetUserAsync(User);//Dominik
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{userManager.GetUserId(User)}'.");//Dominik
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
            };//Dominik

            return View(model);//Dominik
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageAccount(IndexViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);//Kamil
            }

            var user = await userManager.GetUserAsync(User);//Dominik
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{userManager.GetUserId(User)}'.");//Kamil
            }

            var email = user.Email;//Kamil
            if (model.Email != email)
            {
                var setEmailResult = await userManager.SetEmailAsync(user, model.Email);//Dominik
                if (!setEmailResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred setting email for user with ID '{user.Id}'.");//Kamil
                }
            }

            var name = user.Name;//Kamil
            if (model.Name != name)
            {
                var usr = await userManager.GetUserAsync(HttpContext.User);//Dominik
                usr.Name = model.Name;//Dominik
                context.SaveChanges();//Kamil
            }

            var surname = user.Surname;//Kamil
            if (model.Surname != surname)
            {
                var usr = await userManager.GetUserAsync(HttpContext.User);//Dominik
                usr.Surname = model.Surname;//Dominik
                context.SaveChanges();//Kamil
            }

            var city = user.City;//Kamil
            if (model.City != city)
            {
                var usr = await userManager.GetUserAsync(HttpContext.User);//Dominik
                usr.City = model.City;//Dominik
                context.SaveChanges();//Kamil
            }

            var postCode = user.PostCode;//Kamil
            if (model.PostCode != postCode)
            {
                var usr = await userManager.GetUserAsync(HttpContext.User);//Dominik
                usr.PostCode = model.PostCode;//Dominik
                context.SaveChanges();//Kamil
            }

            var street = user.Street;//Kamil
            if (model.Street != street)
            {
                var usr = await userManager.GetUserAsync(HttpContext.User);//Dominik
                usr.Street = model.Street;//Dominik
                context.SaveChanges();//Kamil
            }

            var apartmentNumber = user.ApartmentNumber;//Kamil
            if (model.ApartmentNumber != apartmentNumber)
            {
                var usr = await userManager.GetUserAsync(HttpContext.User);//Dominik
                usr.ApartmentNumber = model.ApartmentNumber;//Dominik
                context.SaveChanges();//Kamil
            }

            var phoneNumber = user.PhoneNumber;//Kamil
            if (model.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await userManager.SetPhoneNumberAsync(user, model.PhoneNumber);//Dominik
                if (!setPhoneResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred setting phone number for user with ID '{user.Id}'.");//Kamil
                }
            }

            StatusMessage = "Your profile has been updated";//Kamil
            return RedirectToAction(nameof(ManageAccount));//Kamil
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
                ModelState.AddModelError(string.Empty, error.Description);//Dominik
            }
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });//Dominik
        }
    }
}
