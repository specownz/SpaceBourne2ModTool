namespace SpaceBourne2ModTool.Forms
{
    partial class AutoDetectSplashForm
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
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.panelList = new System.Windows.Forms.Panel();
            this.lblStatus = new System.Windows.Forms.Label();
            this.progressTotal = new System.Windows.Forms.ProgressBar();
            this.progressItem = new System.Windows.Forms.ProgressBar();
            this.lblTotal = new System.Windows.Forms.Label();
            this.lblItem = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(20, 16);
            this.lblTitle.Size = new System.Drawing.Size(540, 32);
            this.lblTitle.Text = "Auto-Detecting Tools";
            this.lblSubtitle.Location = new System.Drawing.Point(22, 52);
            this.lblSubtitle.Size = new System.Drawing.Size(540, 40);
            this.lblSubtitle.Text = "specSB2modTool is searching for SpaceBourne 2 and external modding tools.\r\nThis is normal — the app is not frozen.";
            this.panelList.Location = new System.Drawing.Point(24, 100);
            this.panelList.Size = new System.Drawing.Size(560, 290);
            this.panelList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblTotal.AutoSize = true;
            this.lblTotal.Location = new System.Drawing.Point(24, 400);
            this.lblTotal.Text = "Overall progress";
            this.progressTotal.Location = new System.Drawing.Point(24, 420);
            this.progressTotal.Size = new System.Drawing.Size(560, 18);
            this.lblItem.AutoSize = true;
            this.lblItem.Location = new System.Drawing.Point(24, 448);
            this.lblItem.Text = "Current search activity";
            this.progressItem.Location = new System.Drawing.Point(24, 468);
            this.progressItem.Size = new System.Drawing.Size(560, 18);
            this.lblStatus.Location = new System.Drawing.Point(24, 496);
            this.lblStatus.Size = new System.Drawing.Size(560, 36);
            this.lblStatus.Text = "Starting…";
            this.btnClose.Location = new System.Drawing.Point(484, 540);
            this.btnClose.Size = new System.Drawing.Size(100, 32);
            this.btnClose.Text = "Please wait…";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            this.ClientSize = new System.Drawing.Size(608, 590);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblSubtitle);
            this.Controls.Add(this.panelList);
            this.Controls.Add(this.lblTotal);
            this.Controls.Add(this.progressTotal);
            this.Controls.Add(this.lblItem);
            this.Controls.Add(this.progressItem);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AutoDetectSplashForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "specSB2modTool — Auto-Detect";
            this.Shown += new System.EventHandler(this.AutoDetectSplashForm_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Panel panelList;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.ProgressBar progressTotal;
        private System.Windows.Forms.ProgressBar progressItem;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Label lblItem;
        private System.Windows.Forms.Button btnClose;
    }
}
