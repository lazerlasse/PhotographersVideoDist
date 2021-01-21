using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PhotographersVideoDist.Models;
using PhotographersVideoDist.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using PhotographersVideoDist.Authorization;
using PhotographersVideoDist.Paging;

namespace PhotographersVideoDist.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly ApplicationDbContext Context;
        protected IAuthorizationService AuthorizationService;
        public ILogger<CustomersController> Logger;

        public CustomersController(ApplicationDbContext context, IAuthorizationService authorizationService, ILogger<CustomersController> logger)
        {
            Context = context;
            AuthorizationService = authorizationService;
            Logger = logger;
        }

        // GET: Customers/Index
        public async Task<IActionResult> Index(int? pageNumber)
        {
            // Check current user have read rights.
            if (!(await AuthorizationService.AuthorizeAsync(User, new Case(), AuthorizationOperations.Read)).Succeeded)
            {
                return Forbid();
            }

            // Query cases from db
            var cases = Context.Cases
                .Include(c => c.ImageAssets)
                .Include(c => c.Postal)
                .Include(c => c.Photographer)
                .AsNoTracking();

            // Set page size.
            int pageSize = 10;

            // Return View and load content async.
            return View(await PaginatedList<Case>.CreateAsync(
                cases.OrderByDescending(c => c.CaseID).Where(c => c.IsPublished == true),
                pageNumber ?? 1,
                pageSize));
        }

        // GET: Customers/Details/5
        public async Task<IActionResult> Case(int? caseID)
        {
            // Check ID not null.
            if (caseID == null)
            {
                return NotFound();
            }

            // Load case from db.
            var caseToView = await Context.Cases
               .Include(c => c.ImageAssets)
               .Include(c => c.VideoAssets)
               .Include(c => c.Postal)
               .Include(c => c.Photographer)
               .Where(c => c.CaseID == caseID)
               .AsNoTracking()
               .FirstOrDefaultAsync();

            // Check case not null.
            if (caseToView == null)
            {
                return NotFound();
            }

            // Check current user have read rights.
            if (!(await AuthorizationService.AuthorizeAsync(User, caseToView, AuthorizationOperations.Read)).Succeeded)
            {
                return Forbid();
            }

            // Return loaded case to view.
            return View(caseToView);
        }
    }
}
