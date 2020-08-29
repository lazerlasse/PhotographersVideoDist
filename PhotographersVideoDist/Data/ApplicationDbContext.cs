using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PhotographersVideoDist.Models;

namespace PhotographersVideoDist.Data
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}

		public DbSet<Case> Cases { get; set; }
		public DbSet<Postal> Postals { get; set; }
		public DbSet<ImageAssets> ImageAssets { get; set; }
		public DbSet<VideoAssets> VideoAssets { get; set; }
	}
}
