namespace SpaceBourne2ModTool.Forms
{
    partial class ConversionToolsForm
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
            this.lstRecipes = new System.Windows.Forms.ListBox();
            this.txtDetails = new System.Windows.Forms.TextBox();
            this.btnWriteGuide = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(16, 12);
            this.lblTitle.Text = "Conversion Tools";
            this.lblHelp.Location = new System.Drawing.Point(16, 45);
            this.lblHelp.Size = new System.Drawing.Size(640, 40);
            this.lblHelp.Text = "How to turn PNG/FBX/WAV/text into formats SpaceBourne 2 can load. Most assets must be cooked in UE 4.27; FMOD/Movies may allow loose replaces.";
            this.lstRecipes.Location = new System.Drawing.Point(16, 95);
            this.lstRecipes.Size = new System.Drawing.Size(640, 140);
            this.lstRecipes.SelectedIndexChanged += new System.EventHandler(this.lstRecipes_SelectedIndexChanged);
            this.txtDetails.Location = new System.Drawing.Point(16, 250);
            this.txtDetails.Multiline = true;
            this.txtDetails.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDetails.Size = new System.Drawing.Size(640, 220);
            this.txtDetails.ReadOnly = true;
            this.txtDetails.Font = new System.Drawing.Font("Consolas", 9F);
            this.btnWriteGuide.Location = new System.Drawing.Point(16, 485);
            this.btnWriteGuide.Size = new System.Drawing.Size(200, 32);
            this.btnWriteGuide.Text = "Write Full Guide to Disk";
            this.btnWriteGuide.Click += new System.EventHandler(this.btnWriteGuide_Click);
            this.btnClose.Location = new System.Drawing.Point(581, 485);
            this.btnClose.Size = new System.Drawing.Size(75, 32);
            this.btnClose.Text = "Close";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            this.ClientSize = new System.Drawing.Size(674, 535);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblHelp);
            this.Controls.Add(this.lstRecipes);
            this.Controls.Add(this.txtDetails);
            this.Controls.Add(this.btnWriteGuide);
            this.Controls.Add(this.btnClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "ConversionToolsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "specSB2modTool";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblHelp;
        private System.Windows.Forms.ListBox lstRecipes;
        private System.Windows.Forms.TextBox txtDetails;
        private System.Windows.Forms.Button btnWriteGuide;
        private System.Windows.Forms.Button btnClose;
    }
}
