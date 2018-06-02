using Drukarka3DData;
using Drukarka3DData.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReflectionIT.Mvc.Paging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Drukarka3D.Controllers
{
    public class ProjectsController : Controller
    {

        private Drukarka3DContext _context;
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signInManager;

        public ProjectsController(Drukarka3DContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [Authorize]
        public async Task<IActionResult> MyProjectsAsync(string filter = "", int page = 1,
             string sortExpression = "Status", string sortOrder = "Ascending",
             int elementsOnPage = 20, int onPage = 10, string message = "")
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            var projects = _context.Order.AsNoTracking().Where(order => order.UserId
                .Equals(_userManager.GetUserId(HttpContext.User)))
                .OrderBy(order => order.UploadDate).AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter))
            {
                projects = projects.Where(p => p.Name.Contains(filter));
            }

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

            if(numberOfElements.Equals(0))
            {
                message = "Brak elementów do wyśtlenia";
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
        public async Task<IActionResult> RemoveFromFavouritesAsync([FromBody]Like like)
        {
            if (_signInManager.IsSignedIn(User))
            {
                var userOrder = _context.Order.Where(order => order.OrderId
                .Equals(Convert.ToInt32(like.OrderId))).FirstOrDefault();

                var favoriteProject = _context.UserFavouriteProject
                .Where(u => u.UserId.Equals(_userManager.GetUserId(HttpContext.User))
                && u.Order.Equals(userOrder)).FirstOrDefault();

                if (favoriteProject != default(UserFavoriteProject))
                {
                    if (favoriteProject.IsFavourite)
                    {
                        favoriteProject.IsFavourite = false;

                        userOrder.Likes -= 1;

                        _context.SaveChanges();
                    }
                }
            }

            return Json(like);
        }//Dawid

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddToFavouritesAsync([FromBody]Like like)
        {
            if (_signInManager.IsSignedIn(User))
            {
                var userOrder = _context.Order.Where(order => order.OrderId
                .Equals(Convert.ToInt32(like.OrderId))).FirstOrDefault();

                var favoriteProject = _context.UserFavouriteProject
                .Where(u => u.UserId.Equals(_userManager.GetUserId(HttpContext.User))
                && u.Order.Equals(userOrder)).FirstOrDefault();

                if (favoriteProject == default(UserFavoriteProject))
                {
                    _context.UserFavouriteProject.Add(new UserFavoriteProject()
                    {
                        User = await _userManager.GetUserAsync(HttpContext.User),
                        Order = userOrder,
                        IsFavourite = true
                    });

                    userOrder.Likes += 1;

                    _context.SaveChanges();
                }
                else
                {
                    if (!favoriteProject.IsFavourite)
                    {
                        favoriteProject.IsFavourite = true;

                        userOrder.Likes += 1;

                        _context.SaveChanges();
                    }

                }
            }

            return Json(like);
        }//Dawid

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateRating([FromBody]Rating rating)
        {
            var userOrder = _context.Order.Where(order => order.OrderId
            .Equals(Convert.ToInt32(rating.Id))).FirstOrDefault();

            var favoriteProject = _context.UserFavouriteProject
                .Where(u => u.UserId.Equals(_userManager.GetUserId(HttpContext.User))
                && u.Order.Equals(userOrder)).FirstOrDefault();

            if (favoriteProject == default(UserFavoriteProject))
            {
                _context.UserFavouriteProject.Add(new UserFavoriteProject()
                {
                    User = await _userManager.GetUserAsync(HttpContext.User),
                    Order = userOrder,
                    IsRated = true
                });

                userOrder.RatingsCount += 1;
                userOrder.RatingsSum += rating.Rate;
                userOrder.Rate = Convert.ToSingle((userOrder.RatingsSum) / (userOrder.RatingsCount));
                _context.SaveChanges();
            }
            else if(!favoriteProject.IsRated)
            {
                favoriteProject.IsRated = true;
                userOrder.RatingsCount += 1;
                userOrder.RatingsSum += rating.Rate;
                userOrder.Rate = Convert.ToSingle((userOrder.RatingsSum) / (userOrder.RatingsCount));
                _context.SaveChanges();
            }
            return Json(new JsonResult(rating));
        }//Kamil

        public void SortOrders(ref IQueryable<Order> projects, string sortExpression)
        {
            switch (sortExpression)
            {
                case "Liczba polubień":
                    projects = projects.OrderByDescending(p => p.Likes);
                    break;
                case "Alfabetycznie":
                    projects = projects.OrderBy(p => p.Name);
                    break;
                case "Najnowsze":
                    projects = projects.OrderByDescending(p => p.UploadDate);
                    break;
                case "Liczba wyświetleń":
                    projects = projects.OrderByDescending(p => p.RatingsCount);
                    break;
                default:
                    break;
            }
        }

        [Authorize]
        public async Task<IActionResult> FavouriteProjectsAsync(string filter = "", int page = 1,
            string sortExpression = "Najnowsze", string sortOrder = "Ascending", int elementsOnPage = 20,
            int onPage = 10, string message="")
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            var projects = from p in _context.UserFavouriteProject
                           join o in _context.Order on p.OrderId equals o.OrderId
                           where p.UserId.Equals(user.Id)
                           select o;

            if (!string.IsNullOrWhiteSpace(filter)) projects = projects.Where(p => p.Name.Contains(filter));
            ViewData["Title"] = "Ulubione";
            SortOrders(ref projects, sortExpression);
            var favourites = _context.UserFavouriteProject.Where(f => f.User.Equals(user) && f.IsFavourite);
            List<Order> tmp = new List<Order>();
            var numberOfElements = projects.Count();
            int numberOfPages = (int)(projects.Count() / onPage);
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

            if (numberOfElements.Equals(0)) message = "Brak elementów do wyśtlenia";

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

 

        [HttpPost]
        [Authorize]
        public IActionResult ProjectShare([FromBody]ProjectPrivacy privacy)
        {
            var result = _context.Order.Where(o => o.OrderId
            .Equals(Convert.ToInt32(privacy.Id))).FirstOrDefault();

            if (privacy.IsPrivate == null) privacy.IsPrivate = "false";

            result.Private = !privacy.IsPrivate.Equals("true");
            _context.SaveChanges();

            return Ok(result);
        }//Dominik

        [Authorize]
        public IActionResult RemoveProject(string OrderId)
        {
            var order = _context.Order.Where(o => o.OrderId.Equals(Convert.ToInt32(OrderId))).FirstOrDefault();

            if (order != default(Order))
            {
                var favourites = _context.UserFavouriteProject.Where(f => f.OrderId.Equals(OrderId));

                if (favourites.Count() > 0)
                {
                    foreach (var i in favourites)
                    {
                        _context.UserFavouriteProject.Remove(i);
                        _context.SaveChanges();
                    }
                }

                _context.Order.Remove(order);
                _context.SaveChanges();
            }

            return RedirectToAction("MyProjectsAsync", "Projects", new { message = "Usunięto projekt." });
        }

        [HttpPost]
        public async Task<IActionResult> ProjectView(IFormCollection param)
        {
            bool isRated = false;

            string tmp = param.Keys.ElementAt(0).Substring(0, param.Keys.ElementAt(0).Length - 2);

            ICollection<Order> order = _context.Order.Where(o => o
            .OrderId.Equals(Convert.ToInt32(tmp))).ToList();

            var owner = _context.Users.Where(u => u.Id.Equals(order
            .FirstOrDefault().UserId)).FirstOrDefault();

            bool isLikedByLoggedUser = false;

            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (user != null && user != default(ApplicationUser))
            {
                var favourite = _context.UserFavouriteProject.Where(f => f.UserId.Equals(user.Id)&&
                    f.OrderId.Equals(order.FirstOrDefault().OrderId)).FirstOrDefault();

                if (favourite != null && favourite != default(UserFavoriteProject))
                {
                    if (favourite.IsFavourite)
                    {
                        isLikedByLoggedUser = true;
                    }
                    isRated = favourite.IsRated;
                }
            }
            order.First().ViewsCount += 1;
            _context.SaveChanges();

            return View(new OrderViewModel()
            {
                IsLikedByLoggedUser = isLikedByLoggedUser,
                LoggedUser = user,
                Owner = owner,
                Order = order,
                IsRated = isRated,
                IsProjectOwner = user != null ? user.Id.Equals(owner.Id) : false,
                IsSignedIn = user != null,
                LikesCount = order.FirstOrDefault().Likes
            });
        }//Dawid
    }
}