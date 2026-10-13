using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using SpaceBourne2ModTool.Core;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Forms
{
    /// <summary>
    /// Robust image viewer for SB2 texture exports (PNG/TGA/BMP/JPG) and packed previews.
    /// Modes: Fit, Actual, Zoom; optional greyscale; alpha checkerboard.
    /// </summary>
    public partial class ImageViewerForm : Form
    {
        private Image _image;
        private float _zoom = 1f;
        private bool _greyscale;
        private bool _checker = true;
        private Point _pan;
        private bool _dragging;
        private Point _dragStart;

        public string CurrentPath { get; private set; }

        public ImageViewerForm(string initialPath = null)
        {
            InitializeComponent();
            AppBranding.Apply(this, AppBranding.ProductTitle + " — Image Viewer");
            canvas.MouseEnter += (s, e) => canvas.Focus();
            ApplyTheme();
            if (!string.IsNullOrEmpty(initialPath) && File.Exists(initialPath))
            {
                Tag = initialPath;
                LoadImage(initialPath);
            }
        }

        void ApplyTheme()
        {
            try { UiTheme.Apply(this); } catch { }
            if (canvas != null) canvas.BackColor = Color.FromArgb(16, 16, 20);
            if (lblTitle != null) lblTitle.ForeColor = UiTheme.Accent;
            try { canvas.TabStop = true; canvas.Focus(); } catch { }
        }

        public void LoadImage(string path)
        {
            CurrentPath = path;
            try
            {
                if (_image != null) { _image.Dispose(); _image = null; }
                // Load without locking file
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    _image = Image.FromStream(fs);
                txtPath.Text = path;
                _zoom = 1f;
                _pan = Point.Empty;
                lblInfo.Text = string.Format("{0}×{1}  {2}  {3}",
                    _image.Width, _image.Height, _image.PixelFormat, Path.GetFileName(path));
                canvas.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Could not load image:\n" + ex.Message, "Image Viewer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff;*.tga|All|*.*";
                if (ofd.ShowDialog() == DialogResult.OK) LoadImage(ofd.FileName);
            }
        }

        void btnFolder_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Texture export folder";
                if (fbd.ShowDialog() != DialogResult.OK) return;
                lst.Items.Clear();
                try
                {
                    foreach (string pattern in new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.tga" })
                        foreach (string f in Directory.GetFiles(fbd.SelectedPath, pattern, SearchOption.AllDirectories))
                            lst.Items.Add(f);
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }

        void lst_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lst.SelectedItem != null) LoadImage(lst.SelectedItem.ToString());
        }

        void btnFit_Click(object sender, EventArgs e)
        {
            if (_image == null) return;
            float zx = (float)canvas.ClientSize.Width / _image.Width;
            float zy = (float)canvas.ClientSize.Height / _image.Height;
            _zoom = Math.Max(0.05f, Math.Min(zx, zy));
            _pan = Point.Empty;
            canvas.Invalidate();
        }

        void btnActual_Click(object sender, EventArgs e) { _zoom = 1f; _pan = Point.Empty; canvas.Invalidate(); }
        void btnZoomIn_Click(object sender, EventArgs e) { _zoom = Math.Min(16f, _zoom * 1.25f); canvas.Invalidate(); }
        void btnZoomOut_Click(object sender, EventArgs e) { _zoom = Math.Max(0.05f, _zoom / 1.25f); canvas.Invalidate(); }
        void chkGrey_CheckedChanged(object sender, EventArgs e) { _greyscale = chkGrey.Checked; canvas.Invalidate(); }
        void chkChecker_CheckedChanged(object sender, EventArgs e) { _checker = chkChecker.Checked; canvas.Invalidate(); }

        void canvas_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.FromArgb(16, 16, 20));
            if (_checker)
            {
                int s = 12;
                for (int y = 0; y < canvas.Height; y += s)
                    for (int x = 0; x < canvas.Width; x += s)
                        if (((x / s) + (y / s)) % 2 == 0)
                            using (var b = new SolidBrush(Color.FromArgb(28, 28, 32)))
                                g.FillRectangle(b, x, y, s, s);
            }
            if (_image == null) return;
            g.InterpolationMode = _zoom < 1 ? InterpolationMode.HighQualityBicubic : InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            int w = (int)(_image.Width * _zoom);
            int h = (int)(_image.Height * _zoom);
            int x0 = (canvas.ClientSize.Width - w) / 2 + _pan.X;
            int y0 = (canvas.ClientSize.Height - h) / 2 + _pan.Y;
            if (_greyscale)
            {
                // Draw via color matrix-ish: convert manually is heavy — use desaturate approx by drawing then
                var attrs = new System.Drawing.Imaging.ImageAttributes();
                float[][] m = {
                    new float[] {0.3f,0.3f,0.3f,0,0},
                    new float[] {0.59f,0.59f,0.59f,0,0},
                    new float[] {0.11f,0.11f,0.11f,0,0},
                    new float[] {0,0,0,1,0},
                    new float[] {0,0,0,0,1}
                };
                attrs.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix(m));
                g.DrawImage(_image, new Rectangle(x0, y0, w, h), 0, 0, _image.Width, _image.Height, GraphicsUnit.Pixel, attrs);
            }
            else
                g.DrawImage(_image, x0, y0, w, h);
        }

        void canvas_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) { _dragging = true; _dragStart = e.Location; }
        }
        void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_dragging) return;
            _pan.X += e.X - _dragStart.X;
            _pan.Y += e.Y - _dragStart.Y;
            _dragStart = e.Location;
            canvas.Invalidate();
        }
        void canvas_MouseUp(object sender, MouseEventArgs e) { _dragging = false; }
        void canvas_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0) _zoom = Math.Min(16f, _zoom * 1.1f);
            else _zoom = Math.Max(0.05f, _zoom / 1.1f);
            canvas.Invalidate();
        }

        void btnSpecVert_Click(object sender, EventArgs e)
        {
            string dir = Path.Combine(SpecVertService.DefaultWorkspaceRoot(), "Modules", "Materials", "Exports");
            if (!Directory.Exists(dir))
                dir = Path.Combine(SpecVertService.DefaultWorkspaceRoot(), "Modules", "Meshes", "Exports");
            if (Directory.Exists(dir))
            {
                lst.Items.Clear();
                foreach (string pattern in new[] { "*.png", "*.jpg", "*.tga", "*.bmp" })
                    foreach (string f in Directory.GetFiles(dir, pattern, SearchOption.AllDirectories))
                        lst.Items.Add(f);
            }
            else
                MessageBox.Show(this, "No SpecVert texture exports found yet.", "Image Viewer", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        void btnClose_Click(object sender, EventArgs e) => Close();

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (_image != null) _image.Dispose();
            base.OnFormClosed(e);
        }
    }

    /// <summary>Compact preview box for embedding in other forms.</summary>
    public class CompactImagePreview : Panel
    {
        private Image _img;
        public CompactImagePreview()
        {
            DoubleBuffered = true;
            BackColor = Color.FromArgb(16, 16, 20);
            Size = new Size(120, 90);
            BorderStyle = BorderStyle.FixedSingle;
        }
        public void SetImage(string path)
        {
            try
            {
                if (_img != null) { _img.Dispose(); _img = null; }
                if (string.IsNullOrEmpty(path) || !File.Exists(path)) { Invalidate(); return; }
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    _img = Image.FromStream(fs);
                Invalidate();
            }
            catch { }
        }
        public void ClearImage()
        {
            if (_img != null) { _img.Dispose(); _img = null; }
            Invalidate();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.Clear(BackColor);
            if (_img == null) return;
            float zx = (float)Width / _img.Width, zy = (float)Height / _img.Height;
            float z = Math.Min(zx, zy);
            int w = (int)(_img.Width * z), h = (int)(_img.Height * z);
            e.Graphics.DrawImage(_img, (Width - w) / 2, (Height - h) / 2, w, h);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing && _img != null) _img.Dispose();
            base.Dispose(disposing);
        }
    }
}
