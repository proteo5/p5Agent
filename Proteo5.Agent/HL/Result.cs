using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proteo5.Agent.HL
{
    public class Result
    {
        public Result(string etl = "")
        {
            this.ETL = etl;
            this.State = ResultsStates.running;
            this.JobsLogs = new List<JobLog>();
            this.InitialTime = DateTime.Now;
        }

        public string ETL { get; set; }
        public string State { get; set; }
        public string Message { get; set; }
        public List<JobLog> JobsLogs { get; set; }
        public Exception Exception { get; set; }
        public DateTime InitialTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    public class JobLog
    {
        public JobLog(string job = "")
        {
            this.Job = job;
            this.State = ResultsStates.running;
            this.InitialTime = DateTime.Now;
        }
        public string Job { get; set; }
        public string Message { get; set; }
        public string State { get; set; }
        public Exception Exception { get; set; }
        public DateTime InitialTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    public class ResultsStates
    {
        public const string success = "success";
        public const string unsuccess = "unsuccess";
        public const string running = "running";
        public const string error = "error";
    }
}