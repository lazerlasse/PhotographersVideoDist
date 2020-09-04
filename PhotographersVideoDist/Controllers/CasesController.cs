using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PhotographersVideoDist.Data;
using PhotographersVideoDist.Models;
using PhotographersVideoDist.Paging;
using PhotographersVideoDist.Authorization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using System.Net;
using PhotographersVideoDist.Utilities;
using Microsoft.AspNetCore.DataProtection;
using System.IO;

namespace PhotographersVideoDist.Controllers
{
	public class CasesController : CaseBaseController
	{
		public CasesController(ApplicationDbContext context, IAuthorizationService authorizationService, UserManager<ApplicationUser> userManager, IDataProtectionProvider protectionProvider, ILogger<CasesController> logger)
			: base(context, authorizationService, userManager, protectionProvider, logger)
		{
		}

		// GET: Cases
		public async Task<IActionResult> Index(int? pageNumber)
		{
			// Check current user have read rights.
			if (!(await AuthorizationService.AuthorizeAsync(User, new Case(), AuthorizationOperations.Read)).Succeeded)
			{
				return Forbid();
			}

			// Query cases from db.
			var cases = Context.Cases
				.Include(p => p.Photographer)
				.Include(p => p.Postal)
				.AsNoTracking();

			// Set page size.
			int pageSize = 3;

			// Return View and load content async.
			return View(await PaginatedList<Case>.CreateAsync(cases, pageNumber ?? 1, pageSize));
		}

		// GET: Cases/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			// Check id not null.
			if (id == null)
			{
				return NotFound();
			}

			// Load case from db.
			var caseToView = await Context.Cases
				.Include(p => p.Photographer)
				.Include(p => p.Postal)
				.AsNoTracking()
				.FirstOrDefaultAsync(c => c.CaseID == id);

			// Check loaded case not null.
			if (caseToView == null)
			{
				return NotFound();
			}

			// Check current user are authorized to read this case..
			if (!(await AuthorizationService.AuthorizeAsync(User, caseToView, AuthorizationOperations.Read)).Succeeded)
			{
				return Forbid();
			}

			// Return view.
			return View(caseToView);
		}

		// GET: Cases/Create
		public async Task<IActionResult> CreateAsync()
		{
			// Create new case and set DateTime to now.
			var caseToCreate = new Case()
			{
				Published = DateTime.Now
			};

			// Check current user have create rights.
			if (!(await AuthorizationService.AuthorizeAsync(User, caseToCreate, AuthorizationOperations.Create)).Succeeded)
			{
				return Forbid();
			}

			// Return View with new case.
			return View(caseToCreate);
		}

		// POST: Cases/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to, for 
		// more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("CaseID,Titel,Details,Comments,Street,Published")] Case caseToCreate, [Bind("PostalCode,Town")] Postal postal)
		{
			if (ModelState.IsValid)
			{
				// Check user have create rights.
				if (!(await AuthorizationService.AuthorizeAsync(User, caseToCreate, AuthorizationOperations.Create)).Succeeded)
				{
					return Forbid();
				}

				// Set current user as photographer.
				caseToCreate.PhotographerID = UserManager.GetUserId(User);

				// Check and update Town and Postalcode.
				if (postal.PostalCode == null && postal.Town != null)
				{
					var postalResult = await Context.Postals
						.AsNoTracking()
						.FirstOrDefaultAsync(p => p.Town.ToLower().Contains(postal.Town.ToLower()));

					if (postalResult != null)
					{
						caseToCreate.PostalCode = postalResult.PostalCode;
					}
				}

				// Add the case to data context and save async.
				Context.Add(caseToCreate);
				await Context.SaveChangesAsync();

				// Return to index.
				return RedirectToAction("AssetsUpload", "Cases", new { id = caseToCreate.CaseID });
			}

			// Saving new case failed, return create view.
			return View(caseToCreate);
		}

		// GET: Cases/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			// Check id not null.
			if (id == null)
			{
				return NotFound();
			}

