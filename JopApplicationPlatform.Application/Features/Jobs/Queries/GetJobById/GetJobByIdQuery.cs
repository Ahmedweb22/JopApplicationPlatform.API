using System;
using System.Collections.Generic;
using System.Text;
using JopApplicationPlatform.Application.DTOs.Responses;
using MediatR;

namespace JopApplicationPlatform.Application.Features.Jobs.Queries.GetJobById
{
    public class GetJobByIdQuery : IRequest<JobDto?>
    {
        public int Id { get; set; }
    }
}
