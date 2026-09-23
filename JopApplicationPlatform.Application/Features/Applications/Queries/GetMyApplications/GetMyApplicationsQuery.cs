using System;
using System.Collections.Generic;
using System.Text;
using JopApplicationPlatform.Application.DTOs.Responses;
using MediatR;

namespace JopApplicationPlatform.Application.Features.Applications.Queries.GetMyApplications
{
    public class GetMyApplicationsQuery : IRequest<List<JobApplicationDto>>
    {
        public int CandidateId { get; set; }
    }
}
