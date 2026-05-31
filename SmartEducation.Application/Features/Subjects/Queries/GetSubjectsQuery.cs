using MediatR;
using SmartEducation.Application.DTOs;
using SmartEducation.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;


namespace SmartEducation.Application.Features.Subjects.Queries
{
    public class GetSubjectsQuery : IRequest<IEnumerable<SubjectDto>>
    {
        // This request parameters list remains empty as we want to pull all subjects
    }

    public class GetSubjectsQueryHandler : IRequestHandler<GetSubjectsQuery, IEnumerable<SubjectDto>>
    {
        // Injecting the specific Subject Repository instead of the direct DbContext
        private readonly ISubjectRepository _subjectRepository;

        public GetSubjectsQueryHandler(ISubjectRepository subjectRepository)
        {
            _subjectRepository = subjectRepository;
        }

        public async Task<IEnumerable<SubjectDto>> Handle(GetSubjectsQuery request, CancellationToken cancellationToken)
        {
            // GetAllAsync automatically excludes any soft-deleted subject behind the scenes!
            var subjects = await _subjectRepository.GetAllAsync();

            return subjects.Select(s => new SubjectDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description
            });
        }
    }
}
