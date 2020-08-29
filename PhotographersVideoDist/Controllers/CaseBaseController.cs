using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PhotographersVideoDist.Data;
using PhotographersVideoDist.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotographersVideoDist.Controllers
{
	public class CaseBaseController : Controller
	{
		protected ApplicationDbContext Context { get; }
		protected IAuthorizationService AuthorizationService { get; }
		protected UserManager<ApplicationUser> UserManager { get; }
		public ILogger<CasesController> Logger { get; }


		public CaseBaseController(ApplicationDbContext context, IAuthorizationService authorizationService, UserManager<ApplicationUser> userManager, ILogger<CasesController> logger)
			: base()
		{
			Context = context;
			AuthorizationService = authorizationService;
			UserManager = userManager;
			Logger = logger;
		}
	}
}
