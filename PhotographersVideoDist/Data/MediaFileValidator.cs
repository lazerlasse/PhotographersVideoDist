using Microsoft.AspNetCore.Http;
using PhotographersVideoDist.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PhotographersVideoDist.Data
{
	public static class MediaFileValidator
	{
		private const int ImageMinimumBytes = 512;

		public static FileUpload Validate(FileUpload fileUpload)
		{
			// Validate files and remove not accepted from upload.
			foreach (var file in fileUpload.AssetsFiles)
			{
				var validated = ValidateMediaFile(file);

				if (!validated)
				{
					fileUpload.AssetsFiles.Remove(file);
				}
			}

			// Return validated files.
			return fileUpload;
		}

		private static bool ValidateMediaFile(IFormFile file)
		{
			//-------------------------------------------
			//  Check the accepted mime types
			//-------------------------------------------
			if (file.ContentType.ToLower() != "image/jpg" &&
						file.ContentType.ToLower() != "image/jpeg" &&
						file.ContentType.ToLower() != "video/mp4" &&
						file.ContentType.ToLower() != "video/avi" &&
						file.ContentType.ToLower() != "video/mpeg" &&
						file.ContentType.ToLower() != "video/mpg" &&
						file.ContentType.ToLower() != "video/quicktime")
			{
				return false;
			}

			//-------------------------------------------
			//  Check the accepted file extension
			//-------------------------------------------
			if (Path.GetExtension(file.FileName).ToLower() != ".jpg"
				&& Path.GetExtension(file.FileName).ToLower() != ".jpeg"
				&& Path.GetExtension(file.FileName).ToLower() != ".mp4"
				&& Path.GetExtension(file.FileName).ToLower() != ".mpeg"
				&& Path.GetExtension(file.FileName).ToLower() != ".mpg"
				&& Path.GetExtension(file.FileName).ToLower() != ".avi"
				&& Path.GetExtension(file.FileName).ToLower() != ".mov")
			{
				return false;
			}

			//-------------------------------------------
			//  Attempt to read the file and check the first bytes
			//-------------------------------------------
			try
			{
				// Try reading the file.
				if (!file.OpenReadStream().CanRead)
				{
					return false;
				}

				//------------------------------------------
				//check whether the file size exceeding the limit or not
				//------------------------------------------ 
				if (file.Length < ImageMinimumBytes)
				{
					return false;
				}

				byte[] buffer = new byte[ImageMinimumBytes];
				file.OpenReadStream().Read(buffer, 0, ImageMinimumBytes);
				string content = System.Text.Encoding.UTF8.GetString(buffer);
				if (Regex.IsMatch(content, @"<script|<html|<head|<title|<body|<pre|<table|<a\s+href|<img|<plaintext|<cross\-domain\-policy",
					RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline))
				{
					return false;
				}
			}
			catch (Exception)
			{
				return false;
			}

			// If the test has passed return true.
			return true;
		}
	}
}
