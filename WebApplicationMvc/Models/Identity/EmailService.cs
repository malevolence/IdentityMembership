using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace WebApplicationMvc.Models
{
	public class EmailService : IIdentityMessageService
	{
		public Task SendAsync(IdentityMessage message)
		{
			var email = new MailMessage("autonotify@productionhub.com", message.Destination);
			email.Subject = message.Subject;
			email.Body = message.Body;
			email.IsBodyHtml = true;
			var smtp = new SmtpClient();
			return smtp.SendMailAsync(email);
		}
	}
}