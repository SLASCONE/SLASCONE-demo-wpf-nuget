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

		public bool UseDemoLicenseKey { get; set; }

		#endregion

		#region Button handlers

		private void Ok_Click(object sender, RoutedEventArgs e)
		{
			UseDemoLicenseKey = false;
			DialogResult = true;
			this.Close();
		}

		private void DemoLicense_Click(object sender, RoutedEventArgs e)
		{
			UseDemoLicenseKey = true;
			DialogResult = true;
			this.Close();
		}

		#endregion

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			this.Close();
		}
	}
}
