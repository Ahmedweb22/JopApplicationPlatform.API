using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace JopApplicationPlatform.Application.Features.Jobs.Commands.CreateJob
{
    public class CreateJobCommand : IRequest<int>
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }
}
