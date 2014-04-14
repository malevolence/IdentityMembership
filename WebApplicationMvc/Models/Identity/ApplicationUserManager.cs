using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplicationMvc.Utils;

namespace WebApplicationMvc.Models
{
	public class ApplicationUserManager : UserManager<ApplicationUser, Guid>
	{
		public ApplicationUserManager(IUserStore<ApplicationUser, Guid> store) : base(store)
		{
		}

		public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
		{
			var manager = new ApplicationUserManager(new CustomUserStore(context.Get<ApplicationDbContext>()));
			manager.UserValidator = new UserValidator<ApplicationUser, Guid>(manager)
			{
				AllowOnlyAlphanumericUserNames = true,
				RequireUniqueEmail = true
			};

			manager.PasswordValidator = new PasswordValidator()
			{
				RequiredLength = 6
			};

			// Let's us sign-in users imported from old authentication method
			// Will automatically re-hash old passwords into the new crypto format on successful login
			manager.PasswordHasher = new SqlPasswordHasher() { DbContext = context.Get<ApplicationDbContext>() };

			manager.UserLockoutEnabledByDefault = true;
			manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
			manager.MaxFailedAccessAttemptsBeforeLockout = 5;

			// Generates tokens for email confirmation and resetting passwords
			var dataProtectionProvider = options.DataProtectionProvider;
			if (dataProtectionProvider != null)
			{
				manager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser, Guid>(dataProtectionProvider.Create("ASP.NET Identity"));
			}

			manager.EmailService = new EmailService();

			return manager;
		}
	}
}