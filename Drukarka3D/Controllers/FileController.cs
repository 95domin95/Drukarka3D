using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Drukarka3DData;
using Drukarka3DData.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace Drukarka3D.Controllers
{
    public class FileController : Controller
    {
        private readonly IFileProvider _fileProvider;
        private Drukarka3DContext _context;
        private UserManager<ApplicationUser> _userManager;

        public FileController(Drukarka3DContext context,
            IFileProvider fileProvider,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _fileProvider = fileProvider;
            _userManager = userManager;
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
            string temp = String.Empty;
            foreach (var i in fileContents)
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

        public IActionResult Files()
        {
            var model = new FilesViewModel();
            foreach (var item in _fileProvider.GetDirectoryContents(""))
            {
                model.Files.Add(
                    new FileDetails { Name = item.Name, Path = item.PhysicalPath });
            }
            return View(model);
        }//Dawid

        public async Task<IActionResult> Download(string filename)
        {
            try
            {
                if (filename == null)
                {
                    throw new FileNotFoundException();
                }


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
            catch (FileNotFoundException)
            {
                //await Response.WriteAsync("<script>alert('Blad: Nie znaleziono pliku!')</script>");
                return RedirectToAction("Index","Home", new { message = "Nie znaleziono pliku!" });
            }

        }//Kamil

        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }//Dominik

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
        }//Dominik

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UploadImage([FromBody]CanvasScreenshot screen)
        {
            try
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
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
                await Response.WriteAsync("<script>alert('" + ex.Message + "');</script>");
                return Json(screen);
            }

        }//Dominik

        public bool CheckProjectNameAvailability(string projectName, ApplicationUser user)
        {
            var allUsrProjects = _context.Order.Where(o => o.Path.Equals(user.Id + projectName)).ToList();

            return allUsrProjects.Count() != 0;
        }//Dawid

        [HttpPost]
        [Authorize]
        [RequestSizeLimit(100_000_000)]
        public async Task<IActionResult> UploadFile(LoaderViewModel vm)
        {
            if (vm.File == null || vm.File.Length == 0)
            {
                //await Response.WriteAsync("<script>alert('Nie wybrano pliku!')</script>");
                return RedirectToAction("Loader","Home", new { message = "Nie wybrano pliku!" });
            }

            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (CheckProjectNameAvailability(vm.ProjectName, user))
            {
                //await Response.WriteAsync("<script>alert('Już stworzyłeś projekt o takiej samej nazwie!')</script>");
                return RedirectToAction("Loader", "Home", new { message = "Już stworzyłeś projekt o takiej samej nazwie!" });
            }

            if (vm.ProjectName == null || vm.ProjectName == "") vm.ProjectName = vm.File.GetFilename().Substring(0, vm.File.GetFilename().Length - 4);

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/DoZatwierdzenia/", user.Id + vm.ProjectName + ".stl");

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await vm.File.CopyToAsync(stream);
            }

            if (vm.IsPrivate == null) vm.IsPrivate = "false";

            _context.Order.Add(new Order
            {
                Status = "Przyjęto",
                User = user,
                UserId = user.Id,
                UploadDate = DateTime.Now,
                Private = vm.IsPrivate.Equals("false"),
                Name = vm.ProjectName,
                Path = user.Id + vm.ProjectName + ".stl",
                UserScreenPath = "/images/" + user.Id + vm.ProjectName + "thumb.png"
            });

            _context.SaveChanges();

            var pathForStl = Path.Combine(
            Directory.GetCurrentDirectory(), "wwwroot\\DoZatwierdzenia\\", vm.ProjectName);
            DirectoryInfo d = new DirectoryInfo(pathForStl);

            //ForwardFiles(pathForStl, userOrder.Path);

            return RedirectToAction("Loader","Home", new { message = "Pomyślnie dodano projekt." });
        }//Dominik

        [HttpPost]
        [Authorize]
        public IActionResult Print([FromBody]FileToPrint data)
        {
            try
            {
                var path1 = Path.Combine(
                Directory.GetCurrentDirectory(), "wwwroot/DoZatwierdzenia/", data.FilePath);

                string currentDate = DateTime.Now.ToString();
                currentDate = currentDate.Replace(':', '_');
                string fileName = currentDate + _userManager.GetUserId(HttpContext.User) + ".stl";

                var path2 = Path.Combine(
                Directory.GetCurrentDirectory(), "wwwroot/DoZatwierdzenia/", fileName);

                System.IO.File.Copy(path1, path2);

                return Json(new JsonResult(data));
            }
            catch(FileNotFoundException)
            {
                return Json(new JsonResult(new { message="Nie znaleziono pliku!"}));
            }
            catch(DirectoryNotFoundException)
            {
                return Json(new JsonResult(new { message = "Nie znaleziono ścieżki do pliku!" }));
            }
        }//Dominik
    }
}