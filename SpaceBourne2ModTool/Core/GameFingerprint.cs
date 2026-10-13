using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// Snapshot of game package archives so we can detect when an update changed files
    /// and decide whether an open mod needs conversion.
    /// </summary>
    public static class GameFingerprint
    {
        public const string ReportFileName = "SB2_GameFingerprint_LATEST.txt";
        public const string ModStampFileName = "GAME_FINGERPRINT.txt";

        public static string ComputeFromInstall(string installPath)
        {
            if (string.IsNullOrEmpty(installPath) || !Directory.Exists(installPath))
                return null;

            var sb = new StringBuilder();
            string paks = FindPaksDir(installPath);
            if (paks == null || !Directory.Exists(paks))
                return "NO_PAKS|" + installPath;

            foreach (string f in Directory.GetFiles(paks, "*.pak"))
            {
                var fi = new FileInfo(f);
                sb.Append(fi.Name).Append('|').Append(fi.Length).Append('|')
                  .Append(fi.LastWriteTimeUtc.Ticks).Append(';');
            }
            // Include a few loose content markers if present
            foreach (string rel in new[] { "SB2\\Content\\Movies", "Content\\Movies", "SB2\\Content\\FMOD" })
            {
                string d = Path.Combine(installPath, rel);
                if (Directory.Exists(d))
                {
                    int n = 0; long bytes = 0;
                    try
                    {
                        foreach (string x in Directory.GetFiles(d, "*", SearchOption.AllDirectories))
                        { n++; try { bytes += new FileInfo(x).Length; } catch { } }
                    }
                    catch { }
                    sb.Append("DIR:").Append(rel).Append('|').Append(n).Append('|').Append(bytes).Append(';');
                }
            }

            string raw = sb.ToString();
            string hash;
            using (var sha = SHA256.Create())
            {
                byte[] h = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
                var hx = new StringBuilder(h.Length * 2);
                foreach (byte b in h) hx.Append(b.ToString("x2"));
                hash = hx.ToString();
            }
            return "v1|" + hash + "|paks=" + (Directory.GetFiles(paks, "*.pak").Length);
        }

        public static string FindPaksDir(string installPath)
        {
            foreach (string cand in new[]
            {
                Path.Combine(installPath, "SB2", "Content", "Paks"),
                Path.Combine(installPath, "Content", "Paks"),
            })
                if (Directory.Exists(cand)) return cand;
            try
            {
                foreach (string dir in Directory.GetDirectories(installPath))
                {
                    string p = Path.Combine(dir, "Content", "Paks");
                    if (Directory.Exists(p)) return p;
                }
            }
            catch { }
            return null;
        }

        public static string CurrentFromConfig()
        {
            string path = ConfigService.Game != null ? ConfigService.Game.InstallPath : null;
            return ComputeFromInstall(path);
        }

        public static void WriteAnalysisReport(string outputDirectory, string fingerprint)
        {
            if (string.IsNullOrEmpty(outputDirectory)) return;
            Directory.CreateDirectory(outputDirectory);
            var sb = new StringBuilder();
            sb.AppendLine("SpaceBourne 2 game fingerprint");
            sb.AppendLine("GeneratedUtc=" + DateTime.UtcNow.ToString("o"));
            sb.AppendLine("Fingerprint=" + (fingerprint ?? "(none)"));
            sb.AppendLine();
            sb.AppendLine("This value is stored with mods. When the game updates, package archive");
            sb.AppendLine("sizes or dates change and the fingerprint changes. Use");
            sb.AppendLine("\"Convert mod to current game files\" to refresh a mod against a new scan.");
            File.WriteAllText(Path.Combine(outputDirectory, ReportFileName), sb.ToString(), Encoding.UTF8);
            File.WriteAllText(Path.Combine(outputDirectory, "SB2_GameFingerprint_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt"), sb.ToString(), Encoding.UTF8);
        }

        public static string ReadLatestFromReports()
        {
            string f = CrossToolHub.FindLatest(ReportFileName);
            if (f == null) return null;
            try
            {
                foreach (string line in File.ReadLines(f))
                    if (line.StartsWith("Fingerprint=", StringComparison.OrdinalIgnoreCase))
                        return line.Substring("Fingerprint=".Length).Trim();
            }
            catch { }
            return null;
        }

        public static string ReadFromMod(string modRoot)
        {
            if (string.IsNullOrEmpty(modRoot)) return null;
            string p = Path.Combine(modRoot, ModStampFileName);
            if (!File.Exists(p)) return null;
            try
            {
                foreach (string line in File.ReadLines(p))
                    if (line.StartsWith("Fingerprint=", StringComparison.OrdinalIgnoreCase))
                        return line.Substring("Fingerprint=".Length).Trim();
            }
            catch { }
            return null;
        }

        public static void WriteToMod(string modRoot, string fingerprint, string notes)
        {
            if (string.IsNullOrEmpty(modRoot)) return;
            Directory.CreateDirectory(modRoot);
            var sb = new StringBuilder();
            sb.AppendLine("Fingerprint=" + (fingerprint ?? ""));
            sb.AppendLine("SavedUtc=" + DateTime.UtcNow.ToString("o"));
            sb.AppendLine("Notes=" + (notes ?? "").Replace('\n', ' '));
            File.WriteAllText(Path.Combine(modRoot, ModStampFileName), sb.ToString(), Encoding.UTF8);
        }

        public static bool NeedsConversion(string modRoot, out string reason)
        {
            reason = null;
            string current = CurrentFromConfig() ?? ReadLatestFromReports();
            if (string.IsNullOrEmpty(current) || current.StartsWith("NO_PAKS"))
            {
                reason = "No current game fingerprint. Set the game path and run a Game File Scan first.";
                return false;
            }
            string modFp = ReadFromMod(modRoot);
            if (string.IsNullOrEmpty(modFp))
            {
                reason = "This mod has never been stamped with a game fingerprint. Conversion will stamp it and refresh path notes from the latest scan.";
                return true;
            }
            if (string.Equals(modFp, current, StringComparison.Ordinal))
            {
                reason = "Game files match the fingerprint stored with this mod. No conversion needed.";
                return false;
            }
            reason = "The game package archives changed since this mod was stamped (game update or different install). Conversion will refresh targets and path guides.";
            return true;
        }
    }
}
