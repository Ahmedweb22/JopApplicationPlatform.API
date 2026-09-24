using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using JopApplicationPlatform.Domain.Enums;

namespace JopApplicationPlatform.Domain.Entities
{
    public class JobApplication
    {
        public int Id { get; set; }
        public int CandidateId { get; set; }
        [ForeignKey(nameof(CandidateId))]
        public Candidate Candidate { get; set; } = null!;
        public int JobId { get; set; }
        [ForeignKey(nameof(JobId))]
        public Job Job { get; set; } = null!;
        public JobApplicationStatus Status { get; set; }
        public DateTime AppliedAt { get; set; }
        public DateTime StatusUpdatedAt { get; set; }
    }
}
