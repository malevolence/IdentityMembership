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
using WebApplicationMvc.Utils;

namespace WebApplicationMvc.Utils
{
    public class CustomBaseController : Controller
    {
		public ApplicationUser CurrentUser
		{
			get
			{
				ApplicationUser user = null;

				if (UserManager != null)
				{
					Guid id = Guid.Parse(User.Identity.GetUserId());
					user = UserManager.FindById(id);
				}

				return user;
			}
		}

		private ApplicationUserManager _userManager;
		public ApplicationUserManager UserManager
		{
			get { return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
			private set { _userManager = value; }
		}
	}
}