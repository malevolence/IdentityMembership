using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplicationMvc.Models
{
	public class SearchUsers
	{
		public SearchUsers()
		{
			this.Results = new List<ApplicationUser>();
			this.TotalCount = 0;
		}

		[Display(Name = "Id")]
		public Guid? UserId { get; set; }
		public string Username { get; set; }

		[Display(Name = "Email Address")]
		public string Email { get; set; }

		[Display(Name = "Display Name")]
		public string DisplayName { get; set; }

		public int TotalCount { get; set; }
		public List<ApplicationUser> Results { get; set; }
	}
}