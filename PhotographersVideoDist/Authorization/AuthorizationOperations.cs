using Microsoft.AspNetCore.Authorization.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotographersVideoDist.Authorization
{
	public static class AuthorizationOperations
	{
		public static OperationAuthorizationRequirement Create =
		  new OperationAuthorizationRequirement { Name = Constants.CreateOperationName };
		public static OperationAuthorizationRequirement Read =
		  new OperationAuthorizationRequirement { Name = Constants.ReadOperationName };
		public static OperationAuthorizationRequirement Update =
		  new OperationAuthorizationRequirement { Name = Constants.UpdateOperationName };
		public static OperationAuthorizationRequirement Delete =
		  new OperationAuthorizationRequirement { Name = Constants.DeleteOperationName };
		public static OperationAuthorizationRequirement IsAdmin =
		  new OperationAuthorizationRequirement { Name = Constants.IsAdminOperationName };
		public static OperationAuthorizationRequirement IsPhotographer =
		  new OperationAuthorizationRequirement { Name = Constants.IsPhotographerOperationName };
		public static OperationAuthorizationRequirement IsCustomer =
		  new OperationAuthorizationRequirement { Name = Constants.IsCustomerOperationName };
	}

	public class Constants
	{
		public static readonly string CreateOperationName = "Create";
		public static readonly string ReadOperationName = "Read";
		public static readonly string UpdateOperationName = "Update";
		public static readonly string DeleteOperationName = "Delete";
		public static readonly string IsAdminOperationName = "IsAdmin";
		public static readonly string IsPhotographerOperationName = "IsPhotographer";
		public static readonly string IsCustomerOperationName = "IsCustomer";

		public static readonly string AdministratorsRole = "Administrator";
		public static readonly string CustomersRole = "Kunde";
		public static readonly string PhotographersRole = "Fotograf";
	}
}
