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

		private bool _isIconCheckVisible;
		private bool _isIconAttentionVisible;
		private bool _isIconExclamationVisible;
		private bool _isIconPendingVisible;

		private bool _canActivateLicense;
		private bool _canUnassignLicense;

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
						inlines.Add(new Run("License needs to be activated."));
						break;

					case LicensingState.NeedsOfflineActivation:
						inlines.Add(new Run("License file found, but needs to be activated."));
						inlines.Add(new LineBreak());
						inlines.Add(new Run("Please request and upload an activation file!"));
						break;

					case LicensingState.Invalid:
						inlines.Add(new Run(_licensingService.LicensingStateDescription));
						break;

					case LicensingState.LicenseFileMissing:
						inlines.Add(new Run("Not licensed. Please upload a license file missing."));
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
		{
			get =>
				LicensingState.FullyValidated == _licensingService.LicensingState
				|| LicensingState.NeedsActivation == _licensingService.LicensingState
				|| LicensingState.TemporaryOfflineValidated == _licensingService.LicensingState
				|| LicensingState.Invalid == _licensingService.LicensingState;
			set
			{
				if (!value)
					return;

				if ((LicensingState.OfflineValidated != _licensingService.LicensingState
				     && LicensingState.NeedsOfflineActivation != _licensingService.LicensingState)
				    || (AskSwitchToOnlineMode?.Invoke() ?? false))
				{
					Task.Run(async () =>
					{
						await _licensingService.SwitchToOnlineLicensingModeAsync().ConfigureAwait(false);
					});
				}
			}
		}

		public bool IsOfflineLicensingMode
		{
			get =>
				LicensingState.OfflineValidated == _licensingService.LicensingState
				|| LicensingState.NeedsOfflineActivation == _licensingService.LicensingState
				|| LicensingState.LicenseFileMissing == _licensingService.LicensingState;
			set
			{
				if (!value)
					return;

				if ((LicensingState.FullyValidated != _licensingService.LicensingState
				     && LicensingState.TemporaryOfflineValidated != _licensingService.LicensingState)
				    || (AskSwitchToOfflineMode?.Invoke() ?? false))
				{
					Task.Run(async () =>
					{
						await _licensingService.SwitchToOfflineLicensingModeAsync().ConfigureAwait(false);
					});
				}
			}
		}

		public bool IsIconCheckVisible
		{
			get => _isIconCheckVisible;
			set
			{
				_isIconCheckVisible = value; 
				OnPropertyChanged(nameof(IsIconCheckVisible));

				if (_isIconCheckVisible)
				{
					// Set all other to false
					IsIconAttentionVisible = false;
					IsIconExclamationVisible = false;
					IsIconPendingVisible = false;
				}
			}
		}

		public bool IsIconAttentionVisible
		{
			get => _isIconAttentionVisible;
			set
			{
				_isIconAttentionVisible = value; 
				OnPropertyChanged(nameof(IsIconAttentionVisible));

				if (_isIconAttentionVisible)
				{
					// Set all other to false
					IsIconCheckVisible = false;
					IsIconExclamationVisible = false;
					IsIconPendingVisible = false;
				}
			}
		}

		public bool IsIconExclamationVisible
		{
			get => _isIconExclamationVisible;
			set
			{
				_isIconExclamationVisible = value; 
				OnPropertyChanged(nameof(IsIconExclamationVisible));

				if (_isIconExclamationVisible)
				{
					// Set all other to false
					IsIconCheckVisible = false;
					IsIconAttentionVisible = false;
					IsIconPendingVisible = false;
				}
			}
		}

		public bool IsIconPendingVisible
		{
			get => _isIconPendingVisible;
			set
			{
				_isIconPendingVisible = value; 
				OnPropertyChanged(nameof(IsIconPendingVisible));

				if (_isIconPendingVisible)
				{
					// Set all other to false
					IsIconCheckVisible = false;
					IsIconAttentionVisible = false;
					IsIconExclamationVisible = false;
				}
			}
		}

		public string LicenseState
			=> _licensingService.LicensingState switch
			{
				LicensingState.FullyValidated => $"License validated; last heartbeat at {_licensingService.CreatedDateUtc:g}",
				LicensingState.OfflineValidated => "License validated (License file)",
				LicensingState.TemporaryOfflineValidated => $"License in temporary offline mode; last heartbeat at {_licensingService.CreatedDateUtc:g}",
				LicensingState.NeedsActivation => "Not licensed. Please activate an online license!",
				LicensingState.NeedsOfflineActivation => "License not activated. Please request and upload an activation file!",
				LicensingState.Invalid => "Not licensed. Please activate an online license!",
				LicensingState.LicenseFileMissing => "Not licensed. Please upload a license file!",
				LicensingState.Pending => "Pending ...",
				_ => throw new ArgumentOutOfRangeException()
			};

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

		#region User interaction 

		public delegate bool AskSwitchToOnlineModeDelegate();

		public AskSwitchToOnlineModeDelegate AskSwitchToOnlineMode { get; set; }

		public delegate bool AskSwitchToOfflineModeDelegate();

		public AskSwitchToOfflineModeDelegate AskSwitchToOfflineMode { get; set; }

		#endregion

		#region Implementation

		private void ActivateLicense()
		{
			var licenseKeyWindow = new LicenseKeyWindow
			{
				DemoLicenseKey = _licensingService.DemoLicenseKey
			};
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
			Application.Current.Dispatcher.Invoke(() =>
			{
				CanActivateLicense = LicensingState.NeedsActivation == e.LicensingState;
				CanUnassignLicense = LicensingState.FullyValidated == e.LicensingState;
				_uploadLicenseFileCommand?.NotifyCanExecuteChanged();
				_uploadActivationFileCommand?.NotifyCanExecuteChanged();
				OnPropertyChanged(nameof(LicenseInfoInlines));
				OnPropertyChanged(nameof(LicenseState));
				OnPropertyChanged(nameof(IsOnlineLicensingMode));
				OnPropertyChanged(nameof(IsOfflineLicensingMode));

				switch (e.LicensingState)
				{
					case LicensingState.FullyValidated:
						IsIconCheckVisible = true;
						break;
					case LicensingState.OfflineValidated:
						IsIconCheckVisible = true;
						break;
					case LicensingState.TemporaryOfflineValidated:
						IsIconAttentionVisible = true;
						break;
					case LicensingState.NeedsActivation:
						IsIconExclamationVisible = true;
						break;
					case LicensingState.NeedsOfflineActivation:
						IsIconExclamationVisible = true;
						break;
					case LicensingState.Invalid:
						IsIconExclamationVisible = true;
						break;
					case LicensingState.LicenseFileMissing:
						IsIconExclamationVisible = true;
						break;
					case LicensingState.Pending:
						IsIconPendingVisible = true;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			});
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