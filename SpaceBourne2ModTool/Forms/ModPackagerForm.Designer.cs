namespace SpaceBourne2ModTool.Forms
{
    partial class ModPackagerForm
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblProject = new System.Windows.Forms.Label();
            this.lblStaging = new System.Windows.Forms.Label();
            this.txtStaging = new System.Windows.Forms.TextBox();
            this.btnCreate = new System.Windows.Forms.Button();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            this.lblProject.AutoSize = true;
            this.lblProject.Location = new System.Drawing.Point(12, 15);
            this.lblProject.Text = "Project: -";
            this.lblStaging.AutoSize = true;
            this.lblStaging.Location = new System.Drawing.Point(12, 45);
            this.lblStaging.Text = "Staging folder:";
            this.txtStaging.Location = new System.Drawing.Point(12, 65);
            this.txtStaging.ReadOnly = true;
            this.txtStaging.Size = new System.Drawing.Size(560, 22);
            this.btnCreate.Location = new System.Drawing.Point(12, 100);
            this.btnCreate.Size = new System.Drawing.Size(140, 32);
            this.btnCreate.Text = "Create / Generate PAK";
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
            this.txtLog.Location = new System.Drawing.Point(12, 150);
            this.txtLog.Multiline = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(560, 200);
            this.btnClose.Location = new System.Drawing.Point(497, 370);
            this.btnClose.Size = new System.Drawing.Size(75, 28);
            this.btnClose.Text = "Close";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            this.ClientSize = new System.Drawing.Size(584, 411);
            this.Controls.Add(this.lblProject);
            this.Controls.Add(this.lblStaging);
            this.Controls.Add(this.txtStaging);
            this.Controls.Add(this.btnCreate);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.btnClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "ModPackagerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Mod Packager";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblProject;
        private System.Windows.Forms.Label lblStaging;
        private System.Windows.Forms.TextBox txtStaging;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Button btnClose;
    }
}
