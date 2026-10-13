namespace SpaceBourne2ModTool.Forms
{
    partial class TrainerConfigForm
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lstOptions = new System.Windows.Forms.ListBox();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblHelp = new System.Windows.Forms.Label();
            this.SuspendLayout();
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(12, 12);
            this.lblTitle.Text = "Community Trainer Options Catalogue";
            this.lblHelp.Location = new System.Drawing.Point(12, 40);
            this.lblHelp.Size = new System.Drawing.Size(560, 40);
            this.lblHelp.Text = "These options appear across WeMod, Cheat Engine tables and other public trainers for SpaceBourne 2. Use them as a checklist when building or documenting your own cheats.";
            this.lstOptions.Location = new System.Drawing.Point(12, 90);
            this.lstOptions.Size = new System.Drawing.Size(560, 300);
            this.btnClose.Location = new System.Drawing.Point(497, 400);
            this.btnClose.Size = new System.Drawing.Size(75, 28);
            this.btnClose.Text = "Close";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            this.ClientSize = new System.Drawing.Size(584, 441);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblHelp);
            this.Controls.Add(this.lstOptions);
            this.Controls.Add(this.btnClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "TrainerConfigForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Trainer Options";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.ListBox lstOptions;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblHelp;
    }
}
