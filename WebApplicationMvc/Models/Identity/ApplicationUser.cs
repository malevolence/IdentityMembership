using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace WebApplicationMvc.Models
{
	public class ApplicationUser : IdentityUser<Guid, CustomUserLogin, CustomUserRole, CustomUserClaim>
	{
		public ApplicationUser()
		{
			Id = Guid.NewGuid();
			CreateDate = DateTime.Now;
			IsApproved = true;
			NotifyAskAndAnswer = true;
			NotifyDirectMessages = true;
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

		[Display(Name = "Direct Messages")]
		public bool NotifyDirectMessages { get; set; }

		[Display(Name = "Ask and Answer Responses")]
		public bool NotifyAskAndAnswer { get; set; }

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
}