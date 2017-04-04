using System;
using System.Collections.Generic;
using System.Collections.Specialized;


namespace PlotComPlus
{
    /// <summary>
    /// Represents the settings in the app.config file.
    /// </summary>
    class Settings
    {

        /// <summary>
        /// Return the column settings in a more programming friendly
        /// List object.  Each item in the List will be a string array
        /// of 2 elements: name and type.
        /// </summary>
        /// <param name="appSettings"></param>
        /// <returns></returns>
        public static List<string[]>
            DesiredSeries(NameValueCollection appSettings)
        {
            List<string[]> result = new List<string[]>();
            string PREFIX = "Column.";
            int START_POS = PREFIX.Length;

            foreach (string key in appSettings.AllKeys)
            {
                if (key.StartsWith(PREFIX))
                {
                    string seriesName = key.Substring(START_POS);
                    string seriesType = "PlotComPlus.Series." + appSettings[key];
                    result.Add(new string[] { seriesName, seriesType });
                }
            }

            return result;
        }
    }
}
