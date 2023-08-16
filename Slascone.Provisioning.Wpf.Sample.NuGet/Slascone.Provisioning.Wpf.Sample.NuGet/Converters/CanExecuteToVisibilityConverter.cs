using System;
using System.Globalization;
using System.Windows.Data;

namespace Slascone.Provisioning.Wpf.Sample.NuGet.Converters
{
	/// <summary>
	/// Value converter that converts the CanExecute property of an ICommand to visibility
	/// </summary>
	public sealed class CanExecuteToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			// Check if value is an ICommand
			if (value is System.Windows.Input.ICommand command)
			{
				// Check if command can be executed
				if (command.CanExecute(parameter))
				{
					return System.Windows.Visibility.Visible;
				}
			}

			// Default: Not visible
			return System.Windows.Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
