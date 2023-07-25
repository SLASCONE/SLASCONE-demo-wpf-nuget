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

			CanActivateLicense = _licensingService.NeedsActivation;
			CanUnassignLicense = !_licensingService.NeedsActivation;
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

				if (_licensingService.IsValid)
				{
					inlines.Add(new Span(new Run($"License will expire on {_licensingService.ExpirationDateUtc:d}.")));
				}
				else if (_licensingService.NeedsActivation)
				{
					inlines.Add(new Run("License needs to be activated.") { Foreground = System.Windows.Media.Brushes.Blue });
				}
				else
				{
					inlines.Add(new Run("License is not valid!") { Foreground = System.Windows.Media.Brushes.Red });
				}

				inlines.Add(new LineBreak());
				inlines.Add(new LineBreak());
				inlines.Add(new Run("Computer Info:") { FontWeight = FontWeights.Bold });
				inlines.Add(new LineBreak());
				inlines.Add(new Run($"Device ID: {_licensingService.DeviceId}"));
				inlines.Add(new LineBreak());
				inlines.Add(new Run($"Operating System: {_licensingService.OperatingSystem}"));
				inlines.Add(new LineBreak());

				return inlines;
			}
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

		#region Implementation

		// Event handler for LicensingService.LicensingStateChanged
		private void LicensingService_LicensingStateChanged(object? sender, LicensingStateChangedEventArgs e)
		{
			CanActivateLicense = e.NeedsActivation;
			CanUnassignLicense = !e.NeedsActivation;
			OnPropertyChanged(nameof(LicenseInfoInlines));
		}

		#endregion
	}
}