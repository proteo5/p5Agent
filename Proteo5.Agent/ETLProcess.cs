using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proteo5.HL;
using System.IO;
using Proteo5.Agent.Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Result = Proteo5.Agent.HL.Result;
using JobLog = Proteo5.Agent.HL.JobLog;
using System.Diagnostics;
using System.Data.SqlClient;
using RepoDb;
using IdGen;

namespace Proteo5.Agent
{
    internal static class ETLProcess
    {
        internal static List<string> Items = new List<string>();
        private static IConfigurationRoot Configuration { get; set; }
        private static EnvironmentData envControl { get; set; }
        private static string _connString { get; set; }

        static ETLProcess()
        {
            //Get configuration;
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            Configuration = builder.Build();
            //Get Enviroment information;
            envControl = EnvironmentsHL.GetEnviroment("control");
            _connString = GetConnString(envControl);
        }

        internal static void LoadItems()
        {
            //Get all the files 
            var path = $"{Environment.CurrentDirectory}\\ETLs";
            Items = Directory.GetFiles(path, "*.json").ToList();
        }

        internal static ETL GetETL(string file)
        {
            //Get the file contents and create an object from Json.
            string jsonString = File.ReadAllText(file);
            ETL etl = JsonConvert.DeserializeObject<ETL>(jsonString);
            return etl;
        }

        internal static void FindNeedToRun()
        {
            List<string> ItemToRemove = new List<string>();
            foreach (var item in Items)
            {
                //Get content of ETL File
                var etl = GetETL(item);

                //Check if the etl is enable
                if (!etl.Enable)
                {
                    ItemToRemove.Add(item);
                    continue;
                }

                //Look for ETL on control
                var etlControl = GetETLControl(etl.Name);
                if (etlControl.Status == "idle")
                {
                    bool mustRun = true;
                    if (etlControl.StartRun.HasValue)
                    {
                        switch (etl.Run.Frequency)
                        {
                            case "every day":
                                TimeSpan ts = TimeSpan.Parse(etl.Run.Time);
                                mustRun = DateTime.Now > etlControl.StartRun.Value.Date.AddDays(1).Add(ts);
                                break;
                            default: break;
                        }
                    }
                    if (!mustRun) ItemToRemove.Add(item);
                }
            }
            //Remove the items not needed to run
            ItemToRemove.ForEach(r => Items.Remove(r));
        }

        internal static void QueueETLs()
        {
            //List<string> ItemTo = new List<string>();
            foreach (var item in Items)
            {
                //Get content of ETL File
                var etl = GetETL(item);
                SetStatus(etl.Name, "queue", false, false);
            }
        }

        internal static Result RunETL(string item, ILogger<Worker> logger)
        {
            logger.LogInformation($"{DateTime.Now:u} Run ETL {item}");
            var result = new Result();
            try
            {
                //Get content of ETL File
                var etl = GetETL(item);
                result.ETL = etl.Name;

                SetStatus(etl.Name, "Running", true, false);

                foreach (var job in etl.Jobs.OrderBy(f => f.Order))
                {
                    logger.LogInformation($"{DateTime.Now:u} Starting Job {job.Job_Name}");
                    JobLog jobLog = new JobLog(job.Job_Name);
                    try
                    {
                        switch (job.Type)
                        {
                            case "run_process": RunProcess(job, logger); break;
                            case "run_query": RunQuery(job, logger); break;
                            default:
                                break;
                        }
                        logger.LogInformation($"{DateTime.Now:u} The job \"{job.Job_Name}\" execution was successful");
                        jobLog.State = ResultsStates.success;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"{DateTime.Now:u} The job have an exception: {ex.Message}.");
                        jobLog.Exception = ex;
                        jobLog.State = ResultsStates.error;
                    }

                    jobLog.EndTime = DateTime.Now;
                    result.JobsLogs.Add(jobLog);

                    if (jobLog.State == ResultsStates.error && job.On_Error_Halt )
                    {
                        logger.LogError($"{DateTime.Now:u} The ETL execution was halt.");
                        break;
                    }
                }
                result.State = result.JobsLogs.Any(f => f.State != ResultsStates.success) ? ResultsStates.unsuccess : ResultsStates.success;
                logger.LogInformation($"{DateTime.Now:u} The ETL \"{etl.Name}\" run {result.State}.");
                SetStatus(etl.Name, "idle", false, true);
            }
            catch (Exception ex)
            {
                result.Exception = ex;
                result.State = ResultsStates.error;
                logger.LogError($"{DateTime.Now:u} The ETL File \"{item}\" have an exception: {ex.Message}");
            }
            result.EndTime = DateTime.Now;
            return result;
        }

        public static void RunProcess(ETLJob eTLJob, ILogger<Worker> logger)
        {

            //https://codeburst.io/run-an-external-executable-in-asp-net-core-5c2f8b6cacd9
            using (var process = new Process())
            {
                string path = $"{Environment.CurrentDirectory}\\Processes\\{eTLJob.Process_Name}";
                if (!File.Exists(path))
                {
                    logger.LogError($"{DateTime.Now} Process File {path} don't exist.");
                    return;
                }
                process.StartInfo.FileName = path;

                if (!string.IsNullOrWhiteSpace(eTLJob.Arguments))
                    process.StartInfo.Arguments = eTLJob.Arguments;

                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

                process.OutputDataReceived += (sender, data) => logger.LogInformation(data.Data);
                process.ErrorDataReceived += (sender, data) => logger.LogInformation(data.Data);

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
            }
        }

        public static void RunQuery(ETLJob eTLJob, ILogger<Worker> logger)
        {
            var enviroment = EnvironmentsHL.GetEnviroment(eTLJob.Environment);
            var connString = GetConnString(enviroment);
            using var connection = new SqlConnection(connString);
            connection.ExecuteNonQuery(eTLJob.Query);
        }

        #region ETL Control
        public static ETLControl GetETLControl(string etlName)
        {
            using var connection = new SqlConnection(_connString);
            var result = connection.Query<ETLControl>(f => f.EtlName == etlName);

            //If found Just return it
            if (result.Any())
            {
                var row = result.FirstOrDefault();
                return row;
            }

            //If not found Insert new item
            var generator = new IdGenerator(0);
            var id = generator.CreateId();
            var etlControl = new ETLControl()
            {
                EtlName = etlName,
                StartRun = null,
                FinishRun = null,
                Status = "idle",
                EtlId = Proteo5.HL.ShortCodes.LongToShortCode(id),
                Id = id
            };
            connection.Insert(etlControl);
            return etlControl;
        }

        public static void SetStatus(string etlName, string status, bool setStart, bool setFinish)
        {
            using var connection = new SqlConnection(_connString);
            var row = GetETLControl(etlName);

            row.Status = status;
            if (setStart) row.StartRun = (DateTime?)DateTime.Now;
            if (setFinish) row.FinishRun = (DateTime?)DateTime.Now;
            connection.Update(row);
            return;
        }

        #endregion //ETL Control

        private static string GetConnString(EnvironmentData envControl) => 
            $"Data Source={envControl.host};Initial Catalog={envControl.database};User Id={envControl.user};Password={envControl.password}";

    }
}
