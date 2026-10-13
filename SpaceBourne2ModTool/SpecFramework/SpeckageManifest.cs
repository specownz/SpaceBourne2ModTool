using System;
using System.Collections.Generic;

namespace SpecFramework
{
    /// <summary>Speckage label stored inside a mod package.</summary>
    public class SpeckageManifest
    {
        public string Format { get; set; } = "speckage_sb2_v1";
        public string StyleVersion { get; set; } = "speckage_v2";
        public string Name { get; set; } = "UntitledMod";
        public string Author { get; set; } = "";
        public string Version { get; set; } = "1.0.0";
        public string Description { get; set; } = "";
        public string GameId { get; set; } = "1646850"; // SpaceBourne 2 Steam
        public string GameName { get; set; } = "SpaceBourne 2";
        public int DefaultLoadOrder { get; set; } = 100;
        public string MountPoint { get; set; } = "../../../";
        public string PakFileName { get; set; } = "";
        public List<string> Files { get; set; } = new List<string>();
        public List<string> DeployedPakFiles { get; set; } = new List<string>();
        public List<string> LooseRelativePaths { get; set; } = new List<string>();
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public string Notes { get; set; } = "Packaged with specSB2modTool.";
    }
}
