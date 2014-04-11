using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using WebApplicationMvc.Utils;

namespace WebApplicationMvc.Controllers
{
    public class TestController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }


		[HttpPost]
		public ActionResult HashPassword(string password, string salt)
		{
			var hash = EncodePassword(password, salt);
			TempData["password"] = password;
			TempData["salt"] = salt;
			TempData["hash"] = hash;
			return RedirectToAction("Index");
		}

		private string EncodePassword(string pass, string salt)
		{
			byte[] bytes = Encoding.Unicode.GetBytes(pass);
			byte[] src = Convert.FromBase64String(salt);
			byte[] dst = new byte[src.Length + bytes.Length];
			Buffer.BlockCopy(src, 0, dst, 0, src.Length);
			Buffer.BlockCopy(bytes, 0, dst, src.Length, bytes.Length);
			HashAlgorithm algorithm = HashAlgorithm.Create("SHA1");
			byte[] inArray = algorithm.ComputeHash(dst);
			return Convert.ToBase64String(inArray);
		}
	}
}