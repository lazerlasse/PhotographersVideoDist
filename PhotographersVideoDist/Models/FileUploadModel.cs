using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PhotographersVideoDist.Models
{
	public class FileUploadModel
	{
		[Display(Name = "Vælg fil")]
		public IFormFile VideoFile { get; set; }
	}
}
