using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplicationMvc.Models
{
	public class ApplicationRoleManager : RoleManager<CustomRole, Guid>
	{
		public ApplicationRoleManager(IRoleStore<CustomRole, Guid> store) : base(store)
		{
		}

		// Factory method
		public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> options, IOwinContext context)
		{
			var manager = new ApplicationRoleManager(new CustomRoleStore(context.Get<ApplicationDbContext>()));
			return manager;
		}
	}
}