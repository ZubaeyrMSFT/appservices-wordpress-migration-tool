using System;

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
        public string message { get; set; }

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
