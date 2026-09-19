using System;
using System.Collections.Generic;
using System.Text;
using JopApplicationPlatform.Application.DTOs.Responses;

namespace JopApplicationPlatform.Application.Interfaces.IServices
{
    public interface IJobService
    {
        Task<int> CreateAsync(CreateJobDto createJobDto);
    }
}
