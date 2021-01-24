using Analogy.Interfaces;
using Analogy.LogViewer.Philips.ICAP.Managers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Analogy.LogViewer.Philips.ICAP.DataSources
{
    public class OfflineICAPLog : Template.OfflineDataProvider
    {
        public bool IsConnected { get; set; }

        public override Guid Id { get; set; } = new Guid("AEDA3F2C-EEBF-4280-AD52-94A94BFBE6C5");
        public override string OptionalTitle { get; set; } = "ICAP Offline Parser";

        public override bool CanSaveToLogFile { get; set; } = true;
        public override string FileOpenDialogFilters { get; set; } = "All supported log file types|*.log;*.etl;*.nlog;*.json|Plain ICAP XML log file (*.log)|*.log|JSON file (*.json)|*.json|NLOG file (*.nlog)|*.nlog|ETW log file (*.etl)|*.etl";
        public override bool DisableFilePoolingOption { get; set; } = false;

        public override string FileSaveDialogFilters =>
            "Plain XML log file (*.log)|*.log|JSON file (*.json)|*.json|Zipped XML log file (*.zip)|*.zip|ETW log file (*.etl)|*.etl";

        public override IEnumerable<string> SupportFormats { get; set; } = new[] { "*.etl", "*.log", "*.nlog", "*.json" };
        public override string InitialFolderFullPath { get; set; } = string.Empty;//Path.Combine("", "data");
        public override bool UseCustomColors { get; set; } = false;
        public override IEnumerable<(string originalHeader, string replacementHeader)> GetReplacementHeaders()
            => Array.Empty<(string, string)>();

        public override (Color backgroundColor, Color foregroundColor) GetColorForMessage(IAnalogyLogMessage logMessage)
            => (Color.Empty, Color.Empty);

        public override async Task InitializeDataProviderAsync(IAnalogyLogger logger)
        {
            await base.InitializeDataProviderAsync(logger);
            LogManager.Instance.SetLogger(logger);
        }

        public override void MessageOpened(AnalogyLogMessage message)
        {
            //nop
        }
        public override async Task<IEnumerable<AnalogyLogMessage>> Process(string fileName, CancellationToken token, ILogMessageCreatedHandler messagesHandler)
        {
            //todo

            //if (fileName.EndsWith(".etl", StringComparison.InvariantCultureIgnoreCase))
            //{
            //    ICAPLogLoader logLoader = new ICAPLogLoader();
            //    return await logLoader.ReadFromFile(fileName, token, messagesHandler).ConfigureAwait(false);

            //}

            if (fileName.EndsWith(".log", StringComparison.InvariantCultureIgnoreCase))
            {
                LogLoader logLoader = new LogXmlLoader();
                return await logLoader.ReadFromFile(fileName, token, messagesHandler).ConfigureAwait(false);
            }
            if (fileName.EndsWith(".nlog", StringComparison.InvariantCultureIgnoreCase))
            {
                LogLoader logLoader = new NLogLoader();
                return await logLoader.ReadFromFile(fileName, token, messagesHandler).ConfigureAwait(false);

            }
            if (fileName.EndsWith(".json", StringComparison.InvariantCultureIgnoreCase))
            {
                LogLoader logLoader = new JSonLoader();
                return await logLoader.ReadFromFile(fileName, token, messagesHandler).ConfigureAwait(false);
            }
            else
            {
                AnalogyLogMessage m = new AnalogyLogMessage();
                m.Text = $"Unsupported file: {fileName}. Skipping file";
                m.Level = AnalogyLogLevel.Critical;
                m.Source = "Analogy";
                m.Module = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
                m.ProcessId = System.Diagnostics.Process.GetCurrentProcess().Id;
                m.Class = AnalogyLogClass.General;
                m.User = Environment.UserName;
                m.Date = DateTime.Now;
                messagesHandler.AppendMessage(m, Environment.MachineName);
                return new List<AnalogyLogMessage>() { m };
            }
        }


        public override Task SaveAsync(List<AnalogyLogMessage> messages, string fileName) => Task.Factory.StartNew(() =>
             {

                 if (fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                 {
                     string tempFileName = Path.GetTempFileName();
                     using (FileStream flStream = new FileStream(tempFileName, FileMode.Create, FileAccess.ReadWrite,
                         FileShare.ReadWrite))
                     {
                         //todo
                         //LogFormatter logFormatter = new LogXmlFormatter(flStream);
                         //Saver.SaveData(messages, logFormatter);
                         //logFormatter.Close();
                     }

                     using (FileStream flOrigStream = new FileStream(tempFileName, FileMode.Open, FileAccess.Read,
                         FileShare.ReadWrite))
                     {
                         using (FileStream flStream = new FileStream(fileName, FileMode.Create,
                             FileAccess.ReadWrite, FileShare.ReadWrite))
                         {
                             //todo
                             //ZIPLib.ZIPObject zip =
                             //    new ZIPLib.ZIPObject(flStream, ZIPLib.ZIPObject.ZIPFileOperationEnum.ZIP);
                             //zip.AppendObject(flOrigStream, "file.log");
                             //zip.Finish();

                             //flStream.Flush();
                         }
                     }
                 }
                 else if (fileName.EndsWith(".etl", StringComparison.OrdinalIgnoreCase))
                 {
                     int index = fileName.LastIndexOf("\\", StringComparison.Ordinal);
                     string filePath = fileName.Substring(0, index) + "\\" + "ETLTempFolder" +
                                       DateTime.Now.ToString("yyyyMMddHHmmss");
                     if (Saver.SaveETWData(messages, filePath))
                     {
                         fileName = fileName.Substring(index + 1, fileName.Length - (index + 1));
                         Saver.RenameDynamicalETLFile(filePath, fileName);
                     }
                 }
                 else if (fileName.EndsWith(".log"))
                 {
                     using (FileStream flStream = new FileStream(fileName, FileMode.Create,
                         FileAccess.ReadWrite, FileShare.ReadWrite))
                     {
                         //todo
                         //LogFormatter logFormatter = new LogXmlFormatter(flStream);
                         //Saver.SaveData(messages, logFormatter);
                         //logFormatter.Close();
                     }
                 }
                 else if (fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                 {
                     try
                     {
                         Saver.ExportToJson(messages, fileName);
                     }
                     catch (Exception exception)
                     {
                         MessageBox.Show(exception.Message, @"Error exporting to Json", MessageBoxButtons.OK,
                             MessageBoxIcon.Error);
                     }

                 }
                 else if (fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                 {
                     try
                     {
                         Saver.ExportToCSV(messages, fileName);
                     }
                     catch (Exception exception)
                     {
                         MessageBox.Show(exception.Message, @"Error exporting to Json", MessageBoxButtons.OK,
                             MessageBoxIcon.Error);
                     }

                 }
             });


        public override bool CanOpenFile(string fileName)

            => fileName.EndsWith(".etl", StringComparison.InvariantCultureIgnoreCase) ||
               fileName.EndsWith(".log", StringComparison.InvariantCultureIgnoreCase) ||
               fileName.EndsWith(".nlog", StringComparison.InvariantCultureIgnoreCase) ||
               fileName.EndsWith(".json", StringComparison.InvariantCultureIgnoreCase);

        public override bool CanOpenAllFiles(IEnumerable<string> fileNames) => fileNames.All(CanOpenFile);


        protected override List<FileInfo> GetSupportedFilesInternal(DirectoryInfo dirInfo, bool recursive)
        {
            List<FileInfo> files = dirInfo.GetFiles("*.etl").Concat(dirInfo.GetFiles("*.log"))
                .Concat(dirInfo.GetFiles("*.nlog")).Concat(dirInfo.GetFiles("*.json"))
                .ToList();
            if (!recursive)
                return files;
            try
            {
                foreach (DirectoryInfo dir in dirInfo.GetDirectories())
                {
                    files.AddRange(GetSupportedFilesInternal(dir, true));
                }
            }
            catch (Exception)
            {
                return files;
            }

            return files;
        }
    }
}
