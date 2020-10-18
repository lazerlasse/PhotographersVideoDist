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
using Microsoft.VisualBasic;
using Microsoft.AspNetCore.SignalR;
using PhotographersVideoDist.Hubs;
using Coravel.Queuing.Interfaces;

namespace PhotographersVideoDist.Controllers
{
	public class CasesController : CaseBaseController
	{
		public CasesController(
			ApplicationDbContext context,
			IAuthorizationService authorizationService,
			UserManager<ApplicationUser> userManager,
			IDataProtectionProvider protectionProvider,
			ILogger<CasesController> logger,
			IHubContext<FtpProgressHub> hubContext,
			IQueue taskQueue)
			: base(context, authorizationService, userManager, protectionProvider, logger, hubContext, taskQueue)
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

			IQueryable<Case> cases;

			if (User.IsInRole("Administrator"))
			{
				cases = Context.Cases
					.Include(p => p.Photographer)
					.Include(p => p.Postal)
					.AsNoTracking();
			}
			else
			{
				// Query cases from db.
				cases = Context.Cases
					.Include(p => p.Photographer)
					.Include(p => p.Postal)
					.AsNoTracking()
					.Where(c => c.PhotographerID == UserManager.GetUserId(User));
			}

			// Set page size.
			int pageSize = 10;

			// Return View and load content async.
			return View(await PaginatedList<Case>.CreateAsync(cases.OrderByDescending(c => c.CaseID), pageNumber ?? 1, pageSize));
		}

