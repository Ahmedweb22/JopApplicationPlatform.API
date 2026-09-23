using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JopApplicationPlatform.Application.DTOs.Responses;
using JopApplicationPlatform.Application.Interfaces.IRepositories;
using JopApplicationPlatform.Domain.Entities;
using MediatR;

namespace JopApplicationPlatform.Application.Features.Jobs.Queries.GetApplicantsForJob
{
    public class GetApplicantsForJobHandler : IRequestHandler<GetApplicantsForJobQuery, IEnumerable<JobApplicationDto>>
    {
        private readonly IRepository<Job> _jobRepository;
        private readonly IRepository<JobApplication> _applicationRepository;

        public GetApplicantsForJobHandler(IRepository<Job> jobRepository, IRepository<JobApplication> applicationRepository)
        {
            _jobRepository = jobRepository;
            _applicationRepository = applicationRepository;
        }

        public async Task<IEnumerable<JobApplicationDto>> Handle(GetApplicantsForJobQuery request, CancellationToken cancellationToken)
        {
            var job = await _jobRepository.GetOneAsync(j => j.Id == request.JobId);
            if (job == null) throw new Exception("Job not found.");
            if (job.RecruiterId != request.RecruiterId) throw new UnauthorizedAccessException("Only the owning recruiter can view applicants.");

            var applications = await _applicationRepository.GetAsync(a => a.JobId == request.JobId);
            return applications.Select(a => new JobApplicationDto
            {
                Id = a.Id,
                CandidateId = a.CandidateId,
                JobId = a.JobId,
                Status = a.Status,
                AppliedAt = a.AppliedAt,
                StatusUpdatedAt = a.StatusUpdatedAt
            });
        }
    }
}
