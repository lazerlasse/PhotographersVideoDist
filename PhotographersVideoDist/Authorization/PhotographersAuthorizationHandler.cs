using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using PhotographersVideoDist.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotographersVideoDist.Authorization
{
	public class PhotographersAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Case>
	{
		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, Case resource)
		{
			// Check user and resource not null.
			if (context.User == null || resource == null)
			{
				return Task.CompletedTask;
			}


			// If we're not asking for CRUD permission or approval/reject, return.
			if (requirement.Name != Constants.CreateOperationName &&
				requirement.Name != Constants.ReadOperationName &&
				requirement.Name != Constants.IsPhotographerOperationName)
			{
				return Task.CompletedTask;
			}

			// Check if current user is in the CasePhotographersRole...
			if (context.User.IsInRole(Constants.PhotographersRole))
			{
				context.Succeed(requirement);
			}

			// No requirement is matched so return...
			return Task.CompletedTask;
		}
	}
}
