using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using SpaceBourne2ModTool.Core;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Forms
{
    /// <summary>
    /// Writes EvE replacement lines into the working folder. The game speaks from FMOD
    /// banks, so the WAV is kept as the source and the rebuilt bank is what gets shipped.
    /// </summary>
    public class EveVoiceForm : Form
    {
        readonly TextBox _name = new TextBox();
        readonly ComboBox _voice = new ComboBox();
        readonly TextBox _line = new TextBox();
        readonly TextBox _log = new TextBox();
        readonly string _bankHint;

        public EveVoiceForm(string relatedFile)
        {
            _bankHint = relatedFile;
            Text = "EvE voice lines";
            Width = 760;
            Height = 560;
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9f);
            AppBranding.Apply(this);
            Build();
            UiTheme.Apply(this);
            foreach (string v in EveVoiceService.InstalledVoices()) _voice.Items.Add(v);
            if (_voice.Items.Count > 0) _voice.SelectedIndex = 0;
            else _voice.Items.Add("(no Windows voices found — use specVoice)");
            _name.Text = "EveLine_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            if (!string.IsNullOrEmpty(relatedFile))
                _log.Text = "Related ripped file:\r\n" + relatedFile + "\r\n\r\n" + Explain();
            else
                _log.Text = Explain();
        }

        static string Explain()
        {
            return "EvE is the ship's voice. SpaceBourne 2 plays it from FMOD sound banks, not from loose WAV files.\r\n" +
                   "This form saves a WAV the same way specVoice does (an installed Windows voice). " +
                   "specVoice's own program is not built into this tool.\r\n" +
                   "To hear it in game: rebuild the matching .bank in FMOD, drop that bank in the working folder, then copy the bank into the open mod. " +
                   "Do not replace the game's original bank in the Steam folder.";
        }

        void Build()
        {
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(10) };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 140));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(root);

            var top = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
            top.Controls.Add(new Label { Text = "Line name", AutoSize = true, Padding = new Padding(0, 6, 4, 0) });
            _name.Width = 220;
            top.Controls.Add(_name);
            top.Controls.Add(new Label { Text = "Voice", AutoSize = true, Padding = new Padding(8, 6, 4, 0) });
            _voice.Width = 260;
            _voice.DropDownStyle = ComboBoxStyle.DropDownList;
            top.Controls.Add(_voice);
            root.Controls.Add(top, 0, 0);

            _line.Dock = DockStyle.Fill;
            _line.Multiline = true;
            _line.ScrollBars = ScrollBars.Vertical;
            root.Controls.Add(_line, 0, 1);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill };
            buttons.Controls.Add(Btn("Save WAV into working folder", (s, e) => Save()));
            buttons.Controls.Add(Btn("Open specVoice", (s, e) => OpenSpec()));
            buttons.Controls.Add(Btn("Find specVoice.exe", (s, e) => Find()));
            buttons.Controls.Add(Btn("Copy newest WAV into open mod", (s, e) => Stage()));
            root.Controls.Add(buttons, 0, 2);

            _log.Dock = DockStyle.Fill;
            _log.Multiline = true;
            _log.ReadOnly = true;
            _log.ScrollBars = ScrollBars.Vertical;
            root.Controls.Add(_log, 0, 3);
        }

        Button Btn(string text, EventHandler click)
        {
            var b = new Button { Text = text, AutoSize = true, Margin = new Padding(4, 4, 4, 4) };
            b.Click += click;
            return b;
        }

        void Save()
        {
            string err;
            string folder = WorkingProjectService.Ensure(out err);
            if (folder == null) { MessageBox.Show(this, err); return; }
            string voice = _voice.SelectedItem as string;
            if (voice != null && voice.StartsWith("(")) voice = null;
            string path = EveVoiceService.SaveWav(folder, _name.Text, voice, _line.Text, out err);
            if (path == null) { MessageBox.Show(this, err ?? "Could not save the WAV."); return; }
            _log.AppendText("\r\nSaved " + path + "\r\nThe text sits beside it as a .txt. The game will not speak this file until a new FMOD bank is built from it.");
        }

        void OpenSpec()
        {
            try { EveVoiceService.LaunchSpecVoice(); _log.AppendText("\r\nOpened specVoice. Save the WAV into the working folder's EvE\\Generated directory."); }
            catch (Exception ex) { MessageBox.Show(this, ex.Message); }
        }

        void Find()
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "specVoice (specVoice.exe)|specVoice.exe|Programs (*.exe)|*.exe";
                dlg.Title = "Point at specVoice.exe";
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                ConfigService.SpecVoicePath = dlg.FileName;
                ConfigService.Save();
                _log.AppendText("\r\nWill open " + dlg.FileName);
            }
        }

        void Stage()
        {
            string err;
            string folder = WorkingProjectService.Ensure(out err);
            if (folder == null) { MessageBox.Show(this, err); return; }
            string dir = EveVoiceService.LinesFolder(folder);
            string newest = null;
            DateTime when = DateTime.MinValue;
            foreach (string f in Directory.GetFiles(dir, "*.wav"))
            {
                var t = File.GetLastWriteTimeUtc(f);
                if (t >= when) { when = t; newest = f; }
            }
            if (newest == null) { MessageBox.Show(this, "No WAV has been saved yet."); return; }
            var item = WorkingProjectService.Inspect(folder, newest);
            string dest = WorkingProjectService.CopyIntoOpenMod(item, out err);
            if (err != null && err.StartsWith("ALREADY:"))
            {
                if (MessageBox.Show(this, "The open mod already has this WAV. Replace it?", "Replace", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    return;
                File.Copy(newest, err.Substring("ALREADY:".Length), true);
                _log.AppendText("\r\nReplaced the WAV in the open mod.");
                return;
            }
            if (err != null) { MessageBox.Show(this, err); return; }
            _log.AppendText("\r\nCopied into the open mod:\r\n" + dest + "\r\nShip the rebuilt .bank, not only this WAV, if you want EvE to say it in game.");
        }
    }
}
