using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpaceBourne2ModTool.Core;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Forms
{
    /// <summary>
    /// First-run (or manual) splash while auto-detecting game + external tools.
    /// Shows checklist with checkmarks, status, and progress bars so the UI never looks frozen.
    /// </summary>
    public partial class AutoDetectSplashForm : Form
    {
        private readonly Dictionary<string, Label> _checkLabels = new Dictionary<string, Label>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Label> _statusLabels = new Dictionary<string, Label>(StringComparer.OrdinalIgnoreCase);
        private readonly string[] _items =
        {
            "SteamLibraries", "Game", "UnrealPak", "FModel", "UModel", "UAssetGUI", "CheatEngine",
            "CodeEditor", "ImageEditor", "Modeling3d"
        };
        private readonly string[] _itemTitles =
        {
            "Steam libraries", "SpaceBourne 2 install", "UnrealPak.exe (UE 4.27)",
            "FModel.exe", "UModel / UE Viewer", "UAssetGUI.exe", "Cheat Engine",
            "Code editor", "Image editor", "3D modeling tool"
        };

        public ToolAutoDetector.DetectResult Result { get; private set; }
        public bool CompletedOk { get; private set; }

        public AutoDetectSplashForm()
        {
            InitializeComponent();
            AppBranding.Apply(this, AppBranding.ProductTitle + " — Auto-Detect");
            BuildChecklist();
            ApplyTheme();
        }

        private void BuildChecklist()
        {
            panelList.Controls.Clear();
            _checkLabels.Clear();
            _statusLabels.Clear();
            int y = 4;
            for (int i = 0; i < _items.Length; i++)
            {
                var chk = new Label
                {
                    Text = "○",
                    Location = new Point(4, y),
                    Size = new Size(28, 22),
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    ForeColor = Color.Gray
                };
                var title = new Label
                {
                    Text = _itemTitles[i],
                    Location = new Point(36, y + 2),
                    Size = new Size(200, 20),
                    ForeColor = Color.White
                };
                var st = new Label
                {
                    Text = "waiting…",
                    Location = new Point(240, y + 2),
                    Size = new Size(320, 20),
                    ForeColor = Color.FromArgb(180, 180, 190),
                    AutoEllipsis = true
                };
                panelList.Controls.Add(chk);
                panelList.Controls.Add(title);
                panelList.Controls.Add(st);
                _checkLabels[_items[i]] = chk;
                _statusLabels[_items[i]] = st;
                y += 28;
            }
            progressTotal.Maximum = _items.Length;
            progressTotal.Value = 0;
            progressItem.Value = 0;
            progressItem.Style = ProgressBarStyle.Marquee;
            progressItem.MarqueeAnimationSpeed = 30;
        }

        private void ApplyTheme()
        {
            BackColor = Color.FromArgb(22, 22, 26);
            ForeColor = Color.White;
            panelList.BackColor = Color.FromArgb(32, 32, 38);
            lblTitle.ForeColor = Color.FromArgb(120, 200, 255);
            lblSubtitle.ForeColor = Color.FromArgb(200, 200, 210);
            lblStatus.ForeColor = Color.FromArgb(180, 220, 180);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.BackColor = Color.FromArgb(50, 50, 58);
            btnClose.ForeColor = Color.White;
            btnClose.Enabled = false;
        }

        private void AutoDetectSplashForm_Shown(object sender, EventArgs e)
        {
            BeginDetect();
        }

        private async void BeginDetect()
        {
            btnClose.Enabled = false;
            lblStatus.Text = "Searching your PC — this may take a minute. Please wait…";
            progressItem.Style = ProgressBarStyle.Marquee;

            try
            {
                var result = await Task.Run(() =>
                    ToolAutoDetector.DetectAll(
                        progress: msg =>
                        {
                            try
                            {
                                if (IsHandleCreated && !IsDisposed)
                                    BeginInvoke(new Action(() =>
                                    {
                                        lblStatus.Text = msg;
                                    }));
                            }
                            catch { }
                        },
                        itemProgress: (key, found, detail) =>
                        {
                            try
                            {
                                if (IsHandleCreated && !IsDisposed)
                                    BeginInvoke(new Action(() => UpdateItem(key, found, detail)));
                            }
                            catch { }
                        }));

                Result = result;
                ToolAutoDetector.ApplyToConfig(result);
                ConfigService.FirstRunAutoDetectDone = true;
                ConfigService.Save();
                CompletedOk = true;

                progressItem.Style = ProgressBarStyle.Continuous;
                progressItem.Value = 100;
                progressTotal.Value = progressTotal.Maximum;
                lblStatus.Text = "Done. " + (string.IsNullOrEmpty(result.GameInstallPath)
                    ? "Game not found — set path manually in Settings."
                    : "Game: " + result.GameInstallPath);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Auto-detect error: " + ex.Message;
                LoggerService.LogError("AutoDetect splash failed", ex);
            }
            finally
            {
                btnClose.Enabled = true;
                btnClose.Text = "Continue";
                progressItem.Style = ProgressBarStyle.Continuous;
            }
        }

        private void UpdateItem(string key, bool found, string detail)
        {
            if (!_checkLabels.ContainsKey(key)) return;
            var chk = _checkLabels[key];
            var st = _statusLabels[key];
            if (found)
            {
                chk.Text = "✓";
                chk.ForeColor = Color.FromArgb(80, 220, 120);
                st.Text = ShortPath(detail);
                st.ForeColor = Color.FromArgb(160, 230, 180);
progressTotal.Value = Math.Min(progressTotal.Maximum, CountFound());
            }
            else
            {
                if (!string.IsNullOrEmpty(detail) && detail != "Searching..." && detail != "Scanning..." && detail != "waiting…")
                {
                    chk.Text = "✗";
                    chk.ForeColor = Color.FromArgb(220, 120, 100);
                    st.Text = detail;
                    st.ForeColor = Color.FromArgb(220, 160, 140);
                }
                else
                {
                    chk.Text = "…";
                    chk.ForeColor = Color.FromArgb(255, 200, 80);
                    st.Text = detail;
                }
            }
        }

        private int CountFound()
        {
            int n = 0;
            foreach (var kv in _checkLabels)
                if (kv.Value.Text == "✓") n++;
            return n;
        }

        private static string ShortPath(string p)
        {
            if (string.IsNullOrEmpty(p)) return "";
            if (p.Length <= 54) return p;
            return "…" + p.Substring(p.Length - 52);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
