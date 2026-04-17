using ProcessingSystem.Enums;
using ProcessingSystem.Models;
using ProcessingSystem.Services;

namespace ProcessingSystem
{
    internal class Program
    {
        public static List<Job> GetTestJobsWithVariousStatuses()
        {
            return new List<Job>
            {
                // Job 1 - Completed, Prime, visok prioritet
                new Job
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Type = JobType.Prime,
                    Payload = "1000,4",
                    Priority = 1,
                    Attempts = 1,
                    Status = JobStatus.Completed,
                    Duration = 1250.5
                },
                
                // Job 2 - Completed, IO, srednji prioritet
                new Job
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Type = JobType.IO,
                    Payload = "500",
                    Priority = 3,
                    Attempts = 1,
                    Status = JobStatus.Completed,
                    Duration = 500.0
                },
                
                // Job 3 - Failed, Prime, visok prioritet (timeout)
                new Job
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Type = JobType.Prime,
                    Payload = "10000,8",
                    Priority = 2,
                    Attempts = 2,
                    Status = JobStatus.Failed,
                    Duration = 2500.0
                },
                
                // Job 4 - Failed, IO, nizak prioritet
                new Job
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Type = JobType.IO,
                    Payload = "3000",
                    Priority = 5,
                    Attempts = 3,
                    Status = JobStatus.Failed,
                    Duration = 3000.0
                },
                
                // Job 5 - Completed, Prime, najviši prioritet
                new Job
                {
                    Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                    Type = JobType.Prime,
                    Payload = "500,2",
                    Priority = 1,
                    Attempts = 1,
                    Status = JobStatus.Completed,
                    Duration = 350.2
                },
                
                // Job 6 - Aborted, IO, nakon 3 pokušaja
                new Job
                {
                    Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                    Type = JobType.IO,
                    Payload = "5000",
                    Priority = 4,
                    Attempts = 3,
                    Status = JobStatus.Failed,
                    Duration = 0
                },
                
                // Job 7 - Pending, Prime, još nije pokrenut
                new Job
                {
                    Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                    Type = JobType.Prime,
                    Payload = "2000,6",
                    Priority = 2,
                    Attempts = 0,
                    Status = JobStatus.Pending,
                    Duration = 0
                },
                
                // Job 8 - Processing, IO, trenutno se izvršava
                new Job
                {
                    Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                    Type = JobType.IO,
                    Payload = "100",
                    Priority = 3,
                    Attempts = 1,
                    Status = JobStatus.Pending,
                    Duration = 0
                },
                
                // Job 9 - Completed, IO, brzi job
                new Job
                {
                    Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                    Type = JobType.IO,
                    Payload = "50",
                    Priority = 4,
                    Attempts = 1,
                    Status = JobStatus.Completed,
                    Duration = 50.0
                },
                
                // Job 10 - Failed, Prime, exception
                new Job
                {
                    Id = Guid.Parse("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
                    Type = JobType.Prime,
                    Payload = "invalid_payload",
                    Priority = 5,
                    Attempts = 2,
                    Status = JobStatus.Failed,
                    Duration = 100.0
                }
            };
        }
        static async Task Main(string[] args)
        {
            string logFilePath = "C:\\Users\\hp\\Desktop\\events.log";
            string configFilePath = "C:\\Users\\hp\\Downloads\\SystemConfig\\SystemConfig.xml";
            string reportsDirPath = "C:\\Users\\hp\\Desktop\\reports";

            
            object logLock = new object();
            var config = ConfigParser.parseConfig(configFilePath);
            if (config == null) return;

            var system = new ProcessingSystem.Services.ProcessingSystem(
                config.WorkerCount,
                config.MaxQueueSize,
                reportsDirPath);

            system.OnJobCompleted += async (DateTime dateTime, Guid jobId, int result) =>
            {
                lock (logLock)
                {
                    string logLine = $"[{dateTime}] [COMPLETED] JobId: {jobId}, Result: {result}";
                    Console.WriteLine(logLine);
                    File.AppendAllText(logFilePath, logLine + Environment.NewLine);
                }
            };

            system.OnJobFailed += async (DateTime dateTime, Guid jobId, string message) =>
            {
                lock (logLock)
                {
                    string logLine = $"[{dateTime}] [{message}] JobId: {jobId}";
                    Console.WriteLine(logLine);
                    File.AppendAllText(logFilePath, logLine + Environment.NewLine);
                }
            };

            foreach (Job job in config.Jobs)
            {
                system.Submit(job);
            }

            Console.WriteLine("Press enter to exit app...");
            Console.ReadLine();

            await system.ShutdownAsync();

        }

    }
}
