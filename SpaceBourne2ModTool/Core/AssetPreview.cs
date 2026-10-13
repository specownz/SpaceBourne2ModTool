using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>Side preview for a ripped file: image, simple mesh, or a short note.</summary>
    public class AssetPreview : Panel
    {
        Image _image;
        SimpleMesh _mesh;
        string _note = "Select a ripped file.\r\nImages and OBJ or STL models preview here.\r\nCooked .uasset files still open in UAssetGUI.";

        public AssetPreview()
        {
            DoubleBuffered = true;
            BackColor = Color.FromArgb(16, 16, 20);
            ForeColor = Color.White;
            Padding = new Padding(8);
        }

        public void ShowFile(string path)
        {
            Clear();
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) { _note = "File is not on disk."; Invalidate(); return; }
            string ext = Path.GetExtension(path).ToLowerInvariant();
            try
            {
                if (ext == ".png" || ext == ".bmp" || ext == ".jpg" || ext == ".jpeg" || ext == ".gif")
                {
                    var info = new FileInfo(path);
                    if (info.Length > 40L * 1024 * 1024) { _note = "Image is over 40 MB. Use Open this file."; }
                    else
                    {
                        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            _image = Image.FromStream(fs);
                        _note = Path.GetFileName(path) + "\r\n" + _image.Width + " × " + _image.Height;
                    }
                }
                else if (ext == ".obj" || ext == ".stl")
                {
                    var info = new FileInfo(path);
                    if (info.Length > 25L * 1024 * 1024) _note = "Model is over 25 MB. Use Open this file.";
                    else
                    {
                        _mesh = SimpleMesh.Load(path);
                        _note = Path.GetFileName(path) + "\r\n" + _mesh.Vertices.Count + " points, " + _mesh.Faces.Count + " faces";
                    }
                }
                else _note = Path.GetFileName(path) + "\r\nNo picture for this type.\r\nUse Open this file for the full viewer or editor.";
            }
            catch (Exception ex) { _note = ex.Message; }
            Invalidate();
        }

        void Clear()
        {
            if (_image != null) { _image.Dispose(); _image = null; }
            _mesh = null;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.Clear(BackColor);
            var area = new Rectangle(8, 8, Math.Max(10, Width - 16), Math.Max(10, Height - 72));
            if (_image != null)
            {
                float sx = area.Width / (float)_image.Width;
                float sy = area.Height / (float)_image.Height;
                float s = Math.Min(sx, sy);
                int w = Math.Max(1, (int)(_image.Width * s));
                int h = Math.Max(1, (int)(_image.Height * s));
                g.DrawImage(_image, area.X + (area.Width - w) / 2, area.Y + (area.Height - h) / 2, w, h);
            }
            else if (_mesh != null && _mesh.Vertices.Count > 0)
                DrawMesh(g, area);
            var note = new Rectangle(8, Height - 64, Math.Max(10, Width - 16), 56);
            TextRenderer.DrawText(g, _note ?? "", Font, note, Color.White,
                TextFormatFlags.WordBreak | TextFormatFlags.Left | TextFormatFlags.Top);
        }

        void DrawMesh(Graphics g, Rectangle area)
        {
            float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
            for (int i = 0; i < _mesh.Vertices.Count; i++)
            {
                var p = _mesh.Vertices[i];
                if (p.X < minX) minX = p.X; if (p.X > maxX) maxX = p.X;
                if (p.Y < minY) minY = p.Y; if (p.Y > maxY) maxY = p.Y;
            }
            float dx = Math.Max(0.0001f, maxX - minX);
            float dy = Math.Max(0.0001f, maxY - minY);
            using (var pen = new Pen(Color.FromArgb(140, 200, 255)))
            {
                int drawn = 0;
                foreach (var face in _mesh.Faces)
                {
                    if (drawn++ > 4000) break;
                    if (face == null || face.Length < 2) continue;
                    Point a = Project(face[0], area, minX, minY, dx, dy);
                    for (int i = 1; i < face.Length; i++)
                    {
                        Point b = Project(face[i], area, minX, minY, dx, dy);
                        g.DrawLine(pen, a, b);
                        a = b;
                    }
                    g.DrawLine(pen, a, Project(face[0], area, minX, minY, dx, dy));
                }
            }
        }

        Point Project(int index, Rectangle area, float minX, float minY, float dx, float dy)
        {
            if (index < 0 || index >= _mesh.Vertices.Count) return area.Location;
            var p = _mesh.Vertices[index];
            int x = area.X + (int)(((p.X - minX) / dx) * (area.Width - 4));
            int y = area.Bottom - (int)(((p.Y - minY) / dy) * (area.Height - 4));
            return new Point(x, y);
        }
    }
}
