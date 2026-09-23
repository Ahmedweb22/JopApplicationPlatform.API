using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JopApplicationPlatform.Application.DTOs.Responses;
using JopApplicationPlatform.Application.Interfaces.IRepositories;
using JopApplicationPlatform.Domain.Entities;
using MediatR;

namespace JopApplicationPlatform.Application.Features.Applications.Queries.GetMyApplications
{
    public class GetMyApplicationsHandler : IRequestHandler<GetMyApplicationsQuery, List<JobApplicationDto>>
    {
        private readonly IRepository<JobApplication> _applicationRepository;

        public GetMyApplicationsHandler(IRepository<JobApplication> applicationRepository)
        {
            _applicationRepository = applicationRepository;
        }

        public async Task<List<JobApplicationDto>> Handle(GetMyApplicationsQuery request, CancellationToken cancellationToken)
        {
            var applications = await _applicationRepository.GetAsync(a => a.CandidateId == request.CandidateId);
            return applications.Select(a => new JobApplicationDto
            {
                Id = a.Id,
                CandidateId = a.CandidateId,
                JobId = a.JobId,
                Status = a.Status,
                AppliedAt = a.AppliedAt,
                StatusUpdatedAt = a.StatusUpdatedAt
            }).ToList();
        }
    }
}
