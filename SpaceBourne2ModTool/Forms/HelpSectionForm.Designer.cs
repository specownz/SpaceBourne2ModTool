namespace SpaceBourne2ModTool.Forms
{
    partial class HelpSectionForm
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
            this.lst = new System.Windows.Forms.ListBox();
            this.txt = new System.Windows.Forms.TextBox();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(12, 10);
            this.lblTitle.Size = new System.Drawing.Size(600, 28);
            this.lblTitle.Text = "Help — same topics as the User Manual";
            this.lst.Location = new System.Drawing.Point(12, 48);
            this.lst.Size = new System.Drawing.Size(260, 420);
            this.lst.SelectedIndexChanged += new System.EventHandler(this.lst_SelectedIndexChanged);
            this.txt.Location = new System.Drawing.Point(290, 48);
            this.txt.Multiline = true;
            this.txt.ReadOnly = true;
            this.txt.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txt.Size = new System.Drawing.Size(490, 420);
            this.btnClose.Location = new System.Drawing.Point(700, 480);
            this.btnClose.Size = new System.Drawing.Size(80, 28);
            this.btnClose.Text = "Close";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            this.ClientSize = new System.Drawing.Size(800, 520);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lst);
            this.Controls.Add(this.txt);
            this.Controls.Add(this.btnClose);
            this.Name = "HelpSectionForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Help";
            this.ResumeLayout(false);
        }
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.ListBox lst;
        private System.Windows.Forms.TextBox txt;
        private System.Windows.Forms.Button btnClose;
    }
}
