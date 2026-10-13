using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpaceBourne2ModTool.Core;
using SpaceBourne2ModTool.Models;
using SpaceBourne2ModTool.Services;
using SpaceBourne2ModTool.Utils;

namespace SpaceBourne2ModTool.Forms
{
    public partial class GameAnalyzerForm : Form
    {
        private GameAnalysisReport _lastReport;
        private string _lastOutputDir;
        private CancellationTokenSource _cts;
        private int _logLines;
        private const int MaxLogLines = 2000;
        private DateTime _lastUiUpdate = DateTime.MinValue;

        public GameAnalyzerForm()
        {
            InitializeComponent();
            AppBranding.Apply(this);
            ApplyContrast();
            txtInstall.Text = ConfigService.Game.InstallPath ?? "";
            txtOutput.Text = Path.Combine(ConfigService.ModsOutputPath, "GameAnalysisReports");
            chkPakList.Checked = true;
            progressTotal.Minimum = 0;
            progressTotal.Maximum = 100;
            progressFile.Minimum = 0;
            progressFile.Maximum = 100;
            btnCancel.Enabled = false;
            AppendLog("Ready. Set install path and click Run Analysis.");
            AppendLog("Deep parse uses a hang-safe UnrealPak reader (stdout+stderr drain, cancel, timeouts).");
            AppendLog("Expect status updates per file and progress on both bars during long PAK lists.");
        }

        private void ApplyContrast()
        {
            BackColor = Color.FromArgb(28, 28, 30);
            ForeColor = Color.White;
            txtLog.BackColor = Color.FromArgb(20, 20, 22);
            txtLog.ForeColor = Color.FromArgb(220, 220, 220);
            txtInstall.BackColor = Color.FromArgb(45, 45, 50);
            txtInstall.ForeColor = Color.White;
            txtOutput.BackColor = Color.FromArgb(45, 45, 50);
            txtOutput.ForeColor = Color.White;
            foreach (Control c in Controls)
            {
                if (c is Label) c.ForeColor = Color.FromArgb(230, 230, 235);
                if (c is Button btn)
                {
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.BackColor = Color.FromArgb(50, 50, 58);
                    btn.ForeColor = Color.White;
                    btn.FlatAppearance.BorderColor = Color.FromArgb(90, 90, 100);
                }
                if (c is CheckBox) c.ForeColor = Color.White;
            }
            lblTitle.ForeColor = Color.FromArgb(100, 200, 255);
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        }

