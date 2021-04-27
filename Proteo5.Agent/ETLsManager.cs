using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proteo5.Agent
{
    internal static class ETLsManager
    {
        internal static void Start(ILogger<Worker> logger)
        {
            logger.LogInformation($"{DateTime.Now:u} Load ETLs");
            ETLProcess.LoadItems();
            logger.LogInformation($"{DateTime.Now:u} Find Need To Run ETLs");
            ETLProcess.FindNeedToRun();
            logger.LogInformation($"{DateTime.Now:u} Queue ETLs");
            ETLProcess.QueueETLs();
            foreach (var item in ETLProcess.Items
                .Where(f => f.Contains("Operations", StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => f))
            {
                var result = ETLProcess.RunETL(item, logger);
            }
            foreach (var item in ETLProcess.Items
                .Where(f => f.Contains("Vault", StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => f))
            {
                var result = ETLProcess.RunETL(item, logger);
            }
            foreach (var item in ETLProcess.Items
                .Where(f => f.Contains("DataMart", StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => f))
            {
                var result = ETLProcess.RunETL(item, logger);
            }
            logger.LogInformation($"{DateTime.Now:u} Finish");
        }

    }
}
