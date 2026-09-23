using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace JopApplicationPlatform.Application.Features.Applications.Commands.CancelApplication
{
    public class CancelApplicationCommand : IRequest
    {
        public int ApplicationId { get; set; }
        public int CandidateId { get; set; }
    }
}
