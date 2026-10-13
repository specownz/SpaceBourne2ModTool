using System;
using System.Drawing;
using System.Windows.Forms;
using SpaceBourne2ModTool.Core;

namespace SpaceBourne2ModTool.Forms
{
    public partial class BeginnerHelpForm : Form
    {
        public BeginnerHelpForm()
        {
            InitializeComponent();
            AppBranding.Apply(this, AppBranding.ProductTitle + " — Beginner Guide");
            FormScaleHelper.Enable(this);
            BackColor = Color.FromArgb(28, 28, 30);
            ForeColor = Color.White;
            lst.BackColor = Color.FromArgb(22, 24, 28);
            lst.ForeColor = Color.White;
            txt.BackColor = Color.FromArgb(22, 24, 28);
            txt.ForeColor = Color.FromArgb(210, 220, 210);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.BackColor = Color.FromArgb(50, 50, 58);
            btnClose.ForeColor = Color.White;
            lblTitle.ForeColor = Color.FromArgb(100, 200, 255);
            foreach (var kv in BeginnerGuide.All())
                lst.Items.Add(kv.Key);
            if (lst.Items.Count > 0) lst.SelectedIndex = 0;
        }

        void lst_SelectedIndexChanged(object sender, EventArgs e)
        {
            string key = lst.SelectedItem as string;
            if (key == null) return;
            var all = BeginnerGuide.All();
            txt.Text = all.ContainsKey(key) ? all[key] : BeginnerGuide.WhatIsAMod;
        }

        void btnClose_Click(object sender, EventArgs e) => Close();
    }
}
