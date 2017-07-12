using System;
using SlideInfo.App.Data;
using SlideInfo.App.Models;

namespace SlideInfo.App.Repositories
{
	public class UnitOfWork : IDisposable
	{
		private readonly SlideInfoDbContext context = new SlideInfoDbContext(null);
		private Repository<Slide> slideRepository;
		private Repository<Comment> commentRepository;
		private Repository<AppUser> userRepository;

		public Repository<Slide> SlideRepository
		{
			get
			{
				if (slideRepository != null) return slideRepository;
				slideRepository = new Repository<Slide>(context);
				return slideRepository;
			}
		}

		public Repository<Comment> CourseRepository
		{
			get
			{
				if (commentRepository != null) return commentRepository;
				commentRepository = new Repository<Comment>(context);
				return commentRepository;
			}
		}

		public Repository<AppUser> UserRepository
		{
			get
			{
				if (userRepository != null) return userRepository;
				userRepository = new Repository<AppUser>(context);
				return userRepository;
			}
		}

		public void Save()
		{
			context.SaveChanges();
		}

		private bool disposed;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					context.Dispose();
				}
			}
			disposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}