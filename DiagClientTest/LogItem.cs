using Microsoft.Diagnostics.Tracing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiagClientTest
{
    public class LogItem
    {
        public string Name { get; set; }    
        public TraceEvent Data { get; set; }

        public override string ToString()
        {
            var evtType = Data?.GetType().Name??"";
            var v = $"{Name} {evtType}";
            

            return v;
        }
    }
}
