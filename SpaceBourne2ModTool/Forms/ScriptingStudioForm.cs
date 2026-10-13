using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using SpaceBourne2ModTool.Core;

namespace SpaceBourne2ModTool.Forms
{
    public partial class ScriptingStudioForm : Form
    {
        public ScriptingStudioForm()
        {
            InitializeComponent();
            AppBranding.Apply(this, AppBranding.ProductTitle + " — Live scripts and programming notes");
            FormScaleHelper.Enable(this);
            ApplyTheme();
            txtHelp.Text = ScriptingTemplates.Ue4ssInstallGuide();
            cmbKind.Items.Clear();
            cmbKind.Items.AddRange(new object[] {
                "Read me first (file mods vs live scripts)",
                "UE4SS install and safety guide",
                "Hello script (print when the game starts)",
                "Safer delayed-hook script template",
                "Key press template (uses game thread correctly)",
                "Pre and post function hook template",
                "Install hooks after game state starts",
                "Unreal Editor Python sample (editor only)",
                "Find object API template (FindFirstOf / StaticFindObject)",
                "C# notes (desktop tools, not in-game)"
            });
            cmbKind.SelectedIndex = 0;
            RefreshOutput();
            string w = Ue4ssProjectService.TryFindGameWin64();
            lblWin64.Text = string.IsNullOrEmpty(w)
                ? "Game Win64 folder: not found yet — set the game path on the home screen first."
                : "Suggested game program folder (Win64): " + w;
        }

        void ApplyTheme()
        {
            BackColor = Color.FromArgb(28, 28, 30);
            ForeColor = Color.White;
            foreach (Control c in Controls)
            {
                if (c is TextBox tb) { tb.BackColor = Color.FromArgb(22, 24, 28); tb.ForeColor = Color.FromArgb(210, 220, 210); }
                if (c is ComboBox) { c.BackColor = Color.FromArgb(40, 40, 48); c.ForeColor = Color.White; }
                if (c is Button b) { b.FlatStyle = FlatStyle.Flat; b.BackColor = Color.FromArgb(50, 50, 58); b.ForeColor = Color.White; }
                if (c is Label) c.ForeColor = Color.FromArgb(180, 190, 200);
            }
            lblTitle.ForeColor = Color.FromArgb(100, 200, 255);
        }

        void RefreshOutput()
        {
            string name = string.IsNullOrWhiteSpace(txtModName.Text) ? "MyLiveScriptMod" : txtModName.Text.Trim();
            switch (cmbKind.SelectedIndex)
            {
                case 1: txtOut.Text = ScriptingTemplates.Ue4ssInstallGuide(); break;
                case 2: txtOut.Text = ScriptingTemplates.Ue4ssHelloLua(name); break;
                case 3: txtOut.Text = ScriptingTemplates.Ue4ssHookSafeTemplate(name); break;
                case 4: txtOut.Text = ScriptingTemplates.Ue4ssKeybindTemplate(name); break;
                case 5: txtOut.Text = ScriptingTemplates.Ue4ssPrePostHookTemplate(name); break;
                case 6: txtOut.Text = ScriptingTemplates.Ue4ssInitGameStateTemplate(name); break;
                case 7: txtOut.Text = ScriptingTemplates.PythonEditorSample(); break;
                case 8: txtOut.Text = ScriptingTemplates.Ue4ssFindObjectTemplate(name); break;
                case 9: txtOut.Text = ScriptingTemplates.CsharpPluginNotes(); break;
                default: txtOut.Text = ScriptingTemplates.ComparisonGuide(); break;
            }
        }

        void cmbKind_SelectedIndexChanged(object sender, EventArgs e) => RefreshOutput();
        void btnGenerate_Click(object sender, EventArgs e) => RefreshOutput();

        void btnExportFolder_Click(object sender, EventArgs e)
        {
            string name = string.IsNullOrWhiteSpace(txtModName.Text) ? "MyLiveScriptMod" : txtModName.Text.Trim();
            int kind = 0;
            if (cmbKind.SelectedIndex == 3) kind = 1;
            else if (cmbKind.SelectedIndex == 4) kind = 2;
            else if (cmbKind.SelectedIndex == 5) kind = 3;
            else if (cmbKind.SelectedIndex == 6) kind = 4;
            else if (cmbKind.SelectedIndex == 8) kind = 5;
            try
            {
                string root = Ue4ssProjectService.CreateLuaModProject(name, kind);
                MessageBox.Show(this,
                    "A ready-to-copy live script folder was created here:\n\n" + root +
                    "\n\nInside you will find:\n" +
                    "• scripts/main.lua (the script)\n" +
                    "• mods_txt_line.txt (paste into UE4SS Mods/mods.txt)\n" +
                    "• README_PLAIN_ENGLISH.txt (step-by-step install)\n\n" +
                    "UE4SS itself is a separate download. This program only builds the mod folder.",
                    "Live script folder created", MessageBoxButtons.OK, MessageBoxIcon.Information);
                try { System.Diagnostics.Process.Start("explorer.exe", "\"" + root + "\""); } catch { }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Could not create the folder:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void btnSave_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                int i = cmbKind.SelectedIndex;
                sfd.Filter = i == 5 ? "Python (*.py)|*.py|All|*.*" :
                    (i >= 2 && i <= 4) ? "Lua (*.lua)|*.lua|All|*.*" : "Text (*.txt)|*.txt|All|*.*";
                sfd.FileName = i == 5 ? "editor_sample.py" : (i >= 2 && i <= 4) ? "main.lua" : "scripting_notes.txt";
                if (sfd.ShowDialog(this) != DialogResult.OK) return;
                File.WriteAllText(sfd.FileName, txtOut.Text);
                MessageBox.Show(this,
                    "Saved.\n\nIf this is a live Lua script, prefer \"Create ready UE4SS folder\" so the scripts/main.lua layout is correct.",
                    "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        void btnClose_Click(object sender, EventArgs e) => Close();
    }
}
