using System;


namespace PlotComPlus
{
    /// <summary>
    /// The application's mode.
    /// </summary>
    enum OutputType
    {
        Interactive, // Popup a graph as a window.
        File,        // Save to a file then exit.
        Directory,   // Save all files to a directory then exit.
    }
}
