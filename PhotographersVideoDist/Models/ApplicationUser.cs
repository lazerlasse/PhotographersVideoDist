using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotographersVideoDist.Models
{
	public class ApplicationUser : IdentityUser
	{
		public string FTP_UserName { get; set; }

		public string FTP_EncryptedPassword { get; set; }
	}
}
