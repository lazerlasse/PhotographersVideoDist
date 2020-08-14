using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using PhotographersVideoDist.Controllers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PhotographersVideoDist.Data
{
	public static class AssetsFileHandler
	{
		public static bool DeleteAssetsFile(string fileName, int? caseId, ILogger<CasesController> logger)
		{
			// Check path is not null.
			if (fileName == null || caseId == null)
			{
				// Log the error and return false.
				logger.LogError("Kunne ikke slette assets fra disken, da stien til filen var tom");
				return false;
			}

			// Get full path for the file to delete.
			var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Cases", caseId.ToString(), fileName);

			// Try deleting the file...
			try
			{
				if (File.Exists(path))
				{
					File.Delete(path);
				}
			}
			catch (Exception ex)
			{
				logger.LogError("Filen blev ikke slettet fra disken: " + ex.Message);
				return false;
			}

			// Succeded return true.
			return true;
		}

		public static bool DeleteAssetsFolder(int? id, ILogger<CasesController> logger)
		{
			// Check id not null.
			if (id == null)
			{
				logger.LogError("Assets mappen blev ikke slettet på disken, på grund af manglende sags nr.!");
				return false;
			}

			// Create a path string for the folder to delete...
			string deletePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Cases", id.ToString());

			// Try delete the folder from disk.
			try
			{
				// Check directory exist...
				if (Directory.Exists(deletePath))
				{
					// Delete the folder...
					Directory.Delete(deletePath);
				}
			}
			catch (Exception ex)
			{
				logger.LogError("Der opstod et problem under forsøget på at slette sags mappen fra disken: " + ex.Message);
				return false;
			}

			return true;
		}

		// Search for files match searach args and extension and return list with file names.
		public static List<string> SearchForAssetsfilesInAssetsFolder(string assetsPath, string searchArgs, string extension)
		{
			IEnumerable<string> files = Directory.EnumerateFiles(assetsPath)
				.Where(f => f.Contains(searchArgs))
				.Where(f => f.Contains(extension));

			List<string> filesResult = new List<string>();

			foreach (var file in files)
			{
				filesResult.Add(Path.GetFileName(file));
			}

			return filesResult;
		}
	}
}
