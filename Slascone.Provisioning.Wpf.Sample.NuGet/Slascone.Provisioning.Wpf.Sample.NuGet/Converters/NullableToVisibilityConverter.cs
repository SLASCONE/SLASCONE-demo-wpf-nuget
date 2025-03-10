using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Slascone.Provisioning.Wpf.Sample.NuGet.Converters
{
	public sealed class NullableToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null == value ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
