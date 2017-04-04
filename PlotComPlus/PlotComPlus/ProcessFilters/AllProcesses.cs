using System;


namespace PlotComPlus.ProcessFilters
{
    /// <summary>
    /// This process filter represents all processes, 
    /// hence always return True.
    /// </summary>
    class AllProcesses : IProcessFilter 
    {
        public bool Contains(string pattern)
        {
            return true;
        }
    }
}
