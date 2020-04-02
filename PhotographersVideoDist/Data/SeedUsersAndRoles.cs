﻿using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotographersVideoDist.Data
{
	public class SeedUsersAndRoles
	{
		public static async Task SeedData(IServiceProvider serviceProvider, string adminUserPWD)
		{
			// Initializing Role and Usermanager 
			var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
			var UserManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

			// Seed Data Async.
			await SeedRoles(RoleManager);
			await SeedUsers(UserManager, adminUserPWD);
		}


		// Seed User Roles...
		private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
		{
			// Create array of roles to add...
			string[] roleNames = { "Administrator", "Fotograf", "Kunde" };

			// Initialize IdentityResult...
			IdentityResult roleResult;

			// Loop through roles and add...
			foreach (var roleName in roleNames)
			{
				var roleExist = await roleManager.RoleExistsAsync(roleName);

				if (!roleExist)
				{
					//create the roles and seed them to the database: Question 1
					roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
				}
			}
		}


		// Seed Default Users...
		private static async Task SeedUsers(UserManager<IdentityUser> userManager, string adminUserPWD)
		{
			// Create List of admin users to add...
			var administrators = new IdentityUser[]
			{
				new IdentityUser
				{
					UserName = "Lasse",
					Email = "brandmandlasse@gmail.com",
					EmailConfirmed = true
				},
			};

			// Loop through the users and add them...
			foreach (IdentityUser user in administrators)
			{
				// Ensure you have these values in your appsettings.json file...
				var _user = await userManager.FindByEmailAsync(user.Email);

				// Check users not already exist...
				if (_user == null)
				{
					var createUser = await userManager.CreateAsync(user, adminUserPWD);
					if (createUser.Succeeded)
					{
						// here we tie the new user to the Administrator role...
						await userManager.AddToRoleAsync(user, "Administrator");
					}
				}
			}

			// Create default operator users...
			var customers = new IdentityUser[]
			{
				new IdentityUser
				{
					UserName = "Kunde",
					Email = "kunde@fixitmedia.dk",
					EmailConfirmed = true
				}
			};

			// Loop through operators and add them...
			foreach (IdentityUser user in customers)
			{
				// Ensure you have these values in your appsettings.json file...
				var _user = await userManager.FindByEmailAsync(user.Email);

				// Check users not already exist...
				if (_user == null)
				{
					var createUser = await userManager.CreateAsync(user, adminUserPWD);
					if (createUser.Succeeded)
					{
						// here we tie the new user to the Administrator role...
						await userManager.AddToRoleAsync(user, "Kunde");
					}
				}
			}


			// Create default requestor users...
			var photographers = new IdentityUser[]
			{
				new IdentityUser
				{
					UserName = "Fotograf",
					Email = "fotograf@fixitmedia.dk",
					EmailConfirmed = true,
				},
				new IdentityUser
				{
					UserName = "Michael",
					Email = "michael@fixitmedia.dk",
					EmailConfirmed = true,
				}
			};

			// Loop through requestors and add them...
			foreach (IdentityUser user in photographers)
			{
				// Ensure you have these values in your appsettings.json file...
				var _user = await userManager.FindByEmailAsync(user.Email);

				// Check users not already exist...
				if (_user == null)
				{
					var createUser = await userManager.CreateAsync(user, adminUserPWD);
					if (createUser.Succeeded)
					{
						// here we tie the new user to the Administrator role...
						await userManager.AddToRoleAsync(user, "Fotograf");
					}
				}
			}
		}
	}
}
