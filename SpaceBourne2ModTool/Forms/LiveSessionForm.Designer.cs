namespace SpaceBourne2ModTool.Forms
{
    partial class LiveSessionForm
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
            this.txtIntro = new System.Windows.Forms.TextBox();
            this.txtNotes = new System.Windows.Forms.TextBox();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.lblLogPath = new System.Windows.Forms.Label();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnSaveNotes = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblNotes = new System.Windows.Forms.Label();
            this.SuspendLayout();
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(12, 10);
            this.lblTitle.Size = new System.Drawing.Size(500, 28);
            this.lblTitle.Text = "Live session observer";
            this.txtIntro.Location = new System.Drawing.Point(12, 42);
            this.txtIntro.Multiline = true;
            this.txtIntro.ReadOnly = true;
            this.txtIntro.Size = new System.Drawing.Size(760, 70);
            this.lblNotes.Location = new System.Drawing.Point(12, 120);
            this.lblNotes.AutoSize = true;
            this.lblNotes.Text = "Your notes while playing";
            this.txtNotes.Location = new System.Drawing.Point(12, 140);
            this.txtNotes.Multiline = true;
            this.txtNotes.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtNotes.Size = new System.Drawing.Size(760, 160);
            this.lblLogPath.Location = new System.Drawing.Point(12, 310);
            this.lblLogPath.Size = new System.Drawing.Size(760, 20);
            this.txtLog.Location = new System.Drawing.Point(12, 334);
            this.txtLog.Multiline = true;
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(760, 140);
            this.btnRefresh.Location = new System.Drawing.Point(12, 486);
            this.btnRefresh.Size = new System.Drawing.Size(120, 30);
            this.btnRefresh.Text = "Refresh log tail";
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            this.btnSaveNotes.Location = new System.Drawing.Point(140, 486);
            this.btnSaveNotes.Size = new System.Drawing.Size(120, 30);
            this.btnSaveNotes.Text = "Save notes";
            this.btnSaveNotes.Click += new System.EventHandler(this.btnSaveNotes_Click);
            this.btnExport.Location = new System.Drawing.Point(270, 486);
            this.btnExport.Size = new System.Drawing.Size(200, 30);
            this.btnExport.Text = "Export merged report";
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            this.btnClose.Location = new System.Drawing.Point(692, 486);
            this.btnClose.Size = new System.Drawing.Size(80, 30);
            this.btnClose.Text = "Close";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            this.ClientSize = new System.Drawing.Size(792, 530);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.txtIntro);
            this.Controls.Add(this.lblNotes);
            this.Controls.Add(this.txtNotes);
            this.Controls.Add(this.lblLogPath);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.btnSaveNotes);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.btnClose);
            this.Name = "LiveSessionForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Live session observer";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        private System.Windows.Forms.Label lblTitle, lblLogPath, lblNotes;
        private System.Windows.Forms.TextBox txtIntro, txtNotes, txtLog;
        private System.Windows.Forms.Button btnRefresh, btnSaveNotes, btnExport, btnClose;
    }
}
