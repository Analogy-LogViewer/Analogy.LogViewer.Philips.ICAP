using Analogy.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Analogy.LogViewer.Philips.ICAP
{
    public class Utils
    {
        static string[] spliter = { Environment.NewLine };


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetAdditionalLogMessage(AnalogyLogMessage msg, string logMessage)
        {

            string[] logData = logMessage.Split(spliter, StringSplitOptions.None);
            if (logData.Length == 0) return;
            if (Guid.TryParse(logData[0], out var messageId))
            {
                msg.ID = messageId;
            }
            else
            {
                msg.Text += logMessage;
                return;
            }

            if (logData.Length < 2) return;
            msg.Class = GetLogClassFromString(logData[1]);
            if (logData.Length < 5) return;
            msg.Category = logData[4];
            if (logData.Length < 7) return;
            msg.MethodName = string.Intern(logData[6]);
            if (logData.Length < 8) return;
            msg.FileName = string.Intern(logData[7]);
            if (logData.Length < 9) return;
            int lineNumber = 0;

            if (!string.IsNullOrEmpty(logData[8]))
            {
                int.TryParse(logData[8], out lineNumber);
            }
            msg.LineNumber = lineNumber;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static AnalogyLogClass GetLogClassFromString(string logClass)
        {
            switch (logClass)
            {
                case "General":
                    return AnalogyLogClass.General;
                case "Security":
                    return AnalogyLogClass.Security;
                case "Hazard":
                    return AnalogyLogClass.Hazard;
                case "PHI":
                    return AnalogyLogClass.PHI;
            }
            return AnalogyLogClass.General;
        }

        private static AnalogyLogMessage ConditionalLogMessage(LogInfoWithJsonAttributes lf)
        {
            AnalogyLogMessage logMessage = new AnalogyLogMessage();
            logMessage.Module = !ContainsGuid(lf.AdditionalInfo) ? lf.ProcessName : lf.HumanReadableModuleId;
            logMessage.Source = lf.HumanReadableEventId;
            logMessage.Level = GetAnalogyLogLevel(lf.Severity);
            logMessage.Text = lf.Description;
            logMessage.ProcessID = Convert.ToInt32(lf.ProcessId);
            logMessage.Date = lf.DateTime;
            logMessage.MachineName = lf.MachineName ?? string.Empty;
            SetAdditionalLogMessage(logMessage, lf.AdditionalInfo + lf.Exception);
            logMessage.MethodName = string.IsNullOrEmpty(lf.Exception) ? "" : lf.Exception;
            return logMessage;
        }

        public static AnalogyLogMessage LogMessageFromLogInfo(LogInfoWithJsonAttributes lf)
        {
            if (string.IsNullOrEmpty(lf.AdditionalInfo))
            {
                AnalogyLogMessage message = new AnalogyLogMessage();
                message.ID = Guid.NewGuid();
                //todo
                //message.Class = lf.Severity == Enum.GetName(typeof(Severity), Severity.Hazard)
                //    ? AnalogyLogClass.Hazard
                //    : AnalogyLogClass.General;
                // message.auditType = "This can't be an audit message";
                //message.atnaMessage = "This can't be an audit message";
                message.MethodName = lf.Exception == null ? "" : lf.StackTrace;
                //message.FileName = ;
                //message.LineNumber = ;
                message.User = "";
                //message.Parameters = "BIG msg will not come here";
                message.Module = lf.ProcessName; // event id can be shown as module
                message.Source = lf.HumanReadableModuleId; // module is nt source in PF2.0
                message.Level = GetAnalogyLogLevel(lf.Severity);
                message.Text = lf.Description + lf.Exception;
                message.ProcessID = Convert.ToInt32(lf.ProcessId);
                message.Date = lf.DateTime;
                message.MachineName = lf.MachineName ?? string.Empty;
                message.Category = "Platform2.0";
                return message;
            }
            else
            {
                return ConditionalLogMessage(lf);
            }
        }
        private static bool ContainsGuid(string message)
        {
            if (string.IsNullOrEmpty(message))
                return false;

            string[] spliter = { Environment.NewLine };
            string[] logData = message.Split(spliter, StringSplitOptions.None);
            if (Guid.TryParse(logData[0], out _))
            {
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnalogyLogLevel GetAnalogyLogLevel(string severity)
        {
            switch (severity)
            {
                case "Critical":
                case "Fatal":
                    return AnalogyLogLevel.Critical;
                case "DebugInfo":
                    return AnalogyLogLevel.Verbose;
                case "None":
                    return AnalogyLogLevel.Disabled;
                case "Error":
                    return AnalogyLogLevel.Error;
                case "Info":
                    return AnalogyLogLevel.Event;
                case "DebugVerbose":
                    return AnalogyLogLevel.Debug;
                case "Warning":
                    return AnalogyLogLevel.Warning;
                case "Event":
                    return AnalogyLogLevel.Event;
                case "Verbose":
                    return AnalogyLogLevel.Verbose;
                case "Debug":
                    return AnalogyLogLevel.Debug;
                case "Disabled":
                    return AnalogyLogLevel.Disabled;
            }

            return AnalogyLogLevel.Event;
        }

        public static List<FileInfo> GetSupportedFiles(DirectoryInfo dirInfo, bool recursive)
        {
            List<FileInfo> files = dirInfo.GetFiles("*.etl").Concat(dirInfo.GetFiles("*.log"))
                .Concat(dirInfo.GetFiles("*.nlog")).Concat(dirInfo.GetFiles("*.json"))
                .Concat(dirInfo.GetFiles("defaultFile_*.xml")).Concat(dirInfo.GetFiles("*.evtx")).ToList();
            if (!recursive)
                return files;
            try
            {
                foreach (DirectoryInfo dir in dirInfo.GetDirectories())
                {
                    files.AddRange(GetSupportedFiles(dir, true));
                }
            }
            catch (Exception)
            {
                return files;
            }

            return files;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnalogyLogMessage CreateMessageFromEvent(EventLogEntry eEntry)
        {
            AnalogyLogMessage m = new AnalogyLogMessage();
            switch (eEntry.EntryType)
            {
                case EventLogEntryType.Error:
                    m.Level = AnalogyLogLevel.Error;
                    break;
                case EventLogEntryType.Warning:
                    m.Level = AnalogyLogLevel.Warning;
                    break;
                case EventLogEntryType.Information:
                    m.Level = AnalogyLogLevel.Event;
                    break;
                case EventLogEntryType.SuccessAudit:
                    m.Level = AnalogyLogLevel.Event;
                    break;
                case EventLogEntryType.FailureAudit:
                    m.Level = AnalogyLogLevel.Error;
                    break;
                default:
                    m.Level = AnalogyLogLevel.Event;
                    break;
            }

            m.Category = eEntry.Category;
            m.Date = eEntry.TimeGenerated;
            m.ID = Guid.NewGuid();
            m.Source = eEntry.Source;
            m.Text = eEntry.Message;
            m.User = eEntry.UserName;
            m.Module = eEntry.Source;
            m.MachineName = eEntry.MachineName;
            return m;
        }

        public static string GetFileNameAsDataSource(string fileName)
        {
            string file = Path.GetFileName(fileName);
            return fileName.Equals(file) ? fileName : $"{file} ({fileName})";

        }

    }

    public abstract class Saver
    {
        public static void ExportToJson(DataTable data, string filename)
        {
            List<AnalogyLogMessage> messages = new List<AnalogyLogMessage>();
            foreach (DataRow dtr in data.Rows)
            {

                AnalogyLogMessage log = (AnalogyLogMessage)dtr["Object"];
                messages.Add(log);
            }

            string json = JsonConvert.SerializeObject(messages);
            File.WriteAllText(filename, json);
        }
        public static void ExportToJson(List<AnalogyLogMessage> messages, string filename)
        {
            string json = JsonConvert.SerializeObject(messages);
            File.WriteAllText(filename, json);
        }

        /// <summary>
        /// SendMessageOTA the send DataTable in ETW format.
        /// </summary>
        /// <param name="data">holds the data to be saved.</param>
        /// <param name="saveFileLocation">holds the save location</param>
        public static bool SaveETWData(DataTable data, string saveFileLocation)
        {
            List<AnalogyLogMessage> messages = new List<AnalogyLogMessage>();
            foreach (DataRow dtr in data.Rows)
            {
                AnalogyLogMessage logMsg = (AnalogyLogMessage)dtr["Object"];
                messages.Add(logMsg);
            }

            return SaveETWData(messages, saveFileLocation);
        }
        public static bool SaveETWData(List<AnalogyLogMessage> messages, string saveFileLocation)
        {
            //todo
            return true;
            //if (!Directory.Exists(saveFileLocation))
            //{
            //    Directory.CreateDirectory(saveFileLocation);
            //}

            //ICAPLogSaver logSaver = new ICAPLogSaver();
            ////Start the log session for given application name defined in LoggingConfig.xml.
            //bool isSessionStarted = logSaver.StartApplicationLogSession("icap_save_session", saveFileLocation, LogFileMode.SequentialFile);
            //if (isSessionStarted)
            //{
            //    //Loop through each row to generate ETW LogMessage.
            //    foreach (var msg in messages)
            //    {
            //        var pm = new LogMessage();
            //        pm.FileName = msg.FileName;
            //        pm.Category = msg.Category;
            //        pm.Class = (LogClass)(int)msg.Class;
            //        pm.Date = msg.Date;
            //        pm.ID = msg.ID;
            //        pm.Level = (LogLevel)(int)msg.Level;
            //        pm.LineNumber = msg.LineNumber;
            //        pm.MethodName = msg.MethodName;
            //        pm.Module = msg.Module;
            //        pm.Parameters = msg.Parameters ?? new string[0];
            //        pm.ProcessID = msg.ProcessID;
            //        pm.Source = msg.Source;
            //        pm.Text = msg.Text;
            //        pm.User = msg.User;
            //        logSaver.LogETWMessage(pm.Date, pm.Module, pm, "LogViewer");
            //    }
            //    //Stop the log session for given application name.
            //    logSaver.StopApplicationLogSession("icap_save_session");
            //}
            //return isSessionStarted;
        }
        ////TODO Temporary fix need to remove this method in future.
        /// <summary>
        /// Rename dynamically created .etl file to user given name.
        /// </summary>
        /// <param name="saveFileLocation">holds the save location</param>
        /// <param name="saveFileName">holds the fileName to save</param>
        public static void RenameDynamicalETLFile(string saveFileLocation, string saveFileName)
        {
            string savePath = saveFileLocation.Substring(0, saveFileLocation.LastIndexOf("\\")) + "\\" + saveFileName;
            string[] etlFiles = Directory.GetFiles(saveFileLocation);
            foreach (string aFile in etlFiles)
            {
                string extn = Path.GetExtension(aFile);
                if (extn == ".etl")
                {
                    if (File.Exists(savePath))
                    {
                        File.Delete(savePath);
                    }
                    if (File.Exists(aFile))
                    {
                        File.Move(aFile, savePath);
                    }

                    break;
                }
            }
            #region Temporary fix need to remove in future.
            //Directory not present then create 
            if (Directory.Exists(saveFileLocation))
            {
                Directory.Delete(saveFileLocation);
            }
            #endregion
        }

        public static void ExportToCSV(List<AnalogyLogMessage> messages, string fileName)
        {
            string text = string.Join(Environment.NewLine, messages.Select(GetCSVFromMessage).ToArray());
            File.WriteAllText(fileName, text);
        }

        private static string GetCSVFromMessage(AnalogyLogMessage m) =>
        $"ID:{m.ID};Text:{m.Text};Category:{m.Category};Source:{m.Source};Level:{m.Level};Class:{m.Class};Module:{m.Module};Method:{m.MethodName};FileName:{m.FileName};LineNumber:{m.LineNumber};ProcessID:{m.ProcessID};User:{m.User};Parameters:{(m.Parameters == null ? string.Empty : string.Join(",", m.Parameters))}";
    }

    /// <summary>
    /// Represents custom filter item types.
    /// </summary>
    public enum DateRangeFilter
    {
        /// <summary>
        /// No filter
        /// </summary>
        None,
        /// <summary>
        /// Current date
        /// </summary>
        Today,
        /// <summary>
        /// Current date and yesterday
        /// </summary>
        Last2Days,
        /// <summary>
        /// Today, yesterday and the day before yesterday
        /// </summary>
        Last3Days,
        /// <summary>
        /// Last 7 days
        /// </summary>
        LastWeek,
        /// <summary>
        /// Last 2 weeks
        /// </summary>
        Last2Weeks,
        /// <summary>
        /// Last one month
        /// </summary>
        LastMonth
    }
}