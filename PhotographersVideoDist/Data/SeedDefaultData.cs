using PhotographersVideoDist.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotographersVideoDist.Data
{
	public static class SeedDefaultData
	{
		public static async Task SeedDatabaseAsync(ApplicationDbContext context)
		{
			if (context.Postals.Any())
			{
				return;     // Status table are seeded...
			}


			// Create some bulk insert from csv document.
			var postals = new List<Postal>();

			await context.Postals.AddRangeAsync(postals);
			await context.SaveChangesAsync();
		}
	}
}
