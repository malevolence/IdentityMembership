using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplicationMvc.Models
{
	public enum SignInStatus
	{
		Success,
		LockedOut,
		Failure,
		Disabled,
		RequiresVerification
	}
}