using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using PhotographersVideoDist.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotographersVideoDist.Authorization
{
	public class CustomersAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Case>
	{
		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, Case resource)
		{
			// Check user and resource are not null.
			if (context.User == null || resource == null)
			{
				return Task.CompletedTask;
			}

			// If we're not asking for following permission, return.
			if (requirement.Name != Constants.IsCustomerOperationName && 
				requirement.Name != Constants.ReadOperationName)
			{
				return Task.CompletedTask;
			}

			// Check the user is in the customers user role.
			if (context.User.IsInRole(Constants.CustomersRole))
			{
				context.Succeed(requirement);
			}

			// If no requirement are matched return.
			return Task.CompletedTask;
		}
	}
}
