using System;
using System.Collections.Generic;
using System.Text;

namespace JopApplicationPlatform.Application.DTOs.Responses
{
    public class CreateJobDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }
}
