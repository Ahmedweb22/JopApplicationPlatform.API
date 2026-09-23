using System;
using System.Collections.Generic;
using System.Text;
using JopApplicationPlatform.Application.DTOs.Responses;
using MediatR;

namespace JopApplicationPlatform.Application.Features.Jobs.Queries.GetApplicantsForJob
{
    public class GetApplicantsForJobQuery : IRequest<IEnumerable<JobApplicationDto>>
    {
        public int JobId { get; set; }
        public int RecruiterId { get; set; }
    }
}
