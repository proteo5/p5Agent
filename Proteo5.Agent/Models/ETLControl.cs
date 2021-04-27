using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proteo5.Agent.Models
{
    public class ETLControl
    {
        public long Id { get; set; }
        public string EtlName { get; set; }
        public string EtlId { get; set; }
        public DateTime? StartRun { get; set; }
        public DateTime? FinishRun { get; set; }
        public string Status { get; set; }
    }
}
