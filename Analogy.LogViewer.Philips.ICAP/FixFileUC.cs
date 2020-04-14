using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Analogy.LogViewer.Philips.ICAP
{
    public partial class FixFileUC : UserControl
    {
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private IProgress<string> reporter;
        private string result = string.Empty;
        private bool HasErrors;
        public FixFileUC()
        {
            InitializeComponent();
            reporter = new Progress<string>(s =>
            {
                richTextBox1.Text = richTextBox1.Text + Environment.NewLine + s;
                HasErrors = true;
            });
        }

        private async void btnOpenFile_Click(object sender, EventArgs e)
        {
            btnSave.Enabled = false;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Plain XML log file (*.log)|*.log";
            openFileDialog1.Title = @"Open File";
            openFileDialog1.Multiselect = false;
            cancellationTokenSource = new CancellationTokenSource();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                txtbFolder.Text = openFileDialog1.FileName;
                LogXmlLoader loader = new LogXmlLoader();

                result = await loader.FixFile(openFileDialog1.FileName, cancellationTokenSource.Token, reporter);
                if (HasErrors)
                    btnSave.Enabled = true;

            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog openFileDialog1 = new SaveFileDialog();
            openFileDialog1.Filter = "Plain XML log file (*.log)|*.log";
            openFileDialog1.Title = @"Save File";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(openFileDialog1.FileName, result);


            }
        }
    }
}
