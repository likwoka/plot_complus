using System;
using System.Collections.Generic;
using System.Drawing;

namespace PlotComPlus
{
    /// <summary>
    /// Generates an infinite stream of random colours.
    /// </summary>
    class ColorGenerator
    {

        /// <summary>
        /// Constructor.
        /// </summary>
        public ColorGenerator()
        {
            _current = new List<Color>(_colors);
            _rng = new Random();
        }


        /// <summary>
        /// Returns a System.Drawing.Color struct.
        /// </summary>
        /// <returns></returns>
        public Color Next()
        {
            int index = _rng.Next(_current.Count);
            Color result = _current[_rng.Next(index)];
            
            _current.RemoveAt(index);
            if (_current.Count == 0)
            {
                _current = new List<Color>(_colors);
            }

            return result;
        }


        /// <summary>
        /// We are listing the colours to be used and order here 
        /// since it is hard to randomize a colour sequence that
        /// is easy to distinguish. 
        /// </summary>
        private static readonly Color[] _colors = new Color[] {
            Color.Aqua,
            Color.Black,
            Color.Blue,
            Color.BlueViolet,
            Color.Brown,
            Color.CadetBlue,
            Color.Chartreuse,
            Color.Chocolate,
            Color.Coral,
            Color.CornflowerBlue,
            Color.Crimson,
            Color.DeepPink,
            Color.DimGray,
            Color.DodgerBlue,
            Color.Firebrick,
            Color.ForestGreen,
            Color.Gold,
            Color.Goldenrod,
            Color.Gray,
            Color.Green,
            Color.GreenYellow,
            Color.HotPink,
            Color.IndianRed,
            Color.Indigo,
            Color.Khaki,
            Color.LawnGreen,
            Color.Lime,
            Color.LimeGreen,
            Color.Magenta,
            Color.Maroon,
            Color.MidnightBlue,
            Color.Navy,
            Color.Olive,
            Color.OliveDrab,
            Color.Orange,
            Color.OrangeRed,
            Color.Orchid,
            Color.PaleGreen,
            Color.PaleVioletRed,
            Color.PeachPuff,
            Color.Peru,
            Color.Pink,
            Color.Plum,
            Color.Purple,
            Color.Red,
            Color.RosyBrown,
            Color.RoyalBlue,
            Color.SaddleBrown,
            Color.Salmon,
            Color.SandyBrown,
            Color.SeaGreen,
            Color.Silver,
            Color.SkyBlue,
            Color.SlateBlue,
            Color.SlateGray,
            Color.SpringGreen,
            Color.SteelBlue,
            Color.Tan,
            Color.Teal,
            Color.Tomato,
            Color.Turquoise,
            Color.Violet,
            Color.Yellow,
            Color.YellowGreen
        };

        private List<Color> _current;
        private Random _rng;
    }
}
