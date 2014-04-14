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
    [Authorize]
    public class AccountController : Controller
    {
		public AccountController()
		{
		}

		public AccountController(ApplicationUserManager userManager)
        {
			_userManager = userManager;
        }

		private IAuthenticationManager AuthenticationManager
		{
			get { return HttpContext.GetOwinContext().Authentication; }
		}

		private ApplicationUserManager _userManager;

		public ApplicationUserManager UserManager
		{
			get { return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
			private set { _userManager = value; }
		}

		private SignInHelper _signInHelper;

		private SignInHelper SignInHelper
		{
			get
			{
				if (_signInHelper == null)
				{
					_signInHelper = new SignInHelper(UserManager, AuthenticationManager);
				}

				return _signInHelper;
			}
		}

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            
			if (User.Identity.IsAuthenticated)
			{
				if (!string.IsNullOrEmpty(returnUrl))
				{
					return View("NoAccess");
				}
				else
				{
					TempData["info"] = "You are already authenticated. No need to login again.";
					return RedirectToAction("Index", "Home");
				}
			}
			return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
				var result = await SignInHelper.PasswordSignIn(model.Email, model.Password, model.RememberMe);
				switch (result)
				{
					case SignInStatus.Success:
						return RedirectToLocal(returnUrl);
					case SignInStatus.LockedOut:
						return RedirectToAction("Lockout");
					case SignInStatus.Disabled:
						return RedirectToAction("Disabled");
					case SignInStatus.RequiresVerification:
						ViewBag.Email = model.Email;
						return View("VerificationNeeded");
					default:
						ModelState.AddModelError("", "Invalid Login Attempt");
						break;
				}
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

		[AllowAnonymous]
		public ActionResult ForgotPassword()
		{
			return View();
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = await UserManager.FindByEmailAsync(model.Email);
				if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
				{
					// Don't reveal that the user does not exist or is not confirmed
					return View("ForgotPassordConfirmation");
				}

				var code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
				var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, Request.Url.Scheme);
				await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking here: <a href=\"" + callbackUrl + "\">Reset Password</a>");
				return View("ForgotPasswordConfirmation");
			}

			return View(model);
		}

		[AllowAnonymous]
		public ActionResult ForgotPasswordConfirmation()
		{
			return View();
		}

		[AllowAnonymous]
		public ActionResult ResetPassword(string code)
		{
			return code == null ? View("Error") : View();
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var user = await UserManager.FindByEmailAsync(model.Email);
			if (user == null)
			{
				// don't reveal that the user doesn't exist
				return RedirectToAction("ResetPasswordConfirmation", "Accout");
			}
			var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
			if (result.Succeeded)
			{
				return RedirectToAction("ResetPasswordConfirmation", "Accout");
			}
			else
			{
				AddErrors(result);
			}

			return View();
		}

		[AllowAnonymous]
		public ActionResult ResetPasswordConfirmation()
		{
			return View();
		}

		[AllowAnonymous]
		public ActionResult Lockout()
		{
			return View();
		}

		[AllowAnonymous]
		public ActionResult Disabled()
		{
			return View();
		}

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser() { DisplayName = model.DisplayName.Trim(), Email = model.Email.Trim().ToLower(), UserName = StringUtils.RandomString(16).ToLower() };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
					var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
					var callBackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, Request.Url.Scheme);
					await UserManager.SendEmailAsync(user.Id, "Confirm Your Account", "Please confirm your account by clicking this link: <a href=\"" + callBackUrl + "\">Confirm Email</a>");
					return View("CheckMail");
					//await SignInHelper.SignInAsync(user, false);
					//return RedirectToAction("Index", "Home");
                }
                else
                {
                    AddErrors(result);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

		[AllowAnonymous]
		public async Task<ActionResult> ConfirmEmail(Guid userId, string code)
		{
			if (userId == null || code == null)
			{
				return View("Error");
			}
			var result = await UserManager.ConfirmEmailAsync(userId, code);
			if (result.Succeeded)
			{
				UserManager.AddToRole(userId, "Users");

				var user = UserManager.FindById(userId);
				user.EmailConfirmed = true;
				UserManager.Update(user);

				TempData["success"] = "Your email was confirmed successfully";
			}
			else
			{
				AddErrors(result);
			}

			return View();
		}


		[AllowAnonymous]
		public ActionResult GenerateEmailCode()
		{
			return View();
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> GenerateEmailCode(string email)
		{

			if (!string.IsNullOrEmpty(email))
			{
				var user = await UserManager.FindByEmailAsync(email);
				if (user == null)
				{
					TempData["error"] = "No account found associated with this email address.";
				}
				else
				{
					var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
					var callBackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, Request.Url.Scheme);
					await UserManager.SendEmailAsync(user.Id, "Confirm Your Account", "Please confirm your account by clicking this link: <a href=\"" + callBackUrl + "\">Confirm Email</a>");
					return View("CheckMail");
				}
			}
			else
			{
				TempData["error"] = "You must enter your email to generate a verification code.";
			}

			return View();
		}

		//
		// POST: /Account/LogOff
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult LogOff()
		{
			AuthenticationManager.SignOut();
			return RedirectToAction("Index", "Home");
		}

		//
		// GET: /Account/Manage
		public ActionResult Manage()
		{
			var user = UserManager.FindById(Guid.Parse(User.Identity.GetUserId()));
			var model = new ManageUserViewModel { Email = user.Email, DisplayName = user.DisplayName, NotifyDirectMessages = user.NotifyDirectMessages, NotifyAskAndAnswer = user.NotifyAskAndAnswer };
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Manage(ManageUserViewModel model)
		{
			if (ModelState.IsValid)
			{
				// Before we do this, make sure to validate the email address format.
				var user = UserManager.FindByName(User.Identity.Name);
				if (user != null)
				{
					user.DisplayName = model.DisplayName.Trim();
					user.Email = model.Email.ToLower();
					user.NotifyDirectMessages = model.NotifyDirectMessages;
					user.NotifyAskAndAnswer = model.NotifyAskAndAnswer;

					var result = await UserManager.UpdateAsync(user);
					if (result == IdentityResult.Success)
					{
						TempData["success"] = "Changes saved successfully.";
						return RedirectToAction("Manage");
					}
					else
					{
						AddErrors(result);
					}
				}
				else
				{
					TempData["error"] = "User not found.";
				}
			}

			return View(model);
		}

		public ActionResult ChangePassword()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
		{
			if (ModelState.IsValid)
			{
				var result = await UserManager.ChangePasswordAsync(Guid.Parse(User.Identity.GetUserId()), model.OldPassword, model.NewPassword);
				if (result.Succeeded)
				{
					TempData["success"] = "Password changed successfully";
				}
				else
				{
					AddErrors(result);
				}
			}
			return RedirectToAction("Manage");
		}

		public ActionResult MyRoles()
		{
			var roleManager = ApplicationRoleManager.Create(null, HttpContext.GetOwinContext());
			ViewBag.PossibleRoles = roleManager.Roles.Select(x => new SelectListItem
			{
				Text = x.Name,
				Value = x.Name
			});

			var roles = UserManager.GetRoles(Guid.Parse(User.Identity.GetUserId()));
			return View(roles);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> AddUserToRole(string role)
		{
			// add user to the "users" role
			if (!string.IsNullOrEmpty(role))
			{
				var result = await UserManager.AddToRoleAsync(Guid.Parse(User.Identity.GetUserId()), role);

				if (result.Succeeded)
				{
					TempData["success"] = "User added to role: " + role;
				}
				else
				{
					AddErrors(result);
				}
			}
			else
			{
				TempData["error"] = "No role selected.";
			}

			return RedirectToAction("MyRoles");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> RemoveUserFromRole(string role)
		{
			// add user to the "users" role
			if (!string.IsNullOrEmpty(role))
			{
				var result = await UserManager.RemoveFromRoleAsync(Guid.Parse(User.Identity.GetUserId()), role);
				if (result.Succeeded)
				{
					TempData["success"] = "User removed from role: " + role;
				}
				else
				{
					AddErrors(result);
				}
			}
			else
			{
				TempData["error"] = "No role selected.";
			}

			return RedirectToAction("MyRoles");
		}

		private ActionResult RedirectToLocal(string returnUrl)
		{
			if (Url.IsLocalUrl(returnUrl))
			{
				return Redirect(returnUrl);
			}
			return RedirectToAction("Index", "Home");
		}

		private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

		protected override void Dispose(bool disposing)
		{
			if (disposing && UserManager != null)
			{
				UserManager.Dispose();
				UserManager = null;
			}
			base.Dispose(disposing);
		}
	}
}