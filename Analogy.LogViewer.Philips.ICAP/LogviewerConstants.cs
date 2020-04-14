namespace Analogy.LogViewer.Philips.ICAP
{
    internal class LogviewerConstants
    {
        internal const string LOG_VIEWER_PIPE_NAME_PREFIX = "\\\\.\\pipe\\PortalViewer";
        internal const string LOG_SERVICE_REMOTING_CONFIG_NAME = "LogService";
        internal const string LOG_PROVIDER_EXISTENCE_CHECK_MUTEX_NAME = "Global\\PortalPmsCTLogServer";


        /***** Constant strings to be displayed in UI for Date time quick filtering *****/
        /// <summary>
        /// No filter. All allowed
        /// </summary>
        internal const string DateFilterNone = "All";
        /// <summary>
        /// From Today 
        /// </summary>
        internal const string DateFilterToday = "Today";
        /// <summary>
        /// From last 2 days 
        /// </summary>
        internal const string DateFilterLast2Days = "Last 2 days";
        /// <summary>
        /// From last 3 days
        /// </summary>
        internal const string DateFilterLast3Days = "Last 3 days";
        /// <summary>
        /// From last week
        /// </summary>
        internal const string DateFilterLastWeek = "Last one week";
        /// <summary>
        /// From last 2 weeks
        /// </summary>
        internal const string DateFilterLast2Weeks = "Last 2 weeks";
        /// <summary>
        /// From last month
        /// </summary>
        internal const string DateFilterLastMonth = "Last one month";
    }
}
