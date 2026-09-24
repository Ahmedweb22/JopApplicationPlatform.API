using System;
using System.Collections.Generic;
using System.Text;
using JopApplicationPlatform.Application.DTOs.Responses;
using JopApplicationPlatform.Application.Interfaces.IRepositories;
using JopApplicationPlatform.Domain.Entities;
using MediatR;

namespace JopApplicationPlatform.Application.Features.Jobs.Commands.CreateJob
{
    public class CreateJobHandler : IRequestHandler<CreateJobCommand, int>
    {
        private readonly IRepository<Job> _jobRepository;
        public CreateJobHandler(IRepository<Job> jobRepository)
        {
            _jobRepository = jobRepository;
        }
        public async Task<int> Handle(CreateJobCommand request, CancellationToken cancellationToken)
        {
            var job = new Job
            {
                Title = request.Title,
                Description = request.Description,
                IsActive = request.IsActive,
                RecruiterId = request.RecruiterId
            };
            await _jobRepository.CreateAsync(job);
            await _jobRepository.CommitAsync();
            return job.Id;
        }
    }
}
