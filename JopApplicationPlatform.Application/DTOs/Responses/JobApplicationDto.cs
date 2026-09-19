using System;
using System.Collections.Generic;
using System.Text;
using JopApplicationPlatform.Domain.Enums;

namespace JopApplicationPlatform.Application.DTOs.Responses
{
    public class JobApplicationDto
    {
        public int Id { get; set; }
        public int CandidateId { get; set; }
        public int JobId { get; set; }
        public JobApplicationStatus Status { get; set; }
        public DateTime AppliedAt { get; set; }
        public DateTime StatusUpdatedAt { get; set; }
    }
}
