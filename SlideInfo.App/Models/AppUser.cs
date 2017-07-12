using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace SlideInfo.App.Models
{
	public class AppUser : IdentityUser
	{
		public string FirstMidName { get; set; }
		public string LastName { get; set; }

		public DateTime LastActive { get; set; }

		public virtual ICollection<Comment> Comments { get; set; }
	}
}
