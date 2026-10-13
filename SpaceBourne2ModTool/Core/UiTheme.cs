using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// Dark modern theme applied recursively (tabs, lists, text boxes, buttons, grids).
    /// </summary>
    public static class UiTheme
    {
        public static readonly Color Bg = Color.FromArgb(28, 28, 32);
        public static readonly Color BgPanel = Color.FromArgb(22, 24, 28);
        public static readonly Color BgInput = Color.FromArgb(36, 38, 44);
        public static readonly Color Fg = Color.FromArgb(230, 232, 236);
        public static readonly Color FgMuted = Color.FromArgb(170, 178, 190);
        public static readonly Color Accent = Color.FromArgb(100, 200, 255);
        public static readonly Color Btn = Color.FromArgb(50, 54, 62);
        public static readonly Color BtnBorder = Color.FromArgb(70, 76, 88);
        public static readonly Color TabSel = Color.FromArgb(40, 48, 60);

        public static void Apply(Form form)
        {
            if (form == null) return;
            form.BackColor = Bg;
            form.ForeColor = Fg;
            ApplyControl(form);
        }

        public static void ApplyControl(Control root)
        {
            if (root == null) return;
            try
            {
                if (root is Form || root is Panel || root is GroupBox || root is SplitContainer || root is TableLayoutPanel || root is FlowLayoutPanel)
                {
                    root.BackColor = Bg;
                    root.ForeColor = Fg;
                }
                if (root is TabControl tc)
                {
                    tc.DrawMode = TabDrawMode.OwnerDrawFixed;
                    tc.DrawItem -= Tab_DrawItem;
                    tc.DrawItem += Tab_DrawItem;
                    tc.BackColor = Bg;
                    tc.ForeColor = Fg;
                    foreach (TabPage page in tc.TabPages)
                    {
                        page.BackColor = Bg;
                        page.ForeColor = Fg;
                        page.UseVisualStyleBackColor = false;
                        ApplyControl(page);
                    }
                }
                if (root is TabPage)
                {
                    root.BackColor = Bg;
                    root.ForeColor = Fg;
                }
                if (root is TextBox || root is RichTextBox || root is ListBox || root is ListView || root is TreeView || root is ComboBox)
                {
                    root.BackColor = BgInput;
                    root.ForeColor = Fg;
                    if (root is TextBox tb) { tb.BorderStyle = BorderStyle.FixedSingle; }
                    if (root is ListBox lb) { lb.BorderStyle = BorderStyle.FixedSingle; }
                }
                if (root is DataGridView dgv)
                {
                    dgv.BackgroundColor = BgPanel;
                    dgv.GridColor = BtnBorder;
                    dgv.DefaultCellStyle.BackColor = BgInput;
                    dgv.DefaultCellStyle.ForeColor = Fg;
                    dgv.DefaultCellStyle.SelectionBackColor = TabSel;
                    dgv.DefaultCellStyle.SelectionForeColor = Fg;
                    dgv.ColumnHeadersDefaultCellStyle.BackColor = BgPanel;
                    dgv.ColumnHeadersDefaultCellStyle.ForeColor = Accent;
                    dgv.EnableHeadersVisualStyles = false;
                }
                if (root is Button b)
                {
                    b.FlatStyle = FlatStyle.Flat;
                    b.BackColor = Btn;
                    b.ForeColor = Fg;
                    b.FlatAppearance.BorderColor = BtnBorder;
                    b.FlatAppearance.BorderSize = 1;
                    b.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 90, 130);
                    b.UseVisualStyleBackColor = false;
                }
                if (root is Label lbl)
                {
                    if (lbl.Font != null && lbl.Font.Bold)
                        lbl.ForeColor = Accent;
                    else
                        lbl.ForeColor = FgMuted;
                    lbl.BackColor = Color.Transparent;
                }
                if (root is CheckBox || root is RadioButton)
                {
                    root.ForeColor = Fg;
                    root.BackColor = Bg;
                }
                if (root is StatusStrip || root is MenuStrip)
                {
                    root.BackColor = Color.FromArgb(20, 20, 24);
                    root.ForeColor = Fg;
                }
                if (root is ProgressBar)
                {
                    root.BackColor = BgPanel;
                }
            }
            catch { }

            foreach (Control child in root.Controls)
                ApplyControl(child);
        }

        static void Tab_DrawItem(object sender, DrawItemEventArgs e)
        {
            var tc = sender as TabControl;
            if (tc == null || e.Index < 0 || e.Index >= tc.TabCount) return;
            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            using (var bg = new SolidBrush(selected ? TabSel : BgPanel))
            using (var fg = new SolidBrush(selected ? Accent : Fg))
            {
                e.Graphics.FillRectangle(bg, e.Bounds);
                var flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
                TextRenderer.DrawText(e.Graphics, tc.TabPages[e.Index].Text, tc.Font, e.Bounds, fg.Color, flags);
            }
        }
    }
}
