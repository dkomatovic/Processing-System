using ProcessingSystem.Enums;
using ProcessingSystem.Models;
using ProcessingSystem.Services;
using ProcessingSystem.Structures;

namespace ProcessingSystem
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Config? config = ConfigParser.parseConfig("C:\\Users\\hp\\Downloads\\SystemConfig\\SystemConfig.xml");
            Console.WriteLine(config);


            List<Job> jobs = new List<Job>
            {
                // Job 1 - Najveći prioritet (1) - Prime tip
                new Job
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Type = JobType.Prime,
                    Payload = "1000,4",  // Do 1000, koristi 4 threada
                    Priority = 1
                },
            
                // Job 2 - Visok prioritet (2) - IO tip
                new Job
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Type = JobType.IO,
                    Payload = "500",  // 500ms sleep
                    Priority = 2
                },
            
                // Job 3 - Srednji prioritet (3) - Prime tip
                new Job
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Type = JobType.Prime,
                    Payload = "500,2",  // Do 500, koristi 2 threada
                    Priority = 3
                },
            
                // Job 4 - Srednji prioritet (3) - IO tip
                new Job
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Type = JobType.IO,
                    Payload = "100",  // 100ms sleep
                    Priority = 3
                },
            
                // Job 5 - Niži prioritet (4) - Prime tip
                new Job
                {
                    Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                    Type = JobType.Prime,
                    Payload = "10000,8",  // Do 10000, koristi 8 threada (max)
                    Priority = 4
                },
            
                // Job 6 - Niži prioritet (4) - IO tip
                new Job
                {
                    Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                    Type = JobType.IO,
                    Payload = "2000",  // 2000ms sleep (granica za timeout)
                    Priority = 4
                },
            
                // Job 7 - Nizak prioritet (5) - Prime tip
                new Job
                {
                    Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                    Type = JobType.Prime,
                    Payload = "100,1",  // Do 100, koristi 1 thread
                    Priority = 5
                },
            
                // Job 8 - Nizak prioritet (5) - IO tip
                new Job
                {
                    Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                    Type = JobType.IO,
                    Payload = "50",  // 50ms sleep
                    Priority = 5
                },
            
                // Job 9 - Najniži prioritet (6) - Prime tip
                new Job
                {
                    Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                    Type = JobType.Prime,
                    Payload = "50,2",  // Do 50, koristi 2 threada
                    Priority = 6
                },
            
                // Job 10 - Najniži prioritet (6) - IO tip (treba da failuje >2000ms)
                new Job
                {
                    Id = Guid.Parse("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
                    Type = JobType.IO,
                    Payload = "3000",  // 3000ms sleep - preko 2s = fail
                    Priority = 6
                }
            };

            JobPriorityQueue queue = new JobPriorityQueue(15);
            try
            {
                foreach (Job j in jobs) queue.Enqueue(j);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            try
            {
                foreach (Job j in jobs) Console.WriteLine(queue.Dequeue());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }


        }

    }
}
