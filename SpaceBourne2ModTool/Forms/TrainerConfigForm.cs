using System;
using System.Windows.Forms;
using SpaceBourne2ModTool.Core;

namespace SpaceBourne2ModTool.Forms
{
    public partial class TrainerConfigForm : Form
    {
        public TrainerConfigForm()
        {
            InitializeComponent();
            AppBranding.Apply(this);
            LoadOptions();
        }

        private void LoadOptions()
        {
            lstOptions.Items.Clear();
            foreach (var opt in TrainerTemplateGenerator.GetDefaultOptions())
            {
                lstOptions.Items.Add($"[{opt.Category}] {opt.DisplayName}  ({opt.ValueType})");
            }
        }

        private void btnClose_Click(object sender, EventArgs e) => Close();
    }
}
