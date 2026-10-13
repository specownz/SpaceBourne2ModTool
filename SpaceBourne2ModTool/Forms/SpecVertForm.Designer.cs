namespace SpaceBourne2ModTool.Forms
{
    partial class SpecVertForm
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblHelp = new System.Windows.Forms.Label();
            this.lblReports = new System.Windows.Forms.Label();
            this.txtReports = new System.Windows.Forms.TextBox();
            this.btnBrowseReports = new System.Windows.Forms.Button();
            this.lblWs = new System.Windows.Forms.Label();
            this.txtWorkspace = new System.Windows.Forms.TextBox();
            this.btnBrowseWs = new System.Windows.Forms.Button();
            this.btnRun = new System.Windows.Forms.Button();
            this.btnOpenWs = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(12, 10);
            this.lblTitle.Size = new System.Drawing.Size(400, 28);
            this.lblTitle.Text = "specVert Workspace Builder";
            this.lblHelp.Location = new System.Drawing.Point(12, 44);
            this.lblHelp.Size = new System.Drawing.Size(640, 52);
            this.lblHelp.Text = "Builds a modular Modules/ tree from Game Analyzer reports (Armor, Dialogs, Quests, Ships, …). Does not modify the game install or recompile the shipping EXE. Re-run after analysis when the game updates.";
            this.lblReports.Location = new System.Drawing.Point(12, 104);
            this.lblReports.Text = "Analysis reports folder";
            this.lblReports.AutoSize = true;
            this.txtReports.Location = new System.Drawing.Point(12, 122);
            this.txtReports.Size = new System.Drawing.Size(500, 23);
            this.btnBrowseReports.Location = new System.Drawing.Point(520, 120);
            this.btnBrowseReports.Size = new System.Drawing.Size(80, 26);
            this.btnBrowseReports.Text = "Browse";
            this.btnBrowseReports.Click += new System.EventHandler(this.btnBrowseReports_Click);
            this.lblWs.Location = new System.Drawing.Point(12, 158);
            this.lblWs.Text = "Workspace output";
            this.lblWs.AutoSize = true;
            this.txtWorkspace.Location = new System.Drawing.Point(12, 176);
            this.txtWorkspace.Size = new System.Drawing.Size(500, 23);
            this.btnBrowseWs.Location = new System.Drawing.Point(520, 174);
            this.btnBrowseWs.Size = new System.Drawing.Size(80, 26);
            this.btnBrowseWs.Text = "Browse";
            this.btnBrowseWs.Click += new System.EventHandler(this.btnBrowseWs_Click);
            this.btnRun.Location = new System.Drawing.Point(12, 216);
            this.btnRun.Size = new System.Drawing.Size(140, 32);
            this.btnRun.Text = "Build specVert";
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            this.btnOpenWs.Location = new System.Drawing.Point(160, 216);
            this.btnOpenWs.Size = new System.Drawing.Size(120, 32);
            this.btnOpenWs.Text = "Open Workspace";
            this.btnOpenWs.Click += new System.EventHandler(this.btnOpenWs_Click);
            this.btnClose.Location = new System.Drawing.Point(520, 216);
            this.btnClose.Size = new System.Drawing.Size(80, 32);
            this.btnClose.Text = "Close";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            this.txtLog.Location = new System.Drawing.Point(12, 264);
            this.txtLog.Multiline = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(588, 200);
            this.txtLog.ReadOnly = true;
            this.ClientSize = new System.Drawing.Size(620, 484);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblHelp);
            this.Controls.Add(this.lblReports);
            this.Controls.Add(this.txtReports);
            this.Controls.Add(this.btnBrowseReports);
            this.Controls.Add(this.lblWs);
            this.Controls.Add(this.txtWorkspace);
            this.Controls.Add(this.btnBrowseWs);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.btnOpenWs);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.txtLog);
            this.Name = "SpecVertForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "specSB2modTool — specVert";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblHelp;
        private System.Windows.Forms.Label lblReports;
        private System.Windows.Forms.TextBox txtReports;
        private System.Windows.Forms.Button btnBrowseReports;
        private System.Windows.Forms.Label lblWs;
        private System.Windows.Forms.TextBox txtWorkspace;
        private System.Windows.Forms.Button btnBrowseWs;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Button btnOpenWs;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.TextBox txtLog;
    }
}
