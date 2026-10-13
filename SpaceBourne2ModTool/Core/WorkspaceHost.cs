using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// Full-screen tab workspace. Tool windows are embedded so they use the area
    /// beside the Quickbar instead of opening as small modal boxes.
    /// </summary>
    public static class WorkspaceHost
    {
        static TabControl _tabs;
        static TabPage _home;

        public static void Attach(Control content, Panel[] homePanels)
        {
            if (content == null || _tabs != null) return;
            _tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                DrawMode = TabDrawMode.OwnerDrawFixed,
                SizeMode = TabSizeMode.Fixed,
                ItemSize = new Size(168, 28),
                Padding = new Point(10, 4),
                Font = new Font("Segoe UI", 9f)
            };
            _tabs.DrawItem += DrawTab;
            var host = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(28, 28, 30) };
            if (homePanels != null)
            {
                foreach (var p in homePanels)
                {
                    if (p == null) continue;
                    p.Parent = host;
                    p.Dock = DockStyle.Fill;
                }
            }
            _home = new TabPage("Home") { BackColor = host.BackColor, Tag = "home" };
            _home.Controls.Add(host);
            _tabs.TabPages.Add(_home);
            content.Controls.Add(_tabs);
            _tabs.BringToFront();
        }

        public static void ShowHome()
        {
            if (_tabs != null && _home != null) _tabs.SelectedTab = _home;
        }

        public static void Open(Form tool)
        {
            if (tool == null) return;
            if (_tabs == null)
            {
                tool.StartPosition = FormStartPosition.CenterScreen;
                tool.WindowState = FormWindowState.Maximized;
                tool.Show();
                return;
            }
            string key = tool.GetType().FullName + "|" + (tool.Tag as string ?? "");
            foreach (TabPage page in _tabs.TabPages)
            {
                if (!string.Equals(page.Tag as string, key, StringComparison.Ordinal)) continue;
                tool.Dispose();
                _tabs.SelectedTab = page;
                return;
            }
            var tab = new TabPage(Trim(tool.Text)) { Tag = key, BackColor = Color.FromArgb(28, 28, 30), ToolTipText = tool.Text };
            tool.TopLevel = false;
            tool.FormBorderStyle = FormBorderStyle.None;
            tool.Dock = DockStyle.Fill;
            tool.FormClosed += (s, e) => { if (tab.Parent != null) _tabs.TabPages.Remove(tab); };
            tab.Controls.Add(tool);
            _tabs.TabPages.Add(tab);
            _tabs.SelectedTab = tab;
            tool.Show();
        }

        public static void CloseActive()
        {
            if (_tabs == null || _tabs.SelectedTab == null || _tabs.SelectedTab == _home) return;
            var form = _tabs.SelectedTab.Controls.Count > 0 ? _tabs.SelectedTab.Controls[0] as Form : null;
            if (form != null) form.Close();
            else _tabs.TabPages.Remove(_tabs.SelectedTab);
        }

        static string Trim(string text)
        {
            if (string.IsNullOrEmpty(text)) return "Tool";
            return text.Length > 28 ? text.Substring(0, 26) + "…" : text;
        }

        static void DrawTab(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _tabs.TabPages.Count) return;
            bool on = e.Index == _tabs.SelectedIndex;
            var r = e.Bounds;
            using (var b = new SolidBrush(on ? Color.FromArgb(40, 90, 150) : Color.FromArgb(24, 24, 28)))
                e.Graphics.FillRectangle(b, r);
            TextRenderer.DrawText(e.Graphics, _tabs.TabPages[e.Index].Text, _tabs.Font, r, Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
    }
}
