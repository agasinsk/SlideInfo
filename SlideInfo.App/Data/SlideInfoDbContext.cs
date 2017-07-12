using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SlideInfo.App.Models;

namespace SlideInfo.App.Data
{
    public class SlideInfoDbContext : IdentityDbContext<AppUser>
    {
		public DbSet<AppUser> AppUsers { get; set; }
	    public DbSet<Comment> Comments { get; set; }
	    public DbSet<Slide> Slides { get; set; }

	    public SlideInfoDbContext(DbContextOptions options)
		    : base(options)
	    {
	    }

	    protected override void OnModelCreating(ModelBuilder builder)
	    {
		    base.OnModelCreating(builder);
		    // Customize the ASP.NET Identity model and override the defaults if needed.
		    // For example, you can rename the ASP.NET Identity table names and more.
		    // Add your customizations after calling base.OnModelCreating(builder);
	    }
	}
}
