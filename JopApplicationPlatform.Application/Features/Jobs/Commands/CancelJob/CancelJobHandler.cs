using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JopApplicationPlatform.Application.Interfaces.IRepositories;
using JopApplicationPlatform.Domain.Entities;
using MediatR;

namespace JopApplicationPlatform.Application.Features.Jobs.Commands.CancelJob
{
    public class CancelJobHandler : IRequestHandler<CancelJobCommand>
    {
        private readonly IRepository<Job> _jobRepository;

        public CancelJobHandler(IRepository<Job> jobRepository)
        {
            _jobRepository = jobRepository;
        }

        public async Task Handle(CancelJobCommand request, CancellationToken cancellationToken)
        {
            var job = await _jobRepository.GetOneAsync(j => j.Id == request.JobId);
            if (job == null) throw new Exception("Job not found.");
            if (job.RecruiterId != request.RecruiterId) throw new UnauthorizedAccessException("Only the owning recruiter can close the job.");

            job.IsActive = false;
            job.ClosedAt = DateTime.UtcNow;
            job.ClosedBy = request.RecruiterId;

            _jobRepository.Update(job);
            await _jobRepository.CommitAsync();
        }
    }
}
