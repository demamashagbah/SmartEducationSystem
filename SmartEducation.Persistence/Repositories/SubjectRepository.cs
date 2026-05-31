using Microsoft.EntityFrameworkCore;
using SmartEducation.Application.Interfaces;
using SmartEducation.Domain.Entities;
using SmartEducation.Persistence.Contexts;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartEducation.Persistence.Repositories
{
    public class SubjectRepository : GenericRepository<Subject>, ISubjectRepository
    {
        public SubjectRepository(ApplicationDbContext context) : base(context)
        {
        }

        // 1. Fetch all subjects assigned to a specific ClassRoom via TeacherAssignment
        public async Task<IEnumerable<Subject>> GetSubjectsByClassRoomIdAsync(Guid classRoomId)
        {
            return await _context.TeacherAssignments
                .AsNoTracking()
                .Where(ta => ta.ClassRoomId == classRoomId && ta.IsDeleted == false)
                .Select(ta => ta.Subject)
                .Where(s => s.IsDeleted == false) // Ensure the subject itself isn't soft-deleted
                .Distinct()
                .ToListAsync();
        }

        // 2. Fetch all subjects taught by a specific Teacher via TeacherAssignment
        public async Task<IEnumerable<Subject>> GetSubjectsByTeacherIdAsync(Guid teacherId)
        {
            return await _context.TeacherAssignments
                .AsNoTracking()
                .Where(ta => ta.TeacherId == teacherId && ta.IsDeleted == false)
                .Select(ta => ta.Subject)
                .Where(s => s.IsDeleted == false) // Ensure the subject itself isn't soft-deleted
                .Distinct()
                .ToListAsync();
        }
    }
}
