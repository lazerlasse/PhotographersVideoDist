using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PhotographersVideoDist.Models
{
	public class FileUpload
	{
		[Display(Name = "Vælg filer")]
		public ICollection<IFormFile> AssetsFiles { get; set; }
	}
}
