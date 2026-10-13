using System;
using System.IO;

namespace SpaceBourne2ModTool.Models
{
    /// <summary>
    /// Holds detected / configured paths and version info for SpaceBourne 2.
    /// Game root typically: ...\steamapps\common\SpaceBourne 2\
    /// Content root:        ...\SB2\Content\
    /// Paks:                ...\SB2\Content\Paks\
    /// </summary>
    public class GameConfig
    {
        public string InstallPath { get; set; } = "";
        public string ContentPath { get; set; } = "";
        public string PaksPath { get; set; } = "";
        public string BinaryPath { get; set; } = "";   // SB2-Win64-Shipping.exe
        public string EngineVersion { get; set; } = "4.27";
        public bool IsValid { get; private set; }

        public void Validate()
        {
            IsValid = false;
            if (string.IsNullOrWhiteSpace(InstallPath) || !Directory.Exists(InstallPath))
                return;

            // Auto-detect standard layout
            string sb2 = Path.Combine(InstallPath, "SB2");
            if (!Directory.Exists(sb2))
                sb2 = Path.Combine(InstallPath, "SpaceBourne 2", "SB2"); // fallback

            ContentPath = Path.Combine(sb2, "Content");
            PaksPath = Path.Combine(ContentPath, "Paks");
            BinaryPath = Path.Combine(sb2, "Binaries", "Win64", "SB2-Win64-Shipping.exe");

            if (!File.Exists(BinaryPath))
            {
                // Try alternative
                BinaryPath = Path.Combine(InstallPath, "SB2", "Binaries", "Win64", "SB2-Win64-Shipping.exe");
            }

            IsValid = Directory.Exists(ContentPath) && Directory.Exists(PaksPath);
        }

        public string GetLocalizationRoot()
        {
            return Path.Combine(ContentPath, "Localization", "Game");
        }
    }
}
