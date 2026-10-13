namespace SpaceBourne2ModTool.Forms
{
    partial class AnalysisReportViewerForm
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
            this.lblRoot = new System.Windows.Forms.Label();
            this.txtRoot = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.btnReload = new System.Windows.Forms.Button();
            this.lstReports = new System.Windows.Forms.ListBox();
            this.txtContent = new System.Windows.Forms.TextBox();
            this.lblPath = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.btnFind = new System.Windows.Forms.Button();
            this.btnOpenFolder = new System.Windows.Forms.Button();
            this.btnOpenFile = new System.Windows.Forms.Button();
            this.btnWorkflow = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(12, 8);
            this.lblTitle.Size = new System.Drawing.Size(400, 28);
            this.lblTitle.Text = "Analysis Report Viewer";
            this.lblRoot.AutoSize = true;
            this.lblRoot.Location = new System.Drawing.Point(12, 44);
            this.lblRoot.Text = "Reports folder";
            this.txtRoot.Location = new System.Drawing.Point(110, 40);
            this.txtRoot.Size = new System.Drawing.Size(520, 23);
            this.btnBrowse.Location = new System.Drawing.Point(640, 38);
            this.btnBrowse.Size = new System.Drawing.Size(80, 26);
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            this.btnReload.Location = new System.Drawing.Point(726, 38);
            this.btnReload.Size = new System.Drawing.Size(80, 26);
            this.btnReload.Text = "Reload";
            this.btnReload.Click += new System.EventHandler(this.btnReload_Click);
            this.lstReports.Location = new System.Drawing.Point(12, 72);
            this.lstReports.Size = new System.Drawing.Size(260, 420);
            this.lstReports.SelectedIndexChanged += new System.EventHandler(this.lstReports_SelectedIndexChanged);
            this.txtContent.Location = new System.Drawing.Point(284, 72);
            this.txtContent.Multiline = true;
            this.txtContent.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtContent.Size = new System.Drawing.Size(520, 400);
            this.txtContent.WordWrap = false;
            this.txtContent.ReadOnly = true;
            this.lblPath.Location = new System.Drawing.Point(284, 478);
            this.lblPath.Size = new System.Drawing.Size(520, 20);
            this.lblPath.AutoEllipsis = true;
            this.lblStatus.Location = new System.Drawing.Point(12, 500);
            this.lblStatus.Size = new System.Drawing.Size(260, 20);
            this.txtSearch.Location = new System.Drawing.Point(284, 502);
            this.txtSearch.Size = new System.Drawing.Size(280, 23);
            this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyDown);
            this.btnFind.Location = new System.Drawing.Point(570, 500);
            this.btnFind.Size = new System.Drawing.Size(70, 26);
            this.btnFind.Text = "Find";
            this.btnFind.Click += new System.EventHandler(this.btnFind_Click);
            this.btnOpenFolder.Location = new System.Drawing.Point(12, 530);
            this.btnOpenFolder.Size = new System.Drawing.Size(100, 28);
            this.btnOpenFolder.Text = "Open Folder";
            this.btnOpenFolder.Click += new System.EventHandler(this.btnOpenFolder_Click);
            this.btnOpenFile.Location = new System.Drawing.Point(118, 530);
            this.btnOpenFile.Size = new System.Drawing.Size(100, 28);
            this.btnOpenFile.Text = "Open File";
            this.btnOpenFile.Click += new System.EventHandler(this.btnOpenFile_Click);
            this.btnWorkflow.Location = new System.Drawing.Point(284, 530);
            this.btnWorkflow.Size = new System.Drawing.Size(140, 28);
            this.btnWorkflow.Text = "Mod Workflow…";
            this.btnWorkflow.Click += new System.EventHandler(this.btnWorkflow_Click);
            this.btnClose.Location = new System.Drawing.Point(726, 530);
            this.btnClose.Size = new System.Drawing.Size(80, 28);
            this.btnClose.Text = "Close";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            this.ClientSize = new System.Drawing.Size(820, 572);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblRoot);
            this.Controls.Add(this.txtRoot);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.btnReload);
            this.Controls.Add(this.lstReports);
            this.Controls.Add(this.txtContent);
            this.Controls.Add(this.lblPath);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.btnFind);
            this.Controls.Add(this.btnOpenFolder);
            this.Controls.Add(this.btnOpenFile);
            this.Controls.Add(this.btnWorkflow);
            this.Controls.Add(this.btnClose);
            this.Name = "AnalysisReportViewerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "specSB2modTool — Analysis Reports";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblRoot;
        private System.Windows.Forms.TextBox txtRoot;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnReload;
        private System.Windows.Forms.ListBox lstReports;
        private System.Windows.Forms.TextBox txtContent;
        private System.Windows.Forms.Label lblPath;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Button btnFind;
        private System.Windows.Forms.Button btnOpenFolder;
        private System.Windows.Forms.Button btnOpenFile;
        private System.Windows.Forms.Button btnWorkflow;
        private System.Windows.Forms.Button btnClose;
    }
}
