using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Analogy.Interfaces;

namespace Analogy.LogViewer.Philips.ICAP
{
    public class NLogLoader : LogLoader
    {
        protected string FileName;

        public override async Task<IEnumerable<AnalogyLogMessage>> ReadFromFile(string filename, CancellationToken token, ILogMessageCreatedHandler logWindow)
        {


            FileName = filename;
            return await base.ReadFromFile(filename, token, logWindow);

        }
        public override async Task<IEnumerable<AnalogyLogMessage>> ReadFromStream(Stream dataStream, CancellationToken token, ILogMessageCreatedHandler logWindow)
        {
            if ((dataStream == null) || (logWindow == null))
            {
                return new List<AnalogyLogMessage>();
            }

            List<AnalogyLogMessage> messages = new List<AnalogyLogMessage>();
            try
            {
                using (StreamReader streamReader = new StreamReader(dataStream, Encoding.UTF8))
                {

                    while (!streamReader.EndOfStream)
                    {
                        try
                        {
                            string line = await streamReader.ReadLineAsync();
                            var items = line.Split(new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);

                            AnalogyLogMessage m = new AnalogyLogMessage();
                            if (DateTime.TryParse(items[0], out DateTime dt))
                                m.Date = dt;
                            m.ProcessId = int.Parse(items[2]);
                            switch (items[3])
                            {
                                case "INFO":
                                    m.Level = AnalogyLogLevel.Event;
                                    break;
                                case "WARN":
                                    m.Level = AnalogyLogLevel.Warning;
                                    break;
                                case "ERROR":
                                    m.Level = AnalogyLogLevel.Error;
                                    break;
                                case "FATAL":
                                    m.Level = AnalogyLogLevel.Critical;
                                    break;
                                default:
                                    m.Level = AnalogyLogLevel.Event;
                                    break;
                            }

                            m.Source = items[5];
                            m.Module = items[6];

                            if (int.TryParse(items[4], out int threadID))
                            {
                                m.ThreadId = threadID;
                            }
                            if (items.Length == 7)
                            {
                                m.Module = items[5];
                                m.Text = items[6];
                            }
                            else if (items.Length == 8)
                            {
                                m.Text = $"{items[1]}: {items[7]}";
                            }
                            else if (items.Length > 8)
                            {
                                string text = items[7];
                                for (int i = 8; i <= items.Length - 1; i++)
                                    text += items[i];
                                m.Text = $"{items[1]}: {text}";
                            }

                            messages.Add(m);
                        }
                        catch (Exception e)
                        {
                            string msg = $"Error processing line: {e}";
                            messages.Add(new AnalogyLogMessage(msg, AnalogyLogLevel.Error, AnalogyLogClass.General, "Analogy", "None"));
                        }
                        if (token.IsCancellationRequested)
                        {
                            string msg = "Processing cancelled by User.";
                            messages.Add(new AnalogyLogMessage(msg, AnalogyLogLevel.Event, AnalogyLogClass.General, "Analogy", "None"));
                            logWindow.AppendMessages(messages, Utils.GetFileNameAsDataSource(FileName));
                            return messages;
                        }
                    }
                }

            }
            catch (Exception e)
            {
                string msg = $"Error processing line: {e}";
                messages.Add(new AnalogyLogMessage(msg, AnalogyLogLevel.Error, AnalogyLogClass.General, "Analogy", "None"));
            }
            if (!messages.Any())
            {
                AnalogyLogMessage empty = new AnalogyLogMessage($"File {FileName} is empty or corrupted", AnalogyLogLevel.Error, AnalogyLogClass.General, "Analogy", "None");
                empty.Source = "Analogy";
                empty.Module = Process.GetCurrentProcess().ProcessName;
                messages.Add(empty);

            }
            logWindow.AppendMessages(messages, Utils.GetFileNameAsDataSource(FileName));
            return messages;
        }

    }
}
