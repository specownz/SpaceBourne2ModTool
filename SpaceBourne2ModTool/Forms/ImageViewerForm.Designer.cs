namespace SpaceBourne2ModTool.Forms
{
    partial class ImageViewerForm
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            if (disposing && _image != null) { _image.Dispose(); _image = null; }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panelTop = new System.Windows.Forms.Panel();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.btnFolder = new System.Windows.Forms.Button();
            this.btnFit = new System.Windows.Forms.Button();
            this.btnActual = new System.Windows.Forms.Button();
            this.btnZoomIn = new System.Windows.Forms.Button();
            this.btnZoomOut = new System.Windows.Forms.Button();
            this.chkGrey = new System.Windows.Forms.CheckBox();
            this.chkChecker = new System.Windows.Forms.CheckBox();
            this.btnReset = new System.Windows.Forms.Button();
            this.canvas = new System.Windows.Forms.Panel();
            this.lblInfo = new System.Windows.Forms.Label();
            this.lst = new System.Windows.Forms.ListBox();
            this.btnClose = new System.Windows.Forms.Button();
            this.panelTop.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();
            // Top bar — full width
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Height = 72;
            this.panelTop.Controls.Add(this.lblTitle);
            this.panelTop.Controls.Add(this.txtPath);
            this.panelTop.Controls.Add(this.btnBrowse);
            this.panelTop.Controls.Add(this.btnFolder);
            this.panelTop.Controls.Add(this.btnFit);
            this.panelTop.Controls.Add(this.btnActual);
            this.panelTop.Controls.Add(this.btnZoomIn);
            this.panelTop.Controls.Add(this.btnZoomOut);
            this.panelTop.Controls.Add(this.chkGrey);
            this.panelTop.Controls.Add(this.chkChecker);
            this.panelTop.Controls.Add(this.btnReset);
            this.lblTitle.Location = new System.Drawing.Point(8, 4);
            this.lblTitle.Size = new System.Drawing.Size(400, 22);
            this.lblTitle.Text = "Image Viewer";
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.txtPath.Location = new System.Drawing.Point(8, 30);
            this.txtPath.Size = new System.Drawing.Size(420, 23);
            this.txtPath.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Top;
            this.btnBrowse.Location = new System.Drawing.Point(436, 28);
            this.btnBrowse.Size = new System.Drawing.Size(70, 26);
            this.btnBrowse.Text = "Open…";
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            this.btnFolder.Location = new System.Drawing.Point(510, 28);
            this.btnFolder.Size = new System.Drawing.Size(80, 26);
            this.btnFolder.Text = "Folder…";
            this.btnFolder.Click += new System.EventHandler(this.btnFolder_Click);
            this.btnFit.Location = new System.Drawing.Point(600, 28);
            this.btnFit.Size = new System.Drawing.Size(50, 26);
            this.btnFit.Text = "Fit";
            this.btnFit.Click += new System.EventHandler(this.btnFit_Click);
            this.btnActual.Location = new System.Drawing.Point(654, 28);
            this.btnActual.Size = new System.Drawing.Size(56, 26);
            this.btnActual.Text = "1:1";
            this.btnActual.Click += new System.EventHandler(this.btnActual_Click);
            this.btnZoomIn.Location = new System.Drawing.Point(714, 28);
            this.btnZoomIn.Size = new System.Drawing.Size(36, 26);
            this.btnZoomIn.Text = "+";
            this.btnZoomIn.Click += new System.EventHandler(this.btnZoomIn_Click);
            this.btnZoomOut.Location = new System.Drawing.Point(754, 28);
            this.btnZoomOut.Size = new System.Drawing.Size(36, 26);
            this.btnZoomOut.Text = "−";
            this.btnZoomOut.Click += new System.EventHandler(this.btnZoomOut_Click);
            this.chkGrey.Location = new System.Drawing.Point(800, 30);
            this.chkGrey.AutoSize = true;
            this.chkGrey.Text = "Greyscale";
            this.chkGrey.CheckedChanged += new System.EventHandler(this.chkGrey_CheckedChanged);
            this.chkChecker.Location = new System.Drawing.Point(890, 30);
            this.chkChecker.AutoSize = true;
            this.chkChecker.Text = "Alpha board";
            this.chkChecker.Checked = true;
            this.chkChecker.CheckedChanged += new System.EventHandler(this.chkChecker_CheckedChanged);
            this.btnReset.Location = new System.Drawing.Point(1000, 28);
            this.btnReset.Size = new System.Drawing.Size(56, 26);
            this.btnReset.Text = "Reset";
            this.btnReset.Click += new System.EventHandler(this.btnActual_Click);
            // Canvas — fills remaining width and height
            this.canvas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.canvas.BackColor = System.Drawing.Color.FromArgb(16, 16, 20);
            this.canvas.Paint += new System.Windows.Forms.PaintEventHandler(this.canvas_Paint);
            this.canvas.MouseDown += new System.Windows.Forms.MouseEventHandler(this.canvas_MouseDown);
            this.canvas.MouseMove += new System.Windows.Forms.MouseEventHandler(this.canvas_MouseMove);
            this.canvas.MouseUp += new System.Windows.Forms.MouseEventHandler(this.canvas_MouseUp);
            this.canvas.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.canvas_MouseWheel);
            // Bottom — file list + info full width
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Height = 110;
            this.panelBottom.Controls.Add(this.lblInfo);
            this.panelBottom.Controls.Add(this.lst);
            this.panelBottom.Controls.Add(this.btnClose);
            this.lblInfo.Location = new System.Drawing.Point(8, 4);
            this.lblInfo.Size = new System.Drawing.Size(900, 18);
            this.lblInfo.Text = "Open a PNG, JPG, BMP, GIF, or TGA export. Drag to pan · wheel to zoom.";
            this.lst.Location = new System.Drawing.Point(8, 26);
            this.lst.Size = new System.Drawing.Size(980, 70);
            this.lst.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom;
            this.lst.SelectedIndexChanged += new System.EventHandler(this.lst_SelectedIndexChanged);
            this.btnClose.Location = new System.Drawing.Point(996, 40);
            this.btnClose.Size = new System.Drawing.Size(72, 28);
            this.btnClose.Text = "Close";
            this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Top;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // Form
            this.ClientSize = new System.Drawing.Size(1080, 720);
            this.MinimumSize = new System.Drawing.Size(800, 500);
            this.Controls.Add(this.canvas);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.panelTop);
            this.Name = "ImageViewerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Image Viewer";
            this.panelTop.ResumeLayout(false);
            this.panelBottom.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnFolder;
        private System.Windows.Forms.Button btnFit;
        private System.Windows.Forms.Button btnActual;
        private System.Windows.Forms.Button btnZoomIn;
        private System.Windows.Forms.Button btnZoomOut;
        private System.Windows.Forms.CheckBox chkGrey;
        private System.Windows.Forms.CheckBox chkChecker;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Panel canvas;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.ListBox lst;
        private System.Windows.Forms.Button btnClose;
    }
}
