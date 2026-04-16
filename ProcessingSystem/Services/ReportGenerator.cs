using ProcessingSystem.Enums;
using ProcessingSystem.Models;
using System.Xml;

namespace ProcessingSystem.Services
{
    public class ReportGenerator
    {
        private readonly string _directoryPath;

        public ReportGenerator(string directoryPath)
        {
            _directoryPath = directoryPath;
        }

        private IEnumerable<object> GetCountByType(List<Job> allJobs)
        {
            return allJobs
                .Where(job => job.Status == JobStatus.Completed)
                .GroupBy(job => job.Type)
                .Select(group => new
                {
                    Type = group.Key,
                    Count = group.Count()
                });
        }

        private IEnumerable<object> GetAverageDurationByType(List<Job> allJobs)
        {
            return allJobs
                .Where(job => job.Status == JobStatus.Completed)
                .GroupBy(job => job.Type)
                .Select(group => new
                {
                    Type = group.Key,
                    Average = group.Average(j => j.Duration)
                });
        }

        private IEnumerable<object> GetFailedJobsByType(List<Job> allJobs)
        {
            return allJobs
                .Where(job => job.Status == JobStatus.Failed)
                .GroupBy(job => job.Type)
                .Select(group => new
                {
                    Type = group.Key,
                    Count = group.Count()
                });
        }

        public void GenerateReportXML(List<Job> allJobs, XmlDocument doc, XmlElement root)
        {
            if (allJobs == null || !allJobs.Any())
            {
                Console.WriteLine("No jobs to generate report from.");
                return;
            }

            try
            {
                var completedByType = GetCountByType(allJobs);
                var avgDurationByType = GetAverageDurationByType(allJobs);
                var failedByType = GetFailedJobsByType(allJobs);

                XmlElement completedReport = doc.CreateElement("Report");
                completedReport.SetAttribute("Name", "Number of completed jobs grouped by Type");
                root.AppendChild(completedReport);

                foreach (var item in completedByType)
                {
                    XmlElement record = doc.CreateElement("Record");

                    XmlElement type = doc.CreateElement("Type");
                    type.InnerText = ((JobType)item.GetType().GetProperty("Type").GetValue(item)).ToString();
                    record.AppendChild(type);

                    XmlElement count = doc.CreateElement("Count");
                    count.InnerText = item.GetType().GetProperty("Count").GetValue(item).ToString();
                    record.AppendChild(count);

                    completedReport.AppendChild(record);
                }

                XmlElement avgReport = doc.CreateElement("Report");
                avgReport.SetAttribute("Name", "Average duration of a completed job grouped by Type");
                root.AppendChild(avgReport);

                foreach (var item in avgDurationByType)
                {
                    XmlElement record = doc.CreateElement("Record");

                    XmlElement type = doc.CreateElement("Type");
                    type.InnerText = ((JobType)item.GetType().GetProperty("Type").GetValue(item)).ToString();
                    record.AppendChild(type);

                    XmlElement average = doc.CreateElement("Average");
                    double avgValue = (double)item.GetType().GetProperty("Average").GetValue(item);
                    average.InnerText = Math.Round(avgValue, 2).ToString();
                    record.AppendChild(average);

                    avgReport.AppendChild(record);
                }

                XmlElement failedReport = doc.CreateElement("Report");
                failedReport.SetAttribute("Name", "Number of failed jobs grouped by Type");
                root.AppendChild(failedReport);

                foreach (var item in failedByType)
                {
                    XmlElement record = doc.CreateElement("Record");

                    XmlElement type = doc.CreateElement("Type");
                    type.InnerText = ((JobType)item.GetType().GetProperty("Type").GetValue(item)).ToString();
                    record.AppendChild(type);

                    XmlElement count = doc.CreateElement("Count");
                    count.InnerText = item.GetType().GetProperty("Count").GetValue(item).ToString();
                    record.AppendChild(count);

                    failedReport.AppendChild(record);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed generating xml structure of the report: {e.Message}");
            }
        }

        public void WriteTestReport(List<Job> allJobs)
        {
            XmlDocument doc = new XmlDocument();

            XmlElement root = doc.CreateElement("Reports");
            doc.AppendChild(root);

            GenerateReportXML(allJobs, doc, root);

            string filePath = Path.Combine(_directoryPath, "test.xml");

            using (var writer = new XmlTextWriter(filePath, System.Text.Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 2;
                doc.Save(writer);
            }

            Console.WriteLine($"Report saved to: {filePath}");
        }
    }
}