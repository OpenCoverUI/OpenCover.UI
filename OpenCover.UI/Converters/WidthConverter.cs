//
// This source code is released under the GPL License; Please read license.md file for more details.
//
using System;
using System.Windows.Controls;
using System.Windows.Data;

namespace OpenCover.UI.Converters
{
	/// <summary>
	/// Relative width converter
	/// </summary>
	public class WidthConverter : IValueConverter
	{
		/// <summary>
		/// Converts a value.
		/// </summary>
		/// <param name="value">The value produced by the binding source.</param>
		/// <param name="targetType">The type of the binding target property.</param>
		/// <param name="parameter">The converter parameter to use.</param>
		/// <param name="culture">The culture to use in the converter.</param>
		/// <returns>
		/// A converted value. If the method returns null, the valid null value is used.
		/// </returns>
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var userControl = value as Control;
			double percentage = System.Convert.ToDouble(parameter);

			userControl.Width = userControl.ActualWidth;

			return Double.NaN;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
