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

		private void OnClickAbout(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("SLASCONE Provisioning Sample Application", "About", MessageBoxButton.OK, MessageBoxImage.Information);
		}
	}
}
