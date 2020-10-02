using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using PhotographersVideoDist.Models;
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
			var UserManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

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
		private static async Task SeedUsers(UserManager<ApplicationUser> userManager, string adminUserPWD)
		{
			// Create List of admin users to add...
			var administrators = new ApplicationUser[]
			{
				new ApplicationUser
				{
					UserName = "Lasse Antonsen",
					Email = "brandmandlasse@gmail.com",
					EmailConfirmed = true
				},
				new ApplicationUser
				{
					UserName = "Kiki Espensen",
					Email = "kiki_15_43@hotmail.com",
					EmailConfirmed = true
				}
			};

			// Loop through the administrators and add them...
			foreach (ApplicationUser user in administrators)
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
						await userManager.AddToRolesAsync(user, new[] { "Administrator", "Fotograf" });
					}
				}
			}

			// Create default customer users...
			var customers = new ApplicationUser[]
			{
				new ApplicationUser
				{
					UserName = "Kunde",
					Email = "kunde@fixitmedia.dk",
					EmailConfirmed = true
				}
			};

			// Loop through customers and add them...
			foreach (ApplicationUser user in customers)
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


			// Create default photographer users...
			var photographers = new ApplicationUser[]
			{
				new ApplicationUser
				{
					UserName = "Micheal Dex Deleurang Pedersen",
					Email = "michael@fixitmedia.dk",
					EmailConfirmed = true,
				},
				new ApplicationUser
				{
					UserName = "Morten Sundgaard",
					Email = "morten@fixitmedia.dk",
					EmailConfirmed = true
				},
				new ApplicationUser
				{
					UserName = "Nicklas Andersson",
					Email = "nicklas@fixitmedia.dk",
					EmailConfirmed = true
				},
				new ApplicationUser
				{
					UserName = "Miki Hedengran",
					Email = "miki@fixitmedia.dk",
					EmailConfirmed = true
				},
				new ApplicationUser
				{
					UserName = "Kenneth Grymen",
					Email = "kenneth@fixitmedia.dk",
					EmailConfirmed = true
				}
			};

			// Loop through photographers and add them...
			foreach (ApplicationUser user in photographers)
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
