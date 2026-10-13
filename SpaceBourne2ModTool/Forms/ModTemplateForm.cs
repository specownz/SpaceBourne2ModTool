using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SpaceBourne2ModTool.Core;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Forms
{
    public partial class ModTemplateForm : Form
    {
        public ModTemplateForm()
        {
            InitializeComponent();
            AppBranding.Apply(this, AppBranding.ProductTitle + " — Mod Templates");
            ApplyTheme();
            ReloadList();
        }

        void ApplyTheme()
        {
            BackColor = Color.FromArgb(28, 28, 30);
            ForeColor = Color.White;
            lst.BackColor = Color.FromArgb(22, 24, 28);
            lst.ForeColor = Color.White;
            txtDetails.BackColor = Color.FromArgb(22, 24, 28);
            txtDetails.ForeColor = Color.FromArgb(200, 220, 200);
            txtModName.BackColor = Color.FromArgb(40, 40, 48);
            txtModName.ForeColor = Color.White;
            foreach (Control c in Controls)
            {
                if (c is Button b) { b.FlatStyle = FlatStyle.Flat; b.BackColor = Color.FromArgb(50, 50, 58); b.ForeColor = Color.White; }
                if (c is Label) c.ForeColor = Color.FromArgb(180, 190, 200);
            }
            lblTitle.ForeColor = Color.FromArgb(100, 200, 255);
        }

        void ReloadList()
        {
            lst.Items.Clear();
            foreach (var t in ModTemplateService.All().OrderBy(x => x.Category).ThenBy(x => x.Name))
                lst.Items.Add(t);
            lst.DisplayMember = "Name";
        }

        void lst_SelectedIndexChanged(object sender, EventArgs e)
        {
            var t = lst.SelectedItem as ModTemplateService.Template;
            if (t == null) { txtDetails.Clear(); return; }
            txtDetails.Text = string.Format(
                "[{0}] {1}\r\nCategory: {2}\r\nLoad order: {3}\r\nSpecVerted: {4}\r\nUser template: {5}\r\n\r\n{6}\r\n\r\nFolders:\r\n  {7}\r\n\r\nWorkflow:\r\n{8}",
                t.Id, t.Name, t.Category, t.DefaultLoadOrder, t.SpecVerted, t.IsUser, t.Description,
                string.Join("\r\n  ", t.StagingFolders ?? new string[0]), t.WorkflowNotes);
        }

        void btnCreate_Click(object sender, EventArgs e)
        {
            var t = lst.SelectedItem as ModTemplateService.Template;
            if (t == null) { MessageBox.Show(this, "Select a template."); return; }
            string name = (txtModName.Text ?? "").Trim();
            if (string.IsNullOrEmpty(name)) name = t.Id + "_Mod";
            try
            {
                string root = ModTemplateService.CreateModFromTemplate(t, name);
                MessageBox.Show(this, "Mod created and template applied:\n\n" + root + "\n\nOpen with Object Editors / Speckage / Mod Loader next.",
                    "Template", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        void btnApplyCurrent_Click(object sender, EventArgs e)
        {
            var t = lst.SelectedItem as ModTemplateService.Template;
            if (t == null) { MessageBox.Show(this, "Select a template."); return; }
            if (!CurrentModContext.IsOpen)
            {
                MessageBox.Show(this, "Open a mod first (File → Open Mod Folder).");
                return;
            }
            try
            {
                string log = ModTemplateService.Apply(t, CurrentModContext.ModRoot, null, this);
                MessageBox.Show(this, log, "Applied", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        void btnSaveUser_Click(object sender, EventArgs e)
        {
            if (!CurrentModContext.IsOpen)
            {
                MessageBox.Show(this, "Open a mod to capture as a custom template.");
                return;
            }
            string name = (txtModName.Text ?? "").Trim();
            if (string.IsNullOrEmpty(name)) name = "MyTemplate";
            try
            {
                var t = ModTemplateService.CaptureFromMod(CurrentModContext.ModRoot, name);
                ReloadList();
                MessageBox.Show(this, "Saved user template: " + t.Name + "\n" + ModTemplateService.UserTemplatesDir);
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message); }
        }

        void btnDeleteUser_Click(object sender, EventArgs e)
        {
            var t = lst.SelectedItem as ModTemplateService.Template;
            if (t == null || !t.IsUser) { MessageBox.Show(this, "Select a user template to delete."); return; }
            ModTemplateService.DeleteUserTemplate(t.Id);
            ReloadList();
        }

        void btnOpenDir_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = "\"" + ModTemplateService.UserTemplatesDir + "\"",
                UseShellExecute = true
            });
        }

        void btnClose_Click(object sender, EventArgs e) => Close();
    }
}
