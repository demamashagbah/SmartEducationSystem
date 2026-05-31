using SmartEducation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartEducation.Application.Interfaces
{
    public interface ISubjectRepository : IGenericRepository<Subject>
    {
        // 1. Get all subjects assigned to a specific ClassRoom
        Task<IEnumerable<Subject>> GetSubjectsByClassRoomIdAsync(Guid classRoomId);

        // 2. Get all subjects taught by a specific Teacher
        Task<IEnumerable<Subject>> GetSubjectsByTeacherIdAsync(Guid teacherId);
    }
}
