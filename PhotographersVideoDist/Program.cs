using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PhotographersVideoDist.Data;

namespace PhotographersVideoDist
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var host = CreateWebHostBuilder(args).Build();

			using (var scope = host.Services.CreateScope())
			{
				var services = scope.ServiceProvider;
				var context = services.GetRequiredService<ApplicationDbContext>();
				//context.Database.Migrate();

				IConfiguration config = host.Services.GetRequiredService<IConfiguration>();

				SqlConnectionStringBuilder MySqlconnection = new SqlConnectionStringBuilder(
				config.GetConnectionString("PVD_db_Connection"))
				{
					// Set Passwords in system/Enviroment varibles.
					Password = Environment.GetEnvironmentVariable("DbPWD")
				};

				// Seed Postal data.
				SeedDefaultData.SeedData(MySqlconnection.ConnectionString, context);

				try
				{
					// Set Passwords in system/Enviroment varibles.
					SeedUsersAndRoles.SeedData(services, Environment.GetEnvironmentVariable("SeedUserPWD")).Wait();
				}
				catch (Exception ex)
				{
					ILogger logger = services.GetRequiredService<ILogger<Program>>();
					logger.LogError(ex.Message, "An error occurred seeding the DB.");
				}
			}

			host.Run();
		}

		public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.UseWebRoot(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"))
				.UseStartup<Startup>()
				.ConfigureLogging(logging =>
				{
					logging.AddConsole();
				})
				.UseKestrel(options =>
				{
					options.Limits.MaxRequestBodySize = null; // or a given limit
				});
	}
}
