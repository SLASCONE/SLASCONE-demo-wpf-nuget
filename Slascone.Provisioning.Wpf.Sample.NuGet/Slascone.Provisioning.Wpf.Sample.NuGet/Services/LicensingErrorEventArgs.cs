using System.Windows;

namespace Slascone.Provisioning.Wpf.Sample.NuGet.Services;

internal struct LicensingErrorEventArgs
{
	public MessageBoxImage MessageBoxImage { get; set; }
	public string Message { get; set; }
}