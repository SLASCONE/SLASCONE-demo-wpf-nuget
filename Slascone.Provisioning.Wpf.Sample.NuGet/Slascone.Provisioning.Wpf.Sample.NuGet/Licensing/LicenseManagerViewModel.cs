using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using Slascone.Provisioning.Wpf.Sample.NuGet.Services;

namespace Slascone.Provisioning.Wpf.Sample.NuGet.Licensing
{
	public class LicenseManagerViewModel : INotifyPropertyChanged
	{
		#region Fields

		private readonly LicensingService _licensingService;
		private bool _canActivateLicense;
		private bool _canUnassignLicense;

		#endregion

		#region Construction

		internal LicenseManagerViewModel(LicensingService licensingService)
		{
			_licensingService = licensingService;
			_licensingService.LicensingStateChanged += LicensingService_LicensingStateChanged;

			CanActivateLicense = LicensingState.NeedsActivation == _licensingService.LicensingState;
			CanUnassignLicense = LicensingState.FullyValidated == _licensingService.LicensingState;
		}

		#endregion

		#region Interface

		public bool CanActivateLicense
		{
			get => _canActivateLicense;
			set
			{
				_canActivateLicense = value;
				OnPropertyChanged(nameof(CanActivateLicense));
			}
		}

		public void ActivateLicense()
		{
			var licenseKeyWindow = new LicenseKeyWindow();
			licenseKeyWindow.ShowDialog();

			if (licenseKeyWindow.DialogResult.HasValue && licenseKeyWindow.DialogResult.Value)
			{
				var useDemoLicenseKey = licenseKeyWindow.UseDemoLicenseKey;
				var licenseKey = licenseKeyWindow.LicenseKey.Text;
				
				if (!useDemoLicenseKey && string.IsNullOrEmpty(licenseKey))
				{
					MessageBox.Show("Please enter a valid license key.", "License Key", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				Task.Run(async () => await _licensingService.ActivateLicenseAsync(licenseKey, useDemoLicenseKey).ConfigureAwait(false));
			}
		}

		public bool CanUnassignLicense
		{
			get => _canUnassignLicense;
			set
			{
				_canUnassignLicense = value;
				OnPropertyChanged(nameof(CanUnassignLicense));
			}
		}

		public void UnassignLicense()
		{
			Task.Run(async () => await _licensingService.UnassignLicenseAsync().ConfigureAwait(false));
		}

		public void RefreshLicenseInfo()
		{
			Task.Run(async () => await _licensingService.RefreshLicenseInformationAsync().ConfigureAwait(false));
		}

		public ObservableCollection<Inline> LicenseInfoInlines
		{
			get
			{
				var inlines = new ObservableCollection<Inline>();

				if (null != _licensingService.Customer)
				{
					inlines.Add(new Run("Licensee:") { FontWeight = FontWeights.Bold });
					inlines.Add(new LineBreak());
					inlines.Add(new Run(_licensingService.Customer?.Company_name));
					inlines.Add(new LineBreak());
					inlines.Add(new LineBreak());
				}

				if (_licensingService.Features.Any())
				{
					inlines.Add(new Run("License features:") { FontWeight = FontWeights.Bold });
					inlines.Add(new LineBreak());
					foreach (var feature in _licensingService.Features)
					{
						inlines.Add(new Span(new Run(feature.Name)));
						inlines.Add(new LineBreak());
					}
					inlines.Add(new LineBreak());
				}

				if (_licensingService.Variables.Any())
				{
					inlines.Add(new Run("License variables:") { FontWeight = FontWeights.Bold });
					inlines.Add(new LineBreak());
					foreach (var variable in _licensingService.Variables.OrderBy(v => v.Name))
					{
						inlines.Add(new Span(new Run($"{variable.Name}: {variable.Value}")));
						inlines.Add(new LineBreak());
					}
					inlines.Add(new LineBreak());
				}

				inlines.Add(new Run("License state:") { FontWeight = FontWeights.Bold });
				inlines.Add(new LineBreak());

				switch (_licensingService.LicensingState)
				{
					case LicensingState.FullyValidated:
						inlines.Add(new Run($"License online validated at {_licensingService.CreatedDateUtc:g}."));
						inlines.Add(new LineBreak());
						inlines.Add(new Run($"License will expire on {_licensingService.ExpirationDateUtc:d}."));
						break;
					case LicensingState.OfflineValidated:
						inlines.Add(new Run($"Offline License found, validated at {_licensingService.CreatedDateUtc:g}."));
						inlines.Add(new LineBreak());
						inlines.Add(new Run($"License will expire on {_licensingService.ExpirationDateUtc:d}."));
						break;
					case LicensingState.NeedsActivation:
						inlines.Add(new Run("License needs to be activated.") { Foreground = System.Windows.Media.Brushes.Blue });
						break;
					case LicensingState.Invalid:
						inlines.Add(new Run(_licensingService.LicensingStateDescription) { Foreground = System.Windows.Media.Brushes.Red });
						break;
					case LicensingState.Pending:
						inlines.Add(new Run("Pending ..."));
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				inlines.Add(new LineBreak());
				inlines.Add(new LineBreak());

				if (LicensingState.FullyValidated == _licensingService.LicensingState ||
				    LicensingState.OfflineValidated == _licensingService.LicensingState)
				{
					inlines.Add(new Run("License keys:") { FontWeight = FontWeights.Bold });
					inlines.Add(new LineBreak());
					inlines.Add(new Run($"License key: {_licensingService.LicenseKey}"));
					inlines.Add(new LineBreak());
					inlines.Add(new Run($"Assignment token key: {_licensingService.TokenKey}"));
					inlines.Add(new LineBreak());
					inlines.Add(new LineBreak());
				}

				inlines.Add(new Run("Computer Info:") { FontWeight = FontWeights.Bold });
				inlines.Add(new LineBreak());
				inlines.Add(new Run($"Product version: {_licensingService.SoftwareVersion}"));
				inlines.Add(new LineBreak());
				inlines.Add(new Run($"Device ID: {_licensingService.DeviceId}"));
				inlines.Add(new LineBreak());
				inlines.Add(new Run($"Operating System: {_licensingService.OperatingSystem}"));
				inlines.Add(new LineBreak());

				return inlines;
			}
		}

		#endregion

		#region Event handlers

		// Event handler for LicensingService.LicensingStateChanged
		private void LicensingService_LicensingStateChanged(object? sender, LicensingStateChangedEventArgs e)
		{
			CanActivateLicense = LicensingState.NeedsActivation == e.LicensingState;
			CanUnassignLicense = LicensingState.FullyValidated == e.LicensingState;
			OnPropertyChanged(nameof(LicenseInfoInlines));
		}

		#endregion

		#region Implementation of INotifyPropertyChanged

		public event PropertyChangedEventHandler? PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
		{
			if (EqualityComparer<T>.Default.Equals(field, value)) return false;
			field = value;
			OnPropertyChanged(propertyName);
			return true;
		}

		#endregion
	}
}