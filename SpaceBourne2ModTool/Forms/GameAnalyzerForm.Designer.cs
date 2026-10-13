namespace SpaceBourne2ModTool.Forms
{
    partial class GameAnalyzerForm
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
            this.lblInstall = new System.Windows.Forms.Label();
            this.txtInstall = new System.Windows.Forms.TextBox();
            this.btnBrowseInstall = new System.Windows.Forms.Button();
            this.lblOutput = new System.Windows.Forms.Label();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.btnBrowseOutput = new System.Windows.Forms.Button();
            this.chkPakList = new System.Windows.Forms.CheckBox();
            this.btnRun = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOpenOutput = new System.Windows.Forms.Button();
            this.btnViewReports = new System.Windows.Forms.Button();
            this.btnWorkflow = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblTotal = new System.Windows.Forms.Label();
            this.progressTotal = new System.Windows.Forms.ProgressBar();
            this.lblFile = new System.Windows.Forms.Label();
            this.progressFile = new System.Windows.Forms.ProgressBar();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblCurrentFile = new System.Windows.Forms.Label();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            //
            this.lblTitle.AutoSize = true;
            this.lblTitle.Location = new System.Drawing.Point(16, 12);
            this.lblTitle.Text = "Game File Analyzer";
            //
            this.lblHelp.Location = new System.Drawing.Point(16, 42);
            this.lblHelp.Size = new System.Drawing.Size(720, 40);
            this.lblHelp.Text = "Hang-safe deep scan: inventory, loose FMOD/Movies, UnrealPak -List with dual-stream drain, cancel, and timeouts. Builds category stats (textures, materials, meshes, blueprints, audio, locres) and format/conversion guidance.";
            //
            this.lblInstall.AutoSize = true;
            this.lblInstall.Location = new System.Drawing.Point(16, 90);
            this.lblInstall.Text = "Game install path";
            this.txtInstall.Location = new System.Drawing.Point(16, 108);
            this.txtInstall.Size = new System.Drawing.Size(600, 22);
            this.btnBrowseInstall.Location = new System.Drawing.Point(624, 106);
            this.btnBrowseInstall.Size = new System.Drawing.Size(100, 26);
            this.btnBrowseInstall.Text = "Browse...";
            this.btnBrowseInstall.Click += new System.EventHandler(this.btnBrowseInstall_Click);
            //
            this.lblOutput.AutoSize = true;
            this.lblOutput.Location = new System.Drawing.Point(16, 140);
            this.lblOutput.Text = "Report output folder";
            this.txtOutput.Location = new System.Drawing.Point(16, 158);
            this.txtOutput.Size = new System.Drawing.Size(600, 22);
            this.btnBrowseOutput.Location = new System.Drawing.Point(624, 156);
            this.btnBrowseOutput.Size = new System.Drawing.Size(100, 26);
            this.btnBrowseOutput.Text = "Browse...";
            this.btnBrowseOutput.Click += new System.EventHandler(this.btnBrowseOutput_Click);
            //
            this.chkPakList.AutoSize = true;
            this.chkPakList.Location = new System.Drawing.Point(16, 190);
            this.chkPakList.Text = "Deep parse UnrealPak -List (UNLIMITED lines; hang-safe drain + Cancel; folder/category maps for mod design)";
            //
            this.btnRun.Location = new System.Drawing.Point(16, 222);
            this.btnRun.Size = new System.Drawing.Size(130, 32);
            this.btnRun.Text = "Run Analysis";
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            //
            this.btnCancel.Location = new System.Drawing.Point(155, 222);
            this.btnCancel.Size = new System.Drawing.Size(100, 32);
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            //
            this.btnOpenOutput.Location = new System.Drawing.Point(265, 222);
            this.btnViewReports.Location = new System.Drawing.Point(385, 222);
            this.btnViewReports.Name = "btnViewReports";
            this.btnViewReports.Size = new System.Drawing.Size(120, 28);
            this.btnViewReports.Text = "View Reports";
            this.btnViewReports.Click += new System.EventHandler(this.btnViewReports_Click);
            this.btnWorkflow.Location = new System.Drawing.Point(515, 222);
            this.btnWorkflow.Name = "btnWorkflow";
            this.btnWorkflow.Size = new System.Drawing.Size(120, 28);
            this.btnWorkflow.Text = "Workflow…";
            this.btnWorkflow.Click += new System.EventHandler(this.btnWorkflow_Click);

            this.btnOpenOutput.Size = new System.Drawing.Size(140, 32);
            this.btnOpenOutput.Text = "Open Report Folder";
            this.btnOpenOutput.Click += new System.EventHandler(this.btnOpenOutput_Click);
            //
            this.btnClose.Location = new System.Drawing.Point(624, 222);
            this.btnClose.Size = new System.Drawing.Size(100, 32);
            this.btnClose.Text = "Close";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            //
            this.lblTotal.AutoSize = true;
            this.lblTotal.Location = new System.Drawing.Point(16, 265);
            this.lblTotal.Text = "Total progress";
            this.progressTotal.Location = new System.Drawing.Point(16, 283);
            this.progressTotal.Size = new System.Drawing.Size(708, 18);
            //
            this.lblFile.AutoSize = true;
            this.lblFile.Location = new System.Drawing.Point(16, 310);
            this.lblFile.Text = "Current file progress";
            this.progressFile.Location = new System.Drawing.Point(16, 328);
            this.progressFile.Size = new System.Drawing.Size(708, 18);
            //
            this.lblStatus.Location = new System.Drawing.Point(16, 355);
            this.lblStatus.Size = new System.Drawing.Size(708, 18);
            this.lblStatus.Text = "Idle";
            //
            this.lblCurrentFile.Location = new System.Drawing.Point(16, 375);
            this.lblCurrentFile.Size = new System.Drawing.Size(708, 18);
            this.lblCurrentFile.Text = "";
            //
            this.txtLog.Location = new System.Drawing.Point(16, 400);
            this.txtLog.Multiline = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtLog.Size = new System.Drawing.Size(708, 230);
            this.txtLog.ReadOnly = true;
            this.txtLog.WordWrap = false;
            //
            this.ClientSize = new System.Drawing.Size(744, 650);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblHelp);
            this.Controls.Add(this.lblInstall);
            this.Controls.Add(this.txtInstall);
            this.Controls.Add(this.btnBrowseInstall);
            this.Controls.Add(this.lblOutput);
            this.Controls.Add(this.txtOutput);
            this.Controls.Add(this.btnBrowseOutput);
            this.Controls.Add(this.chkPakList);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOpenOutput);
            this.Controls.Add(this.btnViewReports);
            this.Controls.Add(this.btnWorkflow);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.lblTotal);
            this.Controls.Add(this.progressTotal);
            this.Controls.Add(this.lblFile);
            this.Controls.Add(this.progressFile);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lblCurrentFile);
            this.Controls.Add(this.txtLog);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.Name = "GameAnalyzerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "specSB2modTool";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblHelp;
        private System.Windows.Forms.Label lblInstall;
        private System.Windows.Forms.TextBox txtInstall;
        private System.Windows.Forms.Button btnBrowseInstall;
        private System.Windows.Forms.Label lblOutput;
        private System.Windows.Forms.TextBox txtOutput;
        private System.Windows.Forms.Button btnBrowseOutput;
        private System.Windows.Forms.CheckBox chkPakList;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOpenOutput;
        private System.Windows.Forms.Button btnViewReports;
        private System.Windows.Forms.Button btnWorkflow;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.ProgressBar progressTotal;
        private System.Windows.Forms.Label lblFile;
        private System.Windows.Forms.ProgressBar progressFile;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblCurrentFile;
        private System.Windows.Forms.TextBox txtLog;
    }
}
