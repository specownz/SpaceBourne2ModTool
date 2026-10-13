using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using SpaceBourne2ModTool.Core;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Forms
{
    public partial class ModelViewerForm : Form
    {
        public enum ViewMode { Wireframe, UnlitGreyscale, Materials }
        private SimpleMesh _mesh;
        private float _yaw = 0.6f, _pitch = 0.35f;
        private bool _orbit;
        private Point _lastMouse;

        public ModelViewerForm(string initialPath = null)
        {
            InitializeComponent();
            try {
                typeof(Panel).GetProperty("DoubleBuffered",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    .SetValue(panelPreview, true, null);
            } catch { }
            AppBranding.Apply(this, AppBranding.ProductTitle + " — 3D Model Viewer");
            ApplyTheme();
            if (cmbMode.Items.Count == 0)
                cmbMode.Items.AddRange(new object[] { "Wireframe", "Unlit grey", "Shaded" });
            if (cmbMode.SelectedIndex < 0) cmbMode.SelectedIndex = 0;
            if (!string.IsNullOrEmpty(initialPath))
            {
                Tag = initialPath;
                TryLoad(initialPath);
            }
            RefreshToolStatus();
        }

        void ApplyTheme()
        {
            try { UiTheme.Apply(this); } catch { }
            if (panelPreview != null) panelPreview.BackColor = Color.FromArgb(18, 18, 22);
            if (lblTitle != null) lblTitle.ForeColor = UiTheme.Accent;
        }

        void RefreshToolStatus()
        {
            string um = ConfigService.UModelPath;
            string msg = string.IsNullOrEmpty(um) || !File.Exists(um)
                ? "UModel not configured — in-app preview supports OBJ and STL. Drag to orbit."
                : "UModel ready. Drag to orbit · Draw mode changes shading.";
            if (lblModeHint != null) lblModeHint.Text = msg;
        }

        void panelPreview_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) { _orbit = true; _lastMouse = e.Location; }
        }

        void panelPreview_MouseUp(object sender, MouseEventArgs e) { _orbit = false; }

        void btnResetView_Click(object sender, EventArgs e)
        {
            _yaw = 0.6f; _pitch = 0.35f;
            panelPreview.Invalidate();
        }

        ViewMode CurrentMode()
        {
            if (cmbMode.SelectedIndex == 1) return ViewMode.UnlitGreyscale;
            if (cmbMode.SelectedIndex == 2) return ViewMode.Materials;
            return ViewMode.Wireframe;
        }

        void TryLoad(string path)
        {
            txtPath.Text = path;
            try
            {
                _mesh = SimpleMesh.Load(path);
                Log("Loaded " + _mesh.FormatNote + ": " + _mesh.Vertices.Count + " verts, " + _mesh.Faces.Count + " faces — " + Path.GetFileName(path));
                lblModeHint.Text = "Drag preview to orbit. Mode: " + CurrentMode();
                panelPreview.Invalidate();
            }
            catch (Exception ex)
            {
                _mesh = null;
                panelPreview.Invalidate();
                Log("In-app preview: " + ex.Message);
                lblModeHint.Text = ex.Message;
                // Offer external open for FBX/uasset
                string ext = Path.GetExtension(path).ToLowerInvariant();
                if (ext == ".fbx" || ext == ".uasset" || ext == ".psk" || ext == ".pskx" || ext == ".gltf" || ext == ".glb")
                {
                    if (MessageBox.Show(this,
                        ex.Message + "\n\nOpen with UModel or default application?",
                        "3D Viewer", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        OpenExternal(path);
                }
            }
        }

        void OpenExternal(string path)
        {
            string um = ConfigService.UModelPath;
            try
            {
                if (!string.IsNullOrEmpty(um) && File.Exists(um) &&
                    (path.EndsWith(".uasset", StringComparison.OrdinalIgnoreCase) ||
                     path.EndsWith(".psk", StringComparison.OrdinalIgnoreCase)))
                {
                    Process.Start(new ProcessStartInfo { FileName = um, Arguments = "\"" + path + "\"", UseShellExecute = true });
                    Log("Launched UModel");
                }
                else
                {
                    Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
                    Log("Opened with default app: " + path);
                }
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Open", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Meshes|*.obj;*.fbx;*.stl;*.gltf;*.glb;*.uasset;*.psk;*.pskx|OBJ|*.obj|FBX|*.fbx|STL|*.stl|All|*.*";
                ofd.Title = "Select mesh";
                if (ofd.ShowDialog() == DialogResult.OK) TryLoad(ofd.FileName);
            }
        }

        void btnBrowseFolder_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Folder of meshes (OBJ recommended)";
                if (fbd.ShowDialog() != DialogResult.OK) return;
                txtPath.Text = fbd.SelectedPath;
                lstMeshes.Items.Clear();
                try
                {
                    foreach (string pattern in new[] { "*.obj", "*.fbx", "*.stl", "*.gltf", "*.glb", "*.uasset", "*.psk" })
                        foreach (string f in Directory.GetFiles(fbd.SelectedPath, pattern, SearchOption.AllDirectories))
                            lstMeshes.Items.Add(f);
                    Log("Listed " + lstMeshes.Items.Count + " mesh files");
                }
                catch (Exception ex) { Log(ex.Message); }
            }
        }

        void lstMeshes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstMeshes.SelectedItem != null) TryLoad(lstMeshes.SelectedItem.ToString());
        }

        void btnOpenUModel_Click(object sender, EventArgs e)
        {
            string path = txtPath.Text.Trim();
            if (File.Exists(path)) OpenExternal(path);
            else
            {
                string um = ConfigService.UModelPath;
                if (string.IsNullOrEmpty(um) || !File.Exists(um))
                    MessageBox.Show(this, "Select a mesh file first, or configure UModel.", "3D Viewer", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    Process.Start(new ProcessStartInfo { FileName = um, UseShellExecute = true });
            }
        }

        void panelPreview_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.FromArgb(18, 18, 22));
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            if (_mesh == null)
            {
                using (var font = new Font("Segoe UI", 9f))
                using (var br = new SolidBrush(Color.FromArgb(120, 130, 140)))
                    g.DrawString("Open an .obj (best) or .stl file to preview.\nFBX: export OBJ from Blender if binary.\n.uasset: use Open in UModel.", font, br, 12, 12);
                return;
            }
            // temporary mutate draw angles via SimpleMesh - pass rotation by adjusting draw
            // SimpleMesh.Draw uses fixed angles; we re-project here with _yaw/_pitch
            DrawMeshRotated(g, panelPreview.ClientRectangle);
        }

        void DrawMeshRotated(Graphics g, Rectangle view)
        {
            var verts = _mesh.Vertices;
            var faces = _mesh.Faces;
            if (verts.Count == 0) return;
            SimpleMesh.Point3 min, max;
            _mesh.GetBounds(out min, out max);
            float cx = (min.X + max.X) * 0.5f, cy = (min.Y + max.Y) * 0.5f, cz = (min.Z + max.Z) * 0.5f;
            float sx = Math.Max(max.X - min.X, 0.0001f), sy = Math.Max(max.Y - min.Y, 0.0001f), sz = Math.Max(max.Z - min.Z, 0.0001f);
            float scale = Math.Min(view.Width, view.Height) * 0.85f / Math.Max(sx, Math.Max(sy, sz));
            int ox = view.Width / 2, oy = view.Height / 2;
            float cosY = (float)Math.Cos(_yaw), sinY = (float)Math.Sin(_yaw);
            float cosP = (float)Math.Cos(_pitch), sinP = (float)Math.Sin(_pitch);
            var pts = new PointF[verts.Count];
            for (int i = 0; i < verts.Count; i++)
            {
                float x = verts[i].X - cx, y = verts[i].Y - cy, z = verts[i].Z - cz;
                float x1 = x * cosY - z * sinY;
                float z1 = x * sinY + z * cosY;
                float y1 = y * cosP - z1 * sinP;
                pts[i] = new PointF(ox + x1 * scale, oy - y1 * scale);
            }
            int mode = (int)CurrentMode();
            if (mode == 0)
            {
                using (var pen = new Pen(Color.FromArgb(140, 210, 255), 1f))
                    foreach (var f in faces)
                        for (int i = 0; i < f.Length; i++)
                        {
                            int a = f[i], b = f[(i + 1) % f.Length];
                            if (a >= 0 && b >= 0 && a < pts.Length && b < pts.Length)
                                g.DrawLine(pen, pts[a], pts[b]);
                        }
            }
            else
            {
                Color fill = mode == 1 ? Color.FromArgb(150, 150, 155) : Color.FromArgb(70, 160, 110);
                Color edge = mode == 1 ? Color.FromArgb(90, 90, 95) : Color.FromArgb(40, 100, 70);
                using (var br = new SolidBrush(fill))
                using (var pen = new Pen(edge))
                    foreach (var f in faces)
                        for (int i = 1; i + 1 < f.Length; i++)
                        {
                            int i0 = f[0], i1 = f[i], i2 = f[i + 1];
                            if (i0 < 0 || i1 < 0 || i2 < 0 || i0 >= pts.Length || i1 >= pts.Length || i2 >= pts.Length) continue;
                            var poly = new[] { pts[i0], pts[i1], pts[i2] };
                            float ax = poly[1].X - poly[0].X, ay = poly[1].Y - poly[0].Y;
                            float bx = poly[2].X - poly[0].X, by = poly[2].Y - poly[0].Y;
                            if (ax * by - ay * bx <= 0 && mode != 1) continue;
                            g.FillPolygon(br, poly);
                            g.DrawPolygon(pen, poly);
                        }
            }
        }

        void panelPreview_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_orbit) return;
            _yaw += (e.X - _lastMouse.X) * 0.01f;
            _pitch += (e.Y - _lastMouse.Y) * 0.01f;
            _lastMouse = e.Location;
            panelPreview.Invalidate();
        }

        void cmbMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblModeHint.Text = "Mode: " + CurrentMode() + (_mesh != null ? " — drag to orbit" : "");
            panelPreview.Invalidate();
        }

        void btnSpecVertMeshes_Click(object sender, EventArgs e)
        {
            string dir = Path.Combine(SpecVertService.DefaultWorkspaceRoot(), "Modules", "Meshes", "Exports");
            if (!Directory.Exists(dir)) dir = Path.Combine(SpecVertService.DefaultWorkspaceRoot(), "Modules", "Meshes");
            if (Directory.Exists(dir)) { txtPath.Text = dir; btnBrowseFolder_Click(sender, e); }
            else MessageBox.Show(this, "Run specVert first, or browse to an OBJ folder.", "3D Viewer", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        void btnImages_Click(object sender, EventArgs e)
        {
            using (var f = new ImageViewerForm()) f.ShowDialog(this);
        }

        void Log(string m) { txtLog.AppendText(m + Environment.NewLine); }
        void btnClose_Click(object sender, EventArgs e) => Close();
    }
}
