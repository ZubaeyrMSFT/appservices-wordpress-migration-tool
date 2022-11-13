using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordPressMigrationTool
{
    public class KuduCommandApiResult
    {
        public Status status { get; set; }
        public string output { get; set; }
        public string error { get; set; }
        public int exitCode { get; set; }

        public KuduCommandApiResult(Status status, string output = null, string error = null, int exitCode = -1)
        {
            this.status = status;
            this.output = output;
            this.error = error;
            this.exitCode = exitCode;
        }
    }

    public class KuduCommandApiResponse
    {
        public string Output { get; set; }
        public string Error { get; set; }
        public int ExitCode { get; set; }
    }
}