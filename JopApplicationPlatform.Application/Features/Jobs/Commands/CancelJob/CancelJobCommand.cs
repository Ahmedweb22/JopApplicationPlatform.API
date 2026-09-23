using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace JopApplicationPlatform.Application.Features.Jobs.Commands.CancelJob
{
    public class CancelJobCommand : IRequest
    {
        public int JobId { get; set; }
        public int RecruiterId { get; set; }
    }
}