        private void btnBrowseInstall_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select SpaceBourne 2 install folder";
                fbd.SelectedPath = txtInstall.Text;
                if (fbd.ShowDialog() == DialogResult.OK)
                    txtInstall.Text = fbd.SelectedPath;
            }
        }

        private void btnBrowseOutput_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Folder for analysis reports";
                fbd.SelectedPath = txtOutput.Text;
                if (fbd.ShowDialog() == DialogResult.OK)
                    txtOutput.Text = fbd.SelectedPath;
            }
        }

        private async void btnRun_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtInstall.Text) || !Directory.Exists(txtInstall.Text))
            {
                MessageBox.Show("Select a valid install folder first.", "Path required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var ct = _cts.Token;

            btnRun.Enabled = false;
            btnCancel.Enabled = true;
            progressTotal.Value = 0;
            progressFile.Value = 0;
            progressTotal.Style = ProgressBarStyle.Continuous;
            progressFile.Style = ProgressBarStyle.Continuous;
            txtLog.Clear();
            _logLines = 0;
            lblStatus.Text = "Starting...";
            lblCurrentFile.Text = "";
            AppendLog("=== Analysis started " + DateTime.Now.ToString("HH:mm:ss") + " ===");

            var analyzer = new GameFileAnalyzer();
            // Unlimited list (0) — hang-proof via dual-stream drain, Cancel, idle + hard timeouts
            analyzer.MaxListLines = 0;
            analyzer.ProgressEveryNLines = 1000;
            analyzer.ProcessIdleTimeoutMs = 180000;
            analyzer.HardTimeoutMs = 3 * 60 * 60 * 1000;

            analyzer.LogLine += msg => SafeUi(() => AppendLog(msg));
            analyzer.ProgressChanged += p => SafeUi(() => ApplyProgress(p));

            string install = txtInstall.Text.Trim();
            string output = txtOutput.Text.Trim();
            bool pakList = chkPakList.Checked;

            try
            {
                GameAnalysisReport report = await Task.Run(() => analyzer.Analyze(install, pakList, ct), ct);
                ct.ThrowIfCancellationRequested();
                _lastReport = report;
                _lastOutputDir = await Task.Run(() => analyzer.WriteReports(report, output), ct);

                AppendLog("");
                AppendLog("=== SUMMARY ===");
                AppendLog("Path valid: " + report.PathLooksValid);
                AppendLog("PAKs: " + report.TotalPakCount + " (" + FileService.FormatSize(report.TotalPakBytes) + ")");
                AppendLog("Lines parsed: " + report.PakListLinesParsed);
                AppendLog("Oodle: " + report.UsesOodle + " | Mount: " + (report.DetectedMountPoint ?? "(n/a)"));
                AppendLog("Mod targets: " + report.EasyModTargets.Count);
                AppendLog("Reports: " + _lastOutputDir);
                AppendLog("  SB2_GameAnalysis_LATEST.json");
                AppendLog("  SB2_ModTargets_LATEST.txt");
                AppendLog("  SB2_Formats_LATEST.txt");
                AppendLog("  SB2_ContentMap_LATEST.txt");
                AppendLog("  SB2_AuthorFeedback_LATEST.txt  ← send this + JSON back to improve the tool");
                AppendLog("  SB2_LooseFiles_LATEST.txt  ← Movies/FMOD on-disk inventory");
                lblStatus.Text = "Complete — reports ready.";
                // Mirror LATEST files into ModsOutput so Content Browser can import depth-3 presets
                try
                {
                    string mirror = ConfigService.ModsOutputPath;
                    if (!string.IsNullOrEmpty(mirror) && Directory.Exists(_lastOutputDir))
                    {
                        FileService.EnsureDirectory(mirror);
                        foreach (string name in new[] {
                            "SB2_ContentMap_LATEST.txt", "SB2_AuthorFeedback_LATEST.txt",
                            "SB2_Formats_LATEST.txt", "SB2_ModTargets_LATEST.txt",
                            "SB2_LooseFiles_LATEST.txt", "SB2_UE4_PakStructure_LATEST.txt",
                            "SB2_GameAnalysis_LATEST.md" })
                        {
                            string src = Path.Combine(_lastOutputDir, name);
                            if (File.Exists(src))
                                File.Copy(src, Path.Combine(mirror, name), true);
                        }
                        AppendLog("Mirrored LATEST reports to " + mirror + " (for Content Browser import).");
                    }
                }
                catch (Exception mx) { AppendLog("Mirror skip: " + mx.Message); }
                progressTotal.Value = 100;
                progressFile.Value = 100;
            }
            catch (OperationCanceledException)
            {
                AppendLog("=== CANCELLED by user ===");
                lblStatus.Text = "Cancelled.";
            }
            catch (Exception ex)
            {
                LoggerService.LogError("Analysis failed", ex);
                AppendLog("ERROR: " + ex.Message);
                if (ex.InnerException != null)
                    AppendLog("  inner: " + ex.InnerException.Message);
                lblStatus.Text = "Failed — see log.";
                MessageBox.Show("Analysis failed:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRun.Enabled = true;
                btnCancel.Enabled = false;
                _cts = null;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                _cts?.Cancel();
                AppendLog("Cancel requested — stopping UnrealPak / scan...");
                lblStatus.Text = "Cancelling...";
            }
            catch { }
        }

        private void ApplyProgress(AnalysisProgress p)
        {
            // Throttle UI updates to ~10/sec to keep UI responsive on huge lists
            if ((DateTime.UtcNow - _lastUiUpdate).TotalMilliseconds < 100
                && p.OverallPercent < 100
                && p.Phase == "PakList")
            {
                // still update bars occasionally
            }
            _lastUiUpdate = DateTime.UtcNow;

            if (p.OverallPercent >= 0 && p.OverallPercent <= 100)
                progressTotal.Value = p.OverallPercent;
            if (p.FilePercent >= 0 && p.FilePercent <= 100)
                progressFile.Value = p.FilePercent;

            lblStatus.Text = string.IsNullOrEmpty(p.Phase)
                ? p.Message
                : ("[" + p.Phase + "] " + p.Message);

            if (!string.IsNullOrEmpty(p.CurrentFile))
                lblCurrentFile.Text = "File: " + p.CurrentFile
                    + (p.FilesTotal > 0 ? "  (" + p.FilesDone + "/" + p.FilesTotal + ")" : "")
                    + (p.LinesProcessed > 0 ? "  lines=" + p.LinesProcessed : "");
        }

        private void SafeUi(Action a)
        {
            try
            {
                if (!IsHandleCreated || IsDisposed) return;
                if (InvokeRequired)
                    BeginInvoke(a);
                else
                    a();
            }
            catch { /* form closed mid-run */ }
        }

        private void AppendLog(string line)
        {
            if (_logLines >= MaxLogLines)
            {
                // trim oldest half to avoid unbounded TextBox growth on day-long runs
                var lines = txtLog.Lines;
                if (lines.Length > MaxLogLines / 2)
                {
                    txtLog.Lines = lines.Skip(lines.Length - MaxLogLines / 2).ToArray();
                    _logLines = txtLog.Lines.Length;
                }
            }
            txtLog.AppendText(line + Environment.NewLine);
            _logLines++;
        }

        private void btnViewReports_Click(object sender, EventArgs e)
        {
            using (var f = new AnalysisReportViewerForm(_lastOutputDir))
                f.ShowDialog(this);
        }

        private void btnWorkflow_Click(object sender, EventArgs e)
        {
            using (var f = new ModWorkflowWizardForm())
                f.ShowDialog(this);
        }

        private void btnOpenOutput_Click(object sender, EventArgs e)
        {
            string dir = _lastOutputDir ?? txtOutput.Text;
            if (Directory.Exists(dir))
                ProcessHelper.StartProcess("explorer.exe", "\"" + dir + "\"");
            else
                MessageBox.Show("Output folder does not exist yet. Run analysis first.", "Open folder", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (_cts != null)
            {
                var r = MessageBox.Show("Analysis is running. Cancel and close?", "Close", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (r != DialogResult.Yes) return;
                try { _cts.Cancel(); } catch { }
            }
            Close();
        }
    }
}
