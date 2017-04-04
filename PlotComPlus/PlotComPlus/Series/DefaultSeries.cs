using System;
using ZedGraph;


namespace PlotComPlus.Series
{
    /// <summary>
    /// The most common series/column/metric.
    /// </summary>
    class DefaultSeries : ISeries
    {

        public DefaultSeries(string name, int position)
        {
            _name = name;
            _position = position;
        }


        public virtual double Transform(string value)
        {
            double result;
            if (double.TryParse(value, out result))
            {
                return result;
            }
            return 0;
        }


        public virtual string Unit
        {
            get
            {
                return "";
            }

        }


        public string Name
        {
            get
            {
                return _name;
            }
        }


        public int Position
        {
            get
            {
                return _position;
            }
        }


        public SymbolType SymbolType
        {
            get
            {
                return SymbolType.None;
            }
        }


        private string _name;
        private int _position;
    }
}
