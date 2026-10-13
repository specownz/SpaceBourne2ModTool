using System;
using System.Drawing;
using System.Windows.Forms;
using SpaceBourne2ModTool.Core;

namespace SpaceBourne2ModTool.Forms
{
    public partial class HelpSectionForm : Form
    {
        public HelpSectionForm()
        {
            InitializeComponent();
            AppBranding.Apply(this, AppBranding.ProductTitle + " — Help");
            FormScaleHelper.Enable(this);
            ApplyTheme();
            foreach (var kv in HelpContent.Topics())
                lst.Items.Add(kv.Key);
            if (lst.Items.Count > 0) lst.SelectedIndex = 0;
        }

        void ApplyTheme()
        {
            BackColor = Color.FromArgb(28, 28, 30);
            ForeColor = Color.White;
            lst.BackColor = Color.FromArgb(22, 24, 28);
            lst.ForeColor = Color.White;
            txt.BackColor = Color.FromArgb(22, 24, 28);
            txt.ForeColor = Color.FromArgb(210, 220, 210);
            foreach (Control c in Controls)
            {
                if (c is Button b) { b.FlatStyle = FlatStyle.Flat; b.BackColor = Color.FromArgb(50, 50, 58); b.ForeColor = Color.White; }
                if (c is Label) c.ForeColor = Color.FromArgb(180, 190, 200);
            }
            lblTitle.ForeColor = Color.FromArgb(100, 200, 255);
        }

        void lst_SelectedIndexChanged(object sender, EventArgs e)
        {
            string key = lst.SelectedItem as string;
            if (key == null) return;
            foreach (var kv in HelpContent.Topics())
            {
                if (kv.Key == key) { txt.Text = kv.Value; return; }
            }
        }

        public void SelectTopic(string titleContains)
        {
            if (string.IsNullOrEmpty(titleContains) || lst.Items.Count == 0) return;
            for (int i = 0; i < lst.Items.Count; i++)
            {
                string s = lst.Items[i] as string;
                if (s != null && s.IndexOf(titleContains, StringComparison.OrdinalIgnoreCase) >= 0)
                { lst.SelectedIndex = i; return; }
            }
        }

        void btnClose_Click(object sender, EventArgs e) => Close();
    }
}
