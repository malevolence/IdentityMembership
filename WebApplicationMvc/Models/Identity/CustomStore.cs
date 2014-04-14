using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplicationMvc.Models
{
	public class CustomUserStore : UserStore<ApplicationUser, CustomRole, Guid, CustomUserLogin, CustomUserRole, CustomUserClaim>
	{
		public CustomUserStore(ApplicationDbContext context) : base(context)
		{
		}
	}

	public class CustomRoleStore : RoleStore<CustomRole, Guid, CustomUserRole>
	{
		public CustomRoleStore(ApplicationDbContext context) : base(context)
		{
		}
	}
}