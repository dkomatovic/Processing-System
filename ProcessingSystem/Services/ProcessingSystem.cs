using ProcessingSystem.Models;
using ProcessingSystem.Structures;
using System.Collections.Concurrent;

namespace ProcessingSystem.Services
{

    public delegate void JobCompleted(DateTime dateTime, Guid jobId, int result);
    public delegate void JobFailed(DateTime dateTime, Guid jobId);
    public class ProcessingSystem
    {
        private readonly JobPriorityQueue _jobs;
        private SemaphoreSlim _semaphore;

        public event JobCompleted OnJobCompleted;
        public event JobFailed OnJobFailed;

        public ProcessingSystem(int workerCount, int maxQueueSize)
        {
            _jobs = new JobPriorityQueue(maxQueueSize);
            _semaphore = new SemaphoreSlim(workerCount);
        }

        public void TestEvents()
        {
            OnJobFailed.Invoke(DateTime.Now.AddHours(2), Guid.NewGuid());
            OnJobCompleted.Invoke(DateTime.Now, Guid.NewGuid(), 2);
            
        }
    }
}
