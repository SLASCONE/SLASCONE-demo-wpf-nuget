using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Slascone.Provisioning.Wpf.Sample.NuGet.Services;

namespace Slascone.Provisioning.Wpf.Sample.NuGet.Licensing
{
	public class LicenseManagerViewModel : INotifyPropertyChanged
	{
		#region Fields

		private readonly LicensingService _licensingService;
		private bool _canActivateLicense;
		private bool _canUnassignLicense;
		private bool _ifInvalidOnlineMode = true;

		private RelayCommand? _activateLicenseCommand;
		private RelayCommand? _unassignLicenseCommand;
		private RelayCommand? _refreshLicenseCommand;

		private RelayCommand? _uploadLicenseFileCommand;
		private RelayCommand? _requestActivationFileCommand;
		private RelayCommand? _uploadActivationFileCommand;

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
				Application.Current.Dispatcher.Invoke(() => _activateLicenseCommand?.NotifyCanExecuteChanged());
			}
		}

		public ICommand ActivateLicenseCommand
			=> _activateLicenseCommand
				??= new RelayCommand(ActivateLicense, () => CanActivateLicense);

		public bool CanUnassignLicense
		{
			get => _canUnassignLicense;
			set
			{
				_canUnassignLicense = value;
				OnPropertyChanged(nameof(CanUnassignLicense));
				Application.Current.Dispatcher.Invoke(() => _unassignLicenseCommand?.NotifyCanExecuteChanged());
			}
		}

		public ICommand UnassignLicenseCommand
			=> _unassignLicenseCommand
				??= new RelayCommand(
					() => Task.Run(async () => await _licensingService.UnassignLicenseAsync().ConfigureAwait(false)),
					() => CanUnassignLicense);

		public ICommand RefreshLicenseCommand
			=> _refreshLicenseCommand
				??= new RelayCommand(() => Task.Run(async () =>
					await _licensingService.RefreshLicenseInformationAsync().ConfigureAwait(false)));

		public ICommand UploadLicenseFileCommand
			=> _uploadLicenseFileCommand ??= new RelayCommand(UploadLicenseFile);

		public ICommand RequestActivationFileCommand
			=> _requestActivationFileCommand
				??= new RelayCommand(() => RequestActivationFile(),
					() => LicensingState.NeedsOfflineActivation == _licensingService.LicensingState);

		public ICommand UploadActivationFileCommand
			=> _uploadActivationFileCommand ??= new RelayCommand(UploadActivationFile);

		public ObservableCollection<Inline> LicenseInfoInlines
		{
			get
			{
				var inlines = new ObservableCollection<Inline>();

				if (LicensingState.NeedsActivation != _licensingService.LicensingState
				    && LicensingState.Invalid != _licensingService.LicensingState
				    && LicensingState.Pending != _licensingService.LicensingState)
				{
					inlines.Add(new Run("Product information:") { FontWeight = FontWeights.Bold });
					inlines.Add(new LineBreak());
					inlines.Add(new Run($"Product name: {_licensingService.ProductName}"));
					inlines.Add(new LineBreak());
					inlines.Add(new Run($"Product edition: {_licensingService.Edition}"));
					inlines.Add(new LineBreak());
					inlines.Add(new LineBreak());
				}

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
						inlines.Add(new Run("License validated with license file (offline licensing)"));
						inlines.Add(new LineBreak());
						inlines.Add(new Run($"License will expire on {_licensingService.ExpirationDateUtc:d}."));
						break;

					case LicensingState.TemporaryOfflineValidated:
						inlines.Add(new Run(
							$"Temporary Offline License found, validated at {_licensingService.CreatedDateUtc:g}."));
						inlines.Add(new LineBreak());
						inlines.Add(new Run($"License will expire on {_licensingService.ExpirationDateUtc:d}."));
						break;

					case LicensingState.NeedsActivation:
						inlines.Add(new Run(_ifInvalidOnlineMode ? "License needs to be activated." : "Not licensed.")
							{ Foreground = System.Windows.Media.Brushes.Blue });
						break;

					case LicensingState.NeedsOfflineActivation:
						inlines.Add(new Run("License file found, but needs to be activated.")
							{ Foreground = System.Windows.Media.Brushes.Blue });
						inlines.Add(new LineBreak());
						inlines.Add(new Run("Please request and upload an activation file!")
							{ Foreground = System.Windows.Media.Brushes.Blue });
						break;

					case LicensingState.Invalid:
						inlines.Add(new Run(_licensingService.LicensingStateDescription)
							{ Foreground = System.Windows.Media.Brushes.Red });
						break;

					case LicensingState.Pending:
						inlines.Add(new Run("Pending ..."));
						break;

					default:
						throw new ArgumentOutOfRangeException();
				}

				inlines.Add(new LineBreak());
				inlines.Add(new LineBreak());

				if (LicensingState.FullyValidated == _licensingService.LicensingState
				    || LicensingState.OfflineValidated == _licensingService.LicensingState
				    || LicensingState.TemporaryOfflineValidated == _licensingService.LicensingState
				    || LicensingState.NeedsOfflineActivation == _licensingService.LicensingState)
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

		public bool IsOnlineLicensingMode
			=> LicensingState.FullyValidated == _licensingService.LicensingState
			|| (LicensingState.NeedsActivation == _licensingService.LicensingState && _ifInvalidOnlineMode)
			|| LicensingState.TemporaryOfflineValidated == _licensingService.LicensingState
			|| (LicensingState.Invalid == _licensingService.LicensingState && _ifInvalidOnlineMode);

		public void SwitchToOnlineLicensingMode()
		{
			_licensingService.RemoveOfflineLicenseFiles();
			_ifInvalidOnlineMode = true;

			Task.Run(async () =>
			{
				await _licensingService.RefreshLicenseInformationAsync().ConfigureAwait(false);
				Application.Current.Dispatcher.Invoke(() =>
				{
					_requestActivationFileCommand?.NotifyCanExecuteChanged();
					_requestActivationFileCommand?.NotifyCanExecuteChanged();
					OnPropertyChanged(nameof(IsOnlineLicensingMode));
					OnPropertyChanged(nameof(IsOfflineLicensingMode));
				});
			});
		}

		public object IsOfflineLicensingMode
			=> LicensingState.OfflineValidated == _licensingService.LicensingState
			   || LicensingState.NeedsOfflineActivation == _licensingService.LicensingState
			   || (LicensingState.NeedsActivation == _licensingService.LicensingState && !_ifInvalidOnlineMode)
			   || (LicensingState.Invalid == _licensingService.LicensingState && !_ifInvalidOnlineMode);

		public void SwitchToOfflineLicensingMode()
		{
			_licensingService.RemoveTemporaryOfflineLicenseFiles();
			_ifInvalidOnlineMode = false;

			Task.Run(async () =>
			{
				if (CanUnassignLicense)
					await _licensingService.UnassignLicenseAsync();

				await _licensingService.RefreshLicenseInformationAsync().ConfigureAwait(false);
				Application.Current.Dispatcher.Invoke(() =>
				{
					_requestActivationFileCommand?.NotifyCanExecuteChanged();
					_requestActivationFileCommand?.NotifyCanExecuteChanged();
					OnPropertyChanged(nameof(IsOnlineLicensingMode));
					OnPropertyChanged(nameof(IsOfflineLicensingMode));
				});
			});
		}

		public async Task RemoveOfflineLicenseFilesAsync()
		{
			_licensingService.RemoveOfflineLicenseFiles();

			await _licensingService.RefreshLicenseInformationAsync().ConfigureAwait(false);
			Application.Current.Dispatcher.Invoke(() => _requestActivationFileCommand?.NotifyCanExecuteChanged());
		}

		private void RequestActivationFile()
		{
			var activationFileRequest = _licensingService.BuildActivationFileRequest();

			var vm = new ActivationFileViewModel
			{
				ApiUrl = activationFileRequest.ApiUrl,
				IsvId = activationFileRequest.IsvId.ToString(),
				ProductId = activationFileRequest.ProductId.ToString(),
				LicenseKey = activationFileRequest.LicenseKey,
				ClientId = activationFileRequest.ClientId
			};

			new ActivationFileWindow
			{
				DataContext = vm
			}.ShowDialog();
		}

		#endregion

		#region Implementation

		private void ActivateLicense()
		{
			var licenseKeyWindow = new LicenseKeyWindow();
			licenseKeyWindow.DemoLicenseKey = _licensingService.DemoLicenseKey;
			licenseKeyWindow.ShowDialog();

			if (licenseKeyWindow.DialogResult.HasValue && licenseKeyWindow.DialogResult.Value)
			{
				var licenseKey = licenseKeyWindow.LicenseKey.Text;

				if (string.IsNullOrEmpty(licenseKey))
				{
					MessageBox.Show("Please enter a valid license key.", "License Key", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				Task.Run(async () => await _licensingService.ActivateLicenseAsync(licenseKey).ConfigureAwait(false));
			}
		}

		private void UploadLicenseFile()
		{
			var openFileDialog = new OpenFileDialog
			{
				Filter = "XML License files (*.xml)|*.xml|All files (*.*)|*.*",
				FilterIndex = 1,
				RestoreDirectory = true
			};

			if (openFileDialog.ShowDialog() == true)
			{
				var licenseFile = openFileDialog.FileName;
				Task.Run(async () =>
				{
					await _licensingService.UploadLicenseFileAsync(licenseFile).ConfigureAwait(false);
					Application.Current.Dispatcher.Invoke(() => _requestActivationFileCommand?.NotifyCanExecuteChanged());
				});
			}
		}

		private void UploadActivationFile()
		{
			var openFileDialog = new OpenFileDialog
			{
				Filter = "XML Activation files (*.xml)|*.xml|All files (*.*)|*.*",
				FilterIndex = 1,
				RestoreDirectory = true
			};

			if (openFileDialog.ShowDialog() == true)
			{
				var activationFile = openFileDialog.FileName;
				Task.Run(async () =>
				{
					await _licensingService.UploadActivationFileAsync(activationFile).ConfigureAwait(false);
					Application.Current.Dispatcher.Invoke(() => _requestActivationFileCommand?.NotifyCanExecuteChanged());
				});
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