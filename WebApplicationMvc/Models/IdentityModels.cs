using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApplicationMvc.Utils;

namespace WebApplicationMvc.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser<Guid, CustomUserLogin, CustomUserRole, CustomUserClaim>
    {
		public ApplicationUser()
		{
			Id = Guid.NewGuid();
			CreateDate = DateTime.Now;
			IsApproved = true;
		}

		[Display(Name = "Added On")]
		public DateTime CreateDate { get; set; }

		[Display(Name = "Is Approved?")]
		public bool IsApproved { get; set; }

		[Display(Name = "Is Bad?")]
		public bool IsBad { get; set; }

		public string Comment { get; set; }

		[Required, StringLength(100)]
		public string DisplayName { get; set; }

		public int? CountryId { get; set; }
		public int? StateId { get; set; }

		[Display(Name = "In Industry?")]
		public bool InIndustry { get; set; }

		public int Credits { get; set; }

		[Display(Name = "Default Profile Id")]
		public int? DefaultProfileId { get; set; }

		public Task<ClaimsIdentity> GenerateUserIdentityAsync(ApplicationUserManager manager)
		{
			return Task.FromResult(GenerateUserIdentity(manager));
		}

		public ClaimsIdentity GenerateUserIdentity(ApplicationUserManager manager)
		{
			var userIdentity = manager.CreateIdentity<ApplicationUser, Guid>(this, DefaultAuthenticationTypes.ApplicationCookie);

			// Add custom user claims here

			return userIdentity;
		}
    }

	public class CustomRole : IdentityRole<Guid, CustomUserRole>
	{
		public CustomRole() { }
		public CustomRole(string name) { this.Name = name; }
	}

	public class CustomUserRole : IdentityUserRole<Guid> { }
	public class CustomUserClaim : IdentityUserClaim<Guid> { }
	public class CustomUserLogin : IdentityUserLogin<Guid> { }


    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, CustomRole, Guid, CustomUserLogin, CustomUserRole, CustomUserClaim>
    {
        public ApplicationDbContext() : base("DefaultConnection")
        {
        }

		public static ApplicationDbContext Create()
		{
			return new ApplicationDbContext();
		}
    }

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

	public class ApplicationRoleManager : RoleManager<CustomRole, Guid>
	{
		public ApplicationRoleManager(IRoleStore<CustomRole, Guid> store) : base(store)
		{
		}

		public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> options, IOwinContext context)
		{
			var manager = new ApplicationRoleManager(new CustomRoleStore(context.Get<ApplicationDbContext>()));
			return manager;
		}
	}

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

			// Let's us sign-in users imported from old app
			manager.PasswordHasher = new SqlPasswordHasher();

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

	public class EmailService : IIdentityMessageService
	{
		public Task SendAsync(IdentityMessage message)
		{
			var email = new MailMessage("autonotify@productionhub.com", message.Destination);
			email.Subject = message.Subject;
			email.Body = message.Body;
			email.IsBodyHtml = false;
			var smtp = new SmtpClient();
			return smtp.SendMailAsync(email);
		}
	}

	public enum SignInStatus
	{
		Success,
		LockedOut,
		Failure,
		Disabled,
		RequiresVerification
	}

	public class SignInHelper
	{
		public SignInHelper(ApplicationUserManager userManager, IAuthenticationManager authManager)
		{
			UserManager = userManager;
			AuthenticationManager = authManager;
		}
		public ApplicationUserManager UserManager { get; private set; }
		public IAuthenticationManager AuthenticationManager { get; private set; }

		public async Task SignInAsync(ApplicationUser user, bool isPersistant)
		{
			var userIdentity = await user.GenerateUserIdentityAsync(UserManager);
			AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistant }, userIdentity);
		}

		public async Task<SignInStatus> PasswordSignIn(string email, string password, bool isPersistant)
		{
			var user = await UserManager.FindByEmailAsync(email);
			if (user == null)
			{
				return SignInStatus.Failure;
			}

			if (await UserManager.IsLockedOutAsync(user.Id))
			{
				return SignInStatus.LockedOut;
			}

			if (!user.IsApproved)
			{
				return SignInStatus.Disabled;
			}

			if (!user.EmailConfirmed)
			{
				return SignInStatus.RequiresVerification;
			}

			if (await UserManager.CheckPasswordAsync(user, password))
			{
				await SignInAsync(user, isPersistant);
				return SignInStatus.Success;
			}

			// Got this far, signin failed. Increment the failedcount
			await UserManager.AccessFailedAsync(user.Id);
			if (await UserManager.IsLockedOutAsync(user.Id))
			{
				return SignInStatus.LockedOut;
			}

			return SignInStatus.Failure;
		}
	}
}