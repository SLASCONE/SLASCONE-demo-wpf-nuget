using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
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
        private string _licensingState;

        #endregion

        #region Construction

        public MainViewModel()
        {
	        LicensingState = "License validation pending ...";
            
	        _licensingService = new LicensingService();
	        _licensingService.LicensingStateChanged += LicensingService_LicensingStateChanged;
            _licensingService.LicensingError += LicensingService_LicensingError;
            
            _licenseManagerViewModel = new LicenseManagerViewModel(_licensingService);
        }

        private void LicensingService_LicensingError(object? sender, LicensingErrorEventArgs e)
        {
            MessageBox.Show(e.Message, "Licensing Error", MessageBoxButton.OK, e.MessageBoxImage);
        }

        private void LicensingService_LicensingStateChanged(object? sender, LicensingStateChangedEventArgs e)
        {
            LicensingState = e.LicensingState;
        }

        #endregion

        #region Interface

        public string LicensingState
        {
            get => _licensingState;
            private set
            {
                _licensingState = value;
                OnPropertyChanged(nameof(LicensingState));
            }
        }

        public void OpenLicenseManager()
        {
	        var licenseManager = new LicenseManager { DataContext = _licenseManagerViewModel };
            licenseManager.ShowDialog();
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
