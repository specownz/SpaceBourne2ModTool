using System;
using System.IO;
using System.Text;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// Aligns an open mod with the current analyzed game install.
    /// Does not invent binary asset rewrites — refreshes path targets, reports links,
    /// and re-stamps the game fingerprint when the install changed.
    /// </summary>
    public static class ModConvertService
    {
        public class Result
        {
            public bool Converted;
            public bool SkippedSame;
            public string Message;
            public string LogPath;
        }

        public static Result ConvertOpenModToCurrentGame(string modRoot)
        {
            var r = new Result();
            if (string.IsNullOrEmpty(modRoot) || !Directory.Exists(modRoot))
            {
                r.Message = "Open a mod project folder first (File → Open Mod Folder).";
                return r;
            }

            string reason;
            bool needs = GameFingerprint.NeedsConversion(modRoot, out reason);
            string current = GameFingerprint.CurrentFromConfig() ?? GameFingerprint.ReadLatestFromReports();

            if (!needs && reason != null && reason.IndexOf("match", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                r.SkippedSame = true;
                r.Message = reason;
                return r;
            }
            if (!needs && reason != null && reason.IndexOf("No current", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                r.Message = reason;
                return r;
            }

            var log = new StringBuilder();
            log.AppendLine("Convert mod to current game files");
            log.AppendLine("Mod: " + modRoot);
            log.AppendLine("Utc: " + DateTime.UtcNow.ToString("o"));
            log.AppendLine("Reason: " + reason);
            log.AppendLine("New fingerprint: " + current);
            log.AppendLine();

            // Refresh TARGET_PATHS from latest ModTargets
            try
            {
                var targets = CrossToolHub.LoadModTargets();
                if (targets.Count > 0)
                {
                    string tp = Path.Combine(modRoot, "TARGET_PATHS.txt");
                    var lines = new StringBuilder();
                    lines.AppendLine("# Refreshed from latest game scan " + DateTime.Now.ToString("g"));
                    foreach (var t in targets)
                    {
                        if (!string.IsNullOrEmpty(t.StagingHint))
                            lines.AppendLine(t.StagingHint);
                        else if (!string.IsNullOrEmpty(t.Name))
                            lines.AppendLine(t.Name);
                    }
                    File.WriteAllText(tp, lines.ToString(), Encoding.UTF8);
                    log.AppendLine("Updated TARGET_PATHS.txt (" + targets.Count + " target lines).");
                }
                else
                    log.AppendLine("No ModTargets report found — run a Game File Scan for richer path refresh.");
            }
            catch (Exception ex) { log.AppendLine("TARGET_PATHS refresh warning: " + ex.Message); }

            // Write conversion guide from latest easy targets
            try
            {
                string guide = Path.Combine(modRoot, "CONVERT_GUIDE.txt");
                var g = new StringBuilder();
                g.AppendLine("After a game update — what to check");
                g.AppendLine("===================================");
                g.AppendLine();
                g.AppendLine("1. Your staged folders still use the same relative paths under SB2/Content.");
                g.AppendLine("2. If an asset was renamed in the update, replace the cooked pair (.uasset + .uexp).");
                g.AppendLine("3. Re-package (Speckage or package archive) and inject again with the Mod Loader.");
                g.AppendLine("4. Movies and FMOD loose files: confirm file names still match the game.");
                g.AppendLine();
                g.AppendLine("Latest scan highlights:");
                string mt = CrossToolHub.FindLatest("SB2_ModTargets_LATEST.txt");
                if (mt != null)
                {
                    int n = 0;
                    foreach (string line in File.ReadLines(mt))
                    {
                        g.AppendLine("  " + line);
                        if (++n >= 40) break;
                    }
                }
                else g.AppendLine("  (no ModTargets report — run Game File Scan)");
                File.WriteAllText(guide, g.ToString(), Encoding.UTF8);
                log.AppendLine("Wrote CONVERT_GUIDE.txt");
            }
            catch (Exception ex) { log.AppendLine("Guide warning: " + ex.Message); }

            // Link analysis folder path
            try
            {
                string reports = CrossToolHub.ReportsRoot;
                File.WriteAllText(Path.Combine(modRoot, "LINKED_ANALYSIS.txt"),
                    "ReportsRoot=" + (reports ?? "") + "\r\nFingerprint=" + current + "\r\n", Encoding.UTF8);
            }
            catch { }

            GameFingerprint.WriteToMod(modRoot, current, "Converted to current game files");
            log.AppendLine("Stamped GAME_FINGERPRINT.txt");

            string logPath = Path.Combine(modRoot, "CONVERT_LOG_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt");
            File.WriteAllText(logPath, log.ToString(), Encoding.UTF8);
            r.Converted = true;
            r.LogPath = logPath;
            r.Message = "Mod was updated for the current game files.\r\n\r\n" + log.ToString();
            return r;
        }
    }
}