			// Load case to edit.
			var caseToEdit = await Context.Cases
				.Include(p => p.Photographer)
				.Include(p => p.Postal)
				.FirstOrDefaultAsync(c => c.CaseID == id);

			// Check loaded case not null.
			if (caseToEdit == null)
			{
				return NotFound();
			}

			// Validate current user have edit rights.
			if (!(await AuthorizationService.AuthorizeAsync(User, caseToEdit, AuthorizationOperations.Update)).Succeeded)
			{
				return Forbid();
			}

			// Return View with case to edit.
			return View(caseToEdit);
		}

		// POST: Cases/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to, for 
		// more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost, ActionName("Edit")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditCase(int? id)
		{
			// Check id not null.
			if (id == null)
			{
				return NotFound();
			}

			// Load case to update from db.
			var caseToUpdate = await Context.Cases
				.Include(p => p.Photographer)
				.Include(p => p.Postal)
				.FirstOrDefaultAsync(c => c.CaseID == id);

			// Check loaded case not null.
			if (caseToUpdate == null)
			{
				return NotFound();
			}

			// Validate current user have update rights.
			if (!(await AuthorizationService.AuthorizeAsync(User, caseToUpdate, AuthorizationOperations.Update)).Succeeded)
			{
				return Forbid();
			}

			// Try update model async.
			if (await TryUpdateModelAsync<Case>(
				caseToUpdate,
				"",
				c => c.CaseID, c => c.Titel, c => c.Details, c => c.Comments, c => c.Street, c => c.PostalCode))
			{
				// Try save changes async..
				try
				{
					await Context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					//Log the error (uncomment ex variable name and write a log.)
					ModelState.AddModelError("", "Ændringerne kunne ikke gemmes. " +
						"Prøv venligst igen! Kontakt support hvis fejlen fortsætter.");
				}

				// Succeded return to index.
				return RedirectToAction(nameof(Index));
			}

			// Save changes failed, return to view.
			return View(caseToUpdate);
		}

		// GET: Cases/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			// Check id not null.
			if (id == null)
			{
				return NotFound();
			}

			// Load case to delete.
			var caseToDelete = await Context.Cases
				.Include(p => p.Photographer)
				.AsNoTracking()
				.FirstOrDefaultAsync(m => m.CaseID == id);

			// Check loaded case not null.
			if (caseToDelete == null)
			{
				return NotFound();
			}

			// Validate current user have delete rights.
			if (!(await AuthorizationService.AuthorizeAsync(User, caseToDelete, AuthorizationOperations.Delete)).Succeeded)
			{
				return Forbid();
			}

			// Return View.
			return View(caseToDelete);
		}


		// POST: Cases/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int? id)
		{
			// Check id not null.
			if (id == null)
			{
				return NotFound();
			}

			// load case to delete.
			var caseToDelete = await Context.Cases
				.Include(c => c.ImageAssets)
				.Include(c => c.VideoAssets)
				.FirstOrDefaultAsync(c => c.CaseID == id);

			// Check loaded case not null.
			if (caseToDelete == null)
			{
				return NotFound();
			}

			// Validate current user have delete rights.
			if (!(await AuthorizationService.AuthorizeAsync(User, caseToDelete, AuthorizationOperations.Delete)).Succeeded)
			{
				return Forbid();
			}

			// Remove case from context and save changes to db.
			Context.Cases.Remove(caseToDelete);

			try
			{
				await Context.SaveChangesAsync();
			}
			catch (DbUpdateException)
			{
				ModelState.AddModelError("", "Sagen blev ikke slettet. " +
						"Prøv venligst igen! Kontakt support hvis fejlen fortsætter.");
			}

			// Remove assets folder and files.
			var deleted = AssetsFileHandler.DeleteAssetsFolder(caseToDelete.CaseID, Logger);

			// Return to index.
			return RedirectToAction(nameof(Index));
		}



		//**************************************************************************//
		//******************* Assets Upload Methods Section ************************//
		//**************************************************************************//

		// GET: Cases/AssetsUpload
		public async Task<IActionResult> AssetsUpload(int? id)
		{
			// Check id not null.
			if (id == null)
			{
				return NotFound();
			}

			// Load case from db.
			AssetsUploadViewModel caseForUpload = new AssetsUploadViewModel
			{
				Case = await Context.Cases
				.Include(i => i.ImageAssets)
				.Include(v => v.VideoAssets)
				.FirstOrDefaultAsync(c => c.CaseID == id)
			};

			// Check case loaded correctly from db.
			if (caseForUpload == null)
			{
				return NotFound();
			}

			// Authenticate user have upload rights.
			if (!(await AuthorizationService.AuthorizeAsync(User, caseForUpload.Case, AuthorizationOperations.Create)).Succeeded)
			{
				return Forbid();
			}

			// Set path to media files in view data.
			ViewBag.MediaPath = "~/wwwroot/Cases/" + id + "/";

			return View(caseForUpload);
		}


		// POST: Cases/AssetsUpload - Upload Action
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Upload(int? id, List<IFormFile> files)
		{
			// Check id not null.
			if (id == null)
			{
				return new JsonResult(new { name = files.FirstOrDefault().FileName, value = "Fejlet" });
			}

			// Create a list of fileupload model for the files to upload.
			IList <FileUpload> fileUploads = new List<FileUpload>();
			foreach (IFormFile file in files)
			{
				fileUploads.Add(
					new FileUpload
					{
						AssetsFile = file
					});
			}

			// Check user have rights to upload files.
			if (!(await AuthorizationService.AuthorizeAsync(User, new Case(), AuthorizationOperations.Upload)).Succeeded)
			{
				return Forbid();
			}

			// Validate files before uploading...
			var validatedFiles = MediaFileValidator.Validate(fileUploads);

			// Create new UploadFileHandler and try uploading files...
			var UploadFileHandler = new UploadFileHandler((int)id, Logger, Context);
			await UploadFileHandler.UploadFiles(validatedFiles);

			// Create return result.
			var result = new { name = validatedFiles.FirstOrDefault().AssetsFile.FileName, value = validatedFiles.FirstOrDefault().UploadStatus };

			// For compatibility with IE's "done" event we need to return a result as well as setting the context.response
			return new JsonResult(result);
		}


		// POST: Cases/AssetsUpload - Delete VideoAssets.
		public async Task<IActionResult> DeleteVideoAssets(int? id, int? caseId)
		{
			// Check id not null.
			if (id == null)
			{
				return NotFound();
			}

			// Load video assets to delete from db.
			var assetsToDelete = await Context.VideoAssets
				.Include(c => c.Case)
				.AsNoTracking()
				.FirstOrDefaultAsync(v => v.VideoAssetsID == id);

			// Check assets loaded correct.
			if (assetsToDelete == null)
			{
				return NotFound();
			}

			// Check user have delete rights.
			if (!(await AuthorizationService.AuthorizeAsync(User, assetsToDelete.Case, AuthorizationOperations.Delete)).Succeeded)
			{
				return Forbid();
			}

			// Delete video file from disk before removing from db.
			var deleted = AssetsFileHandler.DeleteAssetsFile(assetsToDelete.VideoAssetsFileName, assetsToDelete.CaseID, Logger);

			// Check file is deleted from the disk.
			if (deleted)
			{
				// Try delete file from db and disk.
				try
				{
					// Try remove assets from db async.
					Context.VideoAssets.Remove(assetsToDelete);
					await Context.SaveChangesAsync();
				}
				catch (Exception ex)
				{
					// Log error message and return page.
					Logger.LogError("VideoAssets blev ikke slette fra databasen: " + ex.Message);
					return RedirectToAction("AssetsUpload", "Cases", new { id });
				}

				// Log result information.
				Logger.LogInformation("VideoAssets blev fjernet med succes.");
			}
			else
			{
				// Log error message.
				Logger.LogError("Der opstod en uventet fejl, og videoen blev derfor ikke slettet!");
			}

			// If succeded return page.
			return RedirectToAction("AssetsUpload", "Cases", new { id = caseId });
		}


		// POST: Cases/AssetsUpload - Delete ImageAssets.
		public async Task<IActionResult> DeleteImageAssets(int? id, int? caseId)
		{
			// Check id not null.
			if (id == null)
			{
				return NotFound();
			}

			// Load video assets to delete from db.
			var assetsToDelete = await Context.ImageAssets
				.Include(c => c.Case)
				.AsNoTracking()
				.FirstOrDefaultAsync(i => i.ImageAssetsID == id);

			// Check assets loaded correct.
			if (assetsToDelete == null)
			{
				return NotFound();
			}

			// Check user have delete rights.
			if (!(await AuthorizationService.AuthorizeAsync(User, assetsToDelete.Case, AuthorizationOperations.Delete)).Succeeded)
			{
				return Forbid();
			}

			// Delete video file from disk before deleting from db.
			var deleted = AssetsFileHandler.DeleteAssetsFile(assetsToDelete.ImageFileName, assetsToDelete.CaseID, Logger);

			// Check file is deleted from the disk.
			if (deleted)
			{
				// Try delete from db.
				try
				{
					// Try remove assets from db async.
					Context.ImageAssets.Remove(assetsToDelete);
					await Context.SaveChangesAsync();
				}
				catch (Exception ex)
				{
					// Log error message and return page.
					Logger.LogError("ImageAssets blev ikke slette fra databasen: " + ex.Message);
					return RedirectToAction("AssetsUpload", "Cases", new { id });
				}

				// Log result information.
				Logger.LogInformation("ImageAssets blev fjernet med succes.");
			}
			else
			{
				// Log error message.
				Logger.LogError("Der opstod en uventet fejl, og billedet blev derfor ikke slettet!");
			}

			// If succeded return page.
			return RedirectToAction("AssetsUpload", "Cases", new { id = caseId });
		}

		// POST: 
		public async Task<IActionResult> Publish(int? caseID)
		{
			// Check case id not null...
			if (caseID == null)
			{
				return NotFound();
			}

			// Load case from db.
			var caseToPublish = await Context.Cases.FirstOrDefaultAsync(c => c.CaseID == caseID);

			// Check case not null.
			if (caseToPublish == null)
			{
				return NotFound();
			}

			// Check user have rights to publish.
			if (!(await AuthorizationService.AuthorizeAsync(User, caseToPublish, AuthorizationOperations.IsOwner)).Succeeded)
			{
				return Forbid();
			}

			// Set case state to published.
			caseToPublish.IsPublished = true;

			// Try update model async.
			if (await TryUpdateModelAsync<Case>(
				caseToPublish,
				"",
				c => c.Published))
			{
				// Try save changes async..
				try
				{
					await Context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					//Log the error (uncomment ex variable name and write a log.)
					ModelState.AddModelError("", "Ændringerne kunne ikke gemmes. " +
						"Prøv venligst igen! Kontakt support hvis fejlen fortsætter.");
				}
			}

			// Succeded return to index.
			return RedirectToAction(nameof(SendFilesToFTPServer), "Cases", new { caseID = caseToPublish.CaseID });
		}

		// POST: Cases/AssetsUpload - Send Files To FTP Server
		public async Task<IActionResult> SendFilesToFTPServer(int? caseID)
		{
			// Check caseID not null.
			if (caseID == null)
			{
				return NotFound();
			}

			// Check user have rights to perform action.
			if (!(await AuthorizationService.AuthorizeAsync(User, new Case(), AuthorizationOperations.Upload)).Succeeded)
			{
				return Forbid();
			}

			// Load assets to upload.
			var imageAssets = await Context.ImageAssets
				.Where(c => c.CaseID == caseID)
				.AsNoTracking().ToListAsync();

			var videoAssets = await Context.VideoAssets
				.Where(c => c.CaseID == caseID)
				.AsNoTracking().ToListAsync();

			// Loop through assets and make a list of path strings from loaded assets files.
			var filesToUpload = new List<string>();

			foreach (var assets in imageAssets)
			{
				filesToUpload.Add(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Cases", caseID.ToString(), assets.ImageFileName));
			}

			foreach (var assets in videoAssets)
			{
				filesToUpload.Add(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Cases", caseID.ToString(), assets.VideoAssetsFileName));
			}


			// Load current user.
			var user = await UserManager.GetUserAsync(User);

			// Check user not null.
			if (user == null)
			{
				return NotFound();
			}

			// Upload files to FTP Server.
			var ftpClient = new MediaFilesFTPClient(ProtectionProvider, Logger, user.FTP_UserName, user.FTP_EncryptedPassword, user.FTP_Url, user.FTP_RemoteDir);
			await ftpClient.UploadFiles(filesToUpload);

			return RedirectToAction(nameof(Index), "Cases");
		}

		//**************************************************************************//
		//******************  Json result for autocomplete functions   *************//
		//**************************************************************************//

		// Get cities json for autocomplete.
		[HttpPost]
		public JsonResult GetCities(string prefix)
		{
			//var cityList = new List<string>();

			var cityList = (from C in Context.Postals
							where C.Town.ToLower().StartsWith(prefix.ToLower())
							select new { label = C.Town, value = C.PostalCode });

			return Json(cityList);
		}

		// Get postalcode json for autocomplete.
		[HttpPost]
		public JsonResult GetPostals(string prefix)
		{
			var postalList = (from C in Context.Postals
							  where C.PostalCode.StartsWith(prefix)
							  select new { label = C.Town, value = C.PostalCode });

			return Json(postalList);
		}



		//**************************************************************************//
		//***********************  Method for media actions   **********************//
		//**************************************************************************//

		public async Task<IActionResult> GenerateStillsFromVideo(string videoFileName, int? assetsFolderID)
		{
			// Check caseID not null.
			if (assetsFolderID == null)
			{
				return NotFound();
			}

			// Check user have rights to generate stills.
			if (!(await AuthorizationService.AuthorizeAsync(User, new Case(), AuthorizationOperations.MediaProcessing)).Succeeded)
			{
				return Forbid();
			}

			// Make a list for assets to delete if saving to db fails.
			List<string> notSavedAssetsList = new List<string>();

			// Generate stills from video and return list of generated files.
			MediaProcessingFFMPEG mediaProcessing = new MediaProcessingFFMPEG(Logger);
			var generatedAssets = mediaProcessing.GenerateStillsFromVideo(videoFileName, (int)assetsFolderID);

			// Check genrated files not null and add them to db.
			if (generatedAssets != null)
			{
				// Loop through assets and add to db set.
				foreach (var assets in generatedAssets)
				{
					// Generate new image assets to add to db.
					var imageAssets = new ImageAssets
					{
						ImageFileName = assets,
						CaseID = (int)assetsFolderID
					};

					// Try save assets to db.
					var assetsSaved = await SaveImageAssetsToDb(imageAssets);

					// If saving saving assets fail, add to failed list.
					if (!assetsSaved)
					{
						notSavedAssetsList.Add(assets);
					}
				}
			}

			// Delete unsaved assets from disk.
			foreach (var notSavedAssets in notSavedAssetsList)
			{
				AssetsFileHandler.DeleteAssetsFile(notSavedAssets, (int)assetsFolderID, Logger);
			}

			// Refresh page.
			return RedirectToAction("AssetsUpload", "Cases", new { id = assetsFolderID });
		}

		// Save image assets to db.
		public async Task<bool> SaveImageAssetsToDb(ImageAssets imageAssets)
		{
			// Try save the changes to database.
			try
			{
				// Remove assets from context.
				Context.ImageAssets.Add(imageAssets);

				// Save changes async.
				await Context.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				// Log the error.
				Logger.LogError("Der skete en fejl i forsøget på at gemme ændringerne i databasen: " + ex.Message);

				// Return false. Save failed!
				return false;
			}

			// Log image assets added to db set.
			Logger.LogInformation("Image Assets " + imageAssets + " blev gemt i databasen med succes.");

			// Succeded return true.
			return true;
		}

		// Make image primary.

	}
}
