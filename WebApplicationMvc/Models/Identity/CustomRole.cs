using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplicationMvc.Models
{
	// Required for Identity with Primary Key other than string
	public class CustomRole : IdentityRole<Guid, CustomUserRole>
	{
		public CustomRole() { }
		public CustomRole(string name) { this.Name = name; }
	}

	public class CustomUserRole : IdentityUserRole<Guid> { }
	public class CustomUserClaim : IdentityUserClaim<Guid> { }
	public class CustomUserLogin : IdentityUserLogin<Guid> { }
}