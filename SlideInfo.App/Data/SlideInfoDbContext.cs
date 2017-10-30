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
        public DbSet<Property> Properties { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Conversation> Conversations { get; set; }

        public SlideInfoDbContext(DbContextOptions options)
		    : base(options)
	    {
	    }

        public SlideInfoDbContext()
        {
        }
    }
}
