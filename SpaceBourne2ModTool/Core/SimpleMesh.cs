using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>Lightweight mesh for in-app preview (OBJ primary; limited ASCII FBX vertices).</summary>
    public class SimpleMesh
    {
        public readonly List<Point3> Vertices = new List<Point3>();
        public readonly List<int[]> Faces = new List<int[]>(); // vertex indices
        public string SourcePath { get; private set; }
        public string FormatNote { get; private set; }

        public struct Point3
        {
            public float X, Y, Z;
            public Point3(float x, float y, float z) { X = x; Y = y; Z = z; }
        }

        public static SimpleMesh Load(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                throw new FileNotFoundException("Mesh file not found", path);
            string ext = Path.GetExtension(path).ToLowerInvariant();
            var m = new SimpleMesh { SourcePath = path };
            if (ext == ".obj") { m.LoadObj(path); m.FormatNote = "OBJ"; }
            else if (ext == ".fbx") { m.LoadFbxAsciiOrFail(path); }
            else if (ext == ".gltf" || ext == ".glb")
                throw new NotSupportedException("GLTF/GLB: export to OBJ or open in Blender/UModel.");
            else if (ext == ".uasset" || ext == ".psk" || ext == ".pskx")
                throw new NotSupportedException("Cooked/PSK: use Open in UModel for this format.");
            else if (ext == ".stl") { m.LoadStlAscii(path); m.FormatNote = "STL"; }
            else
                throw new NotSupportedException("Unsupported mesh type: " + ext + " — try OBJ or Open in UModel.");
            if (m.Vertices.Count == 0)
                throw new InvalidDataException("No vertices parsed from " + Path.GetFileName(path));
            return m;
        }

        void LoadObj(string path)
        {
            foreach (string raw in File.ReadLines(path))
            {
                string line = raw.Trim();
                if (line.StartsWith("v "))
                {
                    string[] p = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (p.Length >= 4)
                        Vertices.Add(new Point3(F(p[1]), F(p[2]), F(p[3])));
                }
                else if (line.StartsWith("f "))
                {
                    string[] p = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    var idx = new List<int>();
                    for (int i = 1; i < p.Length; i++)
                    {
                        string part = p[i].Split('/')[0];
                        int vi;
                        if (int.TryParse(part, out vi))
                        {
                            if (vi < 0) vi = Vertices.Count + vi + 1;
                            idx.Add(vi - 1);
                        }
                    }
                    if (idx.Count >= 3) Faces.Add(idx.ToArray());
                }
            }
        }

        void LoadStlAscii(string path)
        {
            Point3[] tri = new Point3[3];
            int ti = 0;
            foreach (string raw in File.ReadLines(path))
            {
                string line = raw.Trim();
                if (line.StartsWith("vertex", StringComparison.OrdinalIgnoreCase))
                {
                    string[] p = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (p.Length >= 4)
                    {
                        tri[ti++] = new Point3(F(p[1]), F(p[2]), F(p[3]));
                        if (ti == 3)
                        {
                            int b = Vertices.Count;
                            Vertices.Add(tri[0]); Vertices.Add(tri[1]); Vertices.Add(tri[2]);
                            Faces.Add(new[] { b, b + 1, b + 2 });
                            ti = 0;
                        }
                    }
                }
            }
            FormatNote = "STL ASCII";
        }

        void LoadFbxAsciiOrFail(string path)
        {
            // Binary FBX starts with "Kaydara FBX Binary"
            using (var fs = File.OpenRead(path))
            {
                byte[] hdr = new byte[20];
                int n = fs.Read(hdr, 0, 20);
                string head = System.Text.Encoding.ASCII.GetString(hdr, 0, n);
                if (head.StartsWith("Kaydara FBX Binary", StringComparison.Ordinal))
                    throw new NotSupportedException("Binary FBX: convert to OBJ in Blender, or open with Blender/UModel.");
            }
            // ASCII FBX: look for vertices-ish patterns is unreliable; try Vertices: arrays
            bool any = false;
            foreach (string raw in File.ReadLines(path))
            {
                string line = raw.Trim();
                // Very limited: "a: x,y,z,x,y,z,..."
                if (line.StartsWith("a:") && line.IndexOf(',') > 0)
                {
                    string data = line.Substring(2).Trim();
                    string[] nums = data.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (nums.Length >= 9 && nums.Length % 3 == 0)
                    {
                        for (int i = 0; i + 2 < nums.Length; i += 3)
                        {
                            float x, y, z;
                            if (float.TryParse(nums[i], NumberStyles.Float, CultureInfo.InvariantCulture, out x) &&
                                float.TryParse(nums[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out y) &&
                                float.TryParse(nums[i + 2], NumberStyles.Float, CultureInfo.InvariantCulture, out z))
                            {
                                Vertices.Add(new Point3(x, y, z));
                                any = true;
                            }
                        }
                    }
                }
            }
            if (!any)
                throw new NotSupportedException("Could not parse this FBX. Export OBJ from Blender (File → Export → OBJ).");
            // Point cloud faces: connect sequential triangles if count allows
            for (int i = 0; i + 2 < Vertices.Count; i += 3)
                Faces.Add(new[] { i, i + 1, i + 2 });
            FormatNote = "FBX ASCII (limited)";
        }

        static float F(string s)
        {
            float v;
            float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out v);
            return v;
        }

        public void GetBounds(out Point3 min, out Point3 max)
        {
            min = new Point3(float.MaxValue, float.MaxValue, float.MaxValue);
            max = new Point3(float.MinValue, float.MinValue, float.MinValue);
            foreach (var v in Vertices)
            {
                if (v.X < min.X) min.X = v.X; if (v.Y < min.Y) min.Y = v.Y; if (v.Z < min.Z) min.Z = v.Z;
                if (v.X > max.X) max.X = v.X; if (v.Y > max.Y) max.Y = v.Y; if (v.Z > max.Z) max.Z = v.Z;
            }
        }

        public void Draw(Graphics g, Rectangle view, int mode)
        {
            // mode: 0 wire, 1 unlit grey, 2 shaded green
            if (Vertices.Count == 0) return;
            Point3 min, max;
            GetBounds(out min, out max);
            float cx = (min.X + max.X) * 0.5f, cy = (min.Y + max.Y) * 0.5f, cz = (min.Z + max.Z) * 0.5f;
            float sx = Math.Max(max.X - min.X, 0.0001f), sy = Math.Max(max.Y - min.Y, 0.0001f), sz = Math.Max(max.Z - min.Z, 0.0001f);
            float scale = Math.Min(view.Width, view.Height) * 0.85f / Math.Max(sx, Math.Max(sy, sz));
            int ox = view.X + view.Width / 2, oy = view.Y + view.Height / 2;
            // simple yaw/pitch rotation for depth
            float yaw = 0.6f, pitch = 0.35f;
            float cosY = (float)Math.Cos(yaw), sinY = (float)Math.Sin(yaw);
            float cosP = (float)Math.Cos(pitch), sinP = (float)Math.Sin(pitch);

            PointF Project(Point3 v)
            {
                float x = v.X - cx, y = v.Y - cy, z = v.Z - cz;
                float x1 = x * cosY - z * sinY;
                float z1 = x * sinY + z * cosY;
                float y1 = y * cosP - z1 * sinP;
                float z2 = y * sinP + z1 * cosP;
                return new PointF(ox + x1 * scale, oy - y1 * scale);
            }

            var pts = new PointF[Vertices.Count];
            for (int i = 0; i < Vertices.Count; i++) pts[i] = Project(Vertices[i]);

            if (mode == 0)
            {
                using (var pen = new Pen(Color.FromArgb(140, 210, 255), 1f))
                {
                    foreach (var f in Faces)
                    {
                        for (int i = 0; i < f.Length; i++)
                        {
                            int a = f[i], b = f[(i + 1) % f.Length];
                            if (a >= 0 && b >= 0 && a < pts.Length && b < pts.Length)
                                g.DrawLine(pen, pts[a], pts[b]);
                        }
                    }
                }
            }
            else
            {
                Color fill = mode == 1 ? Color.FromArgb(140, 140, 145) : Color.FromArgb(70, 150, 100);
                Color edge = mode == 1 ? Color.FromArgb(80, 80, 85) : Color.FromArgb(40, 100, 70);
                using (var br = new SolidBrush(fill))
                using (var pen = new Pen(edge, 1f))
                {
                    foreach (var f in Faces)
                    {
                        if (f.Length < 3) continue;
                        // fan triangulate
                        for (int i = 1; i + 1 < f.Length; i++)
                        {
                            int i0 = f[0], i1 = f[i], i2 = f[i + 1];
                            if (i0 < 0 || i1 < 0 || i2 < 0 || i0 >= pts.Length || i1 >= pts.Length || i2 >= pts.Length) continue;
                            var poly = new[] { pts[i0], pts[i1], pts[i2] };
                            // backface approx via cross z
                            float ax = poly[1].X - poly[0].X, ay = poly[1].Y - poly[0].Y;
                            float bx = poly[2].X - poly[0].X, by = poly[2].Y - poly[0].Y;
                            if (ax * by - ay * bx > 0 || mode == 1)
                            {
                                g.FillPolygon(br, poly);
                                g.DrawPolygon(pen, poly);
                            }
                        }
                    }
                }
            }
        }
    }
}
