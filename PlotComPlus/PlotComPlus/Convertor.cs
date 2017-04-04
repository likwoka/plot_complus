using System;
using System.Collections.Generic;
using System.Text;


namespace PlotComPlus
{
    /// <summary>
    /// This class contains all the conversion methods.
    /// </summary>
    static class Convertor
    {
        /// <summary>
        /// Make a elapsed time series by converting from
        /// a timestamp series.  Return a list of double of 
        /// elapsed time in minutes.
        /// </summary>
        /// <param name="timestamps">A list of string of timestamps.</param>
        /// <returns></returns>
        public static double[] TimestampsToTicks(IList<string> timestamps)
        {
            int length = timestamps.Count;
            double[] result = new double[length];
            long start = DateTime.Parse(timestamps[0]).ToFileTime();
            
            for (int i = 1; i < length; i++)
            {
                long diff = DateTime.Parse(timestamps[i]).ToFileTime() - start;
                // from 100 nano-sec to sec; then from sec to min.
                result[i] = diff / 10000000d / 60d; 
            }

            return result;
        }
    }
}
