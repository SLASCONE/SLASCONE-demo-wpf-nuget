using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using QRCoder;
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
		private bool _canRefreshLicense;
		private bool _canRequestActivationFile;

		private BitmapImage _activationFileRequestQrCode;

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
			CanRefreshLicense = LicensingState.NeedsActivation != _licensingService.LicensingState;
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

		public bool CanRefreshLicense
		{
			get => _canRefreshLicense;
			set
			{
				_canRefreshLicense = value;
				OnPropertyChanged(nameof(CanRefreshLicense));
				Application.Current.Dispatcher.Invoke(() => _refreshLicenseCommand?.NotifyCanExecuteChanged());
			}
		}

		public ICommand RefreshLicenseCommand
			=> _refreshLicenseCommand
				??= new RelayCommand(
					() => Task.Run(async () => await _licensingService.RefreshLicenseInformationAsync().ConfigureAwait(false)), 
					() => CanRefreshLicense);

		public ICommand UploadLicenseFileCommand
			=> _uploadLicenseFileCommand ??= new RelayCommand(UploadLicenseFile);

		public ICommand RequestActivationFileCommand
			=> _requestActivationFileCommand ??= new RelayCommand(RequestActivationFile);

		public ICommand UploadActivationFileCommand
			=> _uploadActivationFileCommand
				??= new RelayCommand(UploadActivationFile,
					() => LicensingState.NeedsOfflineActivation == _licensingService.LicensingState);

		public bool CanRequestActivationFile
			=> LicensingState.NeedsOfflineActivation == _licensingService.LicensingState;

		public ObservableCollection<Inline> LicenseInfoInlines
		{
			get
			{
				var inlines = new ObservableCollection<Inline>();

				if (LicensingState.NeedsActivation != _licensingService.LicensingState
				    && LicensingState.Invalid != _licensingService.LicensingState
				    && LicensingState.Pending != _licensingService.LicensingState)
				{
					inlines.Add(new Run("Product information") { FontWeight = FontWeights.Bold });
					inlines.Add(new LineBreak());
					inlines.Add(new Run($"Product name: {_licensingService.ProductName}"));
					inlines.Add(new LineBreak());
					inlines.Add(new Run($"Product edition: {_licensingService.Edition}"));
					inlines.Add(new LineBreak());
					inlines.Add(new LineBreak());
				}

				if (null != _licensingService.Customer)
				{
					inlines.Add(new Run("Customer") { FontWeight = FontWeights.Bold });
					inlines.Add(new LineBreak());
					inlines.Add(new Run(_licensingService.Customer?.Company_name));
					inlines.Add(new LineBreak());
					inlines.Add(new LineBreak());
				}

				if (_licensingService.Features.Any())
				{
					inlines.Add(new Run("License features") { FontWeight = FontWeights.Bold });
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
					inlines.Add(new Run("License variables") { FontWeight = FontWeights.Bold });
					inlines.Add(new LineBreak());
					foreach (var variable in _licensingService.Variables.OrderBy(v => v.Name))
					{
						inlines.Add(new Span(new Run($"{variable.Name}: {variable.Value}")));
						inlines.Add(new LineBreak());
					}

					inlines.Add(new LineBreak());
				}

				if (_licensingService.ConstrainedVariables.Any())
				{
					inlines.Add(new Run("License constrained variables") { FontWeight = FontWeights.Bold });
					inlines.Add(new LineBreak());
					foreach (var variable in _licensingService.ConstrainedVariables.OrderBy(v => v.Name))
					{
						inlines.Add(new Span(new Run($"{variable.Name}: {string.Join(", ", variable.Value)}")));
						inlines.Add(new LineBreak());
					}

					inlines.Add(new LineBreak());
				}

				inlines.Add(new Run("License state") { FontWeight = FontWeights.Bold });
				inlines.Add(new LineBreak());

				switch (_licensingService.LicensingState)
				{
					case LicensingState.FullyValidated:
						inlines.Add(new Run($"Last heartbeat: {_licensingService.CreatedDateUtc.GetValueOrDefault().ToLocalTime():g}"));
						inlines.Add(new LineBreak());
						inlines.Add(new Run($"Expiration date: {_licensingService.ExpirationDateUtc.GetValueOrDefault().ToLocalTime():d}"));
						inlines.Add(new LineBreak());
						inlines.Add(new Run(_licensingService.FreerideGranted));
						break;

					case LicensingState.OfflineValidated:
						inlines.Add(new Run("License validated with license file (offline licensing)"));
						inlines.Add(new LineBreak());
						inlines.Add(new Run($"License will expire on {_licensingService.ExpirationDateUtc.GetValueOrDefault().ToLocalTime():d}."));
						break;

					case LicensingState.TemporaryOfflineValidated:
						inlines.Add(new Run(
							$"Temporary Offline License found, validated at {_licensingService.CreatedDateUtc.GetValueOrDefault().ToLocalTime():g}."));
						inlines.Add(new LineBreak());
						inlines.Add(new Run($"Remaining freeride period: {_licensingService.RemainingFreeride?.ToString("%d")} days"));
						inlines.Add(new LineBreak());
						inlines.Add(new Run($"License will expire on {_licensingService.ExpirationDateUtc.GetValueOrDefault().ToLocalTime():d}."));
						break;

					case LicensingState.NeedsActivation:
						inlines.Add(new Run("Activation required."));
						break;

					case LicensingState.NeedsOfflineActivation:
						inlines.Add(new Run("License file found, but needs to be activated."));
						inlines.Add(new LineBreak());
						inlines.Add(new Run("Please request and upload an activation file!"));
						inlines.Add(new LineBreak());
						inlines.Add(new Hyperlink(new Run(_licensingService.BuildActivationFileRequest().ToString()))
						{
							Command = RequestActivationFileCommand
						});
						break;

					case LicensingState.Invalid:
						inlines.Add(new Run(_licensingService.LicensingStateDescription));
						break;

					case LicensingState.LicenseFileMissing:
						inlines.Add(new Run("Not licensed. Please upload a license file."));
						break;

					case LicensingState.LicenseFileInvalid:
						inlines.Add(new Run($"Invalid: {_licensingService.LicensingStateDescription}."));
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
					inlines.Add(new Run("License keys") { FontWeight = FontWeights.Bold });
					inlines.Add(new LineBreak());
					inlines.Add(new Run($"License key: {_licensingService.LicenseKey}"));
					inlines.Add(new LineBreak());
					inlines.Add(new Run($"Token key: {_licensingService.TokenKey}"));
					inlines.Add(new LineBreak());
					inlines.Add(new Run($"License type: {_licensingService.LicenseType}"));
					inlines.Add(new LineBreak());
					inlines.Add(new LineBreak());
				}

				inlines.Add(new Run("Client info") { FontWeight = FontWeights.Bold });
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
				|| LicensingState.LicenseFileMissing == _licensingService.LicensingState
				|| LicensingState.LicenseFileInvalid == _licensingService.LicensingState;
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
				LicensingState.FullyValidated => $"License validated; Last heartbeat: {_licensingService.CreatedDateUtc.GetValueOrDefault().ToLocalTime():g}",
				LicensingState.OfflineValidated => "License validated (License file)",
				LicensingState.TemporaryOfflineValidated => $"License in temporary offline mode; Freeride period expires on {_licensingService.FreerideExpirationDate:d}",
				LicensingState.NeedsActivation => "Not licensed. Activation required!",
				LicensingState.NeedsOfflineActivation => $"Not licensed: {_licensingService.LicensingStateDescription}.",
				LicensingState.Invalid => $"Not licensed. {_licensingService.LicensingStateDescription}",
				LicensingState.LicenseFileMissing => "Not licensed. Please upload a license file!",
				LicensingState.LicenseFileInvalid => $"Not licensed: {_licensingService.LicensingStateDescription}.",
				LicensingState.Pending => "Pending ...",
				_ => throw new ArgumentOutOfRangeException()
			};

		public BitmapImage ActivationFileRequestQr
		{
			get
			{
				if (null == _activationFileRequestQrCode)
				{
					QRCodeGenerator qrGenerator = new QRCodeGenerator();
					QRCodeData qrCodeData = 
						qrGenerator.CreateQrCode(_licensingService.BuildActivationFileRequest().ToString(), QRCodeGenerator.ECCLevel.Q);
					QRCode qrCode = new QRCode(qrCodeData);
					var qrCodeBitmap = qrCode.GetGraphic(20);

					// Transform the Bitmap to a BitmapImage
					using (MemoryStream memory = new MemoryStream())
					{
						qrCodeBitmap.Save(memory, ImageFormat.Png);
						memory.Position = 0;
						_activationFileRequestQrCode = new BitmapImage();
						_activationFileRequestQrCode.BeginInit();
						_activationFileRequestQrCode.StreamSource = memory;
						_activationFileRequestQrCode.CacheOption = BitmapCacheOption.OnLoad;
						_activationFileRequestQrCode.EndInit();
					}
				}

				return _activationFileRequestQrCode;
			}
		}

		#endregion

		#region User interaction 

		public delegate bool AskSwitchToOnlineModeDelegate();

		public AskSwitchToOnlineModeDelegate AskSwitchToOnlineMode { get; set; }

		public delegate bool AskSwitchToOfflineModeDelegate();

		public AskSwitchToOfflineModeDelegate AskSwitchToOfflineMode { get; set; }

		private void RequestActivationFile()
		{
			Process.Start(new ProcessStartInfo(_licensingService.BuildActivationFileRequest().ToString())
			{
				UseShellExecute = true
			});
		}

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
				CanUnassignLicense = LicensingState.FullyValidated == e.LicensingState || LicensingState.Invalid == e.LicensingState;
				CanRefreshLicense = LicensingState.NeedsActivation != e.LicensingState;
				_uploadLicenseFileCommand?.NotifyCanExecuteChanged();
				_uploadActivationFileCommand?.NotifyCanExecuteChanged();
				OnPropertyChanged(nameof(LicenseInfoInlines));
				OnPropertyChanged(nameof(LicenseState));
				OnPropertyChanged(nameof(IsOnlineLicensingMode));
				OnPropertyChanged(nameof(IsOfflineLicensingMode));
				OnPropertyChanged(nameof(CanRequestActivationFile));
				OnPropertyChanged(nameof(UploadActivationFileCommand));

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
					case LicensingState.LicenseFileInvalid:
						IsIconExclamationVisible = true;
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