		// GET: Cases/Details/5
		public async Task<IActionResult> Details(int? caseID)
		{
			// Check id not null.
			if (caseID == null)
			{
				return NotFound();
			}

			// Load case from db.
			var caseToView = await Context.Cases
				.Include(p => p.Photographer)
				.Include(p => p.Postal)
				.AsNoTracking()
				.FirstOrDefaultAsync(c => c.CaseID == caseID);

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
		public async Task<IActionResult> CreateAsync(int? caseID)
		{
			// Check to create new case or load existing.
			if (caseID == null)
			{
				// Create new case and set DateTime to now.
				var caseToCreate = new CasesViewModel()
				{
					Case = new Case()
					{
						PhotographerID = UserManager.GetUserId(User),
						VideoAssets = new List<VideoAssets>(),
						ImageAssets = new List<ImageAssets>()
					}
				};

				// Check current user have create rights.
				if (!(await AuthorizationService.AuthorizeAsync(User, caseToCreate.Case, AuthorizationOperations.Create)).Succeeded)
				{
					return Forbid();
				}

				// Try to save the new case to get the caseId.
				try
				{
					Context.Cases.Add(caseToCreate.Case);
					await Context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException ex)
				{
					Logger.LogError("Der skete en fejl i forsøget på at oprette ny sag i databasen: " + ex.Message);
					ModelState.AddModelError("", "Kunne ikke oprette ny sag i databasen! " +
						"Prøv venligst igen! Kontakt support hvis fejlen fortsætter.");
				}

				// Return View with new case.
				return View(caseToCreate);
			}
			else
			{
				var caseToCreate = new CasesViewModel()
				{
					Case = await Context.Cases
						.Include(c => c.Photographer)
						.Include(c => c.Postal)
						.Include(c => c.ImageAssets)
						.Include(c => c.VideoAssets)
						.AsNoTracking()
						.FirstOrDefaultAsync(c => c.CaseID == caseID)
				};

				if (caseToCreate.Case == null)
				{
					Logger.LogWarning("GET: Cases/CreateAsync - Sagen blev ikke fundet eller kunne ikke indlæses!");
					return NotFound();
				}

				if (!(await AuthorizationService.AuthorizeAsync(User, caseToCreate.Case, AuthorizationOperations.Create)).Succeeded)
				{
					return Forbid();
				}

				// Return View with new case.
				return View(caseToCreate);
			}
		}

		// POST: Cases/Create
		[HttpPost, ActionName("Create")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> SaveCaseAsync(int? caseID)
		{
			// Check id not null.
			if (caseID == null)
			{
				return NotFound();
			}

			// Load case to update from db.
			var caseToCreate = await Context.Cases
				.Include(p => p.Photographer)
				.Include(p => p.Postal)
				.FirstOrDefaultAsync(c => c.CaseID == caseID);

			// Check loaded case not null.
			if (caseToCreate == null)
			{
				return NotFound();
			}

			// Validate current user have update rights.
			if (!(await AuthorizationService.AuthorizeAsync(User, caseToCreate, AuthorizationOperations.Update)).Succeeded)
			{
				return Forbid();
			}

			// Try update model async.
			if (await TryUpdateModelAsync<Case>(
				caseToCreate,
				"",
				c => c.CaseID, c => c.Titel, c => c.Details, c => c.Comments, c => c.Street, c => c.PostalCode))
			{
				// Try save changes async..
				try
				{
					await Context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException ex)
				{
					Logger.LogError("Ændringer i ny sag blev ikke gemt: " + ex.Message);
					ModelState.AddModelError("", "Ændringerne kunne ikke gemmes. " +
						"Prøv venligst igen! Kontakt support hvis fejlen fortsætter.");
				}

				// Succeded return to create view.
				return RedirectToAction("Create", "Cases", new { caseID = caseToCreate.CaseID });
			}

			// Saving new case failed, return create view.
			return View(caseToCreate);
		}

		// GET: Cases/Edit/5
		public async Task<IActionResult> EditAsync(int? caseID)
		{
			// Check id not null.
			if (caseID == null)
			{
				return NotFound();
			}

			// Load case to edit.
			var caseToEdit = new CasesViewModel()
			{
				Case = await Context.Cases
				.Include(c => c.Photographer)
				.Include(c => c.Postal)
				.Include(c => c.VideoAssets)
				.Include(c => c.ImageAssets)
				.FirstOrDefaultAsync(c => c.CaseID == caseID)
			};

			// Check loaded case not null.
			if (caseToEdit.Case == null)
			{
				return NotFound();
			}

			// Validate current user have edit rights.
			if (!(await AuthorizationService.AuthorizeAsync(User, caseToEdit.Case, AuthorizationOperations.Update)).Succeeded)
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
		public async Task<IActionResult> EditCaseAsync(int? caseID)
		{
			// Check id not null.
			if (caseID == null)
			{
				return NotFound();
			}

			// Load case to update from db.
			var caseToUpdate = new CasesViewModel()
			{
				Case = await Context.Cases
				.Include(p => p.Photographer)
				.Include(p => p.Postal)
				.FirstOrDefaultAsync(c => c.CaseID == caseID)
			};

			// Check loaded case not null.
			if (caseToUpdate.Case == null)
			{
				return NotFound();
			}

			// Validate current user have update rights.
			if (!(await AuthorizationService.AuthorizeAsync(User, caseToUpdate.Case, AuthorizationOperations.Update)).Succeeded)
			{
				return Forbid();
			}

			// Try update model async.
			if (await TryUpdateModelAsync<Case>(
				caseToUpdate.Case,
				"",
				c => c.CaseID, c => c.Titel, c => c.Details, c => c.Comments, c => c.Street, c => c.PostalCode))
			{
				// Try save changes async..
				try
				{
					await Context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException ex)
				{
					Logger.LogWarning("Cases/EditCase: Der opstod en uventet fejl i forsøget på at gemme ændringerne i databasen. " + ex.Message);
					ModelState.AddModelError("", "Ændringerne kunne ikke gemmes. " +
						"Prøv venligst igen! Kontakt support hvis fejlen fortsætter.");
				}

				// Succeded return to index.
				return RedirectToAction("Edit", "Cases", new { caseID = caseToUpdate.Case.CaseID });
			}

			// Save changes failed, return to view.
			return View(caseToUpdate);
		}


		// GET: Cases/Delete/5
		public async Task<IActionResult> Delete(int? caseID)
		{
			// Check id not null.
			if (caseID == null)
			{
				return NotFound();
			}

			// Load case to delete.
			var caseToDelete = await Context.Cases
				.Include(p => p.Photographer)
				.AsNoTracking()
				.FirstOrDefaultAsync(m => m.CaseID == caseID);

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
		public async Task<IActionResult> DeleteConfirmed(int? caseID)
		{
			// Check id not null.
			if (caseID == null)
			{
				return NotFound();
			}

			// load case to delete.
			var caseToDelete = await Context.Cases
				.Include(c => c.ImageAssets)
				.Include(c => c.VideoAssets)
				.FirstOrDefaultAsync(c => c.CaseID == caseID);

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
			catch (DbUpdateException ex)
			{
				Logger.LogError("Cases/Delete: Sagen blev ikke slettet fra databasen: " + ex.Message);
				ModelState.AddModelError("", "Der opstod en uventet fejl og sagen blev derfor ikke slettet. " +
						ex.Message +
						"Prøv venligst igen! Kontakt support hvis fejlen fortsætter.");
			}

			// Remove assets folder and files.
			var deleted = AssetsFileHandler.DeleteAssetsFolder(caseToDelete.CaseID, Logger);

			// Return to index.
			return RedirectToAction(nameof(Index), "Cases");
		}


		// POST: Cases/Create - Cases/Edit - Publish Action
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
			caseToPublish.Published = DateTime.Now;

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
			return RedirectToAction(nameof(SendFilesToFTPServer), "Cases", new { caseID = caseToPublish.CaseID });
		}


		// POST: Cases/Create - Cases/Edit - Send Files To FTP Server Action.
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

			// Create new FTP Client.
			var ftpClient = new MediaFilesFTPClient(
				ProtectionProvider,
				Logger, user.FTP_UserName,
				user.FTP_EncryptedPassword,
				user.FTP_Url,
				user.FTP_RemoteDir,
				HubContext);

			// Create new job id and queue the task.
			string jobId = Guid.NewGuid().ToString("N");
			TaskQueue.QueueAsyncTask(() => ftpClient.UploadFiles(filesToUpload, jobId));

			// Return to the progress page and start updating progress.
			return RedirectToAction("Progress", new { jobId });
		}

		// Progress page for ftp file upload.
		public IActionResult Progress(string jobId)
		{
			ViewBag.JobId = jobId;

			return View();
		}

		//**************************************************************************//
		//******************* Assets Upload Methods Section ************************//
		//**************************************************************************//

		// POST: Cases/Create - Cases/Edit - Upload Action
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
			IList<FileUpload> fileUploads = new List<FileUpload>();
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



		//**************************************************************************//
		//******************  Json result for autocomplete functions   *************//
		//**************************************************************************//

		// GET JSON Result: Cities autocomplete.
		[HttpPost]
		public JsonResult GetCities(string prefix)
		{
			//var cityList = new List<string>();

			var cityList = (from C in Context.Postals
							where C.Town.ToLower().StartsWith(prefix.ToLower())
							select new { label = C.Town, value = C.PostalCode });

			return Json(cityList);
		}


		// GET JSON Result: Postalcode autocomplete.
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

		// POST: Cases/Create - Delete VideoAssets.
		public async Task<IActionResult> DeleteVideoAssetsCreateViewAsync(int? id, int? caseID)
		{
			// Check id not null.
			if (id == null || caseID == null)
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

			// Delete Video Assets.
			await DeleteVideoAssetsAsync(assetsToDelete);

			// Return to page.
			return RedirectToAction("Create", "Cases", new { caseID });
		}

		// POST: Cases/Edit - Delete VideoAssets.
		public async Task<IActionResult> DeleteVideoAssetsEditViewAsync(int? id, int? caseID)
		{
			// Check id not null.
			if (id == null || caseID == null)
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

			// Delete Video Assets.
			await DeleteVideoAssetsAsync(assetsToDelete);

			// Return to page.
			return RedirectToAction("Edit", "Cases", new { caseID });
		}

		// Action: Delete Video Assets.
		public async Task DeleteVideoAssetsAsync(VideoAssets videoAssets)
		{
			// Delete video file from disk before removing from db.
			var deleted = AssetsFileHandler.DeleteAssetsFile(videoAssets.VideoAssetsFileName, videoAssets.CaseID, Logger);

			// Check file is deleted from the disk.
			if (deleted)
			{
				// Try delete file from db and disk.
				try
				{
					// Try remove assets from db async.
					Context.VideoAssets.Remove(videoAssets);
					await Context.SaveChangesAsync();
				}
				catch (Exception ex)
				{
					// Log error message and return.
					Logger.LogError("VideoAssets blev ikke slette fra databasen: " + ex.Message);
					return;
				}

				// Log result information.
				Logger.LogInformation("VideoAssets blev fjernet med succes.");
			}
			else
			{
				// Log error message.
				Logger.LogError("Der opstod en uventet fejl, og videoen blev derfor ikke slettet!");
			}

			// If succeded return.
			return;
		}


		// POST: Cases/Create - Delete ImageAssets.
		public async Task<IActionResult> DeleteImageAssetsCreateViewAsync(int? id, int? caseID)
		{
			// Check id not null.
			if (id == null || caseID == null)
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

			// Delete assets.
			await DeleteImageAssetsAsync(assetsToDelete);

			// Return to page.
			return RedirectToAction("Create", "Cases", new { caseID });
		}

		// POST: Cases/Edit - Delete ImageAssets.
		public async Task<IActionResult> DeleteImageAssetsEditViewAsync(int? id, int? caseID)
		{
			// Check id not null.
			if (id == null || caseID == null)
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

			// Delete assets.
			await DeleteImageAssetsAsync(assetsToDelete);

			// Return to page.
			return RedirectToAction("Edit", "Cases", new { caseID });
		}

		// Action: Delete Image Assets.
		public async Task DeleteImageAssetsAsync(ImageAssets imageAssets)
		{
			// Delete video file from disk before deleting from db.
			var deleted = AssetsFileHandler.DeleteAssetsFile(imageAssets.ImageFileName, imageAssets.CaseID, Logger);

			// Check file is deleted from the disk.
			if (deleted)
			{
				// Try delete from db.
				try
				{
					// Try remove assets from db async.
					Context.ImageAssets.Remove(imageAssets);
					await Context.SaveChangesAsync();
				}
				catch (Exception ex)
				{
					// Log error message and return page.
					Logger.LogError("ImageAssets blev ikke slette fra databasen: " + ex.Message);
					return;
				}

				// Log result information.
				Logger.LogInformation("ImageAssets blev fjernet med succes.");
			}
			else
			{
				// Log error message.
				Logger.LogError("Der opstod en uventet fejl, og billedet blev derfor ikke slettet!");
			}

			// Return.
			return;
		}


		// Action: Cases/Create - Generate Stills from video.
		public async Task<IActionResult> CreateViewAutoStillsAsync(string videoFileName, int? assetsFolderID)
		{
			// Check id not null.
			if (assetsFolderID == null)
			{
				return NotFound();
			}

			// Check user have rights to generate stills.
			if (!(await AuthorizationService.AuthorizeAsync(User, new Case(), AuthorizationOperations.MediaProcessing)).Succeeded)
			{
				return Forbid();
			}

			await GenerateStillsFromVideoAsync(videoFileName, (int)assetsFolderID);

			return RedirectToAction("Create", "Cases", new { caseID = assetsFolderID });
		}


		// Action: Cases/Edit - Generate Stills from video.
		public async Task<IActionResult> EditViewAutoStillsAsync(string videoFileName, int? assetsFolderID)
		{
			// Check id not null.
			if (assetsFolderID == null)
			{
				return NotFound();
			}

			// Check user have rights to generate stills.
			if (!(await AuthorizationService.AuthorizeAsync(User, new Case(), AuthorizationOperations.MediaProcessing)).Succeeded)
			{
				return Forbid();
			}

			await GenerateStillsFromVideoAsync(videoFileName, (int)assetsFolderID);

			return RedirectToAction("Edit", "Cases", new { caseID = assetsFolderID });
		}

		// Action: Generate Auto Stills function.
		public async Task GenerateStillsFromVideoAsync(string videoFileName, int assetsFolderID)
		{
			// Make a list for assets to delete if saving to db fails.
			List<string> notSavedAssetsList = new List<string>();

			// Generate stills from video and return list of generated files.
			MediaProcessingFFMPEG mediaProcessing = new MediaProcessingFFMPEG(Logger);
			var generatedAssets = mediaProcessing.GenerateStillsFromVideo(videoFileName, assetsFolderID);

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
				AssetsFileHandler.DeleteAssetsFile(notSavedAssets, assetsFolderID, Logger);
			}

			// Return.
			return;
		}


		// Action: Save image assets to db.
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
	}
}
