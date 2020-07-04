using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using PhotographersVideoDist.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotographersVideoDist.Authorization
{
	public class AdministratorsAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Case>
	{
		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, Case resource)
		{
			// Check user not null.
			if (context.User == null)
			{
				return Task.CompletedTask;
			}

			// Check current user are in the Administrators user role.
			if (context.User.IsInRole(Constants.AdministratorsRole))
			{
				context.Succeed(requirement);
			}

			// Requirement not succeded, return.
			return Task.CompletedTask;
		}
	}
}
