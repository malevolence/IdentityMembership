﻿using System;
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
						return RedirectToAction("Index", "Home");
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
					await UserManager.SendEmailAsync(user.Id, "Confirm Your Account", "Please confirm your account by clicking this link: " + callBackUrl);
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
				var user = UserManager.FindById(userId);
				user.EmailConfirmed = true;
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
					await UserManager.SendEmailAsync(user.Id, "Confirm Your Account", "Please confirm your account by clicking this link: " + callBackUrl);
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

		////
		//// POST: /Account/Disassociate
		//[HttpPost]
		//[ValidateAntiForgeryToken]
		//public async Task<ActionResult> Disassociate(string loginProvider, string providerKey)
		//{
		//	ManageMessageId? message = null;
		//	IdentityResult result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
		//	if (result.Succeeded)
		//	{
		//		message = ManageMessageId.RemoveLoginSuccess;
		//	}
		//	else
		//	{
		//		message = ManageMessageId.Error;
		//	}
		//	return RedirectToAction("Manage", new { Message = message });
		//}

		//
		// GET: /Account/Manage
		public ActionResult Manage()
		{
			var user = UserManager.FindById(Guid.Parse(User.Identity.GetUserId()));
			var model = new ManageUserViewModel { Email = user.Email, DisplayName = user.DisplayName };
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

		////
		//// POST: /Account/Manage
		//[HttpPost]
		//[ValidateAntiForgeryToken]
		//public async Task<ActionResult> Manage(ManageUserViewModel model)
		//{
		//	bool hasPassword = HasPassword();
		//	ViewBag.HasLocalPassword = hasPassword;
		//	ViewBag.ReturnUrl = Url.Action("Manage");
		//	if (hasPassword)
		//	{
		//		if (ModelState.IsValid)
		//		{
		//			IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
		//			if (result.Succeeded)
		//			{
		//				return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
		//			}
		//			else
		//			{
		//				AddErrors(result);
		//			}
		//		}
		//	}
		//	else
		//	{
		//		// User does not have a password so remove any validation errors caused by a missing OldPassword field
		//		ModelState state = ModelState["OldPassword"];
		//		if (state != null)
		//		{
		//			state.Errors.Clear();
		//		}

		//		if (ModelState.IsValid)
		//		{
		//			IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
		//			if (result.Succeeded)
		//			{
		//				return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
		//			}
		//			else
		//			{
		//				AddErrors(result);
		//			}
		//		}
		//	}

		//	// If we got this far, something failed, redisplay form
		//	return View(model);
		//}

		////
		//// POST: /Account/ExternalLogin
		//[HttpPost]
		//[AllowAnonymous]
		//[ValidateAntiForgeryToken]
		//public ActionResult ExternalLogin(string provider, string returnUrl)
		//{
		//	// Request a redirect to the external login provider
		//	return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
		//}

		////
		//// GET: /Account/ExternalLoginCallback
		//[AllowAnonymous]
		//public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
		//{
		//	var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
		//	if (loginInfo == null)
		//	{
		//		return RedirectToAction("Login");
		//	}

		//	// Sign in the user with this external login provider if the user already has a login
		//	var user = await UserManager.FindAsync(loginInfo.Login);
		//	if (user != null)
		//	{
		//		await SignInAsync(user, isPersistent: false);
		//		return RedirectToLocal(returnUrl);
		//	}
		//	else
		//	{
		//		// If the user does not have an account, then prompt the user to create an account
		//		ViewBag.ReturnUrl = returnUrl;
		//		ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
		//		return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { UserName = loginInfo.DefaultUserName });
		//	}
		//}

		////
		//// POST: /Account/LinkLogin
		//[HttpPost]
		//[ValidateAntiForgeryToken]
		//public ActionResult LinkLogin(string provider)
		//{
		//	// Request a redirect to the external login provider to link a login for the current user
		//	return new ChallengeResult(provider, Url.Action("LinkLoginCallback", "Account"), User.Identity.GetUserId());
		//}

		////
		//// GET: /Account/LinkLoginCallback
		//public async Task<ActionResult> LinkLoginCallback()
		//{
		//	var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
		//	if (loginInfo == null)
		//	{
		//		return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
		//	}
		//	var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
		//	if (result.Succeeded)
		//	{
		//		return RedirectToAction("Manage");
		//	}
		//	return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
		//}

		////
		//// POST: /Account/ExternalLoginConfirmation
		//[HttpPost]
		//[AllowAnonymous]
		//[ValidateAntiForgeryToken]
		//public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
		//{
		//	if (User.Identity.IsAuthenticated)
		//	{
		//		return RedirectToAction("Manage");
		//	}

		//	if (ModelState.IsValid)
		//	{
		//		// Get the information about the user from the external login provider
		//		var info = await AuthenticationManager.GetExternalLoginInfoAsync();
		//		if (info == null)
		//		{
		//			return View("ExternalLoginFailure");
		//		}
		//		var user = new ApplicationUser() { UserName = model.UserName };
		//		var result = await UserManager.CreateAsync(user);
		//		if (result.Succeeded)
		//		{
		//			result = await UserManager.AddLoginAsync(user.Id, info.Login);
		//			if (result.Succeeded)
		//			{
		//				await SignInAsync(user, isPersistent: false);
		//				return RedirectToLocal(returnUrl);
		//			}
		//		}
		//		AddErrors(result);
		//	}

		//	ViewBag.ReturnUrl = returnUrl;
		//	return View(model);
		//}

		////
		//// GET: /Account/ExternalLoginFailure
		//[AllowAnonymous]
		//public ActionResult ExternalLoginFailure()
		//{
		//	return View();
		//}

		//[ChildActionOnly]
		//public ActionResult RemoveAccountList()
		//{
		//	var linkedAccounts = UserManager.GetLogins(User.Identity.GetUserId());
		//	ViewBag.ShowRemoveButton = HasPassword() || linkedAccounts.Count > 1;
		//	return (ActionResult)PartialView("_RemoveAccountPartial", linkedAccounts);
		//}

        protected override void Dispose(bool disposing)
        {
            if (disposing && UserManager != null)
            {
                UserManager.Dispose();
                UserManager = null;
            }
            base.Dispose(disposing);
        }

		private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
    }
}