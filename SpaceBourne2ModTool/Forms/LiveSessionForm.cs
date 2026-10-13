using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using SpaceBourne2ModTool.Core;

namespace SpaceBourne2ModTool.Forms
{
    public partial class LiveSessionForm : Form
    {
        string _notePath;

        public LiveSessionForm()
        {
            InitializeComponent();
            AppBranding.Apply(this, AppBranding.ProductTitle + " — Live session observer");
            FormScaleHelper.Enable(this);
            UiTheme.Apply(this);
            txtIntro.Text =
                "This optional tool watches for a UE4SS log file (if you installed that separate program) " +
                "and lets you write notes while you play. It does not inject code into the game process.\r\n\r\n" +
                "Export a merged report to combine your notes with the latest Game File Scan targets. " +
                "That helps improve mod paths after you see what actually appears in-game.";
            _notePath = LiveSessionService.StartSessionNoteFile();
            txtNotes.Text = File.ReadAllText(_notePath);
            RefreshLog();
        }

        void RefreshLog()
        {
            string log = LiveSessionService.FindUe4ssLog();
            lblLogPath.Text = string.IsNullOrEmpty(log)
                ? "UE4SS log: not found (optional — file mods do not need it)"
                : "UE4SS log: " + log;
            txtLog.Text = LiveSessionService.TailLog(log, 80);
        }

        void btnRefresh_Click(object sender, EventArgs e) => RefreshLog();

        void btnSaveNotes_Click(object sender, EventArgs e)
        {
            File.WriteAllText(_notePath, txtNotes.Text);
            MessageBox.Show(this, "Notes saved to:\n" + _notePath);
        }

        void btnExport_Click(object sender, EventArgs e)
        {
            File.WriteAllText(_notePath, txtNotes.Text);
            string path = LiveSessionService.ExportMergedReport(_notePath);
            MessageBox.Show(this, "Merged report written:\n" + path +
                "\n\nShare this with analysis feedback or keep it next to your mod project.",
                "Exported", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        void btnClose_Click(object sender, EventArgs e) => Close();
    }
}
