﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Slascone.Provisioning.Wpf.Sample.NuGet.Converters
{
	/// <summary>
	///     Value converter that translates true to <see cref="Visibility.Visible" /> and false to
	///     <see cref="Visibility.Collapsed" />.
	/// </summary>
	public sealed class BooleanToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			bool visible = (value is bool && (bool)value);
			return visible ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
