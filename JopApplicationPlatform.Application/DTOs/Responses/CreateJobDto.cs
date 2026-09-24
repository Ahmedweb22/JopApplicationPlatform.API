using System;
using System.Collections.Generic;
using System.Text;

namespace JopApplicationPlatform.Application.DTOs.Responses
{
    public class CreateJobDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
