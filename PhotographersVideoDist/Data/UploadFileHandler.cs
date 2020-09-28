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

		public async Task<IList<FileUpload>> UploadFiles(IList<FileUpload> fileUpload)
		{
			// Loop through files and upload them.
			foreach (var file in fileUpload)
			{
				// Try upload file and save to disk.
				var uploaded = await Upload(file);

				// Check file is uploaded.
				if (uploaded)
				{
					// Check file exist.
					if (File.Exists(Path.Combine(caseAssetsFolder, file.AssetsFile.FileName)))
					{
						// Save uploaded file to database.
						var saved = SaveUploadToDb(file.AssetsFile);

						// If save to db failed, delete file from disk.
						if (!saved.Result)
						{
							if (!AssetsFileHandler.DeleteAssetsFile(file.AssetsFile.FileName, _caseID, _logger))
							{
								_logger.LogError("Filen blev ikke slette fra disken: " + file.AssetsFile.FileName);
								file.UploadStatus = "Fejlet";
							};
						}
						else
						{
							file.UploadStatus = "Uploaded";
						}
					}
				}
			}

			// Return the FileUpload list of completed upload.
			return fileUpload;
		}

		private static async Task<bool> Upload(FileUpload file)
		{
			// Generate full file path for the file.
			var fullFilePath = Path.Combine(caseAssetsFolder, file.AssetsFile.FileName);

			if (!File.Exists(fullFilePath))
			{
				// Save file to disk.
				try
				{
					using var fileStream = new FileStream(fullFilePath, FileMode.Create);
					await file.AssetsFile.CopyToAsync(fileStream);
					_logger.LogInformation("Filen blev gemt med succes. " + Path.GetFileName(fullFilePath));
				}
				catch (Exception ex)
				{
					// Send error to logger..
					_logger.LogError("Filen kunne ikke uploades: " + ex.Message);

					// Set FileUpload status message.
					file.UploadStatus = "Fejlet";

					// Upload failed, return false.
					return false;
				}

				return true;
			}
			else
			{
				// Log information.
				_logger.LogInformation("Filen blev ikke uploaded, da den allerede er uploadet! " + file.AssetsFile.FileName);

				// Set FileUpload status message.
				file.UploadStatus = "Allerede Uploaded";

				// Return true succeded.
				return false;
			}
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
					_logger.LogInformation("ImageAssets ID: " + image.ImageAssetsID + " Filnavn: " + image.ImageFileName + " blev gemt i databasen med succes.");
				}
				catch (Exception ex)
				{
					_logger.LogError("Kunne ikke gemme billedet i databasen! Filnavn: " + image.ImageFileName + " Meddelelse: " + ex.Message);
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
				if (!await _context.VideoAssets.Where(v => v.VideoAssetsFileName == video.VideoAssetsFileName).Where(v => v.CaseID == video.CaseID).AnyAsync())
				{
					// Save content to the database.
					try
					{
						_context.VideoAssets.Add(video);
						await _context.SaveChangesAsync();
						_logger.LogInformation("VideoAssets ID: " + video.VideoAssetsID + " Filnavn: " + video.VideoAssetsFileName + " blev gemt i databasen med succes.");
					}
					catch (Exception ex)
					{
						_logger.LogError("Videoen blev ikke gemt i databasen: Filnavn: " + video.VideoAssetsFileName + " Meddelese: " + ex.Message);
						return false;
					}
				}
			}

			// Save assets to db succeded, return true.
			return true;
		}
	}
}
