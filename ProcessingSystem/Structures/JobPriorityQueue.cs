using ProcessingSystem.Models;

namespace ProcessingSystem.Structures
{
    public class JobPriorityQueue
    {
        private readonly PriorityQueue<Job, int> _queue = new PriorityQueue<Job, int>();
        private readonly object _lock = new object();
        private int _capacity;

        public JobPriorityQueue(int capacity)
        {
            if (capacity <= 0) 
                throw new ArgumentException("Queue capacity cannot be equal to or less than 0");

            _capacity = capacity;
        }

        public void Enqueue(Job job)
        {
            if (job == null) throw new ArgumentNullException("Queue cannot contain null elements");

            lock (_lock)
            {
                if (_queue.Count == _capacity)
                    throw new ArgumentOutOfRangeException($"Queue is at capacity ({_capacity})");
                _queue.Enqueue(job, job.Priority);
            }
        }

        public Job? Dequeue()
        {
            lock (_lock)
            {
                if (_queue.Count > 0)
                    return _queue.Dequeue();
                
                return null;
            }
        }

        public List<Job> GetTopJobs(int n)
        {
            if (n <= 0)
                return new List<Job>();

            lock (_lock)
            {
                var result = new List<Job>();
                var tempItems = new List<(Job job, int priority)>();

                try
                {
                    while (_queue.Count > 0 && result.Count < n)
                    {
                        var job = _queue.Dequeue();
                        result.Add(job);
                        tempItems.Add((job, job.Priority));
                    }
                }
                finally
                {
                    foreach (var item in tempItems)
                    {
                        _queue.Enqueue(item.job, item.priority);
                    }
                }

                return result;
            }
        }

        public Job? GetJob(Guid id)
        {
            lock (_lock)
            {
                var tempItems = new List<(Job job, int priority)>();
                Job? foundJob = null;

                try
                {
                    while (_queue.Count > 0)
                    {
                        var job = _queue.Dequeue();
                        if (job.Id == id)
                            foundJob = job;

                        tempItems.Add((job, job.Priority));
                    }
                }
                finally
                {
                    foreach (var item in tempItems)
                    {
                        _queue.Enqueue(item.job, item.priority);
                    }
                }

                return foundJob;
            }
        }
    }
}
