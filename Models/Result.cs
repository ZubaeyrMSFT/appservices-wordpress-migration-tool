using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordPressMigrationTool
{
    public enum Status {
        Success,
        Failed,
    };

    public class Result
    {
        public Status status { get; set; }
        public String message { get; set; }

        public Result(Status status, string message) {
            this.status = status;
            this.message = message;
        }
    }
}
