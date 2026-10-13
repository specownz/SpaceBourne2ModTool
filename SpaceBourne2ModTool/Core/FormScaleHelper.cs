using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// Auto-scale form controls on resize. Records design-time bounds at first enable,
    /// then proportionally moves/sizes child controls (skips MenuStrip/StatusStrip/docked fills).
    /// </summary>
    public static class FormScaleHelper
    {
        static readonly Dictionary<Form, ScaleState> States = new Dictionary<Form, ScaleState>();

        class ScaleState
        {
            public Size DesignClient;
            public readonly List<CtrlSnap> Snaps = new List<CtrlSnap>();
        }

        class CtrlSnap
        {
            public Control C;
            public Rectangle Bounds;
            public AnchorStyles Anchor;
            public DockStyle Dock;
        }

        public static void Enable(Form form)
        {
            if (form == null) return;
            form.Resize -= OnResize;
            form.Load -= OnLoad;
            form.Load += OnLoad;
            form.Resize += OnResize;
            form.MinimumSize = new Size(Math.Max(form.MinimumSize.Width, 320), Math.Max(form.MinimumSize.Height, 240));
        }

        static void OnLoad(object sender, EventArgs e)
        {
            var form = sender as Form;
            if (form == null) return;
            Capture(form);
        }

        static void Capture(Form form)
        {
            var st = new ScaleState { DesignClient = form.ClientSize };
            if (st.DesignClient.Width < 1 || st.DesignClient.Height < 1) return;
            SnapTree(form, st);
            States[form] = st;
            form.FormClosed -= OnClosed;
            form.FormClosed += OnClosed;
        }

        static void SnapTree(Control parent, ScaleState st)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is MenuStrip || c is StatusStrip) continue;
                if (c.Dock == DockStyle.Fill) { SnapTree(c, st); continue; }
                st.Snaps.Add(new CtrlSnap { C = c, Bounds = c.Bounds, Anchor = c.Anchor, Dock = c.Dock });
                if (c.HasChildren && !(c is DataGridView) && !(c is ListView) && !(c is TreeView))
                    SnapTree(c, st);
            }
        }

        static void OnResize(object sender, EventArgs e)
        {
            var form = sender as Form;
            if (form == null || form.WindowState == FormWindowState.Minimized) return;
            ScaleState st;
            if (!States.TryGetValue(form, out st) || st.DesignClient.Width < 1) { Capture(form); return; }
            float sx = (float)form.ClientSize.Width / st.DesignClient.Width;
            float sy = (float)form.ClientSize.Height / st.DesignClient.Height;
            if (sx < 0.2f || sy < 0.2f) return;
            form.SuspendLayout();
            try
            {
                foreach (var snap in st.Snaps)
                {
                    if (snap.C == null || snap.C.IsDisposed) continue;
                    if (snap.Dock != DockStyle.None) continue;
                    // Prefer Anchor when set; still scale for pure Top|Left designs
                    var b = snap.Bounds;
                    int x = (int)Math.Round(b.X * sx);
                    int y = (int)Math.Round(b.Y * sy);
                    int w = Math.Max(16, (int)Math.Round(b.Width * sx));
                    int h = Math.Max(12, (int)Math.Round(b.Height * sy));
                    try { snap.C.SetBounds(x, y, w, h); } catch { }
                }
            }
            finally { form.ResumeLayout(true); }
        }

        static void OnClosed(object sender, FormClosedEventArgs e)
        {
            var form = sender as Form;
            if (form != null) States.Remove(form);
        }
    }
}
