using ProcessingSystem.Enums;

namespace ProcessingSystem.Models
{
    public class Job
    {
        public Guid Id { get; set; }
        public JobType Type { get; set; }
        public string Payload { get; set; }
        public int Priority { get; set; }
        public int Attempts { get; set; }
        public JobStatus Status { get; set; }

        public Job()
        {
            Id = Guid.NewGuid();
            Attempts = 0;
            Status = JobStatus.Pending;
        }

        public override string ToString()
        {
            return $"Type: {Type.ToString()}, Payload: {Payload}, Priority: {Priority}";
        }
    }
}
