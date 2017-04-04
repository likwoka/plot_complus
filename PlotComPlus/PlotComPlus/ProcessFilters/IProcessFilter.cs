using System;


namespace PlotComPlus.ProcessFilters
{
    interface IProcessFilter
    {
        bool Contains(string pattern);
    }
}
