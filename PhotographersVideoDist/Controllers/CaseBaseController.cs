using Coravel.Queuing.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using PhotographersVideoDist.Data;
using PhotographersVideoDist.Hubs;
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
		protected IDataProtectionProvider ProtectionProvider { get; }
		public ILogger<CasesController> Logger { get; }
		public readonly IHubContext<FtpProgressHub> HubContext;
		public readonly IQueue TaskQueue;


		public CaseBaseController(
			ApplicationDbContext context,
			IAuthorizationService authorizationService,
			UserManager<ApplicationUser> userManager,
			IDataProtectionProvider dataProtectionProvider,
			ILogger<CasesController> logger,
			IHubContext<FtpProgressHub> hubContext,
			IQueue taskQueue)
			: base()
		{
			Context = context;
			AuthorizationService = authorizationService;
			UserManager = userManager;
			ProtectionProvider = dataProtectionProvider;
			Logger = logger;
			HubContext = hubContext;
			TaskQueue = taskQueue;
		}
	}
}
