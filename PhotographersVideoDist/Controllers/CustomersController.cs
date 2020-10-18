using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PhotographersVideoDist.Models;
using PhotographersVideoDist.Data;

namespace PhotographersVideoDist.Controllers
{
    public class CustomersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Customers/Index
        public async Task<IActionResult> Index()
        {
            var cases = _context.Cases
                .Include(c => c.ImageAssets)
                .Include(c => c.VideoAssets)
                .Include(c => c.Postal)
                .Include(c => c.Photographer)
                .AsNoTracking();
            
            return View(await cases.ToListAsync());
        }

        // GET: Customers/Details/5
        public async Task<IActionResult> Case(int? caseID)
        {
            if (caseID == null)
            {
                return NotFound();
            }

            var caseToView = await _context.Cases
               .Include(c => c.ImageAssets)
               .Include(c => c.VideoAssets)
               .Include(c => c.Postal)
               .Include(c => c.Photographer)
               .AsNoTracking()
               .Where(c => c.CaseID == caseID)
               .FirstOrDefaultAsync();

            if (caseToView == null)
            {
                return NotFound();
            }

            return View(caseToView);
        }
    }
}
