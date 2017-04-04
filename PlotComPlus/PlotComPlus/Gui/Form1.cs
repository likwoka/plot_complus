using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ZedGraph;


namespace PlotComPlus.Gui
{
    /// <summary>
    /// Represents the popup window which contains a graph control
    /// that a user can interact (zoom, save) with.
    /// </summary>
    partial class Form1 : Form
    {
        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="plotter">Plotter draws graph.</param>
        /// <param name="processes">Processes to be shown.</param>
        public Form1(GraphPlotter plotter, List<Process> processes)
        {
            InitializeComponent();
            _plotter = plotter;
            _processes = processes;
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            MasterPane master = zgc.MasterPane;
            master = _plotter.Plot(master, _processes);
            zgc.AxisChange();
            zgc.IsShowPointValues = true;

            using (Graphics g = this.CreateGraphics())
            {
                // Align all mini-graphs to single column.
                master.SetLayout(g, PaneLayout.SingleColumn);

                // Set the ticks/scale to a sensible value automatically.
                foreach (GraphPane pane in master.PaneList)
                {
                    pane.XAxis.ResetAutoScale(pane, g);
                    pane.YAxis.ResetAutoScale(pane, g);
                }
            }

            this.ResetGraphSize();
        }


        private void Form1_Resize(object sender, EventArgs e)
        {
            this.ResetGraphSize();
        }

        
        /// <summary>
        /// This allow the control to be resized with the
        /// pop up window.
        /// </summary>
        private void ResetGraphSize()
        {
            zgc.Location = new Point(10, 10);
            // Leave a small margin around 
            // the outside of the control.
            zgc.Size = new Size(ClientRectangle.Width - 20,
                ClientRectangle.Height - 20);
        }


        private GraphPlotter _plotter;
        private List<Process> _processes;
    }
}