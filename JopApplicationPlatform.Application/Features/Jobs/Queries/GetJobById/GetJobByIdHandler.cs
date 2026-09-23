using System;
using System.Collections.Generic;
using System.Text;
using JopApplicationPlatform.Application.DTOs.Responses;
using JopApplicationPlatform.Application.Interfaces.IRepositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace JopApplicationPlatform.Application.Features.Jobs.Queries.GetJobById
{
    public class GetJobByIdHandler : IRequestHandler<GetJobByIdQuery, JobDto>
    {
        private readonly IRepository<Domain.Entities.Job> _jobRepository;
        
        public GetJobByIdHandler(IRepository<Domain.Entities.Job> jobRepository)
        {
            _jobRepository = jobRepository;
        }

        public async Task<JobDto> Handle(GetJobByIdQuery request, CancellationToken cancellationToken)
        {
            var job = await _jobRepository.GetOneAsync(j => j.Id == request.Id);
            if (job == null) return null;

            return new JobDto
            {
                Id = job.Id,
                Title = job.Title,
                Description = job.Description,
                IsActive = job.IsActive,
                RecruiterId = job.RecruiterId
            };
        }
    }
}
