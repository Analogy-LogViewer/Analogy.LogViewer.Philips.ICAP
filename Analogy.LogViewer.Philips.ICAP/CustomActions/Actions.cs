using Analogy.Interfaces;
using Analogy.Interfaces.DataTypes;
using Analogy.LogViewer.Philips.ICAP.Properties;
using Analogy.LogViewer.Philips.ICAP.Tools;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Analogy.LogViewer.Philips.ICAP.CustomActions
{
    public class LogConfiguratorAction : IAnalogyCustomAction
    {
        public Action Action => OpenLogConfigurator;
        public Guid Id { get; set; } = new Guid("6808072B-8186-4BFC-9061-4FEB8E9BE472");
        public Image LargeImage { get; set; } = Resources.PageSetup_32x32;
        public Image SmallImage { get; set; } = Resources.PageSetup_16x16;

        public string Title { get; set; } = "External Log Configurator";
        public AnalogyCustomActionType Type { get; } = AnalogyCustomActionType.BelongsToProvider;
        public AnalogyToolTip? ToolTip { get; set; }
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
        public Guid Id { get; set; } = new Guid("874C40CC-5FFA-4C8F-B494-2EAFB8EDC112");
        public Image LargeImage { get; set; } = Resources.BreakingChange_32x32;
        public Image SmallImage { get; set; } = Resources.BreakingChange_16x16;
        public string Title { get; set; } = "Fix Corrupted XML File";
        public AnalogyCustomActionType Type { get; } = AnalogyCustomActionType.BelongsToProvider;
        public AnalogyToolTip? ToolTip { get; set; }

        private void InternalAction()
        {
            FixFileForm f = new FixFileForm();
            f.Show();

        }
    }


    public class SplunkAction : IAnalogyCustomAction
    {
        public Action Action => InternalAction;
        public Guid Id { get; set; } = new Guid("102AA3EA-A5AD-4C23-BD63-372542BD6A83");

        public Image LargeImage { get; set; } = Resources.Convert_32x32;
        public Image SmallImage { get; set; } = Resources.Convert_16x16;
        public string Title { get; set; } = "Splunk Convertor";
        public AnalogyCustomActionType Type { get; } = AnalogyCustomActionType.BelongsToProvider;
        public AnalogyToolTip? ToolTip { get; set; }

        private void InternalAction()
        {


        }
    }
}
