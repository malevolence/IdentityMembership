using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebApplicationMvc.Models;

namespace WebApplicationMvc.Helpers
{
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