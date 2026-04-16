namespace ProcessingSystem.Models
{
    public class Config
    {
        public int WorkerCount { get; set; }
        public int MaxQueueSize { get; set; }

        public List<Job> Jobs = new List<Job>();

        public override string ToString()
        {
            string jobsInfo = Jobs.Count == 0
                ? "none"
                : $"\n  {string.Join("\n  ", Jobs.Select((j, i) => $"[{i}] {j}"))}";

            return $"WorkerCount: {WorkerCount}\n" +
                   $"MaxQueueSize: {MaxQueueSize}\n" +
                   $"Jobs ({Jobs.Count}): {jobsInfo}";
        }

    }
}
