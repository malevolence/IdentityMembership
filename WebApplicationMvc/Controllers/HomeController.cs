using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using WebApplicationMvc.Models;

namespace WebApplicationMvc.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
			var roleManager = ApplicationRoleManager.Create(null, HttpContext.GetOwinContext());
			ViewBag.Users = userManager.Users.OrderByDescending(x => x.CreateDate).Take(10).ToList();
			ViewBag.Roles = roleManager.Roles.OrderBy(x => x.Name).ToList();

			return View();
		}

		public ActionResult About()
		{
			ViewBag.Message = "Your application description page.";

			return View();
		}

		public ActionResult Contact()
		{
			ViewBag.Message = "Your contact page.";

			return View();
		}
	}
}