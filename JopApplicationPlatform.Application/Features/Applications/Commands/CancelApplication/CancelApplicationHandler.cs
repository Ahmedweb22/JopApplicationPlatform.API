using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JopApplicationPlatform.Application.Interfaces.IRepositories;
using JopApplicationPlatform.Domain.Entities;
using JopApplicationPlatform.Domain.Enums;
using MediatR;

namespace JopApplicationPlatform.Application.Features.Applications.Commands.CancelApplication
{
    public class CancelApplicationHandler : IRequestHandler<CancelApplicationCommand>
    {
        private readonly IRepository<JobApplication> _applicationRepository;

        public CancelApplicationHandler(IRepository<JobApplication> applicationRepository)
        {
            _applicationRepository = applicationRepository;
        }

        public async Task Handle(CancelApplicationCommand request, CancellationToken cancellationToken)
        {
            var application = await _applicationRepository.GetOneAsync(a => a.Id == request.ApplicationId);
            if (application == null) throw new Exception("Application not found.");
            if (application.CandidateId != request.CandidateId) throw new UnauthorizedAccessException("Only the owning candidate can cancel this application.");
            
            application.Status = JobApplicationStatus.Cancelled;
            application.StatusUpdatedAt = DateTime.UtcNow;

            _applicationRepository.Update(application);
            await _applicationRepository.CommitAsync();
        }
    }
}
