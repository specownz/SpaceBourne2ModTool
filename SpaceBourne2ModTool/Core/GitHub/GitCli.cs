using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace SpaceBourne2ModTool.Core.GitHub
{
    /// <summary>Thin wrapper around the git command-line tool when installed on PATH.</summary>
    public static class GitCli
    {
        public static string GitPath { get; set; }

        public static bool IsAvailable()
        {
            string p = ResolveGit();
            return !string.IsNullOrEmpty(p);
        }

        public static string ResolveGit()
        {
            if (!string.IsNullOrEmpty(GitPath) && File.Exists(GitPath)) return GitPath;
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "where",
                    Arguments = "git",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                using (var p = Process.Start(psi))
                {
                    string o = p.StandardOutput.ReadToEnd();
                    p.WaitForExit(5000);
                    if (p.ExitCode == 0 && !string.IsNullOrWhiteSpace(o))
                    {
                        string line = o.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
                        if (File.Exists(line)) { GitPath = line; return line; }
                    }
                }
            }
            catch { }
            foreach (string c in new[]
            {
                @"C:\Program Files\Git\cmd\git.exe",
                @"C:\Program Files (x86)\Git\cmd\git.exe"
            })
                if (File.Exists(c)) { GitPath = c; return c; }
            return null;
        }

        public class Result
        {
            public int ExitCode;
            public string StdOut, StdErr;
            public bool Ok { get { return ExitCode == 0; } }
            public string Combined
            {
                get
                {
                    var sb = new StringBuilder();
                    if (!string.IsNullOrEmpty(StdOut)) sb.AppendLine(StdOut.TrimEnd());
                    if (!string.IsNullOrEmpty(StdErr)) sb.AppendLine(StdErr.TrimEnd());
                    return sb.ToString().Trim();
                }
            }
        }

        public static Result Run(string workingDir, string args, int timeoutMs = 120000)
        {
            var r = new Result();
            string git = ResolveGit();
            if (string.IsNullOrEmpty(git))
            {
                r.ExitCode = -1;
                r.StdErr = "Git was not found. Install Git for Windows or use Download ZIP import.";
                return r;
            }
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = git,
                    Arguments = args,
                    WorkingDirectory = string.IsNullOrEmpty(workingDir) ? Environment.CurrentDirectory : workingDir,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                // Prefer HTTPS remote helper environment from user PAT when pushing
                using (var p = Process.Start(psi))
                {
                    r.StdOut = p.StandardOutput.ReadToEnd();
                    r.StdErr = p.StandardError.ReadToEnd();
                    if (!p.WaitForExit(timeoutMs))
                    {
                        try { p.Kill(); } catch { }
                        r.ExitCode = -2;
                        r.StdErr = (r.StdErr ?? "") + "\nGit timed out.";
                        return r;
                    }
                    r.ExitCode = p.ExitCode;
                }
            }
            catch (Exception ex)
            {
                r.ExitCode = -1;
                r.StdErr = ex.Message;
            }
            return r;
        }

        public static bool IsRepo(string dir)
        {
            return Directory.Exists(Path.Combine(dir ?? "", ".git"));
        }

        public static Result Status(string dir) { return Run(dir, "status --short --branch"); }
        public static Result Init(string dir) { return Run(dir, "init -b main"); }
        public static Result AddAll(string dir) { return Run(dir, "add -A"); }
        public static Result Commit(string dir, string message)
        {
            string msg = (message ?? "Update").Replace("\"", "'");
            return Run(dir, "commit -m \"" + msg + "\"");
        }
        public static Result RemoteAdd(string dir, string name, string url)
        {
            return Run(dir, "remote add " + name + " " + Quote(url));
        }
        public static Result RemoteSetUrl(string dir, string name, string url)
        {
            return Run(dir, "remote set-url " + name + " " + Quote(url));
        }
        public static Result Push(string dir, string remote, string branch, bool setUpstream)
        {
            string args = "push " + (setUpstream ? "-u " : "") + remote + " " + branch;
            return Run(dir, args, 300000);
        }
        public static Result Pull(string dir) { return Run(dir, "pull --ff-only", 300000); }
        public static Result Clone(string url, string destDir)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(destDir) ?? ".");
            return Run(Path.GetDirectoryName(destDir), "clone " + Quote(url) + " " + Quote(destDir), 600000);
        }
        public static Result ConfigLocal(string dir, string key, string value)
        {
            return Run(dir, "config " + key + " " + Quote(value));
        }

        static string Quote(string s)
        {
            if (s == null) return "\"\"";
            if (s.IndexOfAny(new[] { ' ', '"', '&' }) >= 0) return "\"" + s.Replace("\"", "\\\"") + "\"";
            return s;
        }

        /// <summary>Embed token in HTTPS URL for push/pull (user:token@host). Prefer this over interactive auth.</summary>
        public static string AuthenticatedHttpsUrl(string httpsCloneUrl, string user, string token)
        {
            if (string.IsNullOrEmpty(httpsCloneUrl) || string.IsNullOrEmpty(token)) return httpsCloneUrl;
            // https://github.com/owner/repo.git
            if (!httpsCloneUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) return httpsCloneUrl;
            string rest = httpsCloneUrl.Substring("https://".Length);
            string u = string.IsNullOrEmpty(user) ? "git" : Uri.EscapeDataString(user);
            string t = Uri.EscapeDataString(token);
            return "https://" + u + ":" + t + "@" + rest;
        }
    }
}
