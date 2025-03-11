using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

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

			// Get main view model from application resources
			DataContext = Application.Current.FindResource(nameof(MainViewModel)) as MainViewModel;
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
			Process.Start(new ProcessStartInfo("https://demo.slascone.com")
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

		private void OnClickOffline(object sender, RoutedEventArgs e)
		{
			if (DataContext is not MainViewModel vm)
				return;
			
			if (sender is MenuItem mi)
			{
				if (vm.Offline)
					vm.Offline = false;
				else
					vm.Offline = true;
			}
		}

		protected override void OnClosed(EventArgs e)
		{
			if (DataContext is MainViewModel vm)
				vm.Dispose();

			base.OnClosed(e);
		}

		private void OnClickFeature(object sender, RoutedEventArgs e)
		{
			if (DataContext is not MainViewModel vm)
				return;

			if (sender is MenuItem mi)
				vm.AddUsageHeartbeat(mi.Header.ToString());
		}

		private void OnClickLimitation(object sender, RoutedEventArgs e)
		{
			if (DataContext is not MainViewModel vm)
				return;

			if (sender is MenuItem mi)
				vm.AddConsumptionHeartbeat(mi.Tag.ToString());
		}
	}
}
