using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PhotographersVideoDist.Models
{
	public class ApplicationUser : IdentityUser
	{
		[Display(Name = "FTP Brugernavn")]
		public string FTP_UserName { get; set; }

		[Display(Name = "FTP Password")]
		public string FTP_EncryptedPassword { get; set; }

		[Display(Name = "FTP Adresse")]
		public string FTP_Url { get; set; }

		[Display(Name = "FTP Mappe (Hvis filer skal uploades til undermappe)")]
		public string FTP_RemoteDir { get; set; }
	}
}
