using Slascone.Provisioning.Wpf.Sample.NuGet.Services;
using System.Diagnostics;
using System.Reflection;
using System.Security.Policy;
using System.Windows;

namespace Slascone.Provisioning.Wpf.Sample.NuGet.Main
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			DataContext = new MainViewModel();
		}

		private void OnClickExit(object sender, RoutedEventArgs e)
		{
			// Exit the application
			Application.Current.Shutdown();
		}

		private void OnClickLicensing(object sender, RoutedEventArgs e)
		{
			((MainViewModel)DataContext).OpenLicenseManager();
		}

		private void OnClickHyperlinkSlasconeHomepage(object sender, RoutedEventArgs e)
		{
			Process.Start(new ProcessStartInfo("https://slascone.com")
			{
				UseShellExecute = true
			});
		}

		private void OnClickHyperlinkSlasconeSupport(object sender, RoutedEventArgs e)
		{
			Process.Start(new ProcessStartInfo("https://support.slascone.com")
			{
				UseShellExecute = true
			});
		}

		private void OnClickHyperlinkSlasconePortal(object sender, RoutedEventArgs e)
		{
			Process.Start(new ProcessStartInfo("https://portal.slascone.com")
			{
				UseShellExecute = true
			});
		}

		private void OnClickAbout(object sender, RoutedEventArgs e)
		{
			MessageBox.Show(
				$"SLASCONE Provisioning Sample Application Version {Assembly.GetAssembly(typeof(MainWindow)).GetName().Version}",
				"About",
				MessageBoxButton.OK,
				MessageBoxImage.Information);
		}
	}
}
