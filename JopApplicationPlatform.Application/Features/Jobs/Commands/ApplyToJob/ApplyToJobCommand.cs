using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace JopApplicationPlatform.Application.Features.Jobs.Commands.ApplyToJob
{
    public class ApplyToJobCommand : IRequest<int>
    {
        public int JobId { get; set; }
        public int CandidateId { get; set; }
    }
}
