using System;
using System.IO;
using System.Configuration;
using System.Collections.Generic;
using System.Collections.Specialized;

using System.Windows.Forms;

using PlotComPlus.Gui;
using PlotComPlus.Logging;
using PlotComPlus.Series;
using PlotComPlus.ProcessFilters;


namespace PlotComPlus
{
    /// <summary>
    /// The console application.  Note that in interactive
    /// mode a window will pop up.
    /// </summary>
    static public class Program
    {
        /// <summary>
        /// The default output file extension.  This will
        /// be used if nothing is specified or the specified
        /// extension is invalid.
        /// </summary>
        private static string DEFAULT_EXT = ".png";


        /// <summary>
        /// Can only generate file with these extensions.
        /// </summary>
        private static List<string> VALID_EXTS = new List<string>(
            new string[] { DEFAULT_EXT, ".jpg", ".gif", ".tif", ".bmp", ".emf" }
        );


        /// <summary>
        /// The application's entry point.
        /// </summary>
        [STAThread]
        public static int Main(string[] args)
        {
            string patterns = null;
            string logFiles = null;
            string outputPath = null;
            OutputType outputChoice = 0;

            List<string> helps = new List<string>(
                new string[] { "--help", "-h", "/?", "/HELP", "-help" });
            
            foreach (string arg in args)
            {
                if (helps.Contains(arg))
                {
                    ShowHelp();
                    return 1;
                }
            }

            switch (args.Length)
            {
                case 1:
                    // Valid case:
                    // plot_complus.exe log_file.txt
                    logFiles = args[0];
                    outputChoice = OutputType.Interactive;
                    break;

                case 2:
                    // Valid case:
                    // plot_complus.exe log_file.txt process_filter
                    logFiles = args[0];
                    patterns = args[1];
                    outputChoice = OutputType.Interactive;
                    break;
                
                case 3:
                    // Valid cases:
                    // plot_complus.exe log_file.txt -o out_graph.png
                    // plot_complus.exe log_file.txt -a out_dir
                    if (args[1] == "-o" || args[1] == "-a")
                    {
                        logFiles = args[0];
                        outputChoice = (args[1] == "-o" ? 
                            OutputType.File : OutputType.Directory);
                        outputPath = args[2];
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Invalid arguments {0}!", args[1]);
                        return 1;
                    }
                
                case 4:
                    // Valid case:
                    // plot_complus.exe log_file.txt process_filter -o out_graph.png
                    if (args[2] == "-o")
                    {
                        logFiles = args[0];
                        patterns = args[1];
                        outputChoice = OutputType.File;
                        outputPath = args[3];

                        break;
                    }
                    else
                    {
                        Console.WriteLine("Invalid arguments {0}!", args[2]);
                        return 1;
                    }
                
                default:
                    Console.WriteLine("Wrong number of input arguments!");
                    return 1;
            }


            IProcessFilter processFilter;
            if (patterns != null)
            {
                processFilter = new ProcessFilter(patterns);
            }
            else
            {
                processFilter = new AllProcesses();
            }

            ILogger logger = new ConsoleLogger();
            SeriesFilter seriesFilter = new SeriesFilter();

            List<string[]> tupleList = Settings.DesiredSeries(
              ConfigurationManager.AppSettings);

            foreach (string[] seriesTuple in tupleList)
            {
                seriesFilter.AddDesiredSeries(seriesTuple[0], seriesTuple[1]);
            }

            GraphPlotter plotter = new GraphPlotter(seriesFilter);

            LogParser parser = new LogParser(processFilter, seriesFilter, logger);
            List<Process> processes = parser.ParseLogs(logFiles);
            
            switch (outputChoice)
            {
                case OutputType.Directory:
                    outputPath = Path.GetFullPath(outputPath);

                    if (!Directory.Exists(outputPath))
                    {
                        Directory.CreateDirectory(outputPath);
                    }

                    Dictionary<string, List<Process>> processGroup = 
                        new Dictionary<string, List<Process>>();

                    foreach (Process p in processes)
                    {
                        if (!processGroup.ContainsKey(p.ProcessName))
                        {
                            processGroup[p.ProcessName] = new List<Process>();
                        }
                        processGroup[p.ProcessName].Add(p);
                    }

                    foreach(KeyValuePair<string, List<Process>> pair in processGroup)
                    {
                        string path = Path.Combine(outputPath, pair.Key + DEFAULT_EXT);
                        plotter.WriteToFile(pair.Value, path);
                    }
                    break;

                case OutputType.File:
                    outputPath = Path.GetFullPath(outputPath);
                    string ext = Path.GetExtension(outputPath).ToLower();

                    if (ext.Length > 0)
                    {
                        if (!VALID_EXTS.Contains(ext))
                        {
                            Console.WriteLine("The graph cannot be saved in {0}!" +
                                "  Will save it in {1} instead.", ext, DEFAULT_EXT);
                            outputPath = Path.GetFileNameWithoutExtension(outputPath)
                                + DEFAULT_EXT;
                        }
                    }
                    else
                    {
                        Console.WriteLine("The graph will be saved in {0}.", DEFAULT_EXT);
                        outputPath = Path.GetFileNameWithoutExtension(outputPath)
                            + DEFAULT_EXT;
                    }

                    plotter.WriteToFile(processes, outputPath);
                    break;
                
                case OutputType.Interactive:
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new Form1(plotter, processes));                    
                    break;
            }

            return 0;            
        }


        private static void ShowHelp()
        {
            string msg = @"Description:
    Plot the output log file from mon_complus.vbs.  The valid file 
    extension for saving the plot to a file are .jpg, .pdf, .png, 
    and .svg.

Assumptions:
1)  Each log file must have a header line at the top of the file.
2)  The columns in the log file must begin in order from left with:
    Time, Computer Name, Process Name
3)  The fields in the log file must be separated by comma (,)

Syntax:
    plot_complus.exe log[,log2,...] [process1,process2,...] [options]

Options:
    -o      Save the graph to a file instead of displaying it.
    -a      Generate a graph for each COM+ application and save them.
    -h      Display this message.

Examples:
1)  Show all processes in a log (generally not too useful):
    plot_complus.exe log1.txt  

2)  Show a process from 2 different logs on 1 graph:
    plot_complus.exe log1.txt,log2.txt process1

3)  Same as above, but save it to a file:
    plot_complus.exe log1.txt,log2.txt process1 -o graph.png

4)  Show 2 processes from a log on 1 graph:
    plot_complus.exe log1.txt process1,process2

5)  Same as above, but save it to a file:
    plot_complus.exe log1.txt process1,process2 -o graph.png

6)  Save a graph for each process in a log to a folder:
    plot_complus.exe log1.txt -a graph_folder
   
Requirement: 
    .NET 2.0
";
            Console.Write(msg);
        }

    }
}
