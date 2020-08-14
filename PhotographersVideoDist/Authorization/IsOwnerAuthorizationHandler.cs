using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using PhotographersVideoDist.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotographersVideoDist.Authorization
{
	public class IsOwnerAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Case>
	{
		UserManager<IdentityUser> _userManager;

		public IsOwnerAuthorizationHandler(UserManager<IdentityUser> userManager)
		{
			_userManager = userManager;
		}

		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, Case resource)
		{
			// Check user and resource not null.
			if (context.User == null || resource == null)
			{
				return Task.CompletedTask;
			}

			// If we're not asking for the following permission, return.
			if (requirement.Name != Constants.UpdateOperationName &&
				requirement.Name != Constants.IsOwnerOperationName &&
				requirement.Name != Constants.DeleteOperationName)
			{
				return Task.CompletedTask;
			}

			// Check current user id match Photographer id in resource.
			if (resource.PhotographerID == _userManager.GetUserId(context.User))
			{
				context.Succeed(requirement);
			}

			// Requirements not succeded, return.
			return Task.CompletedTask;
		}
	}
}
