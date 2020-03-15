using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Analogy.Interfaces;
using Newtonsoft.Json;

namespace Analogy.LogViewer.Philips
{
    public class LogInfoWithJsonAttributes
    {
        [JsonProperty("Platform_ModuleID")]
        public string ModuleId { get; internal set; }
        [JsonProperty("HumanReadableModuleID")]
        public string HumanReadableModuleId { get; internal set; }
        [JsonProperty("Platform_EventID")]
        public string EventId { get; internal set; }
        [JsonProperty("HumanReadableEventID")]
        public string HumanReadableEventId { get; internal set; }
        [JsonProperty("DateTime")]
        public DateTime DateTime { get; internal set; }
        [JsonProperty("LogType")]
        public string LogType { get; internal set; }
        [JsonProperty("EventType")]
        public string EventType { get; internal set; }
        [JsonProperty("Severity")]
        public string Severity { get; internal set; }
        [JsonProperty("Description")]
        public string Description { get; internal set; }
        [JsonProperty("ThreadId")]
        public int ThreadId { get; internal set; }
        [JsonProperty("ThreadName")]
        public string ThreadName { get; internal set; }
        [JsonProperty("ProcessId")]
        public string ProcessId { get; internal set; }
        [JsonProperty("ProcessName")]
        public string ProcessName { get; internal set; }
        [JsonProperty("MachineName")]
        public string MachineName { get; internal set; }
        [JsonProperty("AdditionalInfo")]
        public string AdditionalInfo { get; internal set; }
        [JsonProperty("ContextInfo")]
        public string ContextInfo { get; internal set; }
        [JsonProperty("Exception")]
        public string Exception { get; internal set; }
        [JsonProperty("StackTrace")]
        public string StackTrace { get; internal set; }

        public override string ToString()
        {
            return $"{nameof(ModuleId)}: {ModuleId}, {nameof(HumanReadableModuleId)}: {HumanReadableModuleId}, {nameof(EventId)}: {EventId}, {nameof(HumanReadableEventId)}: {HumanReadableEventId}, {nameof(DateTime)}: {DateTime}, {nameof(LogType)}: {LogType}, {nameof(EventType)}: {EventType}, {nameof(Severity)}: {Severity}, {nameof(Description)}: {Description}, {nameof(ThreadId)}: {ThreadId}, {nameof(ThreadName)}: {ThreadName}, {nameof(ProcessId)}: {ProcessId}, {nameof(ProcessName)}: {ProcessName}, {nameof(MachineName)}: {MachineName}, {nameof(AdditionalInfo)}: {AdditionalInfo}, {nameof(ContextInfo)}: {ContextInfo}, {nameof(Exception)}: {Exception}, {nameof(StackTrace)}: {StackTrace}";
        }
    }
    public class JSonLoader : LogLoader
    {
        protected string FileName;
        private ILogMessageCreatedHandler LogWindow;

        public override async Task<IEnumerable<AnalogyLogMessage>> ReadFromFile(string filename, CancellationToken token, ILogMessageCreatedHandler logWindow)
        {

            LogWindow = logWindow;
            FileName = filename;
            return await base.ReadFromFile(filename,token, logWindow);

        }
        public override async Task<IEnumerable<AnalogyLogMessage>> ReadFromStream(Stream dataStream, CancellationToken token, ILogMessageCreatedHandler logWindow)
        {
            if (dataStream == null || logWindow == null)
            {
                return new List<AnalogyLogMessage>();
            }

            return await Task<IEnumerable<AnalogyLogMessage>>.Factory.StartNew(() =>
            {
                using (StreamReader streamReader = new StreamReader(dataStream, Encoding.UTF8))
                {
                    try
                    {
                        string data = streamReader.ReadToEnd();
                        if (data.Contains("Platform_ModuleID") && data.Contains("HumanReadableModuleID"))
                        {
                            List<LogInfoWithJsonAttributes> logInfos =
                                (List<LogInfoWithJsonAttributes>)JsonConvert.DeserializeObject(data,
                                    typeof(List<LogInfoWithJsonAttributes>));
                            List<AnalogyLogMessage> messages = logInfos.Select(Utils.LogMessageFromLogInfo).ToList();
                            if (!messages.Any())
                            {
                                AnalogyLogMessage empty = new AnalogyLogMessage($"File {FileName} is empty or corrupted", AnalogyLogLevel.Error, AnalogyLogClass.General, "Analogy", "None");
                                messages.Add(empty);
                            }
                            logWindow.AppendMessages(messages, Utils.GetFileNameAsDataSource(FileName));
                            return messages;
                        }
                        else
                        {
                            List<AnalogyLogMessage> messages = (List<AnalogyLogMessage>)JsonConvert.DeserializeObject(data, typeof(List<AnalogyLogMessage>));
                            if (!messages.Any())
                            {
                                AnalogyLogMessage empty = new AnalogyLogMessage($"File {FileName} is empty or corrupted", AnalogyLogLevel.Error, AnalogyLogClass.General, "Analogy", "None");
                                messages.Add(empty);
                            }
                            logWindow.AppendMessages(messages, Utils.GetFileNameAsDataSource(FileName));
                            return messages;
                        }
                    }
                    catch (Exception ex)
                    {
                        List<AnalogyLogMessage> messages = new List<AnalogyLogMessage>();
                        AnalogyLogMessage empty = new AnalogyLogMessage($"File {FileName} is empty or corrupted. Error: {ex.Message}", AnalogyLogLevel.Error, AnalogyLogClass.General, "Analogy", "None");
                        empty.Source = "Analogy";
                        empty.Module = Process.GetCurrentProcess().ProcessName;
                        messages.Add(empty);
                        LogWindow.AppendMessages(messages, Utils.GetFileNameAsDataSource(FileName));
                        return messages;
                    }
                }
            },token);
        }


    }
}

