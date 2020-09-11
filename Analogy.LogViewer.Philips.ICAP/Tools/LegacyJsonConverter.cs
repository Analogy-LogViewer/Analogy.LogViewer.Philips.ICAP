using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Threading;
using Analogy.Interfaces;
using Analogy.Interfaces.DataTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Analogy.LogViewer.Philips.ICAP.Tools
{
    public class LegacyJsonConverter
    {
        public IProgress<string> TextProgressReporter { get; private set; }
        public IProgress<AnalogyProgressReport> PercentageProgressReporter { get; private set; }
        public List<Dictionary<string, object>> GloList = new List<Dictionary<string, object>>();
        private List<ETLRecord> GloJsonList = new List<ETLRecord>();
        private ManualResetEventSlim manualResetEvent = new ManualResetEventSlim();
        public LegacyJsonConverter(IProgress<string> textProgressReporter, IProgress<AnalogyProgressReport> percentageProgressReporter)
        {
            TextProgressReporter = textProgressReporter;
            PercentageProgressReporter = percentageProgressReporter;

        }

        [DllImport("advapi32.dll", EntryPoint = "OpenTraceW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern ulong OpenTrace([In, Out] ref EventTraceLogfile logfile);

        [DllImport("advapi32.dll")]
        public static extern int ProcessTrace(
          [In] ulong[] handleArray,
          [In] uint handleCount,
          [In] IntPtr startTime,
          [In] IntPtr endTime);

        [DllImport("advapi32.dll")]
        public static extern int CloseTrace(ulong traceHandle);

        [SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32.dll")]
        private static extern int FileTimeToSystemTime(
          ref long fileTime,
          ref SystemTime systemTime);

        [SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32.dll")]
        public static extern int FileTimeToLocalFileTime(ref long fileTime, ref long localFileTime);

        [DllImport("tdh.dll")]
        internal static extern int TdhLoadManifest([MarshalAs(UnmanagedType.LPWStr), In] string Manifest);

        public event ProgressHandler Progress;

        public void EventRecordCallBack(ref EventRecord eventRecord)
        {
            if (eventRecord.EventHeader.ProviderId == new Guid("{68fdd900-4a3e-11d1-84f4-0000f80464e3}") ||
                eventRecord.EventHeader.ProviderId != new Guid("{FBD53C47-4585-43E7-9F1A-4D8C6109761A}") &&
                eventRecord.EventHeader.ProviderId != new Guid("{2DA26C77-0A1B-4E76-925E-210483DACE15}"))
            {
                //                manualResetEvent.Set();
                return;
            }
            TraceEventInfoWrapper eventInfoWrapper = new TraceEventInfoWrapper(eventRecord);
            Dictionary<string, object> dict = eventInfoWrapper.GetProperties(eventRecord);
            if (GloList != null)
            {
                ETLRecord etlRecord = new ETLRecord();
                if (dict != null && dict.Any())
                {
                    foreach (KeyValuePair<string, object> keyValuePair in dict)
                    {
                        if (!string.IsNullOrEmpty(keyValuePair.Key))
                        {
                            var key = keyValuePair.Key;
                            var value = keyValuePair.Value;
                            switch (key)
                            {
                                case "Platform_ModuleID":
                                    etlRecord.Platform_ModuleID = Convert.ToUInt32(value);
                                    break;
                                case "HumanReadableModuleID":
                                    etlRecord.HumanReadableModuleID = value.ToString();
                                    break;
                                case "Platform_EventID":
                                    etlRecord.Platform_EventID = Convert.ToUInt32(value);
                                    break;
                                case "HumanReadableEventID":
                                    etlRecord.HumanReadableEventID = value.ToString();
                                    break;
                                case "DateTime":
                                    etlRecord.DateTime = ((DateTime)value).ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                                    break;
                                case "LogType":
                                    etlRecord.LogType = value.ToString();
                                    break;
                                case "EventType":
                                    etlRecord.EventType = value.ToString();
                                    break;
                                case "Severity":
                                    etlRecord.Severity = value.ToString();
                                    break;
                                case "Description":
                                    etlRecord.Description = value.ToString();
                                    break;
                                case "ThreadId":
                                    etlRecord.ThreadId = Convert.ToUInt32(value);
                                    break;
                                case "ThreadName":
                                    etlRecord.ThreadName = value.ToString();
                                    break;
                                case "ProcessId":
                                    etlRecord.ProcessId = Convert.ToUInt32(value);
                                    break;
                                case "ProcessName":
                                    etlRecord.ProcessName = value.ToString();
                                    break;
                                case "MachineName":
                                    etlRecord.MachineName = value.ToString();
                                    break;
                                case "ContextInfo":
                                    etlRecord.ContextInfo = value.ToString();
                                    break;
                                case "AdditionalInfo":
                                    etlRecord.AdditionalInfo = value.ToString();
                                    break;
                                case "Exception":
                                    etlRecord.Exception = value.ToString();
                                    break;
                                case "StackTrace":
                                    etlRecord.StackTrace = value.ToString();
                                    break;
                            }
                        }
                    }
                }

                if (string.IsNullOrEmpty(etlRecord.Description))
                    etlRecord.Description = etlRecord.AdditionalInfo;
                GloJsonList.Add(etlRecord);
            }
            eventInfoWrapper.Dispose();
        }


        public void ProcessFile(string file)
        {
            ReadInternal(file);
        }
        public void WriteJsonFile(string sourcePath, string destinationPath, string extension, CancellationToken cancellationToken)
        {
            FileInfo[] files = new DirectoryInfo(sourcePath).GetFiles("*.etl", SearchOption.AllDirectories);
            int total = files.Length;
            int num = 0;
            foreach (FileInfo fileInfo in files)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;

                }
                ReadInternal(fileInfo.FullName);
                JsonSerializer jsonSerializer = new JsonSerializer();
                jsonSerializer.Converters.Add(new KeyValuePairConverter());
                jsonSerializer.NullValueHandling = NullValueHandling.Ignore;
                if (!Directory.Exists(destinationPath))
                    Directory.CreateDirectory(destinationPath);
                if (GloJsonList.Count != 0)
                {
                    string contents = JsonConvert.SerializeObject(GloJsonList).Replace("\\n", " ").Replace("\\r", " ");
                    string withoutExtension = Path.GetFileNameWithoutExtension(fileInfo.Name);
                    string target = destinationPath + withoutExtension + extension;
                    File.AppendAllText(target, contents);
                    GloJsonList.Clear();
                    PercentageProgressReporter?.Report(new AnalogyProgressReport($"Json created ({target})", ++num, total, fileInfo.Name));


                }
                else
                {
                    PercentageProgressReporter.Report(new AnalogyProgressReport($"Json skipped (no data)", ++num, total, fileInfo.Name));
                }
            }
        }

        public bool BufferCallback(ref EventTraceLogfile eventRecord)
        {
            return true;
        }

        public void ReadInternal(string filePath)
        {
            EventTraceLogfile logfile = new EventTraceLogfile();
            logfile.LogFileName = filePath;
            logfile.EventRecordCallback = EventRecordCallBack;
            logfile.ProcessTraceMode = 268435456U;
            logfile.BufferCallback = BufferCallback;
            TdhLoadManifest("icap_logging_manifest.xml");
            ulong traceHandle = OpenTrace(ref logfile);
            if (traceHandle <= 0UL)
                return;
            //manualResetEvent.Wait(cancellationToken);
            ProcessTrace(new ulong[1] { traceHandle }, 1U, IntPtr.Zero, IntPtr.Zero);
            CloseTrace(traceHandle);
        }

        public struct EventTraceHeader
        {
            public ushort Size;
            public ushort FieldTypeFlags;
            public uint Version;
            public uint ThreadId;
            public uint ProcessId;
            public long TimeStamp;
            public Guid Guid;
            public uint KernelTime;
            public uint UserTime;
        }

        public struct EventTrace
        {
            public EventTraceHeader Header;
            public uint InstanceId;
            public uint ParentInstanceId;
            public Guid ParentGuid;
            public IntPtr MofData;
            public uint MofLength;
            public uint ClientContext;
        }

        public struct TraceLogfileHeader
        {
            public uint BufferSize;
            public uint Version;
            public uint ProviderVersion;
            public uint NumberOfProcessors;
            public long EndTime;
            public uint TimerResolution;
            public uint MaximumFileSize;
            public uint LogFileMode;
            public uint BuffersWritten;
            public Guid LogInstanceGuid;
            public IntPtr LoggerName;
            public IntPtr LogFileName;
            public Win32TimeZoneInfo TimeZone;
            public long BootTime;
            public long PerfFreq;
            public long StartTime;
            public uint ReservedFlags;
            public uint BuffersLost;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct EventTraceLogfile
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string LogFileName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string LoggerName;
            public long CurrentTime;
            public uint BuffersRead;
            public uint ProcessTraceMode;
            public EventTrace CurrentEvent;
            public TraceLogfileHeader LogfileHeader;
            public EventTraceBufferCallback BufferCallback;
            public uint BufferSize;
            public uint Filled;
            public uint EventsLost;
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public EventRecordCallback EventRecordCallback;
            public uint IsKernelTrace;
            public IntPtr Context;
        }

        public struct EventRecord
        {
            public EventHeader EventHeader;
            public EtwBufferContext BufferContext;
            public ushort ExtendedDataCount;
            public ushort UserDataLength;
            public IntPtr ExtendedData;
            public IntPtr UserData;
            public IntPtr UserContext;

            public struct EtwBufferContext
            {
                public byte ProcessorNumber;
                public byte Alignment;
                public ushort LoggerId;
            }
        }

        public struct EventHeader
        {
            public ushort Size;
            public ushort HeaderType;
            public ushort Flags;
            public ushort EventProperty;
            public uint ThreadId;
            public uint ProcessId;
            public long TimeStamp;
            public Guid ProviderId;
            public EtwEventDescriptor EventDescriptor;
            public ulong ProcessorTime;
            public Guid ActivityId;
        }

        public struct EtwEventDescriptor
        {
            public ushort Id;
            public byte Version;
            public byte Channel;
            public byte Level;
            public byte Opcode;
            public ushort Task;
            public ulong Keyword;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct Win32TimeZoneInfo
        {
            public int Bias;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public char[] StandardName;
            public SystemTime StandardDate;
            public int StandardBias;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public char[] DaylightName;
            public SystemTime DaylightDate;
            public int DaylightBias;
        }

        public struct SystemTime
        {
            public ushort year;
            public ushort month;
            public ushort dayOfWeek;
            public ushort day;
            public ushort hour;
            public ushort minute;
            public ushort second;
            public ushort milliseconds;

            public static SystemTime FromDateTime(DateTime targetTime)
            {
                long fileTime = targetTime.ToFileTime();
                long num = 0;
                if (FileTimeToLocalFileTime(ref fileTime, ref num) == 0)
                    ;
                SystemTime systemTime = new SystemTime();
                if (FileTimeToSystemTime(ref num, ref systemTime) == 0)
                    ;
                return systemTime;
            }
        }

        public delegate void EventRecordCallback([In] ref EventRecord eventRecord);

        public delegate bool EventTraceBufferCallback([In] ref EventTraceLogfile eventTraceLogfile);

        public delegate void ProgressHandler(string operation,
          int progress,
          int total,
          string fileName,
          bool convertSource);
    }
    public sealed class TraceEventInfoWrapper : IDisposable
    {
        public IntPtr address;
        public TraceEventInfo traceEventInfo;
        public bool hasProperties;
        public EventPropertyInfo[] eventPropertyInfoArray;

        public TraceEventInfoWrapper(LegacyJsonConverter.EventRecord eventRecord)
        {
            int BufferSize = 0;
            int eventInformation1 = TdhGetEventInformation(ref eventRecord, 0U, IntPtr.Zero, IntPtr.Zero, ref BufferSize);
            if (eventInformation1 == 1168)
            {
                hasProperties = false;
            }
            else
            {
                hasProperties = true;
                if (eventInformation1 != 122)
                    throw new Win32Exception(eventInformation1);
                address = Marshal.AllocHGlobal(BufferSize);
                int eventInformation2 = TdhGetEventInformation(ref eventRecord, 0U, IntPtr.Zero, address, ref BufferSize);
                if (eventInformation2 != 0)
                    throw new Win32Exception(eventInformation2);
                traceEventInfo = (TraceEventInfo)Marshal.PtrToStructure(address, typeof(TraceEventInfo));
                int num1 = Marshal.SizeOf((object)traceEventInfo);
                if (BufferSize != num1)
                {
                    int num2 = Marshal.SizeOf(typeof(EventPropertyInfo));
                    int length = (BufferSize - num1) / num2;
                    eventPropertyInfoArray = new EventPropertyInfo[length];
                    long num3 = address.ToInt64() + num1;
                    for (int index = 0; index < length; ++index)
                    {
                        EventPropertyInfo structure = (EventPropertyInfo)Marshal.PtrToStructure(new IntPtr(num3 + index * num2), typeof(EventPropertyInfo));
                        eventPropertyInfoArray[index] = structure;
                    }
                }
                if (traceEventInfo.LevelNameOffset > 0U)
                    LevelName = Marshal.PtrToStringUni(new IntPtr(address.ToInt64() + traceEventInfo.LevelNameOffset));
                if (traceEventInfo.ChannelNameOffset > 0U)
                    ChannelName = Marshal.PtrToStringUni(new IntPtr(address.ToInt64() + traceEventInfo.ChannelNameOffset));
                if (traceEventInfo.TaskNameOffset > 0U)
                    TaskName = Marshal.PtrToStringUni(new IntPtr(address.ToInt64() + traceEventInfo.TaskNameOffset));
                if (traceEventInfo.EventMessageOffset > 0U)
                    EventMessageName = Marshal.PtrToStringUni(new IntPtr(address.ToInt64() + traceEventInfo.EventMessageOffset));
                if (traceEventInfo.OpcodeNameOffset <= 0U)
                    return;
                EventName = Marshal.PtrToStringUni(new IntPtr(address.ToInt64() + traceEventInfo.OpcodeNameOffset));
            }
        }

        ~TraceEventInfoWrapper()
        {
            ReleaseMemory();
        }

        public string EventName { set; get; }

        public string ChannelName { set; get; }

        public string LevelName { set; get; }

        public string TaskName { set; get; }

        public string EventMessageName { set; get; }

        public void Dispose()
        {
            ReleaseMemory();
            GC.SuppressFinalize(this);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<string, object> GetProperties(LegacyJsonConverter.EventRecord eventRecord)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>(traceEventInfo.TopLevelPropertyCount);
            if (hasProperties)
            {
                int num1 = 0;
                IntPtr num2;
                for (int index = 0; index < traceEventInfo.TopLevelPropertyCount; ++index)
                {

                    EventPropertyInfo eventPropertyInfo = eventPropertyInfoArray[index];
                    string stringUni = Marshal.PtrToStringUni(new IntPtr(address.ToInt64() + eventPropertyInfo.NameOffset));
                    num2 = new IntPtr(eventRecord.UserData.ToInt64() + num1);
                    int length;
                    object obj = null;
                    length = eventPropertyInfo.LengthPropertyIndex;
                    switch (eventPropertyInfo.NonStructTypeValue.InType)
                    {
                        case EventPropertyInfo.TdhInType.UnicodeString:
                            string stringUni2 = Marshal.PtrToStringUni(num2);
                            if (stringUni != null)
                            {
                                length = (stringUni2.Length + 1) * 2;
                                obj = stringUni2;
                            }

                            break;
                        case EventPropertyInfo.TdhInType.Int32:
                            obj = Marshal.ReadInt32(num2);
                            break;
                        case EventPropertyInfo.TdhInType.UInt32:
                            obj = (uint)Marshal.ReadInt32(num2);
                            break;
                        case EventPropertyInfo.TdhInType.Int64:
                            obj = Marshal.ReadInt64(num2);
                            break;
                        case EventPropertyInfo.TdhInType.UInt64:
                            obj = (ulong)Marshal.ReadInt64(num2);
                            break;
                        case EventPropertyInfo.TdhInType.Pointer:
                            obj = Marshal.ReadIntPtr(num2);
                            break;
                        case EventPropertyInfo.TdhInType.FileTime:
                            obj = DateTime.FromFileTime(Marshal.ReadInt64(num2));
                            break;
                        case EventPropertyInfo.TdhInType.SystemTime:
                            obj = new DateTime(Marshal.ReadInt16(num2),
                                Marshal.ReadInt16(num2, 2), Marshal.ReadInt16(num2, 6),
                                Marshal.ReadInt16(num2, 8), Marshal.ReadInt16(num2, 10),
                                Marshal.ReadInt16(num2, 12), Marshal.ReadInt16(num2, 14));
                            break;
                    }

                    num1 += length;
                    if (stringUni != null)
                        dictionary.Add(stringUni, obj);
                }
                if (num1 < eventRecord.UserDataLength)
                {
                    num2 = new IntPtr(eventRecord.UserData.ToInt64() + num1);
                    int length = eventRecord.UserDataLength - num1;
                    byte[] numArray = new byte[length];
                    for (int ofs = 0; ofs < length; ++ofs)
                        numArray[ofs] = Marshal.ReadByte(num2, ofs);
                    dictionary.Add("__ExtraPayload", numArray);
                }
            }
            else
            {
                string stringUni = Marshal.PtrToStringUni(eventRecord.UserData);
                dictionary.Add("EventData", stringUni);
            }
            return dictionary;
        }




        public void ReleaseMemory()
        {
            if (!(address != IntPtr.Zero))
                return;
            Marshal.FreeHGlobal(address);
            address = IntPtr.Zero;
        }

        [DllImport("tdh.dll")]
        public static extern int TdhGetEventInformation(
          [In] ref LegacyJsonConverter.EventRecord Event,
          [In] uint TdhContextCount,
          [In] IntPtr TdhContext,
          [Out] IntPtr eventInfoPtr,
          [In, Out] ref int BufferSize);
    }
    [DataContract]
    public class ETLRecord
    {
        [DataMember]
        public uint Platform_ModuleID { get; set; }

        [DataMember]
        public string HumanReadableModuleID { get; set; }

        [DataMember]
        public uint Platform_EventID { get; set; }

        [DataMember]
        public string HumanReadableEventID { get; set; }

        [DataMember]
        public string DateTime { get; set; }

        [DataMember]
        public string LogType { get; set; }

        [DataMember]
        public string EventType { get; set; }

        [DataMember]
        public string Severity { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public uint ThreadId { get; set; }

        [DataMember]
        public string ThreadName { get; set; }

        [DataMember]
        public uint ProcessId { get; set; }

        [DataMember]
        public string ProcessName { get; set; }

        [DataMember]
        public string MachineName { get; set; }

        [DataMember]
        public string ContextInfo { get; set; }

        [DataMember]
        public string AdditionalInfo { get; set; }

        [DataMember]
        public string Exception { get; set; }

        [DataMember]
        public string StackTrace { get; set; }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct EventPropertyInfo
    {
        [FieldOffset(0)]
        public PropertyFlags Flags;
        [FieldOffset(4)]
        public uint NameOffset;
        [FieldOffset(8)]
        public NonStructType NonStructTypeValue;
        [FieldOffset(8)]
        public StructType StructTypeValue;
        [FieldOffset(16)]
        public ushort CountPropertyIndex;
        [FieldOffset(18)]
        public ushort LengthPropertyIndex;
        [FieldOffset(20)]
        public uint Reserved;

        public struct NonStructType
        {
            public TdhInType InType;
            public TdhOutType OutType;
            public uint MapNameOffset;
        }

        public struct StructType
        {
            public ushort StructStartIndex;
            public ushort NumOfStructMembers;
            public uint _Padding;
        }
        public enum TdhInType : ushort
        {
            Null = 0,
            UnicodeString = 1,
            AnsiString = 2,
            Int8 = 3,
            UInt8 = 4,
            Int16 = 5,
            UInt16 = 6,
            Int32 = 7,
            UInt32 = 8,
            Int64 = 9,
            UInt64 = 10, // 0x000A
            Float = 11, // 0x000B
            Double = 12, // 0x000C
            Boolean = 13, // 0x000D
            Binary = 14, // 0x000E
            Guid = 15, // 0x000F
            Pointer = 16, // 0x0010
            FileTime = 17, // 0x0011
            SystemTime = 18, // 0x0012
            SID = 19, // 0x0013
            HexInt32 = 20, // 0x0014
            HexInt64 = 21, // 0x0015
            CountedString = 300, // 0x012C
            CountedAnsiString = 301, // 0x012D
            ReversedCountedString = 302, // 0x012E
            ReversedCountedAnsiString = 303, // 0x012F
            NonNullTerminatedString = 304, // 0x0130
            NonNullTerminatedAnsiString = 305, // 0x0131
            UnicodeChar = 306, // 0x0132
            AnsiChar = 307, // 0x0133
            SizeT = 308, // 0x0134
            HexDump = 309, // 0x0135
            WbemSID = 310 // 0x0136
        }
        public enum TdhOutType : ushort
        {
            Null = 0,
            String = 1,
            DateTime = 2,
            Byte = 3,
            UnsignedByte = 4,
            Short = 5,
            UnsignedShort = 6,
            Int = 7,
            UnsignedInt = 8,
            Long = 9,
            UnsignedLong = 10, // 0x000A
            Float = 11, // 0x000B
            Double = 12, // 0x000C
            Boolean = 13, // 0x000D
            Guid = 14, // 0x000E
            HexBinary = 15, // 0x000F
            HexInt8 = 16, // 0x0010
            HexInt16 = 17, // 0x0011
            HexInt32 = 18, // 0x0012
            HexInt64 = 19, // 0x0013
            PID = 20, // 0x0014
            TID = 21, // 0x0015
            PORT = 22, // 0x0016
            IPV4 = 23, // 0x0017
            IPV6 = 24, // 0x0018
            SocketAddress = 25, // 0x0019
            CimDateTime = 26, // 0x001A
            EtwTime = 27, // 0x001B
            Xml = 28, // 0x001C
            ErrorCode = 29, // 0x001D
            ReducedString = 300, // 0x012C
            NoPrint = 301 // 0x012D
        }
    }

    public struct TraceEventInfo
    {
        public Guid ProviderGuid;
        public Guid EventGuid;
        public EtwEventDescriptor EventDescriptor;
        public DecodingSource DecodingSource;
        public uint ProviderNameOffset;
        public uint LevelNameOffset;
        public uint ChannelNameOffset;
        public uint KeywordsNameOffset;
        public uint TaskNameOffset;
        public uint OpcodeNameOffset;
        public uint EventMessageOffset;
        public uint ProviderMessageOffset;
        public uint BinaryXmlOffset;
        public uint BinaryXmlSize;
        public uint ActivityIDNameOffset;
        public uint RelatedActivityIDNameOffset;
        public uint PropertyCount;
        public int TopLevelPropertyCount;
        public TemplateFlags Flags;
    }

    public struct EtwEventDescriptor
    {
        public ushort Id;
        public byte Version;
        public byte Channel;
        public byte Level;
        public byte Opcode;
        public ushort Task;
        public ulong Keyword;
    }

    public enum TemplateFlags
    {
        TemplateEventDdata = 1,
        TemplateUserData = 2
    }

    [Flags]
    public enum PropertyFlags
    {
        None = 0,
        Struct = 1,
        ParamLength = 2,
        ParamCount = 4,
        WbemXmlFragment = 8,
        ParamFixedLength = 16 // 0x00000010
    }
    public enum DecodingSource
    {
        DecodingSourceXmlFile,
        DecodingSourceWbem,
        DecodingSourceWPP
    }
}
