using System;


namespace PlotComPlus.Series
{
    /// <summary>
    /// For memory type of column/metric/series.
    /// </summary>
    class MemorySeries : DefaultSeries
    {
        public MemorySeries(string name, int position) : 
            base(name, position) {}


        /// <summary>
        /// Display value in MBytes instead of Bytes. 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override double Transform(string value)
        {
            double result;
            double.TryParse(value, out result);
            return result / 1000000d;
        }


        public override string Unit
        {
            get
            {
                return "(MB)";
            }
        }
    }
}
