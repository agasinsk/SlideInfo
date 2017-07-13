using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SlideInfo.App.Data;

namespace SlideInfo.App.Repositories
{
	public class AsyncRepository<TEntity> : IRepository<TEntity> where TEntity : class
	{
		internal SlideInfoDbContext Context;
		internal DbSet<TEntity> DbSet;

		public AsyncRepository(SlideInfoDbContext context)
		{
			Context = context;
			DbSet = context.Set<TEntity>();
		}

	    public async Task<List<TEntity>> GetAllAsync()
	    {
            return await DbSet.ToListAsync();
	    }

        public virtual TEntity GetById(object id)
		{
			return DbSet.Find(id);
		}

	    public async Task<TEntity> GetByIdAsync(int id)
	    {
	        return await DbSet.FindAsync(id);
	    }

	    public virtual IEnumerable<TEntity> Get(
	        Expression<Func<TEntity, bool>> filter = null,
	        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
	        string includeProperties = "")
	    {
	        IQueryable<TEntity> query = DbSet;

	        if (filter != null)
	        {
	            query = query.Where(filter);
	        }

	        foreach (var includeProperty in includeProperties.Split
	            (new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
	        {
	            query = query.Include(includeProperty);
	        }

	        if (orderBy != null)
	        {
	            return orderBy(query).ToList();
	        }
	        return query.ToList();
	    }

	    public async Task<IEnumerable<TEntity>> GetAsync(
	        Expression<Func<TEntity, bool>> filter = null,
	        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
	        string includeProperties = "")
	    {
	        IQueryable<TEntity> query = DbSet;

	        if (filter != null)
	        {
	            query = query.Where(filter);
	        }

	        foreach (var includeProperty in includeProperties.Split
	            (new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
	        {
	            query = query.Include(includeProperty);
	        }

	        if (orderBy != null)
	        {
	            return await orderBy(query).ToListAsync();
	        }
	        return await query.ToListAsync();
	    }

        public virtual void Insert(TEntity entity)
		{
			DbSet.Add(entity);
		}

	    public async Task<TEntity> InsertAsync(TEntity entity)
	    {
	        DbSet.Add(entity);
	        await Context.SaveChangesAsync();
	        return entity;
	    }

        public virtual void Delete(object id)
		{
			var entityToDelete = DbSet.Find(id);
			Delete(entityToDelete);
		}

		public virtual void Delete(TEntity entityToDelete)
		{
			if (Context.Entry(entityToDelete).State == EntityState.Detached)
			{
				DbSet.Attach(entityToDelete);
			}
			DbSet.Remove(entityToDelete);
		}

	    public async Task DeleteAsync(object id)
	    {
	        var entityToDelete = DbSet.Find(id);
	        await DeleteAsync(entityToDelete);
	    }

        public async Task DeleteAsync(TEntity entityToDelete)
	    {
	        if (Context.Entry(entityToDelete).State == EntityState.Detached)
	        {
	            DbSet.Attach(entityToDelete);
	            Context.Entry(entityToDelete).State = EntityState.Deleted;
	        }
	        DbSet.Remove(entityToDelete);
	        await Context.SaveChangesAsync();
        }

        public virtual void Update(TEntity entityToUpdate)
		{
			DbSet.Attach(entityToUpdate);
			Context.Entry(entityToUpdate).State = EntityState.Modified;
		}

	    public async Task UpdateAsync(TEntity entityToUpdate)
	    {
            DbSet.Attach(entityToUpdate);
	        Context.Entry(entityToUpdate).State = EntityState.Modified;
	        await Context.SaveChangesAsync();
	    }
    }
}