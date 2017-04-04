using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;


namespace PlotComPlus.ProcessFilters
{
    /// <summary>
    /// This class identifies the processes the 
    /// user is interested in when parsing a 
    /// log file.
    /// </summary>
    class ProcessFilter : IProcessFilter
    {
        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="patterns">a string of comma 
        /// separated list of patterns.</param>
        public ProcessFilter(string patterns)
        {
            List<string> groups = 
                new List<string>(patterns.Split(new char[] {','}));

            groups.ForEach(
                delegate(string v)
                {
                    v = string.Format("({0})+", v);
                }
            );

            string finalPattern = string.Join("|", groups.ToArray());
            _r = new Regex(finalPattern, RegexOptions.IgnoreCase);
        }


        /// <summary>
        /// Does the process name match with the processes
        /// the user is interested in?
        /// </summary>
        /// <param name="processName"></param>
        /// <returns></returns>
        public bool Contains(string processName)
        {
            return _r.IsMatch(processName);
        }


        private Regex _r;
    }
}
