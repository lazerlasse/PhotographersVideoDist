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
		[Display(Name = "Upload Status")]
		public string UploadStatus { get; set; }

		[Display(Name = "Assets fil")]
		public IFormFile AssetsFile { get; set; }
	}
}
