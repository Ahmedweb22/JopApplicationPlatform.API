using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace JopApplicationPlatform.Application.Features.Jobs.Commands.CreateJob
{
    public class CreateJobCommand : IRequest<int>
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int RecruiterId { get; set; }
    }
}
