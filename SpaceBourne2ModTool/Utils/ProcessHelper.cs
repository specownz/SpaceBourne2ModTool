using System;
using System.Diagnostics;
using System.IO;

namespace SpaceBourne2ModTool.Utils
{
    public static class ProcessHelper
    {
        public static bool StartProcess(string fileName, string arguments = "", string workingDir = null)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments ?? "",
                    WorkingDirectory = workingDir ?? Path.GetDirectoryName(fileName) ?? "",
                    UseShellExecute = true
                };
                Process.Start(psi);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsGameRunning()
        {
            foreach (var p in Process.GetProcessesByName("SB2-Win64-Shipping"))
            {
                try { if (!p.HasExited) return true; } catch { }
            }
            return false;
        }
    }
}
