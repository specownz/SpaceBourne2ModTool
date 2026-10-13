using System;

namespace SpaceBourne2ModTool.Models
{
    /// <summary>
    /// Progress payload for long-running game analysis.
    /// overallPercent / filePercent are 0–100; -1 means indeterminate.
    /// </summary>
    public class AnalysisProgress
    {
        public string Phase { get; set; } = "";
        public string Message { get; set; } = "";
        public string CurrentFile { get; set; } = "";
        public int OverallPercent { get; set; } = -1;
        public int FilePercent { get; set; } = -1;
        public long LinesProcessed { get; set; }
        public long BytesHint { get; set; }
        public int FilesDone { get; set; }
        public int FilesTotal { get; set; }
        public DateTime Utc { get; set; } = DateTime.UtcNow;
    }
}
