using System;


namespace PlotComPlus
{
    /// <summary>
    /// TODO: Non-serializable exception is BAD BAD BAD!
    /// </summary>
    public class SeriesNotFoundException : Exception
    {
        public SeriesNotFoundException() : base(MSG) {}

        private const string MSG = "Desired columns not found in log files!";
    }
}
