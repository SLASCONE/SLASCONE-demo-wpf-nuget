using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
        private bool _offline;

        #endregion

        #region Construction

        public MainViewModel()
        {
	        LicensingStateDescription = "License validation pending ...";
            LicensingStateIsPending = true;
            
			_licensingService = new LicensingService();
	        _licensingService.LicensingStateChanged += LicensingService_LicensingStateChanged;
            
            _licenseManagerViewModel = new LicenseManagerViewModel(_licensingService);

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
				}
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

		public void OpenLicenseManager()
        {
	        var licenseManager = new LicenseManagerWindow { DataContext = _licenseManagerViewModel };
            licenseManager.ShowDialog();
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
