using System;
using System.Collections.Generic;
using System.Text;

using ZedGraph;
using System.Drawing;
using System.Drawing.Imaging;

using PlotComPlus.Series;
using PlotComPlus.Logging;


namespace PlotComPlus
{

    /// <summary>
    /// Plot a graph.
    /// </summary>
    class GraphPlotter
    {

        /// <summary>
        /// The constructor.
        /// </summary>
        public GraphPlotter(SeriesFilter seriesFilter)
        {
            _seriesFilter = seriesFilter;
            _colors = new ColorGenerator();
        }


        /// <summary>
        /// Plot the graph and save it to a file.
        /// </summary>
        /// <param name="processes">The processes we want to graph.</param>
        /// <param name="path">The filename to save the graph to.</param>
        public void WriteToFile(List<Process> processes, string path)
        {
            MasterPane master = Plot(
                new MasterPane("", new RectangleF(0, 0, 780, 580)), 
                processes);
           
            Bitmap b = new Bitmap(1, 1);
            using (Graphics g = Graphics.FromImage(b))
            {
                master.SetLayout(g, PaneLayout.SingleColumn);
                master.AxisChange(g);
            }

            master.GetImage().Save(path);
        }


        /// <summary>
        /// Plot the graph to the MasterPane given, and then return it.
        /// </summary>
        /// <param name="master">An empty MasterPane.</param>
        /// <param name="processes">The processes we want to graph.</param>
        /// <returns>The MasterPane with the graph.</returns>
        public MasterPane Plot(MasterPane master, List<Process> processes)
        {
            float FONT_SIZE = 22.0f;

            master.PaneList.Clear();
            master.Margin.All = 10;
            master.InnerPaneGap = 5;



            for (int i = 0; i < _seriesFilter.Series.Count; i++)
            {
                ISeries series = _seriesFilter.Series[i];
                GraphPane p = new GraphPane(new Rectangle(10, 10, 10, 10),
                    "", "Elapsed Time (min)", series.Name + " " + series.Unit);

                // Show the X title and scale on the last GraphPane only.
                if (i == _seriesFilter.Series.Count - 1)
                {
                    p.XAxis.Title.IsVisible = true;
                    p.XAxis.Scale.IsVisible = true;

                    p.XAxis.Title.FontSpec.Size = FONT_SIZE;
                    p.XAxis.Scale.FontSpec.Size = FONT_SIZE;
                }
                else
                {
                    p.XAxis.Title.IsVisible = false;
                    p.XAxis.Scale.IsVisible = false;
                }

                p.Legend.IsVisible = true;
                p.Legend.FontSpec.Size = 16.0f;
                p.Legend.IsHStack = false;
                p.Legend.Position = LegendPos.InsideBotRight;

                p.Border.IsVisible = false;
                p.Title.IsVisible = false;

                p.XAxis.MajorTic.IsOutside = false;
                p.XAxis.MinorTic.IsOutside = false;

                p.XAxis.MajorGrid.IsVisible = true;
                p.XAxis.MinorGrid.IsVisible = false;

                p.YAxis.MajorTic.IsOutside = false;
                p.YAxis.MinorTic.IsOutside = false;

                p.YAxis.MajorGrid.IsVisible = true;
                p.YAxis.MinorGrid.IsVisible = false;

                // Set the BaseDimension, so fonts are scale a little bigger.
                // Note that this attribute is still pretty magical.
                p.BaseDimension = 8.0f;
                p.YAxis.Title.FontSpec.Size = FONT_SIZE;
                p.YAxis.Scale.FontSpec.Size = FONT_SIZE;
                
                // This sets the minimum amount of space for the left 
                // and right side, respectively.  The reason for this 
                // is so that the ChartRect's all end up being the 
                // same size.
                p.YAxis.MinSpace = 80;
                p.Y2Axis.MinSpace = 20;

                p.Margin.All = 0;

                master.Add(p);
            }

            foreach (Process process in processes)
            {
                double[] timeValues = Convertor.TimestampsToTicks(process.TimeSeries);
                Color color = _colors.Next();

                for (int i = 0; i < _seriesFilter.Series.Count; i++)
                {
                    ISeries series = _seriesFilter.Series[i];
                    double[] yValues = new double[timeValues.Length];
                    process.Get(series.Name).CopyTo(yValues, 0);

                    master.PaneList[i].AddCurve(
                        process.Id,
                        timeValues,
                        yValues,
                        color,
                        series.SymbolType);
                }
            }

            return master;
        }


        private SeriesFilter _seriesFilter;
        private ColorGenerator _colors;
    }
}
