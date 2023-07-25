using System;
using System.ComponentModel;
using System.Windows;

namespace Slascone.Provisioning.Wpf.Sample.NuGet.Licensing
{
	/// <summary>
	/// Interaction logic for LicenseManager.xaml
	/// </summary>
	public partial class LicenseManager : Window
	{
		public LicenseManager()
		{
			InitializeComponent();

			DataContextChanged += LicenseManager_DataContextChanged;
		}

		private void LicenseManager_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (DataContext is not LicenseManagerViewModel vm) 
				return;

			vm.PropertyChanged += LicenseManager_PropertyChanged;

			licenseInformation.Inlines.Clear();
			licenseInformation.Inlines.AddRange(((LicenseManagerViewModel)DataContext).LicenseInfoInlines);
		}

		private void LicenseManager_PropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case nameof(LicenseManagerViewModel.LicenseInfoInlines):
					Application.Current.Dispatcher.Invoke(() =>
					{
						licenseInformation.Inlines.Clear();
						licenseInformation.Inlines.AddRange(((LicenseManagerViewModel)DataContext).LicenseInfoInlines);
					});
					break;
			}
		}

		private void OnClickActivateLicense(object sender, RoutedEventArgs e)
		{
			if (DataContext is not LicenseManagerViewModel vm)
				return;

			vm.ActivateLicense();
		}

		private void OnClickUnassignLicense(object sender, RoutedEventArgs e)
		{
			if (DataContext is not LicenseManagerViewModel vm) 
				return;

			vm.UnassignLicense();
		}

		private void OnClickRefreshLicense(object sender, RoutedEventArgs e)
		{
			if (DataContext is not LicenseManagerViewModel vm)
				return;

			vm.RefreshLicenseInfo();
		}
	}
}
