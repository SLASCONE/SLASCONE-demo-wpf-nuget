using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Slascone.Client;
using Slascone.Client.Interfaces;
using Slascone.Provisioning.Wpf.Sample.NuGet.Main;

namespace Slascone.Provisioning.Wpf.Sample.NuGet.Services.SimulateNoInternet
{
	// ------------------------------------------------------------------------------
	// This decorator simulates a temporarily lost internet connection.
	// You don't need this in a real application.
	//
	// If offline mode is detected, the all requests will return a
	// ApiResponse with a status code of 400 and a message that indicates that
	// the internet connection is lost.
	// You don't need this in a real application .
	// ------------------------------------------------------------------------------

	public class SlasconeProvisioningClientV2NoInternetDecorator : ISlasconeProvisioningClientV2
	{
		#region Members

		private ISlasconeProvisioningClientV2 _decoratedSlasconeProvisioningClientV2;
		private bool _offline = false;

		#endregion

		#region Construction

		public SlasconeProvisioningClientV2NoInternetDecorator(ISlasconeProvisioningClientV2 decoratedSlasconeProvisioningClientV2)
		{
			_decoratedSlasconeProvisioningClientV2 = decoratedSlasconeProvisioningClientV2;

			var notifyPropertyChanged = Application.Current.FindResource(nameof(MainViewModel)) as INotifyPropertyChanged;
			if (null != notifyPropertyChanged)
				notifyPropertyChanged.PropertyChanged += OnPropertyChanged;
		}

		private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if ("Offline" == e.PropertyName && sender is MainViewModel vm) 
				_offline = vm.Offline;
		}

		#endregion

		#region Implementation of ISlasconeProvisioningClientV2

		public async Task<ApiResponse<LicenseInfoDto>> ActivateLicenseAsync(ActivateClientDto device)
		{
			if (_offline) return await OfflineResponse<LicenseInfoDto>();

			return await _decoratedSlasconeProvisioningClientV2.ActivateLicenseAsync(device);
		}

		public async Task<ApiResponse<LicenseInfoDto>> ActivateLicenseAsync(ActivateClientDto device, CancellationToken cancellationToken)
		{
			if (_offline) return await OfflineResponse<LicenseInfoDto>();

			return await _decoratedSlasconeProvisioningClientV2.ActivateLicenseAsync(device, cancellationToken);
		}

		public async Task<ApiResponse<FileResponse>> ActivateOfflineLicenseAsync(Guid? product_id, string license_key, string client_id, string software_version)
		{
			if (_offline) return await OfflineResponse<FileResponse>();

			return await _decoratedSlasconeProvisioningClientV2.ActivateOfflineLicenseAsync(product_id, license_key, client_id, software_version);
		}

		public async Task<ApiResponse<FileResponse>> ActivateOfflineLicenseAsync(Guid? product_id, string license_key, string client_id, string software_version,
			CancellationToken cancellationToken)
		{
			if (_offline) return await OfflineResponse<FileResponse>();

			return await _decoratedSlasconeProvisioningClientV2.ActivateOfflineLicenseAsync(product_id, license_key, client_id, software_version, cancellationToken);
		}

		public async Task<ApiResponse<LicenseInfoDto>> AddHeartbeatAsync(AddHeartbeatDto heartbeatDto)
		{
			if (_offline) return await OfflineResponse<LicenseInfoDto>();

			return await _decoratedSlasconeProvisioningClientV2.AddHeartbeatAsync(heartbeatDto);
		}

		public async Task<ApiResponse<LicenseInfoDto>> AddHeartbeatAsync(AddHeartbeatDto heartbeatDto, CancellationToken cancellationToken)
		{
			if (_offline) return await OfflineResponse<LicenseInfoDto>();

			return await _decoratedSlasconeProvisioningClientV2.AddHeartbeatAsync(heartbeatDto, cancellationToken);
		}

		public async Task<ApiResponse<ICollection<LicenseDto>>> GetLicensesByCustomerAsync(GetLicensesByCustomerDto getLicensesByCustomerDto)
		{
			if (_offline) return await OfflineResponse<ICollection<LicenseDto>>();

			return await _decoratedSlasconeProvisioningClientV2.GetLicensesByCustomerAsync(getLicensesByCustomerDto);
		}

		public async Task<ApiResponse<ICollection<LicenseDto>>> GetLicensesByCustomerAsync(GetLicensesByCustomerDto getLicensesByCustomerDto, CancellationToken cancellationToken)
		{
			if (_offline) return await OfflineResponse<ICollection<LicenseDto>>();

			return await _decoratedSlasconeProvisioningClientV2.GetLicensesByCustomerAsync(getLicensesByCustomerDto, cancellationToken);
		}

		public async Task<ApiResponse<ICollection<LicenseDto>>> GetLicensesByUserAsync(GetLicensesByUserDto getLicensesByUserDto)
		{
			if (_offline) return await OfflineResponse<ICollection<LicenseDto>>();

			return await _decoratedSlasconeProvisioningClientV2.GetLicensesByUserAsync(getLicensesByUserDto);
		}

		public async Task<ApiResponse<ICollection<LicenseDto>>> GetLicensesByUserAsync(GetLicensesByUserDto getLicensesByUserDto, CancellationToken cancellationToken)
		{
			if (_offline) return await OfflineResponse<ICollection<LicenseDto>>();

			return await _decoratedSlasconeProvisioningClientV2.GetLicensesByUserAsync(getLicensesByUserDto, cancellationToken);
		}

		public async Task<ApiResponse<ICollection<LicenseDto>>> GetLicensesByLicenseKeyAsync(GetLicensesByLicenseKeyDto getLicensesByLicenseKeyDto)
		{
			if (_offline) return await OfflineResponse<ICollection<LicenseDto>>();

			return await _decoratedSlasconeProvisioningClientV2.GetLicensesByLicenseKeyAsync(getLicensesByLicenseKeyDto);
		}

