using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebApplicationMvc.Models;
using WebApplicationMvc.Utils;

namespace WebApplicationMvc.Controllers
{
	[Authorize(Roles = "Employees")]
    public class UsersController : Controller
    {
		private ApplicationUserManager _userManager;

		public ApplicationUserManager UserManager
		{
			get { return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
			private set { _userManager = value; }
		}

		public ActionResult Index()
        {
            return View(new SearchUsers());
        }

		[HttpPost]
		public ActionResult Index(SearchUsers search)
		{
			// perform the search
			var users = BuildSearchQuery(search);

			search.TotalCount = users.Count();
			search.Results = users.OrderByDescending(x => x.CreateDate).Take(25).ToList();

			return View(search);
		}

		private IQueryable<ApplicationUser> BuildSearchQuery(SearchUsers search)
		{
			var query = UserManager.Users;

			if (search.UserId.HasValue)
				query = query.Where(x => x.Id == search.UserId.Value);

			if (!string.IsNullOrEmpty(search.Username))
				query = query.Where(x => x.UserName == search.Username);

			if (!string.IsNullOrEmpty(search.Email))
				query = query.Where(x => x.Email.Contains(search.Email));

			if (!string.IsNullOrEmpty(search.DisplayName))
				query = query.Where(x => x.DisplayName.Contains(search.DisplayName));

			return query;
		}
	}
}