namespace SpaceBourne2ModTool.Forms
{
    partial class ModWorkflowWizardForm
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
            this.lblGoal = new System.Windows.Forms.Label();
            this.cmbGoal = new System.Windows.Forms.ComboBox();
            this.lblTools = new System.Windows.Forms.Label();
            this.txtPlan = new System.Windows.Forms.TextBox();
            this.btnLaunchFModel = new System.Windows.Forms.Button();
            this.btnLaunchUAsset = new System.Windows.Forms.Button();
            this.btnLaunchUModel = new System.Windows.Forms.Button();
            this.btnLoose = new System.Windows.Forms.Button();
            this.btnBrowser = new System.Windows.Forms.Button();
            this.btnEditor = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(16, 12);
            this.lblTitle.Size = new System.Drawing.Size(400, 28);
            this.lblTitle.Text = "Mod Workflow Wizard";
            this.lblGoal.AutoSize = true;
            this.lblGoal.Location = new System.Drawing.Point(16, 52);
            this.lblGoal.Text = "What do you want to mod?";
            this.cmbGoal.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbGoal.Location = new System.Drawing.Point(16, 74);
            this.cmbGoal.Size = new System.Drawing.Size(520, 23);
            this.cmbGoal.SelectedIndexChanged += new System.EventHandler(this.cmbGoal_SelectedIndexChanged);
            this.lblTools.Location = new System.Drawing.Point(16, 110);
            this.lblTools.Size = new System.Drawing.Size(520, 90);
            this.txtPlan.Location = new System.Drawing.Point(16, 210);
            this.txtPlan.Multiline = true;
            this.txtPlan.ReadOnly = true;
            this.txtPlan.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtPlan.Size = new System.Drawing.Size(520, 220);
            this.btnLaunchFModel.Location = new System.Drawing.Point(16, 444);
            this.btnLaunchFModel.Size = new System.Drawing.Size(100, 28);
            this.btnLaunchFModel.Text = "FModel";
            this.btnLaunchFModel.Click += new System.EventHandler(this.btnLaunchFModel_Click);
            this.btnLaunchUAsset.Location = new System.Drawing.Point(122, 444);
            this.btnLaunchUAsset.Size = new System.Drawing.Size(100, 28);
            this.btnLaunchUAsset.Text = "UAssetGUI";
            this.btnLaunchUAsset.Click += new System.EventHandler(this.btnLaunchUAsset_Click);
            this.btnLaunchUModel.Location = new System.Drawing.Point(228, 444);
            this.btnLaunchUModel.Size = new System.Drawing.Size(100, 28);
            this.btnLaunchUModel.Text = "UModel";
            this.btnLaunchUModel.Click += new System.EventHandler(this.btnLaunchUModel_Click);
            this.btnLoose.Location = new System.Drawing.Point(16, 482);
            this.btnLoose.Size = new System.Drawing.Size(120, 28);
            this.btnLoose.Text = "Loose Mods";
            this.btnLoose.Click += new System.EventHandler(this.btnLoose_Click);
            this.btnBrowser.Location = new System.Drawing.Point(142, 482);
            this.btnBrowser.Size = new System.Drawing.Size(120, 28);
            this.btnBrowser.Text = "Content Browser";
            this.btnBrowser.Click += new System.EventHandler(this.btnBrowser_Click);
            this.btnEditor.Location = new System.Drawing.Point(268, 482);
            this.btnEditor.Size = new System.Drawing.Size(120, 28);
            this.btnEditor.Text = "Code Editor";
            this.btnEditor.Click += new System.EventHandler(this.btnEditor_Click);
            this.btnClose.Location = new System.Drawing.Point(436, 482);
            this.btnClose.Size = new System.Drawing.Size(100, 28);
            this.btnClose.Text = "Close";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            this.ClientSize = new System.Drawing.Size(556, 528);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblGoal);
            this.Controls.Add(this.cmbGoal);
            this.Controls.Add(this.lblTools);
            this.Controls.Add(this.txtPlan);
            this.Controls.Add(this.btnLaunchFModel);
            this.Controls.Add(this.btnLaunchUAsset);
            this.Controls.Add(this.btnLaunchUModel);
            this.Controls.Add(this.btnLoose);
            this.Controls.Add(this.btnBrowser);
            this.Controls.Add(this.btnEditor);
            this.Controls.Add(this.btnClose);
            this.Name = "ModWorkflowWizardForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "specSB2modTool — Workflow Wizard";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblGoal;
        private System.Windows.Forms.ComboBox cmbGoal;
        private System.Windows.Forms.Label lblTools;
        private System.Windows.Forms.TextBox txtPlan;
        private System.Windows.Forms.Button btnLaunchFModel;
        private System.Windows.Forms.Button btnLaunchUAsset;
        private System.Windows.Forms.Button btnLaunchUModel;
        private System.Windows.Forms.Button btnLoose;
        private System.Windows.Forms.Button btnBrowser;
        private System.Windows.Forms.Button btnEditor;
        private System.Windows.Forms.Button btnClose;
    }
}
