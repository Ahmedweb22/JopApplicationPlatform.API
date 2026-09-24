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
        private readonly IRepository<Candidate> _candidateRepository;
        private readonly IRepository<User> _userRepository;

        public GetMyApplicationsHandler(
            IRepository<JobApplication> applicationRepository,
            IRepository<Candidate> candidateRepository,
            IRepository<User> userRepository)
        {
            _applicationRepository = applicationRepository;
            _candidateRepository = candidateRepository;
            _userRepository = userRepository;
        }

        public async Task<List<JobApplicationDto>> Handle(GetMyApplicationsQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetOneAsync(u => u.Id == request.CandidateId);
            var candidate = await _candidateRepository.GetOneAsync(c => c.Id == request.CandidateId || (user != null && c.Email == user.Email));
            int candidateId = candidate?.Id ?? request.CandidateId;

            var applications = await _applicationRepository.GetAsync(a => a.CandidateId == candidateId);
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
