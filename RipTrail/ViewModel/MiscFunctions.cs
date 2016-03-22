using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RipTrail.ViewModel
{
    class MiscFunctions
    {

        /// <summary>
        /// Removes the file extension from the file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string RemoveFileExtension(string fileName)
        {
            string ext = string.Empty;
            int fileExtPos = fileName.LastIndexOf(".", StringComparison.Ordinal);
            if (fileExtPos >= 0)
                ext = fileName.Substring(0, fileExtPos);

            return ext;
        }


        /// <summary>
        ///  Create a message box.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="caption"></param>
        /// <returns></returns>
        public Boolean ShowMessage(string message, string caption)
        {
            MessageBoxResult result = MessageBox.Show(message, caption, MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                return true;
            }
            else
            {
                return false;
            }           
        }


        /// <summary>
        /// Get current accent color
        /// </summary>
        /// <returns>A color object</returns>
        public Color GetAccentColor()
        {
            return (Color)Application.Current.Resources["PhoneAccentColor"];           
        }


        /// <summary>
        /// Gets current phone background color
        /// </summary>
        /// <returns></returns>
        public Color GetPhoneBackgroundColor()
        {
            return (Color)Application.Current.Resources["PhoneBackgroundColor"];
        }


        /// <summary>
        /// Compares two values and returns the greater
        /// </summary>
        /// <param name="value"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public double MaxDouble(double value, double max)
        {
            if (max < value)
                return value;
            else
                return max;
        }


        /// <summary>
        /// returns the average
        /// </summary>
        /// <param name="total"></param>
        /// <param name="divisor"></param>
        /// <returns></returns>
        public double AverageDouble(double total, int divisor)
        {
            return Math.Round(total / divisor, 2);
        }

    }
}
