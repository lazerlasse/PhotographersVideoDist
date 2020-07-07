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

namespace PhotographersVideoDist.Controllers
{
	public class CasesController : CaseBaseController
	{
		public CasesController(ApplicationDbContext context, IAuthorizationService authorizationService, UserManager<IdentityUser> userManager)
			: base(context, authorizationService, userManager)
		{
		}

		// GET: Cases
		public async Task<IActionResult> Index(int? pageNumber)
		{
			// Check current user have read rights.
			if (!(await AuthorizationService.AuthorizeAsync(User, new Case(), AuthorizationOperations.Read)).Succeeded)
			{
				Forbid();
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
				Forbid();
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
				Forbid();
			}

			// Return View with new case.
			return View(caseToCreate);
		}

		// POST: Cases/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to, for 
		// more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("CaseID,Titel,Details,Comments,Street,PostalCode,Published")] Case caseToCreate)
		{
			if (ModelState.IsValid)
			{
				// Set current user as photographer.
				caseToCreate.PhotographerID = UserManager.GetUserId(User);

				// Check user have create rights.
				if (!(await AuthorizationService.AuthorizeAsync(User, caseToCreate, AuthorizationOperations.Create)).Succeeded)
				{
					Forbid();
				}

				// Add the case to data context and save async.
				Context.Add(caseToCreate);
				await Context.SaveChangesAsync();

				// Return to index.
				return RedirectToAction(nameof(Index));
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
				Forbid();
			}

			// Populate dropdown boxes.
			ViewData["PostalCode"] = new SelectList(Context.Postals, "PostalCode", "Town", caseToEdit.PostalCode);
			
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
				Forbid();
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
						"Prøv venligst igen! Hvis fejlen fortsætter, kontakt en administrator.");
				}

				// Succeded return to index.
				return RedirectToAction(nameof(Index));
			}

			// Update failed - Pupolate postal dropdown box.
			ViewData["PostalCode"] = new SelectList(Context.Postals, "PostalCode", "Town", caseToUpdate.PostalCode);

			// Return back to edit view.
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
				.Include(p => p.Postal)
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
				Forbid();
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
				.FirstOrDefaultAsync(c => c.CaseID == id);

			// Check loaded case not null.
			if (caseToDelete == null)
			{
				return NotFound();
			}

			// Validate current user have delete rights.
			if (!(await AuthorizationService.AuthorizeAsync(User, caseToDelete, AuthorizationOperations.Delete)).Succeeded)
			{
				Forbid();
			}

			// Remove case from context and save changes to db.
			Context.Cases.Remove(caseToDelete);
			await Context.SaveChangesAsync();

			// Return to index.
			return RedirectToAction(nameof(Index));
		}

		//**************************************************************************//
		//******************  Json result for autocomplete functions   *************//
		//**************************************************************************//
		[HttpPost]
		public JsonResult GetCities(string prefix)
		{
			//var cityList = new List<string>();

			var cityList = (from C in Context.Postals
					where C.Town.ToLower().StartsWith(prefix.ToLower())
					select new { label = C.Town, value = C.PostalCode });

			return Json(cityList);
		}

		private bool CaseExists(int id)
		{
			return Context.Cases.Any(e => e.CaseID == id);
		}
	}
}
