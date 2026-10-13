using System;
using System.IO;
using System.Text;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// Optional live session observer: tails UE4SS.log when present and records user notes.
    /// Does not inject into the game process. Exports merge with Game File Analyzer insights.
    /// </summary>
    public static class LiveSessionService
    {
        public static string SessionsRoot
        {
            get
            {
                string d = Path.Combine(ConfigService.ModsOutputPath ?? "", "LiveSessions");
                Directory.CreateDirectory(d);
                return d;
            }
        }

        public static string FindUe4ssLog()
        {
            string win64 = Ue4ssProjectService.TryFindGameWin64();
            if (string.IsNullOrEmpty(win64)) return null;
            foreach (string cand in new[]
            {
                Path.Combine(win64, "UE4SS.log"),
                Path.Combine(win64, "ue4ss", "UE4SS.log"),
                Path.Combine(win64, "UE4SS", "UE4SS.log"),
            })
                if (File.Exists(cand)) return cand;
            return null;
        }

        public static string StartSessionNoteFile()
        {
            string path = Path.Combine(SessionsRoot, "session_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt");
            var sb = new StringBuilder();
            sb.AppendLine("Live session log (specSB2modTool)");
            sb.AppendLine("StartedUtc=" + DateTime.UtcNow.ToString("o"));
            sb.AppendLine("Ue4ssLog=" + (FindUe4ssLog() ?? "(not found — install UE4SS optionally for richer logs)"));
            sb.AppendLine();
            sb.AppendLine("Write observations below (what you saw in-game, quest names, bugs, balance notes).");
            sb.AppendLine("These notes can be merged with Game File Analyzer reports to improve targets.");
            sb.AppendLine("-----");
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            return path;
        }

        public static string TailLog(string logPath, int maxLines)
        {
            if (string.IsNullOrEmpty(logPath) || !File.Exists(logPath)) return "(no log file)";
            try
            {
                var lines = File.ReadAllLines(logPath);
                int start = Math.Max(0, lines.Length - maxLines);
                var sb = new StringBuilder();
                for (int i = start; i < lines.Length; i++) sb.AppendLine(lines[i]);
                return sb.ToString();
            }
            catch (Exception ex) { return "Could not read log: " + ex.Message; }
        }

        public static string ExportMergedReport(string sessionNotesPath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Merged live session + analysis hints");
            sb.AppendLine("Utc=" + DateTime.UtcNow.ToString("o"));
            sb.AppendLine();
            sb.AppendLine("=== Session notes ===");
            if (File.Exists(sessionNotesPath)) sb.AppendLine(File.ReadAllText(sessionNotesPath));
            sb.AppendLine();
            sb.AppendLine("=== Latest ModTargets (from Game File Scan) ===");
            string mt = CrossToolHub.FindLatest("SB2_ModTargets_LATEST.txt");
            if (mt != null)
            {
                int n = 0;
                foreach (string line in File.ReadLines(mt))
                {
                    sb.AppendLine(line);
                    if (++n >= 80) break;
                }
            }
            else sb.AppendLine("(run a Game File Scan first)");
            sb.AppendLine();
            sb.AppendLine("=== UE4SS log tail ===");
            sb.AppendLine(TailLog(FindUe4ssLog(), 100));
            string outp = Path.Combine(SessionsRoot, "MERGED_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt");
            File.WriteAllText(outp, sb.ToString(), Encoding.UTF8);
            // Also copy as LATEST for CrossToolHub consumers
            try { File.Copy(outp, Path.Combine(SessionsRoot, "SB2_LiveSession_LATEST.txt"), true); } catch { }
            return outp;
        }
    }
}
