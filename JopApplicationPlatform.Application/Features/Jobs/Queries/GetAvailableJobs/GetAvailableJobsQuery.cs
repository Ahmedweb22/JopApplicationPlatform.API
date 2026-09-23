using System;
using System.Collections.Generic;
using System.Text;
using JopApplicationPlatform.Application.DTOs.Responses;
using MediatR;

namespace JopApplicationPlatform.Application.Features.Jobs.Queries.GetAvailableJobs
{
    public class GetAvailableJobsQuery : IRequest<IEnumerable<JobDto>>
    {

    }
}
