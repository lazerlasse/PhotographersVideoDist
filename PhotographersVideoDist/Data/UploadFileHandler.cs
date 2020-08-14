using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Web.CodeGeneration;
using PhotographersVideoDist.Controllers;
using PhotographersVideoDist.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PhotographersVideoDist.Data
{
	public class UploadFileHandler
	{
		private static string caseAssetsFolder;
		private static ILogger<CasesController> _logger;
		private static ApplicationDbContext _context;
		private static int _caseID;

		public UploadFileHandler(int caseID, ILogger<CasesController> logger, ApplicationDbContext context)
		{
			// Set logger, context and caseID.
			_logger = logger;
			_context = context;
			_caseID = caseID;

			// Generate path for assets destination folder for files to upload.
			caseAssetsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Cases", _caseID.ToString());

			// Create the folder.
			Directory.CreateDirectory(caseAssetsFolder);
		}

		public async Task<FileUpload> UploadFiles(FileUpload fileUpload)
		{
			// Loop through files and upload them.
			foreach (var file in fileUpload.AssetsFiles)
			{
				// Try upload file and save to disk.
				var uploaded = await Upload(file);

				// Check file is uploaded.
				if (uploaded)
				{
					// Check file exist.
					if (File.Exists(Path.Combine(caseAssetsFolder, file.FileName)))
					{
						// Save uploaded file to database.
						var saved = SaveUploadToDb(file);

						// If save to db failed, delete file from disk.
						if (!saved.Result)
						{
							if (!AssetsFileHandler.DeleteAssetsFile(file.FileName, _caseID, _logger))
							{
								_logger.LogError("Filen blev ikke slette fra disken: " + file.FileName);
								fileUpload.AssetsFiles.Remove(file);
							};
						}
					}
				}
			}

			// Return the FileUpload list of completed upload.
			return fileUpload;
		}

		private static async Task<bool> Upload(IFormFile file)
		{
			// Generate full file path for the file.
			var fullFilePath = Path.Combine(caseAssetsFolder, file.FileName);

			// Save file to disk.
			try
			{
				using var fileStream = new FileStream(fullFilePath, FileMode.Create);
				await file.CopyToAsync(fileStream);
			}
			catch (Exception ex)
			{
				// Send error to logger..
				_logger.LogError("Filen kunne ikke uploades: " + ex.Message);

				// Upload failed, return false.
				return false;
			}

			// Return true succeded.
			return true;
		}

		private async Task<bool> SaveUploadToDb(IFormFile file)
		{
			// Generate needed assets path strings.
			var fullFilePath = Path.Combine(caseAssetsFolder, file.FileName);
			var fileExtension = Path.GetExtension(fullFilePath).ToLower();

			// Create new Image object if file are Image file and save context...
			if (fileExtension == ".jpg" || fileExtension == ".jpeg")
			{
				// Create object to save...
				ImageAssets image = new ImageAssets()
				{
					ImageFileName = Path.GetFileName(file.FileName),
					CaseID = _caseID
				};

				// Try save assets to db.
				try
				{
					_context.ImageAssets.Add(image);
					await _context.SaveChangesAsync();
				}
				catch (Exception ex)
				{
					_logger.LogError("Kunne ikke gemme billedet i databasen! " + ex.Message);
					return false;
				}
			}

			// Create new VideoAssets object if the file are Video file and save context...
			if (fileExtension == ".mp4" || fileExtension == ".mpeg"
				|| fileExtension == ".mpg" || fileExtension == ".avi" || fileExtension == ".mov")
			{
				// Create object to save...
				VideoAssets video = new VideoAssets()
				{
					VideoAssetsFileName = Path.GetFileName(file.FileName),
					CaseID = _caseID
				};

				// Check if already exist.
				if (!await _context.VideoAssets.Where(v => v.VideoAssetsFileName == video.VideoAssetsFileName).AnyAsync())
				{

					// Save content to the database.
					try
					{
						_context.VideoAssets.Add(video);
						await _context.SaveChangesAsync();
					}
					catch (Exception ex)
					{
						_logger.LogError("Videoen blev ikke gemt i databasen: " + ex.Message);
						return false;
					}
				}

			}

			// Save assets to db succeded, return true.
			return true;
		}
	}
}
