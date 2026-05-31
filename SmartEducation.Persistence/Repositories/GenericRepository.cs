using Microsoft.EntityFrameworkCore;
using SmartEducation.Application.Interfaces;
using SmartEducation.Persistence.Contexts;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartEducation.Persistence.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        
        protected readonly ApplicationDbContext _context;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. GetById should only retrieve the record if it is NOT soft-deleted
        public async Task<T?> GetByIdAsync(Guid id)
        {
            var entity = await _context.Set<T>().FindAsync(id);

            // Using dynamic reflection to check your BaseEntity property 'IsDeleted' safely
            if (entity != null && EF.Property<bool>(entity, "IsDeleted") == true)
            {
                return null;
            }
            return entity;
        }

        // 2. GetAll should filter out any soft-deleted records automatically
        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            return await _context.Set<T>()
                .AsNoTracking()
                .Where(e => EF.Property<bool>(e, "IsDeleted") == false)
                .ToListAsync();
        }

        public async Task<T> AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            return entity;
        }

        // 3. Update should always refresh the 'UpdatedAt' timestamp automatically
        public async Task UpdateAsync(T entity)
        {
            if (_context.Entry(entity).Properties.Any(p => p.Metadata.Name == "UpdatedAt"))
            {
                _context.Entry(entity).Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
            }

            _context.Entry(entity).State = EntityState.Modified;
            await Task.CompletedTask;
        }

        // 4. Delete operation is  perform a Soft Delete instead of a hard removal
        public async Task DeleteAsync(T entity)
        {
            if (_context.Entry(entity).Properties.Any(p => p.Metadata.Name == "IsDeleted"))
            {
                _context.Entry(entity).Property("IsDeleted").CurrentValue = true;
            }

            if (_context.Entry(entity).Properties.Any(p => p.Metadata.Name == "UpdatedAt"))
            {
                _context.Entry(entity).Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
            }

            _context.Entry(entity).State = EntityState.Modified;
            await Task.CompletedTask;
        }
    }
}
