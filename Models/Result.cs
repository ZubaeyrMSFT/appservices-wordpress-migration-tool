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
}
