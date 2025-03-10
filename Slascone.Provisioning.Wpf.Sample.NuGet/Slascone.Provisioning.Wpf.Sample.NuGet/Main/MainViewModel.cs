using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using Slascone.Client;
using Slascone.Provisioning.Wpf.Sample.NuGet.Licensing;
using Slascone.Provisioning.Wpf.Sample.NuGet.Services;

namespace Slascone.Provisioning.Wpf.Sample.NuGet.Main
{
    internal class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        #region Fields

        private readonly LicensingService _licensingService;
        private readonly LicenseManagerViewModel _licenseManagerViewModel;
        private string _licensingStateDescription;
        private bool _licensingStateIsPending;
        private bool _licensingStateIsValid;
        private bool _licensingStateIsOffline;
        private bool _licensingStateIsInvalid;
        private bool _licensingStateIsNoUserSignedIn;
        private bool _offline;
        private ObservableCollection<ProvisioningFeatureDto> _features = new ObservableCollection<ProvisioningFeatureDto>();
        private ObservableCollection<ProvisioningLimitationDto> _limitations = new ObservableCollection<ProvisioningLimitationDto>();

        #endregion

		#region Construction

		public MainViewModel()
        {
	        LicensingStateDescription = "License validation pending ...";
            LicensingStateIsPending = true;
            
			_licensingService = new LicensingService(new SlasconeClientConfiguration(),  App.AuthenticationService);
	        _licensingService.LicensingStateChanged += LicensingService_LicensingStateChanged;
            
            _licenseManagerViewModel = new LicenseManagerViewModel(_licensingService, App.AuthenticationService);

			_licenseManagerViewModel.RefreshLicenseCommand.Execute(null);
        }

        #endregion

		#region Interface

		public bool LicensingStateIsPending
		{
			get => _licensingStateIsPending;
			set
			{
				SetField(ref _licensingStateIsPending, value);

				if (_licensingStateIsPending)
				{
					// Set all others to false
                    LicensingStateIsValid = false;
                    LicensingStateIsOffline = false;
                    LicensingStateIsInvalid = false;
                    LicensingStateIsNoUserSignedIn = false;
				}
			}
		}

		public bool LicensingStateIsValid
		{
			get => _licensingStateIsValid;
			set
			{
				SetField(ref _licensingStateIsValid, value);

				if (_licensingStateIsValid)
				{
					// Set all others to false
                    LicensingStateIsPending = false;
					LicensingStateIsOffline = false;
					LicensingStateIsInvalid = false;
					LicensingStateIsNoUserSignedIn = false;
				}
			}
		}

		public bool LicensingStateIsOffline
		{
			get => _licensingStateIsOffline;
			set
			{
				SetField(ref _licensingStateIsOffline, value);

				if (_licensingStateIsOffline)
				{
					// Set all others to false
					LicensingStateIsPending = false;
					LicensingStateIsValid = false;
					LicensingStateIsInvalid = false;
					LicensingStateIsNoUserSignedIn = false;
				}
			}
		}

		public bool LicensingStateIsInvalid
		{
			get => _licensingStateIsInvalid;
			set
			{
				SetField(ref _licensingStateIsInvalid, value);

				if (_licensingStateIsInvalid)
				{
					// Set all others to false
					LicensingStateIsPending = false;
					LicensingStateIsValid = false;
					LicensingStateIsOffline = false;
					LicensingStateIsNoUserSignedIn= false;
				}

				OnPropertyChanged(nameof(ShowLicenseManagerButton));
			}
		}

		public bool LicensingStateIsNoUserSignedIn
		{
			get => _licensingStateIsNoUserSignedIn;
			set
			{
				SetField(ref _licensingStateIsNoUserSignedIn, value);

				if (_licensingStateIsNoUserSignedIn)
				{
					// Set all others to false
					LicensingStateIsPending = false;
					LicensingStateIsValid = false;
					LicensingStateIsOffline = false;
					LicensingStateIsInvalid = false;
				}
				
				OnPropertyChanged(nameof(ShowLicenseManagerButton));
			}
		}

		public string LicensingStateDescription
        {
            get => _licensingStateDescription;
            private set => SetField(ref _licensingStateDescription, value);
        }

		public bool Offline
		{
			get => _offline;
			set => SetField(ref _offline, value);
		}

		public bool ShowLicenseManagerButton
			=> _licensingStateIsInvalid || _licensingStateIsNoUserSignedIn;

		public ObservableCollection<ProvisioningFeatureDto> Features 
			=> _features;

		public ObservableCollection<ProvisioningLimitationDto> Limitations
			=> _limitations;

		public void OpenLicenseManager()
        {
	        var licenseManager = new LicenseManagerWindow { DataContext = _licenseManagerViewModel };
            licenseManager.ShowDialog();
        }

        public void AddUsageHeartbeat(string featureName)
        {
	        if (_licensingStateIsInvalid)
		        return;

			Task.Run(async () => await _licensingService.AddUsageHeartbeatAsync(featureName));
		}


		public void AddConsumptionHeartbeat(string limitationId)
		{
			if (_licensingStateIsInvalid)
				return;
			
			var limitation = _limitations.FirstOrDefault(l => l.Id.Equals(Guid.Parse(limitationId)));

			if (limitation == null)
				return;

			var limitationWindow = new LimitationWindow();
			limitationWindow.DataContext = new LimitationViewModel(_licensingService, limitation);
			limitationWindow.ShowDialog();
		}

		#endregion

		#region Event handling

		private void LicensingService_LicensingStateChanged(object? sender, LicensingStateChangedEventArgs e)
		{
			switch (e.LicensingState)
			{
				case LicensingState.FullyValidated:
					LicensingStateIsValid = true;
					break;

				case LicensingState.OfflineValidated:
					LicensingStateIsValid = true;
					break;

				case LicensingState.TemporaryOfflineValidated:
					LicensingStateIsOffline = true;
					break;

				case LicensingState.NeedsActivation:
					LicensingStateIsInvalid = true;
					break;
				
				case LicensingState.NeedsOfflineActivation:
					LicensingStateIsInvalid = true;
					break;

				case LicensingState.Invalid:
					LicensingStateIsInvalid = true;
					break;

				case LicensingState.NotSignedIn:
					LicensingStateIsNoUserSignedIn = true;
					break;

				case LicensingState.LicenseFileMissing:
					LicensingStateIsInvalid = true;
					break;

				case LicensingState.Pending:
					LicensingStateIsPending = true;
					break;

				case LicensingState.LicenseFileInvalid:
					LicensingStateIsInvalid = true;
					break;

				case LicensingState.FloatingLimitExceeded:
					LicensingStateIsInvalid = true;
					break;

				case LicensingState.SessionOpenFailed:
					LicensingStateIsInvalid = true;
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}

			LicensingStateDescription = e.LicensingStateDescription;

			var features = _licensingService.Features;

			Application.Current.Dispatcher.Invoke(() =>
			{
				// Remove all features from "Features" menu
				_features.Clear();

				// Add features to "Features" menu
				if (!_licensingStateIsInvalid)
				{
					foreach (var feature in features)
					{
						_features.Add(feature);
					}
				}

				// Remove all limitations from "Limitations" menu
				_limitations.Clear();

				// Add limitations to "Limitations" menu
				foreach (var limitation in _licensingService.Limitations)
				{
					_limitations.Add(limitation);
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
            if (EqualityComparer<T>.Default.Equals(field, value)) 
	            return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

		#endregion

		#region Implementation of IDisposable
		
		public void Dispose()
        {
	        _licensingService.Dispose();
        }

        #endregion
    }
}
