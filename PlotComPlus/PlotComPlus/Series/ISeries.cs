using System;
using ZedGraph;


namespace PlotComPlus.Series
{
    /// <summary>
    /// Represents a series/columns/metrics.
    /// </summary>
    interface ISeries
    {
        /// <summary>
        /// Covert the value from a string to a double.
        /// </summary>
        double Transform(string value);

        /// <summary>
        /// The unit of the value, to be displayed 
        /// in the Y-axis title.
        /// </summary>
        string Unit { get; }

        /// <summary>
        /// The name of the series/columns/metrics. 
        /// Use for matching up with the log file columns,
        /// as well as for the Y-axis title.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The column in the log file.  The left-most column
        /// is 1.
        /// </summary>
        int Position { get; }

        /// <summary>
        /// For graphing.
        /// </summary>
        SymbolType SymbolType { get; }
    }
}
