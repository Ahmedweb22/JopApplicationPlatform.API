using System.Collections.Generic;
using System.Threading.Tasks;
using JopApplicationPlatform.Application.DTOs.Requestes;
using JopApplicationPlatform.Application.DTOs.Responses;

namespace JopApplicationPlatform.Application.Interfaces.IServices
{
    public interface IApplicationService
    {
        Task<int> ApplyAsync(int candidateId, ApplyForJobDto applyDto);
        Task<List<JobApplicationDto>> GetMyApplicationsAsync(int candidateId);
        Task CancelApplicationAsync(int applicationId, int candidateId);
    }
}
