using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using PlotComPlus.Series;
using PlotComPlus.ProcessFilters;
using PlotComPlus.Logging;


namespace PlotComPlus
{
    /// <summary>
    /// Parse mon_complus.vbs output log files. 
    /// </summary>
    class LogParser
    {
        
        public LogParser(IProcessFilter p, SeriesFilter s, ILogger logger)
        {
            _processFilter = p;
            _seriesFilter = s;
            _logger = logger;
        }


        /// <summary>
        /// Parse all log files into a list of processes.
        /// </summary>
        /// <param name="paths">A string of comma-separated list 
        /// of paths to the log files.</param>
        public List<Process> ParseLogs(string paths)
        {
            string[] splittedPaths = paths.Split(',');
            List<Process> result = new List<Process>();

            foreach (string path in splittedPaths)
            {
                List<Process> partial = this.ParseLog(path);
                result.AddRange(partial);
            }

            result.Sort(
                delegate(Process x, Process y)
                {
                    string xKey = string.Format("{0} {1} {2}", x.ProcessName, x.LogFileName, x.ProcessId);
                    string yKey = string.Format("{0} {1} {2}", y.ProcessName, y.LogFileName, y.ProcessId);
                    return xKey.CompareTo(yKey);
                }
            );
            return result;
        }


        /// <summary>
        /// Parse a log file.
        /// A log file must have a header line.
        /// Its column must begin in order from the left with:
        /// Time, Computer Name, Process Name
        /// </summary>
        /// <param name="path">Path to the log file.</param>
        private List<Process> ParseLog(string path)
        {
            Dictionary<string, Process> result = new Dictionary<string,Process>();
            try
            {
                string absolutePath = Path.GetFullPath(path);
                using (StreamReader logFile = new StreamReader(absolutePath))
                {

                    string line;
                    while ((line = logFile.ReadLine()) != null)
                    {
                        string[] field = line.Split(new char[] { ',' });

                        // line can be an invalid line (empty, not enough fields).
                        if (field.Length < 4)
                        {
                            continue;
                        }
                        else if (field[0] == "Time")
                        {
                            // line can be a header line.
                            if (!_areHeadersVerified)
                            {
                                _seriesFilter.InitializeWithHeader(line);
                                _areHeadersVerified = true;
                            }
                        }
                        else
                        {
                            // line can be a stat line.
                            string nameAndId = field[2];
                            if (_processFilter.Contains(nameAndId))
                            {
                                Process p = null;
                                string id = Process.ComposeId(nameAndId, path);
                                
                                if (!result.TryGetValue(id, out p))
                                {
                                    p = new Process(nameAndId, path);
                                    result[id] = p;
                                }

                                string time = field[0];
                                if (time.Length > 0)
                                {
                                    p.TimeSeries.Add(time);
                                    foreach (ISeries series in _seriesFilter.Series)
                                    {
                                        p.Set(series.Name, series.Transform(field[series.Position]));
                                    }
                                } 
                            }
                        }
                        
                    }
                }
            }
            catch (IOException)
            {
                _logger.Log(string.Format("Log file {0} is not found!", path));
            }
            return new List<Process>(result.Values);
        }


        private IProcessFilter _processFilter;
        private SeriesFilter _seriesFilter;
        private ILogger _logger;
        private bool _areHeadersVerified;
    }
}
