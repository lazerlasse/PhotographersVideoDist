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

namespace PhotographersVideoDist.Controllers
{
    public class CasesController : CaseBaseController
    {
        public CasesController(ApplicationDbContext context, IAuthorizationService authorizationService, UserManager<IdentityUser> userManager)
            :base(context, authorizationService, userManager)
        {
        }

        // GET: Cases
        public async Task<IActionResult> Index()
        {
            // Query cases from db.
            var cases = Context.Cases
                .Include(p => p.Photographer)
                .Include(p => p.Postal)
                .AsNoTracking();

            // Return View and load content async.
            return View(await cases.ToListAsync());
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

            // Return view.
            return View(caseToView);
        }

        // GET: Cases/Create
        public IActionResult Create()
        {
            ViewData["PostalCode"] = new SelectList(Context.Postals, "PostalCode", "Town");
            return View();
        }

        // POST: Cases/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CaseID,Titel,Details,Comments,Street,PostalCode")] Case caseToCreate)
        {
            if (ModelState.IsValid)
            {
                // Set Published date to DateTime.Now.
                caseToCreate.Published = DateTime.Now;

                // Set current user as photographer.
                caseToCreate.PhotographerID = UserManager.GetUserId(User);

                // Add the case to data context and save async.
                Context.Add(caseToCreate);
                await Context.SaveChangesAsync();

                // Return to index.
                return RedirectToAction(nameof(Index));
            }

            // Saving case failed, return create view.
            ViewData["PostalCode"] = new SelectList(Context.Postals, "PostalCode", "Town", caseToCreate.PostalCode);
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

            // Populate dropdown boxes.
            ViewData["PostalCode"] = new SelectList(Context.Postals, "PostalCode", "Town", caseToEdit.PostalCode);
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
                    ModelState.AddModelError("", "Unable to save changes. " +
                        "Try again, and if the problem persists, " +
                        "see your system administrator.");
                }

                // Succeded return to index.
                return RedirectToAction(nameof(Index));
            }

            // Update failed - Pupolate postal dropdown box.
            ViewData["PostalCode"] = new SelectList(Context.Postals, "PostalCode", "Town", caseToUpdate.PostalCode);
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

            // Remove case from context and save changes to db.
            Context.Cases.Remove(caseToDelete);
            await Context.SaveChangesAsync();

            // Return to index.
            return RedirectToAction(nameof(Index));
        }

        private bool CaseExists(int id)
        {
            return Context.Cases.Any(e => e.CaseID == id);
        }
    }
}
