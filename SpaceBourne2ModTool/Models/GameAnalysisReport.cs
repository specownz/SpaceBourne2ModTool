using System;
using System.Collections.Generic;

namespace SpaceBourne2ModTool.Models
{
    public class GameAnalysisReport
    {
        public string ToolVersion { get; set; } = "1.9.0";
        public DateTime AnalyzedAtUtc { get; set; } = DateTime.UtcNow;
        public string InstallPath { get; set; }
        public bool PathLooksValid { get; set; }
        public string DetectedContentPath { get; set; }
        public string DetectedPaksPath { get; set; }
        public string DetectedBinaryPath { get; set; }
        public string BinaryFileVersion { get; set; }
        public string BinaryProductVersion { get; set; }
        public long BinarySizeBytes { get; set; }
        public string SteamAppId { get; set; }
        public string EngineHint { get; set; } = "UE4.27 (expected for SpaceBourne 2)";

        public List<PakFileInfo> PakFiles { get; set; } = new List<PakFileInfo>();
        public List<string> IoStoreFiles { get; set; } = new List<string>();
        public List<string> TopLevelInstallEntries { get; set; } = new List<string>();
        public List<string> ContentSubfolders { get; set; } = new List<string>();
        public List<string> LocalizationLanguages { get; set; } = new List<string>();
        public List<string> ConfigFiles { get; set; } = new List<string>();
        public List<string> NotableFiles { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public List<string> Suggestions { get; set; } = new List<string>();
        public Dictionary<string, List<string>> PakListSamples { get; set; } = new Dictionary<string, List<string>>();

        public string DetectedMountPoint { get; set; }
        public Dictionary<string, int> CompressionCounts { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ExtensionCounts { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> PathPrefixCounts { get; set; } = new Dictionary<string, int>();
        public List<string> SampleGameAssetPaths { get; set; } = new List<string>();
        public List<string> SampleEnginePaths { get; set; } = new List<string>();
        public List<string> LocresPathsFound { get; set; } = new List<string>();
        public List<string> ConfigPathsInPak { get; set; } = new List<string>();
        public List<string> EasyModTargets { get; set; } = new List<string>();
        public int PakListLinesParsed { get; set; }
        public bool UsesOodle { get; set; }
        public bool UsesSingleMonolithicPak { get; set; }

        public long TotalPakBytes { get; set; }
        public int TotalPakCount { get; set; }
        public bool HasIoStore { get; set; }
        public bool HasLooseContentBeyondPaks { get; set; }
        public Dictionary<string, string> ExtraMetadata { get; set; } = new Dictionary<string, string>();

        // v2.0 — external tool cross-ref + deeper insight
        public string ToolProbeSummary { get; set; }
        public List<string> ToolWorkflowRecommendations { get; set; } = new List<string>();
        public List<string> ToolCrossRefNotes { get; set; } = new List<string>();
        public List<string> PluginPathsSampled { get; set; } = new List<string>();
        public List<string> UmapPathsSampled { get; set; } = new List<string>();
        public List<string> AssetTrioHealthNotes { get; set; } = new List<string>();
        public List<string> Ue427PracticeNotes { get; set; } = new List<string>();
        public int UassetCount { get; set; }
        public int UexpCount { get; set; }
        public int UbulkCount { get; set; }
        public int UmapCount { get; set; }
        public int PluginCount { get; set; }
        public int IniCount { get; set; }
        public int LocresCount { get; set; }
    }

    public class PakFileInfo
    {
        public string FileName { get; set; }
        public string FullPath { get; set; }
        public long SizeBytes { get; set; }
        public string SizeHuman { get; set; }
        public DateTime LastWriteUtc { get; set; }
        public bool NameEndsWith_P { get; set; }
        public bool LooksLikeVanilla { get; set; }
        public bool LooksLikeMod { get; set; }
        public string Notes { get; set; }
    }
}
