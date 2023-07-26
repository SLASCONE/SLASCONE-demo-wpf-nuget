using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Slascone.Provisioning.Wpf.Sample.NuGet.Licensing;
using Slascone.Provisioning.Wpf.Sample.NuGet.Services;
using LicenseManager = Slascone.Provisioning.Wpf.Sample.NuGet.Licensing.LicenseManager;

namespace Slascone.Provisioning.Wpf.Sample.NuGet.Main
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        #region Fields

        private readonly LicensingService _licensingService;
        private readonly LicenseManagerViewModel _licenseManagerViewModel;
        private string _licensingStateDescription;
        private bool _licensingStateIsPending;
        private bool _licensingStateIsValid;
        private bool _licensingStateIsOffline;
        private bool _licensingStateIsInvalid;

        #endregion

        #region Construction

        public MainViewModel()
        {
	        LicensingStateDescription = "License validation pending ...";
            LicensingStateIsPending = true;
            
	        _licensingService = new LicensingService();
	        _licensingService.LicensingStateChanged += LicensingService_LicensingStateChanged;
            _licensingService.LicensingError += LicensingService_LicensingError;
            
            _licenseManagerViewModel = new LicenseManagerViewModel(_licensingService);
        }

        #endregion

		#region Interface

		public bool LicensingStateIsPending
		{
			get => _licensingStateIsPending;
			set
			{
				_licensingStateIsPending = value;
				OnPropertyChanged(nameof(LicensingStateIsPending));

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
				_licensingStateIsValid = value;
				OnPropertyChanged(nameof(LicensingStateIsValid));

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
				_licensingStateIsOffline = value;
				OnPropertyChanged(nameof(LicensingStateIsOffline));

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
				_licensingStateIsInvalid = value;
				OnPropertyChanged(nameof(LicensingStateIsInvalid));

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
            private set
            {
                _licensingStateDescription = value;
                OnPropertyChanged(nameof(LicensingStateDescription));
            }
        }

        public void OpenLicenseManager()
        {
	        var licenseManager = new LicenseManager { DataContext = _licenseManagerViewModel };
            licenseManager.ShowDialog();
        }

		#endregion

		#region Event handling

		private void LicensingService_LicensingError(object? sender, LicensingErrorEventArgs e)
		{
			MessageBox.Show(e.Message, "Licensing Error", MessageBoxButton.OK, e.MessageBoxImage);
		}

		private void LicensingService_LicensingStateChanged(object? sender, LicensingStateChangedEventArgs e)
		{
			switch (e.LicensingState)
			{
				case LicensingState.FullyValidated:
					LicensingStateIsValid = true;
					break;
				case LicensingState.OfflineValidated:
					LicensingStateIsOffline = true;
					break;
				case LicensingState.NeedsActivation:
					LicensingStateIsInvalid = true;
					break;
				case LicensingState.Invalid:
					LicensingStateIsInvalid = true;
					break;
				case LicensingState.Pending:
					LicensingStateIsPending = true;
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
    }
}