		public async Task<ApiResponse<ICollection<LicenseDto>>> GetLicensesByLicenseKeyAsync(GetLicensesByLicenseKeyDto getLicensesByLicenseKeyDto,
			CancellationToken cancellationToken)
		{
			if (_offline) return await OfflineResponse<ICollection<LicenseDto>>();

			return await _decoratedSlasconeProvisioningClientV2.GetLicensesByLicenseKeyAsync(getLicensesByLicenseKeyDto, cancellationToken);
		}

		public async Task<ApiResponse<string>> UnassignLicenseAsync(UnassignDto unassign_dto)
		{
			if (_offline) return await OfflineResponse<string>();

			return await _decoratedSlasconeProvisioningClientV2.UnassignLicenseAsync(unassign_dto);
		}

		public async Task<ApiResponse<string>> UnassignLicenseAsync(UnassignDto unassign_dto, CancellationToken cancellationToken)
		{
			if (_offline) return await OfflineResponse<string>();

			return await _decoratedSlasconeProvisioningClientV2.UnassignLicenseAsync(unassign_dto, cancellationToken);
		}

		public async Task<ApiResponse<LicenseInfoDto>> GetDeviceInfoAsync(ValidateLicenseDto license)
		{
			if (_offline) return await OfflineResponse<LicenseInfoDto>();

			return await _decoratedSlasconeProvisioningClientV2.GetDeviceInfoAsync(license);
		}

		public async Task<ApiResponse<LicenseInfoDto>> GetDeviceInfoAsync(ValidateLicenseDto license, CancellationToken cancellationToken)
		{
			if (_offline) return await OfflineResponse<LicenseInfoDto>();

			return await _decoratedSlasconeProvisioningClientV2.GetDeviceInfoAsync(license, cancellationToken);
		}

		public async Task<ApiResponse<ConsumptionDto>> GetConsumptionStatusAsync(ValidateConsumptionStatusDto consumptionStatusDto)
		{
			if (_offline) return await OfflineResponse<ConsumptionDto>();

			return await _decoratedSlasconeProvisioningClientV2.GetConsumptionStatusAsync(consumptionStatusDto);
		}

		public async Task<ApiResponse<ConsumptionDto>> GetConsumptionStatusAsync(ValidateConsumptionStatusDto consumptionStatusDto, CancellationToken cancellationToken)
		{
			if (_offline) return await OfflineResponse<ConsumptionDto>();

			return await _decoratedSlasconeProvisioningClientV2.GetConsumptionStatusAsync(consumptionStatusDto, cancellationToken);
		}

		public async Task<ApiResponse<LicenseStateDto>> ToogleLicenseStateAsync(ToggleLicenseStateDto toggleLicenseStateDto)
		{
			if (_offline) return await OfflineResponse<LicenseStateDto>();

			return await _decoratedSlasconeProvisioningClientV2.ToogleLicenseStateAsync(toggleLicenseStateDto);
		}

		public async Task<ApiResponse<LicenseStateDto>> ToogleLicenseStateAsync(ToggleLicenseStateDto toggleLicenseStateDto, CancellationToken cancellationToken)
		{
			if (_offline) return await OfflineResponse<LicenseStateDto>();

			return await _decoratedSlasconeProvisioningClientV2.ToogleLicenseStateAsync(toggleLicenseStateDto, cancellationToken);
		}

		public async Task<ApiResponse<SessionStatusDto>> OpenSessionAsync(SessionRequestDto sessionRequestDto)
		{
			if (_offline) return await OfflineResponse<SessionStatusDto>();

			return await _decoratedSlasconeProvisioningClientV2.OpenSessionAsync(sessionRequestDto);
		}

		public async Task<ApiResponse<SessionStatusDto>> OpenSessionAsync(SessionRequestDto sessionRequestDto, CancellationToken cancellationToken)
		{
			if (_offline) return await OfflineResponse<SessionStatusDto>();

			return await _decoratedSlasconeProvisioningClientV2.OpenSessionAsync(sessionRequestDto, cancellationToken);
		}

		public async Task<ApiResponse<string>> CloseSessionAsync(SessionRequestDto sessionRequestDto)
		{
			if (_offline) return await OfflineResponse<string>();

			return await _decoratedSlasconeProvisioningClientV2.CloseSessionAsync(sessionRequestDto);
		}

		public async Task<ApiResponse<string>> CloseSessionAsync(SessionRequestDto sessionRequestDto, CancellationToken cancellationToken)
		{
			if (_offline) return await OfflineResponse<string>();

			return await _decoratedSlasconeProvisioningClientV2.CloseSessionAsync(sessionRequestDto, cancellationToken);
		}

        public async Task<ApiResponse<int>> GetActiveFloatingTokensCountAsync(Guid license_id)
        {
            if (_offline) return await OfflineResponse<int>();

			return await _decoratedSlasconeProvisioningClientV2.GetActiveFloatingTokensCountAsync(license_id);
        }

        public async Task<ApiResponse<int>> GetActiveFloatingTokensCountAsync(Guid license_id, CancellationToken cancellationToken)
        {
            if (_offline) return await OfflineResponse<int>();

            return await _decoratedSlasconeProvisioningClientV2.GetActiveFloatingTokensCountAsync(license_id, cancellationToken);
        }

        #endregion

        #region Implementation

        private async Task<ApiResponse<T>> OfflineResponse<T>()
		{
			await Task.Delay(1000);
			return new ApiResponse<T>
			{
				Error = null,
				StatusCode = 400,
				Message = "Simulated 'No internet' situation",
				Result = default
			};
		}

		#endregion
	}
}