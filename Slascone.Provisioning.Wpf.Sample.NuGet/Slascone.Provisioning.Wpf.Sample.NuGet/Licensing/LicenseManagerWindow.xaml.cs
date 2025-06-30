﻿using System;
using System.ComponentModel;
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

		private void OnClickClose(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		#endregion

		#region Event handler

		private void LicenseManager_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
            if (!(DataContext is LicenseManagerViewModel vm))
				return;

			vm.PropertyChanged += LicenseManager_PropertyChanged;
			vm.AskSwitchToOnlineMode = AskSwitchToOnlineMode;
			vm.AskSwitchToOfflineMode = AskSwitchToOfflineMode;

			licenseInformation.Inlines.Clear();
			licenseInformation.Inlines.AddRange(((LicenseManagerViewModel)DataContext).LicenseInfoInlines);
		}

		private bool AskSwitchToOfflineMode()
		{
			return
				MessageBoxResult.Yes ==
				MessageBox.Show(this,
					$"Do you really want to switch to Offline Licensing Mode?{Environment.NewLine}The license will be unassigned.",
					"Switch licensing mode",
					MessageBoxButton.YesNo,
					MessageBoxImage.Question);
		}

		private bool AskSwitchToOnlineMode()
		{
			return MessageBoxResult.Yes ==
			       MessageBox.Show(this,
				       $"Do you really want to switch to Online Licensing Mode?{Environment.NewLine}All license files will be removed.",
				       "Switch licensing mode",
				       MessageBoxButton.YesNo,
				       MessageBoxImage.Question);
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
