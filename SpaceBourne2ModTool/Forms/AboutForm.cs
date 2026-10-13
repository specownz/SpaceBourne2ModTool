using System;
using System.Drawing;
using System.Windows.Forms;
using SpaceBourne2ModTool.Core;

namespace SpaceBourne2ModTool.Forms
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
            AppBranding.Apply(this, AppBranding.ProductTitle + " — About");
            ApplyContent();
            ApplyTheme();
        }

        private void ApplyContent()
        {
            lblTitle.Text = AppBranding.ProductTitle;
            lblVersion.Text = "Version " + AppBranding.Version;
            lblSubtitle.Text = AppBranding.Description;
            txtBody.Text = AppBranding.AboutBodyText();
            lblTagline.Text = AppBranding.Tagline;

            try
            {
                if (AppBranding.LogoImage != null)
                {
                    picLogo.Image = AppBranding.LogoImage;
                    picLogo.SizeMode = PictureBoxSizeMode.Zoom;
                }
            }
            catch { }
        }

        private void ApplyTheme()
        {
            BackColor = Color.FromArgb(24, 24, 28);
            ForeColor = Color.White;
            txtBody.BackColor = Color.FromArgb(32, 32, 38);
            txtBody.ForeColor = Color.FromArgb(220, 220, 225);
            lblTitle.ForeColor = Color.FromArgb(120, 200, 255);
            lblVersion.ForeColor = Color.FromArgb(180, 180, 190);
            lblSubtitle.ForeColor = Color.FromArgb(200, 200, 210);
            lblTagline.ForeColor = Color.FromArgb(160, 140, 200);
            lblGrok.ForeColor = Color.FromArgb(140, 200, 160);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.BackColor = Color.FromArgb(50, 50, 58);
            btnClose.ForeColor = Color.White;
        }

        private void btnClose_Click(object sender, EventArgs e) => Close();
    }
}
