using Analogy.Implementation.Philips.Tools;
using Analogy.Interfaces;
using Analogy.LogViewer.Philips.Properties;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Analogy.LogViewer.Philips.CustomActions
{
    public class LogConfiguratorAction : IAnalogyCustomAction
    {
        public Action Action => OpenLogConfigurator;
        public Guid ID { get; } = new Guid("6808072B-8186-4BFC-9061-4FEB8E9BE472");
        public Image Image { get; } = Resources.PageSetup_32x32;
        public string Title { get; } = "External Log Configurator";
        private string logConfiguratorEXE = "LogConfigurator.exe";

        private void OpenLogConfigurator()
        {
            if (File.Exists(logConfiguratorEXE))
            {
                try
                {
                    Process.Start(logConfiguratorEXE);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            //LogConfiguratorForm log = new LogConfiguratorForm();
            //log.Show(this);
        }
    }


    public class FixCorruptedFilelAction : IAnalogyCustomAction
    {
        public Action Action => InternalAction;
        public Guid ID { get; } = new Guid("874C40CC-5FFA-4C8F-B494-2EAFB8EDC112");
        public Image Image { get; } = Resources.BreakingChange_32x32;
        public string Title { get; } = "Fix Corrupted XML File";

        private void InternalAction()
        {
            FixFileForm f = new FixFileForm();
            f.Show();

        }
    }


    public class SplunkAction : IAnalogyCustomAction
    {
        public Action Action => InternalAction;
        public Guid ID { get; } = new Guid("102AA3EA-A5AD-4C23-BD63-372542BD6A83");
        public Image Image { get; } = Resources.Convert_32x32;
        public string Title { get; } = "Splunk Convertor";

        private void InternalAction()
        {


        }
    }
}
