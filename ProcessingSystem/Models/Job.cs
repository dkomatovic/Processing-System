using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProcessingSystem.Enums;

namespace ProcessingSystem.Models
{
    public class Job
    {
        public Guid Id { get; set; }
        public JobType Type { get; set; }
        public string Payload { get; set; }
        public int Priority { get; set; }

        public Job(JobType type, string payload, int priority)
        {
            Id = Guid.NewGuid();
            Type = type;
            Payload = payload;
            if (priority > 0)
            {
                Priority = priority;
            } else
            {
                throw new InvalidDataException("Priority cannot be equal to or less than 0");
            }
        }
    }
}
