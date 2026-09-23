using System;
using System.Collections.Generic;
using System.Text;
using JopApplicationPlatform.Application.DTOs.Responses;
using JopApplicationPlatform.Application.Interfaces.IRepositories;
using MediatR;

namespace JopApplicationPlatform.Application.Features.Jobs.Queries.GetAvailableJobs
{
    public class GetAvailableJobsHandler : IRequestHandler<GetAvailableJobsQuery, IEnumerable<JobDto>>
    {  
        private readonly IRepository<Domain.Entities.Job> _jobRepository;
        public GetAvailableJobsHandler(IRepository<Domain.Entities.Job> jobRepository)
        {
            _jobRepository = jobRepository;
        }
        public async Task<IEnumerable<JobDto>> Handle(GetAvailableJobsQuery request, CancellationToken cancellationToken)
        {
            var jobs = await _jobRepository.GetAsync(j => j.IsActive);
            return jobs.Select(j => new JobDto
            {
                Id = j.Id,
                Title = j.Title,
                Description = j.Description,
                IsActive = j.IsActive,
                RecruiterId = j.RecruiterId
            });
        }
    }
}
