using System;
using System.Collections.Generic;


namespace PlotComPlus
{
    /// <summary>
    /// This class represents a particular process 
    /// recorded in the log file.
    /// </summary>
    class Process
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="processNameAndId">In this format: 
        /// ProcessName(ProcessId)</param>
        /// <param name="logFileName"></param>
        public Process(string processNameAndId, string logFileName)
        {
            string[] splitted = processNameAndId.Split(new char[] {'(', ')'});
            _processName = splitted[0];
            if (splitted.Length > 1)
            {
                int.TryParse(splitted[1], out _processId);
            }
            
            _logFileName = logFileName;
            _id = ComposeId(processNameAndId, logFileName);

            _timeSeries = new List<string>();
            _series = new Dictionary<string, List<double>>();
        }


        /// <summary>
        /// Retur an Id that can be used to identify a particular 
        /// process in a log file.
        /// </summary>
        /// <param name="processNameAndId"></param>
        /// <param name="logFileName"></param>
        /// <returns></returns>
        public static string ComposeId(string processNameAndId, string logFileName)
        {
            return string.Format("{0} {1}", processNameAndId, logFileName);
        }


        /// <summary>
        /// Set a value to a series of this process.
        /// </summary>
        /// <param name="seriesName">The series.</param>
        /// <param name="value">The value.</param>
        public void Set(string seriesName, double value)
        {
            List<double> theSeries;
            if (!_series.TryGetValue(seriesName, out theSeries))
            {
                theSeries = new List<double>();
                _series[seriesName] = theSeries;
            }
            theSeries.Add(value);
        }


        /// <summary>
        /// Return a series of this process.
        /// </summary>
        /// <param name="seriesName"></param>
        /// <returns></returns>
        public IList<double> Get(string seriesName)
        {
            return _series[seriesName];
        }
        

        /// <summary>
        /// Label is in the following format:
        /// ProcessName(ProcessId) LogFileName
        /// (read only).
        /// </summary>
        public string Id
        {
            get
            {
                return _id;
            }
        }


        /// <summary>
        /// The process ID (read only).
        /// </summary>
        public int ProcessId
        {
            get
            {
                return _processId;
            }
        }


        /// <summary>
        /// The process name (read only).
        /// </summary>
        public string ProcessName
        {
            get
            {
                return _processName;
            }
        }


        /// <summary>
        /// The log file that this process belongs to (read only).
        /// </summary>
        public string LogFileName
        {
            get
            {
                return _logFileName;
            }
        }


        /// <summary>
        /// The time series (read only).
        /// Use this to append new entry to the time series.
        /// </summary>
        public IList<string> TimeSeries
        {
            get
            {
                return _timeSeries;
            }
        }


        private string _logFileName;
        private string _id;
        private int _processId;
        private string _processName;
        private List<string> _timeSeries;
        private Dictionary<string, List<double>> _series;
    }
}
