using System;
using System.Windows;

namespace Slascone.Provisioning.Wpf.Sample.NuGet.Licensing
{
	/// <summary>
	/// Interaction logic for LicenseKeyWindow.xaml
	/// </summary>
	public partial class LicenseKeyWindow : Window
	{
		#region Construction

		public LicenseKeyWindow()
		{
			InitializeComponent();
		}

		#endregion

		#region Properties

		public string DemoLicenseKey { get; set; }

		#endregion

		#region Button handlers

		private void OnClickDemoLicense(object sender, RoutedEventArgs e)
		{
			LicenseKey.Text = DemoLicenseKey;
		}

		private void OnClickOk(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
			this.Close();
		}

		private void OnClickCancel(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			this.Close();
		}

		#endregion
	}
}
