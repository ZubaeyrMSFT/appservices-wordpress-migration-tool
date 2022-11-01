using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordPressMigrationTool
{
    public enum Status {
        Completed,
        Failed,
        Cancelled
    };

    public class Result
    {
        public Status status { get; set; }
        public String message { get; set; }

        public Result(Status status, string message) {
            this.status = status;
            this.message = message;
        }

        public override string ToString()
        {
            return "[status=" + this.status + " , message=" + this.message + "]";
        }
    }

    public class KuduCommandApiResult {
        public Status status { get; set; }
        public string output { get; set; }
        public string error { get; set; }
        public int exitCode { get; set; }

        public Result(Status status, string output = "", string error = "", int exitCode = 0) {
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
