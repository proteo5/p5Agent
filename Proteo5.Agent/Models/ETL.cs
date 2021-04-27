using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proteo5.Agent.Models
{
    public class ETL
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Id { get; set; }
        public ETLRun Run { get; set; }
        public List<ETLJob> Jobs { get; set; }
        public bool Enable { get; set; }
    }

    public class ETLRun
    {
        public string Frequency { get; set; }
        public string Time { get; set; }
    }

    public class ETLJob
    {
        //General Job Properties
        public string Job_Name { get; set; }
        public string Type { get; set; }
        public int Order { get; set; }
        public bool Enable { get; set; }
        public bool On_Error_Halt { get; set; }
        //Process job Properties
        public string Arguments { get; set; }
        public string Process_Name { get; set; }
        //Query job Properties
        public string Query { get; set; }
        public string Environment { get; set; }
        //Copy Table job properties
        //Not yet implemented
        //public string FromDB { get; set; }
        //public string ToDB { get; set; }
        //public string Table { get; set; }
        //public string PrimaryKey { get; set; }
        //public List<string> Fields { get; set; }

    }
}
