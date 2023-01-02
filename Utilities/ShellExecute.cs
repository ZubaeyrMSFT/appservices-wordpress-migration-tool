using System;
using System.Diagnostics;


namespace WordPressMigrationTool.Utilities
{
    public class ShellExecute
    {
        public static async Task Login(string command)
        {
            Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = "cmd.exe";
            proc.StartInfo.Arguments = "/C \" " + command + " \"";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            proc.Start();
            proc.WaitForExit();
        }
    }
}

