using ProcessingSystem.Enums;
using ProcessingSystem.Models;

namespace ProcessingSystem.Services
{

    public delegate void JobCompleted(DateTime dateTime, Guid jobId, int result);
    public delegate void JobFailed(DateTime dateTime, Guid jobId, string message);
    public class ProcessingSystem
    {
        private readonly int _maxQueueSize;
        private readonly int _workerCount;

        private readonly object _lock = new object();
        private readonly PriorityQueue<Job, int> _jobs = new PriorityQueue<Job, int>();
        private readonly Dictionary<Guid, Job> _knownJobs = new Dictionary<Guid, Job>();

        private readonly Dictionary<Guid, TaskCompletionSource<int>> _jobResults = new Dictionary<Guid, TaskCompletionSource<int>>();

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly List<Task> _workers = new List<Task>();

        // Reports
        private readonly List<Job> _pastJobs = new List<Job>();
        private readonly ReportGenerator _reportGenerator;
        private readonly Timer _reportTimer;
        private readonly object _pastJobsLock = new object();
        private bool _isGeneratingReport = false;


        public event JobCompleted OnJobCompleted;
        public event JobFailed OnJobFailed;

        public ProcessingSystem(int workerCount, int maxQueueSize, string reportsDirPath)
        {
            _maxQueueSize = maxQueueSize;
            _workerCount = workerCount;

            for (int i = 0; i < _workerCount; i++)
            {
                _workers.Add(Task.Run(() => ProcessJob(_cts.Token)));
            }

            _reportGenerator = new ReportGenerator(reportsDirPath);
            _reportTimer = new Timer(
                callback: async _ => await GenerateReportAsync(),
                state: null,
                dueTime: TimeSpan.FromSeconds(3),
                period: TimeSpan.FromMinutes(3)
            );
        }

        public JobHandle Submit(Job job)
        {
            lock (_lock)
            {
                if (_knownJobs.Keys.Contains(job.Id))
                    throw new ArgumentException("Cannot submint duplicate job");

                if (_jobs.Count >= _maxQueueSize)
                    throw new ArgumentOutOfRangeException("Queue is full");

                _jobs.Enqueue(job, job.Priority);
                _knownJobs[job.Id] = job;

                var tcs = new TaskCompletionSource<int>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

                _jobResults[job.Id] = tcs;
                return new JobHandle(job.Id, tcs.Task);
            }
        }

        private async Task ProcessJob(CancellationToken token)
        {
            try
            {
                while (true)
                {
                    if (token.IsCancellationRequested)
                        break;

                    Job job = null;
                    TaskCompletionSource<int> tcs = null;

                    lock (_lock)
                    {
                        if (_jobs.Count > 0)
                        {
                            job = _jobs.Dequeue();
                            _jobResults.TryGetValue(job.Id, out tcs);
                            _jobResults.Remove(job.Id);
                        }
                    }

                    if (job == null)
                    {
                        try
                        {
                            await Task.Delay(100, token);
                        }
                        catch (TaskCanceledException)
                        {
                            break;
                        }

                        continue;
                    }

                    var sw = System.Diagnostics.Stopwatch.StartNew();

                    int result = -1;
                    bool success = false;

                    for (int attempt = 1; attempt <= 3; attempt++)
                    {
                        try
                        {
                            var task = job.Type switch
                            {
                                JobType.Prime => ProcessPrime(job.Payload),
                                JobType.IO => ProcessIO(job.Payload),
                                _ => Task.FromResult(-1)
                            };

                            var delayTask = Task.Delay(2000, token);

                            var completed = await Task.WhenAny(task, delayTask);

                            if (completed == delayTask)
                                throw new TimeoutException();

                            result = await task;
                            success = true;
                            break;
                        }
                        catch
                        {
                            if (attempt == 3)
                            {
                                job.Status = JobStatus.Failed;
                                OnJobFailed?.Invoke(DateTime.Now, job.Id, "ABORT");
                                result = -1;
                            }
                            else
                            {
                                job.Status = JobStatus.Failed;
                                OnJobFailed?.Invoke(DateTime.Now, job.Id, "RETRY");
                            }
                        }
                    }

                    sw.Stop();

                    lock (_pastJobsLock)
                    {
                        _pastJobs.Add(job);
                    }

                    tcs?.SetResult(result);

                    if (success)
                    {
                        job.Status = JobStatus.Completed;
                        job.Duration = sw.ElapsedMilliseconds / 1000;
                        OnJobCompleted?.Invoke(DateTime.Now, job.Id, result);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                
            }
        }


        private Task<int> ProcessPrime(string payload)
        {
            string[] parts = payload.Split(",");
            int numbers = int.Parse(parts[0].Split(":")[1].Replace("_", ""));
            int threads = int.Parse(parts[1].Split(":")[1]);
            if (threads > 8) threads = 8;

            return Task.Run(() =>
            {
                var range = numbers / threads;
                var tasks = new List<Task<int>>();

                for (int i = 0; i < threads; i++)
                {
                    int start = i * range + 1;
                    int end = (i == threads - 1) ? numbers : (i + 1) * range;

                    tasks.Add(Task.Run(() => CountPrimes(start, end)));
                }

                Task.WaitAll(tasks.ToArray());
                return tasks.Sum(t => t.Result);
            });
        }

        private int CountPrimes(int start, int end)
        {
            int i = start;
            int count = 0;
            while (i < end)
            {
                if (IsPrime(i)) count++;
                i++;
            }

            return count;
        }

        private bool IsPrime(int num)
        {
            if (num < 2) return false;
            if (num == 2) return true;
            if (num % 2 == 0) return false;

            int limit = (int)Math.Sqrt(num);
            for (int i = 3; i <= limit; i += 2)
            {
                if (num % i == 0) return false;
            }
            return true;
        }

        private async Task<int> ProcessIO(string payload)
        {
            int sleepFor = int.Parse(payload.Split(":")[1].Replace("_", ""));

            await Task.Delay(sleepFor);
            return Random.Shared.Next(0, 101);
        }

        private async Task GenerateReportAsync()
        {
            // don't generate a new report if the last one isn't finised generating
            if (_isGeneratingReport)
            {
                Console.WriteLine("Report generation already in progress, skipping...");
                return;
            }

            _isGeneratingReport = true;

            try
            {
                List<Job> pastJobsSnapshot;
                lock (_pastJobsLock)
                {
                    pastJobsSnapshot = _pastJobs.ToList();
                }

                if (!pastJobsSnapshot.Any()) return;


                await Task.Run(() => _reportGenerator.CreateReportFile(pastJobsSnapshot));
                Console.WriteLine($"Report generated successfully at {DateTime.Now}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating report: {ex.Message}");
            }
            finally
            {
                _isGeneratingReport = false;
            }
        }

        public IEnumerable<Job> GetTopNJobs(int n)
        {
            lock (_lock)
            {
                return _jobs.UnorderedItems
                    .Select(job => job.Element)
                    .OrderBy(job => job.Priority)
                    .Take(n)
                    .ToList();
            }
        }

        public Job? GetJob(Guid jobId)
        {
            if (_knownJobs.ContainsKey(jobId)) return _knownJobs[jobId];
            else return null;
        }

        public async Task ShutdownAsync()
        {
            _cts.Cancel();
            await Task.WhenAll(_workers);
        }


    }
}
