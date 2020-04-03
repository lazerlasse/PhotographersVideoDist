﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PhotographersVideoDist.Data;
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
		protected UserManager<IdentityUser> UserManager { get; }


		public CaseBaseController(ApplicationDbContext context, IAuthorizationService authorizationService, UserManager<IdentityUser> userManager)
			: base()
		{
			Context = context;
			AuthorizationService = authorizationService;
			UserManager = userManager;
		}
	}
}
