using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Drukarka3D.Models;
using Drukarka3DData.Models;
using Drukarka3DData;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.EntityFrameworkCore;

namespace Drukarka3D.Controllers
{
    public class HomeController : Controller
    {
        private Drukarka3DContext context;
        private readonly IFileProvider fileProvider;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public HomeController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, IFileProvider fileProvider,
            Drukarka3DContext context)
        {
            this.context = context;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.fileProvider = fileProvider;
        }

        public IActionResult MyOrders()
        {
            ICollection<Order> userOrders = context.Order.Where(order => order.User.Id
            .Equals(userManager.GetUserId(HttpContext.User))).ToList();
            return View(userOrders);
        }

        [HttpPost]
        [RequestSizeLimit(100_000_000)]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                await Response.WriteAsync("<script>alert('Nie wybrano pliku!')</script>");
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

            var userFile = new Drukarka3DData.Models.File
            {
                Path = path,
                User = await userManager.GetUserAsync(HttpContext.User),
                Name = file.GetFilename()
            };

            context.File.Add(userFile);
            context.SaveChanges();

            var userOrder = new Order
            {
                Status = "Przyjęto",
                User = await userManager.GetUserAsync(HttpContext.User),
                File = userFile,
                UploadDate = DateTime.Today.ToString("D"),
                Name = file.GetFilename(),
                Path = file.GetFilename()
            };

            context.Order.Add(userOrder);
            context.SaveChanges();

            var pathForStl = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot\\DoZatwierdzenia\\");
            DirectoryInfo d = new DirectoryInfo(pathForStl);

            

            foreach (var i in d.GetFiles("*.stl"))
            {
                string strCmdText;
                strCmdText = "/C /wwwroot/slic3r/slic3r/Slic3r-console.exe input --output output";
                System.Diagnostics.Process.Start("CMD.exe", strCmdText);
            }
            
            //test

            return RedirectToAction("Loader");
        }

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

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
