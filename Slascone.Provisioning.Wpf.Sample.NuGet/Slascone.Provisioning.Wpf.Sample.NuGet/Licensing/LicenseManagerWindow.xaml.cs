using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace Slascone.Provisioning.Wpf.Sample.NuGet.Licensing
{
	/// <summary>
	/// Interaction logic for LicenseManagerWindow.xaml
	/// </summary>
	public partial class LicenseManagerWindow : Window
	{
		#region Constructor

		public LicenseManagerWindow()
		{
			InitializeComponent();

			DataContextChanged += LicenseManager_DataContextChanged;
		}

		#endregion

		#region GUI interactions

		private void OnClickOnlineLicensing(object sender, RoutedEventArgs e)
		{
			if (DataContext is not LicenseManagerViewModel vm)
				return;

			if (MessageBoxResult.Yes ==
			    MessageBox.Show(this,
				    $"Do you really want to switch to Online Licensing Mode?{Environment.NewLine}All license files will be removed.",
				    "Switch licensing mode",
				    MessageBoxButton.YesNo,
				    MessageBoxImage.Question))
			{
				vm.SwitchToOnlineLicensingMode();
			}
		}

		private void OnClickOfflineLicensing(object sender, RoutedEventArgs e)
		{
			if (DataContext is not LicenseManagerViewModel vm)
				return;

			if (MessageBoxResult.Yes ==
			    MessageBox.Show(this,
				    $"Do you really want to switch to Offline Licensing Mode?{Environment.NewLine}The license will be unassigned.",
				    "Switch licensing mode",
				    MessageBoxButton.YesNo,
				    MessageBoxImage.Question))
			{
				vm.SwitchToOfflineLicensingMode();
			}
		}

		private void OnClickRemoveOfflineLicense(object sender, RoutedEventArgs e)
		{
			if (DataContext is not LicenseManagerViewModel vm)
				return;

			if (MessageBoxResult.Yes == MessageBox.Show(this,
				    "Do you really want to remove all offline license files?",
				    "Really?",
				    MessageBoxButton.YesNo,
				    MessageBoxImage.Question))
			{
				Task.Run(async () => await vm.RemoveOfflineLicenseFilesAsync());
			}
		}

		private void OnClickClose(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		#endregion

		#region Event handler

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

		#endregion
	}
}
