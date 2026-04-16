using ProcessingSystem.Enums;
using ProcessingSystem.Models;
using System.Xml.Linq;

namespace ProcessingSystem.Services
{
    public static class ConfigParser
    {
        public static Config? parseConfig(string filePath)
        {
            try
            {
                Config config = new Config();
                XDocument document = XDocument.Load(filePath);

                config.WorkerCount = (int)document.Root.Element("WorkerCount");
                config.MaxQueueSize = (int)document.Root.Element("MaxQueueSize");

                config.Jobs = document.Root.Element("Jobs")
                    .Elements("Job")
                    .Select(job => new Job
                    {
                        Type = Enum.Parse<JobType>((string)job.Attribute("Type")),
                        Payload = (string)job.Attribute("Payload"),
                        Priority = (int)job.Attribute("Priority")
                    }).ToList();

                return config;
            }
            catch
            {
                Console.WriteLine("Failed parsing config, returning null");
                return null;
            }
        }


    }
}
