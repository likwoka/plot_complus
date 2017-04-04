using System;
using System.Collections.Generic;
using System.Reflection;
using PlotComPlus.Series;
using PlotComPlus.Logging;


namespace PlotComPlus
{
    /// <summary>
    /// Verify the series/columns/metrics the user is 
    /// interested in do exist in the log file, and
    /// then capture their positions.                          
    /// </summary>
    class SeriesFilter
    {
        
        public SeriesFilter()
        {
            _desiredSeries = new List<string[]>();
            _series = new List<ISeries>();
        }

        /// <summary>
        /// Add a series to the filter.  Call this method
        /// for each desired series/columns/metrics 
        /// before calling InitializeWithHeader().
        /// </summary>
        /// <param name="seriesName"></param>
        /// <param name="seriesType"></param>
        public void AddDesiredSeries(string seriesName, string seriesType)
        {
            _desiredSeries.Add(new string[] { seriesName, seriesType });
        }


        /// <summary>
        /// Initialize the filter with a header row.  Will
        /// throw a SeriesNotFoundException if no columns 
        /// are found!
        /// </summary>
        /// <param name="headerLine"></param>
        public void InitializeWithHeader(string headerLine)
        {
            string[] headers = headerLine.Split(new char[] { ',' });
            int max = headers.Length;

            foreach (string[] desired in _desiredSeries)
            {               
                for (int i = 0; i < max; i++)
                {
                    if (headers[i] == desired[0])
                    {
                        Assembly assembly = Assembly.GetExecutingAssembly();
                        
                        ISeries s = (ISeries)(assembly.CreateInstance(
                            desired[1], true, BindingFlags.CreateInstance, null, 
                            new object[] {desired[0], i}, null, null));
                        _series.Add(s);
                        break;
                    }
                }
            }

            if (_series.Count == 0)
            {
                throw new SeriesNotFoundException();
            }
        }


        /// <summary>
        /// The list of series we found in the log file.
        /// </summary>
        public IList<ISeries> Series
        {
            get
            {
                return _series;
            }
        }


        /// <summary>
        /// This is the series/columns/metrics that the user want
        /// to capture, ie. the columns listed in the configuration file.
        /// </summary>
        private List<string[]> _desiredSeries;


        /// <summary>
        /// This is the actual list of series/columns/metrics that
        /// are found in the log files and are matched with the desired
        /// series/columns/metrics.
        /// </summary>
        private List<ISeries> _series;
    }
}
